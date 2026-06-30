using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemsService _svc;
        public MenuItemsController(IMenuItemsService svc)
        {
            _svc = svc;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll([FromQuery] bool activeOnly = false)
        => Ok(await _svc.GetAllAsync(activeOnly));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<MenuItemDto>> Create(CreateMenuItemDto dto)
            => Ok(await _svc.CreateAsync(dto));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<MenuItemDto>> Update(int id, CreateMenuItemDto dto)
            => Ok(await _svc.UpdateAsync(id, dto));

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/toggle-active")]
        public async Task<IActionResult> Toggle(int id)
            => await _svc.ToggleActiveAsync(id) ? Ok() : NotFound();
    }
}
