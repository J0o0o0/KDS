namespace KDS.Server.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int? StationId { get; set; }
        public string? StationName { get; set; }
    }

    public class AssignRoleDto
    {
        public string Role { get; set; } = string.Empty; // Admin | Cashier | Cook | Expediter
    }

    public class AssignStationDto
    {
        // Nullable so a Cook can be explicitly un-assigned from any station.
        public int? StationId { get; set; }
    }
}
