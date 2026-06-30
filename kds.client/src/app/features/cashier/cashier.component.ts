import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MenuItemsService } from '../../core/services/menu-items.service';
import { ComponentsService } from '../../core/services/components.service';
import { AddOnsService } from '../../core/services/add-ons.service';
import { OrdersService } from '../../core/services/orders.service';
import { TopBarComponent } from '../../shared/top-bar.component';
import {
  AddOn,
  CreateOrderItemRequest,
  MenuComponent as MenuComponentModel,
  MenuItem,
  OrderType,
} from '../../core/models';

interface ComponentRow {
  rowId: number;
  componentId: number;
  componentName: string;
  quantity: number;
  variantId: number | null;
  availableVariants: { id: number; name: string; priceDelta: number }[];
  availableAddOns: { addOnId: number; addOnName: string; maxQuantity: number }[];
  selectedAddOnQuantities: Map<number, number>;
}

interface ComponentGroup {
  groupComponentId: number;
  groupComponentName: string;
  totalQuantity: number;
  swapOptions: { componentId: number; componentName: string }[];
  rows: ComponentRow[];
}

interface CartLine {
  menuItem: MenuItem;
  quantity: number;
  notes: string;
  groups: ComponentGroup[];
}

let nextRowId = 1;

@Component({
  selector: 'app-cashier',
  standalone: true,
  imports: [CommonModule, FormsModule, TopBarComponent],
  templateUrl: './cashier.component.html',
  styleUrls: ['./cashier.component.scss'],
})
export class CashierComponent implements OnInit {
  readonly Math = Math;

  readonly menuItems = signal<MenuItem[]>([]);
  readonly allComponents = signal<MenuComponentModel[]>([]);
  readonly allAddOns = signal<AddOn[]>([]);
  readonly activeCategory = signal<string>('All');

  readonly cart = signal<CartLine[]>([]);
  readonly configuring = signal<CartLine | null>(null);

  // When editing an existing cart line (vs. adding a new one), this holds
  // its index in the cart array so confirmAddToCart() knows to replace it
  // in place rather than push a new line.
  readonly editingCartIndex = signal<number | null>(null);

  readonly orderType = signal<OrderType>('DineIn');
  tableNumber = '';
  customerName = '';

  readonly submitting = signal(false);
  readonly lastOrderNumber = signal<number | null>(null);

  constructor(
    private menuItemsSvc: MenuItemsService,
    private componentsSvc: ComponentsService,
    private addOnsSvc: AddOnsService,
    private ordersSvc: OrdersService
  ) {}

  ngOnInit(): void {
    this.menuItemsSvc.getAll(true).subscribe((items) => this.menuItems.set(items));
    this.componentsSvc.getAll(true).subscribe((c) => this.allComponents.set(c));
    this.addOnsSvc.getAll(true).subscribe((a) => this.allAddOns.set(a));
  }

  categories = computed(() => {
    const cats = new Set(this.menuItems().map((m) => m.category));
    return ['All', ...cats];
  });

  filteredMenuItems = computed(() => {
    const cat = this.activeCategory();
    return cat === 'All' ? this.menuItems() : this.menuItems().filter((m) => m.category === cat);
  });

  cartTotal = computed(() => this.cart().reduce((sum, line) => sum + this.lineTotal(line), 0));

  private fullComponent(componentId: number): MenuComponentModel | undefined {
    return this.allComponents().find((c) => c.id === componentId);
  }

  private rowUnitPrice(row: ComponentRow): number {
    let price = 0;
    const variant = row.availableVariants.find((v) => v.id === row.variantId);
    if (variant) price += variant.priceDelta;
    for (const [addOnId, qty] of row.selectedAddOnQuantities) {
      const addOn = this.allAddOns().find((a) => a.id === addOnId);
      if (addOn && !addOn.isRemoval) price += addOn.price * qty;
    }
    return price;
  }

  private lineTotal(line: CartLine): number {
    let perUnit = line.menuItem.basePrice;
    for (const group of line.groups) {
      for (const row of group.rows) {
        perUnit += this.rowUnitPrice(row) * row.quantity;
      }
    }
    return perUnit * line.quantity;
  }

  variantName(row: ComponentRow): string {
    return row.availableVariants.find((v) => v.id === row.variantId)?.name ?? '';
  }

  variantPriceDelta(row: ComponentRow): number {
    return row.availableVariants.find((v) => v.id === row.variantId)?.priceDelta ?? 0;
  }

  /** Add-on unit price, looked up from the global add-on catalog by id. */
  addOnUnitPrice(addOnId: number): number {
    return this.allAddOns().find((a) => a.id === addOnId)?.price ?? 0;
  }

  addOnName(addOnId: number): string {
    return this.allAddOns().find((a) => a.id === addOnId)?.name ?? '';
  }

  addOnIsRemoval(addOnId: number): boolean {
    return this.allAddOns().find((a) => a.id === addOnId)?.isRemoval ?? false;
  }

  /** Selected add-ons for a row as an array (for *ngFor-friendly iteration), each with its line price. */
  rowAddOnEntries(row: ComponentRow): { addOnId: number; quantity: number }[] {
    return Array.from(row.selectedAddOnQuantities.entries()).map(([addOnId, quantity]) => ({ addOnId, quantity }));
  }

  /** Summary string of a cart line's components for the collapsed cart-list view, e.g. "2x Patty (Cheese), 1x Bun". */
  lineComponentsSummary(line: CartLine): { group: ComponentGroup; row: ComponentRow }[] {
    const result: { group: ComponentGroup; row: ComponentRow }[] = [];
    for (const group of line.groups) {
      for (const row of group.rows) {
        result.push({ group, row });
      }
    }
    return result;
  }

  startConfiguring(menuItem: MenuItem): void {
    const groups: ComponentGroup[] = menuItem.components.map((mc) => {
      const full = this.fullComponent(mc.componentId);
      const row = this.buildRow(mc.componentId, mc.quantity);
      return {
        groupComponentId: mc.componentId,
        groupComponentName: mc.componentName,
        totalQuantity: mc.quantity,
        swapOptions: full?.swappableWith ?? [],
        rows: [row],
      };
    });

    this.editingCartIndex.set(null); // adding a brand new line, not editing
    this.configuring.set({
      menuItem,
      quantity: 1,
      notes: '',
      groups,
    });
  }

  /** Reopen the configuration modal for an existing cart line, pre-filled with its current state. */
  startEditingCartLine(index: number): void {
    const line = this.cart()[index];
    if (!line) return;

    // Deep-clone so cancelling the modal doesn't mutate the cart line in place.
    const clonedGroups: ComponentGroup[] = line.groups.map((g) => ({
      ...g,
      rows: g.rows.map((r) => ({
        ...r,
        rowId: nextRowId++,
        availableVariants: [...r.availableVariants],
        availableAddOns: [...r.availableAddOns],
        selectedAddOnQuantities: new Map(r.selectedAddOnQuantities),
      })),
    }));

    this.editingCartIndex.set(index);
    this.configuring.set({
      menuItem: line.menuItem,
      quantity: line.quantity,
      notes: line.notes,
      groups: clonedGroups,
    });
  }

  private buildRow(componentId: number, quantity: number): ComponentRow {
    const full = this.fullComponent(componentId);
    const defaultVariant = full?.variants.find((v) => v.isDefault) ?? full?.variants[0];
    return {
      rowId: nextRowId++,
      componentId,
      componentName: full?.name ?? '',
      quantity,
      variantId: defaultVariant?.id ?? null,
      availableVariants: full?.variants.map((v) => ({ id: v.id, name: v.name, priceDelta: v.priceDelta })) ?? [],
      availableAddOns: full?.allowedAddOns ?? [],
      selectedAddOnQuantities: new Map<number, number>(),
    };
  }

  setRowVariant(row: ComponentRow, variantId: number): void {
    row.variantId = variantId;
  }

  isAddOnSelected(row: ComponentRow, addOnId: number): boolean {
    return row.selectedAddOnQuantities.has(addOnId);
  }

  addOnQty(row: ComponentRow, addOnId: number): number {
    return row.selectedAddOnQuantities.get(addOnId) ?? 1;
  }

  toggleRowAddOn(row: ComponentRow, addOnId: number): void {
    if (row.selectedAddOnQuantities.has(addOnId)) row.selectedAddOnQuantities.delete(addOnId);
    else row.selectedAddOnQuantities.set(addOnId, 1);
  }

  incrementRowAddOnQty(row: ComponentRow, addOnId: number, max: number): void {
    const current = row.selectedAddOnQuantities.get(addOnId) ?? 1;
    row.selectedAddOnQuantities.set(addOnId, Math.min(max, current + 1));
  }

  decrementRowAddOnQty(row: ComponentRow, addOnId: number): void {
    const current = row.selectedAddOnQuantities.get(addOnId) ?? 1;
    row.selectedAddOnQuantities.set(addOnId, Math.max(1, current - 1));
  }

  swapRowComponent(group: ComponentGroup, row: ComponentRow, newComponentId: number): void {
    const full = this.fullComponent(newComponentId);
    if (!full) return;
    const defaultVariant = full.variants.find((v) => v.isDefault) ?? full.variants[0];
    row.componentId = newComponentId;
    row.componentName = full.name;
    row.variantId = defaultVariant?.id ?? null;
    row.availableVariants = full.variants.map((v) => ({ id: v.id, name: v.name, priceDelta: v.priceDelta }));
    row.availableAddOns = full.allowedAddOns;
    row.selectedAddOnQuantities = new Map();
  }

  splitGroup(group: ComponentGroup): void {
    const splittable = group.rows.find((r) => r.quantity > 1);
    if (!splittable) return;

    const moveQty = Math.floor(splittable.quantity / 2);
    splittable.quantity -= moveQty;

    const newRow = this.buildRow(group.groupComponentId, moveQty);
    group.rows.push(newRow);
  }

  canSplitGroup(group: ComponentGroup): boolean {
    return group.rows.some((r) => r.quantity > 1);
  }

  removeRow(group: ComponentGroup, row: ComponentRow): void {
    const idx = group.rows.findIndex((r) => r.rowId === row.rowId);
    if (idx < 0 || group.rows.length === 1) return;

    group.rows.splice(idx, 1);
    const target = group.rows[Math.max(0, idx - 1)];
    target.quantity += row.quantity;
  }

  adjustRowQuantity(group: ComponentGroup, row: ComponentRow, delta: 1 | -1): void {
    if (group.rows.length < 2) return;
    const last = group.rows[group.rows.length - 1];
    const partner = row === last ? group.rows[0] : last;

    if (delta === 1) {
      if (partner.quantity <= 1) return;
      row.quantity += 1;
      partner.quantity -= 1;
    } else {
      if (row.quantity <= 1) return;
      row.quantity -= 1;
      partner.quantity += 1;
    }
  }

  groupTotalAssigned(group: ComponentGroup): number {
    return group.rows.reduce((sum, r) => sum + r.quantity, 0);
  }

  confirmAddToCart(line: CartLine): void {
    const editIndex = this.editingCartIndex();
    if (editIndex !== null) {
      this.cart.update((c) => c.map((existing, i) => (i === editIndex ? line : existing)));
    } else {
      this.cart.update((c) => [...c, line]);
    }
    this.editingCartIndex.set(null);
    this.configuring.set(null);
  }

  cancelConfiguring(): void {
    this.editingCartIndex.set(null);
    this.configuring.set(null);
  }

  removeFromCart(index: number, event?: Event): void {
    event?.stopPropagation(); // don't trigger startEditingCartLine when clicking Remove
    this.cart.update((c) => c.filter((_, i) => i !== index));
  }

  submitOrder(): void {
    if (this.cart().length === 0) return;
    this.submitting.set(true);

    const items: CreateOrderItemRequest[] = this.cart().map((line) => ({
      menuItemId: line.menuItem.id,
      quantity: line.quantity,
      notes: line.notes || null,
      components: line.groups.flatMap((group) =>
        group.rows.map((row) => ({
          componentId: row.componentId,
          variantId: row.variantId,
          quantity: row.quantity,
          addOns: Array.from(row.selectedAddOnQuantities.entries()).map(([addOnId, quantity]) => ({
            addOnId,
            quantity,
          })),
        }))
      ),
    }));

    this.ordersSvc
      .create({
        orderType: this.orderType(),
        tableNumber: this.orderType() === 'DineIn' ? this.tableNumber : null,
        customerName: this.orderType() === 'Pickup' ? this.customerName : null,
        items,
      })
      .subscribe({
        next: (order) => {
          this.lastOrderNumber.set(order.orderNumber);
          this.cart.set([]);
          this.tableNumber = '';
          this.customerName = '';
          this.submitting.set(false);
        },
        error: () => this.submitting.set(false),
      });
  }
}
