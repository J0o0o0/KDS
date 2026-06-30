using KDS.Server.Models;

namespace KDS.Server.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(AppUser user);
    }
}
