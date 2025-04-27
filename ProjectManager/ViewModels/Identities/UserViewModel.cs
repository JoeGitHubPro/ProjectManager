namespace ProjectManager.ViewModels.Identities
{
    public class UserViewModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Roles { get; set; } // Comma-separated list of roles
        public string? Password { get; set; }

    }
}
