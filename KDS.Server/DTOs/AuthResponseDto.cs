namespace KDS.Server.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime ExpiresAt { get; set; }

        public int? StationId { get; set; }
        public string? StationName { get; set; }

    }
}
