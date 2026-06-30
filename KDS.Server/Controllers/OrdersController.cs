using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _svc;
        public OrdersController(IOrdersService svc)
        {
            _svc = svc;
        }

        // POST /api/orders — Cashier creates an order
        [Authorize(Roles = "Admin,Cashier")]
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
        {
            var cashierId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cashierName = User.FindFirstValue(ClaimTypes.Email)!;
            

            try
            {
                var order = await _svc.CreateAsync(dto, cashierId, cashierName);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/orders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var order = await _svc.GetByIdAsync(id);
            return order is null ? NotFound() : Ok(order);
        }

        // GET /api/orders/active — all orders not yet served
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetActive()
            => Ok(await _svc.GetActiveAsync());

        // GET /api/orders — all orders within a date range
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(DateTime from, DateTime to)
            => Ok(await _svc.GetAllAsync(from, to));

        // GET /api/orders/station/{stationId} — filtered for one station screen
        [HttpGet("station/{stationId:int}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStation(int stationId)
            => Ok(await _svc.GetByStationAsync(stationId));

        // PATCH /api/orders/{orderId}/components/{componentId}/status
        // Cook marks their component as ready, or Expediter marks any component
        [Authorize(Roles = "Admin,Cook,Expediter")]
        [HttpPatch("{orderId:int}/components/{componentId:int}/status")]
        public async Task<IActionResult> UpdateComponentStatus(int orderId, int componentId, [FromBody] UpdateStatusDto dto)
            => await _svc.UpdateComponentStatusAsync(orderId, componentId, dto.Status) ? Ok() : NotFound();

        // PATCH /api/orders/{orderId}/status — Expediter marks whole order ready/served
        [Authorize(Roles = "Admin,Expediter")]
        [HttpPatch("{orderId:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusDto dto)
            => await _svc.UpdateOrderStatusAsync(orderId, dto.Status) ? Ok() : NotFound();
    }
}

