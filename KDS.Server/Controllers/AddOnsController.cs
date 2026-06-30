using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddOnsController : ControllerBase
    {
        private readonly IAddOnsService _svc;
        public AddOnsController(IAddOnsService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddOnDto>>> GetAll([FromQuery] bool activeOnly = false)
        => Ok(await _svc.GetAllAsync(activeOnly));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<AddOnDto>> Create(CreateAddOnDto dto)
        => Ok(await _svc.CreateAsync(dto));

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/toggle-active")]
        public async Task<IActionResult> Toggle(int id)
        => await _svc.ToggleActiveAsync(id) ? Ok() : NotFound();
    }
}
