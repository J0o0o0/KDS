using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly AppDbContext _dbContext;
        private readonly IKitchenNotifier _notifier;
        public OrdersService(AppDbContext dbContext, IKitchenNotifier notifier)
        {
            _dbContext = dbContext;
            _notifier = notifier;
        }

        // ─── CREATE ─────────────────────────────────────────────
        public async Task<OrderDto> CreateAsync(CreateOrderDto dto, string cashierId, string cashierName)
        {
            // Parse order type
            if (!Enum.TryParse<OrderType>(dto.OrderType, true, out var orderType))
                throw new ArgumentException("Invalid OrderType. Valid: DineIn, Pickup");

            if (orderType == OrderType.DineIn && string.IsNullOrWhiteSpace(dto.TableNumber))
                throw new ArgumentException("TableNumber is required for DineIn");
            if (orderType == OrderType.Pickup && string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new ArgumentException("CustomerName is required for Pickup");

            // Load all menu items referenced in the order
            var menuItemIds = dto.Items.Select(i => i.MenuItemId).Distinct().ToList();
            var menuItems = await _dbContext.MenuItems
                .Include(m => m.Components).ThenInclude(mc => mc.Component)
                .Where(m => menuItemIds.Contains(m.Id))
                .ToListAsync();

            if (menuItems.Count != menuItemIds.Count)
                throw new ArgumentException("One or more MenuItemId values are invalid");

            // Load ALL components referenced in the order (direct + swapped) with variants + station
            var allComponentIdsInOrder = dto.Items
                .SelectMany(i => i.Components)
                .Select(c => c.ComponentId)
                .Distinct().ToList();

            var allComponents = await _dbContext.Components
                .Include(c => c.Variants)
                .Include(c => c.DefaultStation)
                .Where(c => allComponentIdsInOrder.Contains(c.Id))
                .ToListAsync();

            var componentsDict = allComponents.ToDictionary(c => c.Id);

            // Load all swap pairs involving any component in the order
            var allSwapPairs = await _dbContext.SwapPairs
                .Where(s => allComponentIdsInOrder.Contains(s.ComponentAId)
                         || allComponentIdsInOrder.Contains(s.ComponentBId)
                         )
                .ToListAsync();

            // Load all addons referenced
            var allAddOnIds = dto.Items
                .SelectMany(i => i.Components)
                .SelectMany(c => c.AddOns)
                .Select(a => a.AddOnId)
                .Distinct().ToList();
            var addons = await _dbContext.AddOns
                .Where(a => allAddOnIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            var order = new Order
            {
                OrderNumber = orderNumber,
                OrderType = orderType,
                TableNumber = orderType == OrderType.DineIn ? dto.TableNumber : null,
                CustomerName = orderType == OrderType.Pickup ? dto.CustomerName : null,
                Notes = dto.Notes,
                Status = OrderStatus.New,
                CashierId = cashierId,
                CashierName = cashierName,
                CreatedAt = DateTime.UtcNow
            };

            decimal orderTotal = 0;

            foreach (var itemDto in dto.Items)
            {
                var menuItem = menuItems.First(m => m.Id == itemDto.MenuItemId);
                var menuItemComponents = menuItem.Components.ToList(); // the "slots"

                // Track running quantity per slot (for validation)
                var slotQuantities = menuItemComponents.ToDictionary(mc => mc.Id, _ => 0);

                decimal itemDeltas = 0;

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    MenuItemName = menuItem.Name,
                    Quantity = itemDto.Quantity,
                    Notes = itemDto.Notes,
                    Status = ItemStatus.New
                };

                foreach (var configDto in itemDto.Components)
                {
                    // Resolve the ACTUAL component (could be swapped)
                    if (!componentsDict.TryGetValue(configDto.ComponentId, out var actualComponent))
                        throw new ArgumentException($"Component {configDto.ComponentId} not found");

                    // Find which menu item slot this config fills (direct or swap)
                    MenuItemComponent? matchedSlot = null;

                    // Direct match?
                    matchedSlot = menuItemComponents.FirstOrDefault(mc => mc.ComponentId == configDto.ComponentId);

                    // Swap match?
                    if (matchedSlot is null)
                    {
                        foreach (var slot in menuItemComponents)
                        {
                            var (a, b) = slot.ComponentId < configDto.ComponentId
                                ? (slot.ComponentId, configDto.ComponentId)
                                : (configDto.ComponentId, slot.ComponentId);
                            if (allSwapPairs.Any(s => s.ComponentAId == a && s.ComponentBId == b))
                            {
                                matchedSlot = slot;
                                break;
                            }
                        }
                    }

                    if (matchedSlot is null)
                        throw new ArgumentException(
                            $"Component '{actualComponent.Name}' is not part of menu item '{menuItem.Name}' and has no valid swap");

                    // Track quantity against the slot
                    slotQuantities[matchedSlot.Id] += configDto.Quantity;

                    // Resolve variant from the ACTUAL component (not the original slot component)
                    ComponentVariant? variant = null;
                    if (configDto.VariantId.HasValue)
                    {
                        variant = actualComponent.Variants.FirstOrDefault(v => v.Id == configDto.VariantId);
                        if (variant is null)
                            throw new ArgumentException(
                                $"Variant {configDto.VariantId} not found on component '{actualComponent.Name}'");
                    }
                    else
                    {
                        variant = actualComponent.Variants.FirstOrDefault(v => v.IsDefault && v.IsActive);
                        if (variant is null && actualComponent.Variants.Any())
                            variant = actualComponent.Variants.First(v => v.IsActive);
                    }

                    // Station from the ACTUAL component (swapped component may go to a different station)
                    int stationId = variant?.OverrideStationId ?? actualComponent.DefaultStationId;
                    string stationName = actualComponent.DefaultStation?.Name ?? "Unknown";

                    // Price deltas
                    if (variant != null)
                        itemDeltas += variant.PriceDelta * configDto.Quantity;

                    var orderComp = new OrderItemComponent
                    {
                        ComponentId = actualComponent.Id,
                        ComponentName = actualComponent.Name,        // snapshot of ACTUAL
                        VariantId = variant?.Id,
                        VariantName = variant?.Name ?? "",           // snapshot
                        Quantity = configDto.Quantity,
                        StationId = stationId,                       // snapshot
                        StationName = stationName,                   // snapshot
                        Status = ItemStatus.New
                    };

                    // AddOns
                    foreach (var addOnDto in configDto.AddOns)
                    {
                        if (!addons.TryGetValue(addOnDto.AddOnId, out var addon))
                            throw new ArgumentException($"AddOn {addOnDto.AddOnId} not found");

                        // Validate addon is allowed for the ACTUAL component
                        var allowed = await _dbContext.ComponentAllowedAddOns
                            .AnyAsync(a => a.ComponentId == actualComponent.Id && a.AddOnId == addon.Id);
                        if (!allowed)
                            throw new ArgumentException(
                                $"AddOn '{addon.Name}' is not allowed on component '{actualComponent.Name}'");

                        int totalQty = addOnDto.Quantity * configDto.Quantity;
                        itemDeltas += addon.Price * totalQty;

                        orderComp.AddOns.Add(new OrderItemComponentAddOn
                        {
                            AddOnId = addon.Id,
                            AddOnName = addon.Name,
                            Price = addon.Price,
                            Quantity = totalQty,
                            IsRemoval = addon.IsRemoval
                        });
                    }

                    orderItem.Components.Add(orderComp);
                }

                // Validate: every slot must be filled with the correct total quantity
                foreach (var slot in menuItemComponents)
                {
                    var expected = slot.Quantity;
                    var actual = slotQuantities[slot.Id];
                    if (actual != expected)
                        throw new ArgumentException(
                            $"Menu item '{menuItem.Name}' expects {expected} × '{slot.Component!.Name}', " +
                            $"but order provides {actual}. " +
                            $"(Direct + swapped quantities must total {expected} per slot.)");
                }

                // Calculate prices
                orderItem.UnitPrice = menuItem.BasePrice + (itemDeltas / itemDto.Quantity);
                orderItem.LineTotal = (menuItem.BasePrice * itemDto.Quantity) + itemDeltas;

                orderTotal += orderItem.LineTotal;
                order.Items.Add(orderItem);
            }

            order.TotalAmount = orderTotal;

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            var orderDto = await MapToDtoAsync(order.Id);
            await _notifier.NotifyOrderCreatedAsync(orderDto);
            return orderDto;
        }

        // ─── READ ───────────────────────────────────────────────
        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var exists = await _dbContext.Orders.AnyAsync(o => o.Id == id);
            if (!exists) return null;
            return await MapToDtoAsync(id);
        }

        public async Task<IEnumerable<OrderDto>> GetActiveAsync()
        {
            var ids = await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Ready)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.Id)
                .ToListAsync();

            var result = new List<OrderDto>();
            foreach (var id in ids)
                result.Add(await MapToDtoAsync(id));
            return result;
        }
        public async Task<IEnumerable<OrderDto>> GetAllAsync(DateTime from, DateTime to)
        {
            var ids = await _dbContext.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.Id)
                .ToListAsync();

            var result = new List<OrderDto>();
            foreach (var id in ids)
                result.Add(await MapToDtoAsync(id));
            return result;
        }

        public async Task<IEnumerable<OrderDto>> GetByStationAsync(int stationId)
        {
            var ids = await _dbContext.OrderItemComponents
                .Where(c => c.StationId == stationId &&
                            (c.Status == ItemStatus.New || c.Status == ItemStatus.InProgress))
                .Select(c => c.OrderItem!.OrderId)
                .Distinct()
                .ToListAsync();

            var result = new List<OrderDto>();
            foreach (var id in ids)
                result.Add(await MapToDtoAsync(id));
            return result;
        }

        // ─── STATUS UPDATES ────────────────────────────────────
        public async Task<bool> UpdateComponentStatusAsync(int orderId, int componentId, string statusStr)
        {
            if (!Enum.TryParse<ItemStatus>(statusStr, true, out var status))
                return false;

            var comp = await _dbContext.OrderItemComponents
                .FirstOrDefaultAsync(c => c.Id == componentId && c.OrderItem!.OrderId == orderId);
            if (comp is null) return false;

            comp.Status = status;
            await _dbContext.SaveChangesAsync();

            // Auto-progress the parent order item
            await AutoProgressOrderItemAsync(comp.OrderItemId);

            // Auto-progress the order
            await AutoProgressOrderAsync(orderId);

            var updatedOrder = await MapToDtoAsync(orderId);
            await _notifier.NotifyOrderUpdatedAsync(updatedOrder);
            await _notifier.NotifyComponentStatusChangedAsync(orderId, componentId, status.ToString());

            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string statusStr)
        {
            if (!Enum.TryParse<OrderStatus>(statusStr, true, out var status))
                return false;

            var order = await _dbContext.Orders
                .Include(o => o.Items).ThenInclude(i => i.Components)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null) return false;

            order.Status = status;

            if (status == OrderStatus.Ready) order.ReadyAt ??= DateTime.UtcNow;
            if (status == OrderStatus.Served) order.ServedAt ??= DateTime.UtcNow;
            if (status == OrderStatus.Cancelled) order.CancelledAt ??= DateTime.UtcNow;
            // If expediter marks order InProgress, mark all components InProgress
            if (status == OrderStatus.InProgress)
            {
                foreach (var item in order.Items)
                {
                    foreach (var comp in item.Components)
                    {
                        if (comp.Status != ItemStatus.Bumped && comp.Status != ItemStatus.Ready)
                            comp.Status = ItemStatus.InProgress;
                    }
                    if (item.Status != ItemStatus.Bumped && item.Status != ItemStatus.Ready)
                        item.Status = ItemStatus.InProgress;
                }
            }
            // If expediter marks order Ready, mark all components Ready
            if (status == OrderStatus.Ready)
            {
                foreach (var item in order.Items)
                {
                    foreach (var comp in item.Components)
                    {
                        if (comp.Status != ItemStatus.Bumped)
                            comp.Status = ItemStatus.Ready;
                    }
                    if (item.Status != ItemStatus.Bumped)
                        item.Status = ItemStatus.Ready;
                }
            }

            await _dbContext.SaveChangesAsync();

            var updatedOrder = await MapToDtoAsync(orderId);
            await _notifier.NotifyOrderUpdatedAsync(updatedOrder);
            await _notifier.NotifyOrderStatusChangedAsync(orderId, status.ToString());

            return true;
        }

        // ─── HELPERS ────────────────────────────────────────────
        private async Task<int> GenerateOrderNumberAsync()
        {
            var max = await _dbContext.Orders.AnyAsync()
                ? await _dbContext.Orders.MaxAsync(o => o.OrderNumber)
                : 1000;
            return max + 1;
        }

        private async Task AutoProgressOrderItemAsync(int orderItemId)
        {
            var item = await _dbContext.OrderItems
                .Include(i => i.Components)
                .FirstOrDefaultAsync(i => i.Id == orderItemId);
            if (item is null) return;

            if (item.Components.All(c => c.Status == ItemStatus.Ready || c.Status == ItemStatus.Bumped))
            {
                if (item.Status != ItemStatus.Bumped)
                    item.Status = ItemStatus.Ready;
            }
            else if (item.Components.Any(c => c.Status == ItemStatus.InProgress || c.Status == ItemStatus.Ready))
            {
                if (item.Status == ItemStatus.New)
                    item.Status = ItemStatus.InProgress;
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task AutoProgressOrderAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null) return;

            if (order.Items.All(i => i.Status == ItemStatus.Ready || i.Status == ItemStatus.Bumped))
            {
                if (order.Status == OrderStatus.New || order.Status == OrderStatus.InProgress)
                {
                    order.Status = OrderStatus.Ready;
                    order.ReadyAt ??= DateTime.UtcNow;
                }
            }
            else if (order.Items.Any(i => i.Status == ItemStatus.InProgress || i.Status == ItemStatus.Ready))
            {
                if (order.Status == OrderStatus.New)
                    order.Status = OrderStatus.InProgress;
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task<OrderDto> MapToDtoAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items).ThenInclude(i => i.Components).ThenInclude(c => c.AddOns)
                .FirstAsync(o => o.Id == orderId);

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderType = order.OrderType.ToString(),
                TableNumber = order.TableNumber,
                CustomerName = order.CustomerName,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                ReadyAt = order.ReadyAt,
                ServedAt = order.ServedAt,
                CashierName = order.CashierName,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    MenuItemName = i.MenuItemName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal,
                    Notes = i.Notes,
                    Status = i.Status.ToString(),
                    Components = i.Components.Select(c => new OrderItemComponentDto
                    {
                        Id = c.Id,
                        ComponentName = c.ComponentName,
                        VariantName = c.VariantName,
                        Quantity = c.Quantity,
                        StationName = c.StationName,
                        StationId = c.StationId,
                        Status = c.Status.ToString(),
                        AddOns = c.AddOns.Select(a => new OrderItemComponentAddOnDto
                        {
                            AddOnName = a.AddOnName,
                            Quantity = a.Quantity,
                            IsRemoval = a.IsRemoval
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }
    }
}

