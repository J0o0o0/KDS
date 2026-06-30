using Microsoft.AspNetCore.Identity;

namespace KDS.Server.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? StationId { get; set; }
        public Station? Station { get; set; }
    }
}