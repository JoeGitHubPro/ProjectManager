using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Service;

namespace ProjectManager.StartUp
{
    public static class Seed
    {
        public static async Task SeedAdminAsync()
        {
            using (AuthenticationService service = new AuthenticationService())
            {
                var user = service.GetUser("Admin");
                if (user is null)
                {
                    await service.RegisterAsync("Admin", "Admin@mail.com", "01012345678", "0", Roles.Admin);

                }
                else
                {
                    return;
                }
            }
        }
        public async static Task SeedRolesAsync()
        {
            using (AppDbContext _context = new AppDbContext())
            {
                var isContainElements = _context.Roles.Any();

                if (isContainElements)
                    return;

                // List of roles from the Roles class
                var roles = new List<string>
        {
            Roles.Admin,
            Roles.Manager,
            Roles.User
        };

                RoleStore<IdentityRole> roleStore = new RoleStore<IdentityRole>(_context);

                foreach (var roleName in roles)
                {
                    if (!_context.Roles.Any(r => r.Name == roleName))
                    {
                        IdentityRole newRole = new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = roleName,
                            NormalizedName = roleName.ToUpper(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        };

                        await roleStore.CreateAsync(newRole);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
