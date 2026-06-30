using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using KDS.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KDS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _jwt;
        private readonly AppDbContext _db;

        public AuthController(UserManager<AppUser> userManager, IJwtTokenService jwt, AppDbContext db)
        {
            _userManager = userManager;
            _jwt = jwt;
            _db = db;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Station)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null)
                return Unauthorized("Invalid credentials");
            if(!user.IsActive)
            {
                return Unauthorized("This Account Has Been DeActivated");
            }

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
                return Unauthorized("Invalid credentials");

            var token = await _jwt.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles.ToList(),
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                StationId = user.StationId,
                StationName = user.Station?.Name
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                return BadRequest("Email already registered");

            string[] validRoles = { "Admin", "Cashier", "Cook", "Expediter" };
            foreach (var role in dto.Roles)
                if (!validRoles.Contains(role))
                    return BadRequest($"Invalid role '{role}'. Valid: {string.Join(", ", validRoles)}");

            // Station only applies if the user is being made a Cook.
            // If they're not a Cook, any StationId sent is ignored rather than erroring.
            int? stationId = null;
            if (dto.Roles.Contains("Cook") && dto.StationId is not null)
            {
                var stationExists = await _db.Stations.AnyAsync(s => s.Id == dto.StationId);
                if (!stationExists)
                    return BadRequest("Invalid stationId — no station with that id exists.");
                stationId = dto.StationId;
            }

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                IsActive = true,
                EmailConfirmed = true,
                StationId = stationId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRolesAsync(user, dto.Roles);

            return Ok(new AuthResponseDto
            {
                Token = await _jwt.GenerateTokenAsync(user),
                Email = user.Email!,
                FullName = user.FullName,
                Roles = dto.Roles,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                StationId = user.StationId,
                StationName = stationId is not null
                    ? (await _db.Stations.FirstOrDefaultAsync(s => s.Id == stationId))?.Name
                    : null
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.Station)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Email,
                user.FullName,
                Roles = roles,
                user.IsActive,
                user.StationId,
                StationName = user.Station?.Name
            });
        }
    }
}
