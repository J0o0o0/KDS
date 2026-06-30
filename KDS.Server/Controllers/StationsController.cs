using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StationsController : ControllerBase
    {
        private readonly IStationsService _svc;
        public StationsController(IStationsService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StationDto>>> GetAll()
        => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StationDto>> GetOne(int id)
        {
            var s = await _svc.GetAsync(id);
            return s is null ? NotFound() : Ok(s);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<StationDto>> Create(CreateStationDto dto)
        {
            var s = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetOne), new { id = s.Id }, s);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateStationDto dto)
        => await _svc.UpdateAsync(id, dto) ? NoContent() : NotFound();

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/toggle-active")]
        public async Task<IActionResult> Toggle(int id)
        => await _svc.ToggleActiveAsync(id) ? Ok() : NotFound();

    }

}
