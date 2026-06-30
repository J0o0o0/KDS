using KDS.Server.DTOs;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _svc;
        public UsersController(IUsersService svc)
        {
            _svc = svc;
        }

        // GET /api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
            => Ok(await _svc.GetAllAsync());

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _svc.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        // PATCH /api/users/{id}/toggle-active
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
            => await _svc.ToggleActiveAsync(id) ? Ok() : NotFound();

        // POST /api/users/{id}/roles — add a role to a user
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<UserDto>> AddRole(string id, AssignRoleDto dto)
        {
            var (found, validRole, user) = await _svc.AddRoleAsync(id, dto.Role);
            if (!found) return NotFound();
            if (!validRole) return BadRequest($"Invalid role '{dto.Role}'. Valid: Admin, Cashier, Cook, Expediter");
            return Ok(user);
        }

        // DELETE /api/users/{id}/roles/{role} — remove a role from a user
        [HttpDelete("{id}/roles/{role}")]
        public async Task<ActionResult<UserDto>> RemoveRole(string id, string role)
        {
            var (found, user) = await _svc.RemoveRoleAsync(id, role);
            return found ? Ok(user) : NotFound();
        }

        // PATCH /api/users/{id}/station — assign or clear (stationId: null) a Cook's station
        [HttpPatch("{id}/station")]
        public async Task<ActionResult<UserDto>> AssignStation(string id, AssignStationDto dto)
        {
            var (found, validStation, user) = await _svc.AssignStationAsync(id, dto.StationId);
            if (!found) return NotFound();
            if (!validStation) return BadRequest("Invalid stationId — no station with that id exists.");
            return Ok(user);
        }
    }
}
