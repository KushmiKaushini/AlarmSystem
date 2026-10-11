namespace AlarmSystem.Models.UserManagement
{
    public class CreateUserRequest
    {
        public string? Name { get; set; }
        public string? AzureAccount { get; set; }
        public string? ServiceId { get; set; }
        public string? RoleName { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? AzureAccount { get; set; }
        public string? ServiceId { get; set; }
        public string? RoleName { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UserRecordDto
    {
        public int Id { get; set; }
        public string ServiceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
    }
}