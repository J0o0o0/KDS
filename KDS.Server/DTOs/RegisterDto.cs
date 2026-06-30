using System.ComponentModel.DataAnnotations;

namespace KDS.Server.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public List<string> Roles { get; set; } = new() { "Cashier" };
        public int? StationId { get; set; }

    }
}
