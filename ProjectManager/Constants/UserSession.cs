namespace ProjectManager.Constants
{
    public static class UserSession
    {
        public static string? Username { get; set; }
        public static string? Email { get; set; }
        public static string? Role { get; set; }
        public static void ClearSession()
        {
            Username = null;
            Email = null;
            Role = null;
        }

    }
}
