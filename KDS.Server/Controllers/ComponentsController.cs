using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComponentsController : ControllerBase
    {
        private readonly IComponentsService _svc;
        public ComponentsController(IComponentsService svc)
        {
            _svc = svc;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComponentDto>>> GetAll([FromQuery] bool activeOnly = false)
        => Ok(await _svc.GetAllAsync(activeOnly));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ComponentDto>> Create(CreateComponentDto dto)
            => Ok(await _svc.CreateAsync(dto));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ComponentDto>> Update(int id, CreateComponentDto dto)
            => Ok(await _svc.UpdateAsync(id ,dto));

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/variants")]
        public async Task<IActionResult> AddVariant(int id, CreateVariantDto dto)
            => await _svc.AddVariantAsync(id, dto) ? Ok() : NotFound();

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/addons")]
        public async Task<IActionResult> AddAllowedAddOn(int id, AllowedAddOnDto dto)
            => await _svc.AddAllowedAddOnAsync(id, dto) ? Ok() : NotFound();

        [Authorize(Roles = "Admin")]
        [HttpPost("{idA:int}/swaps/{idB:int}")]
        public async Task<IActionResult> AddSwap(int idA, int idB)
            => await _svc.AddSwapAsync(idA, idB) ? Ok() : NotFound();
    }
}
