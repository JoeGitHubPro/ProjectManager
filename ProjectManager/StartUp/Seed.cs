using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Service;

namespace ProjectManager.StartUp
{
    public static class Seed
    {
        public static async Task SeedAdminAsync()
        {
            using var service = new AuthenticationService();
            var user = await service.GetUserAsync("Admin")
                .ConfigureAwait(false);

            if (user != null) return;

            var result = await service.RegisterAsync(
                "Admin",
                "Admin@mail.com",
                "01012345678",
                "7nJ4oq7@f*Dg",
                Roles.Admin)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"Admin user seeding failed: {errors}");
            }
        }

        public static async Task SeedRolesAsync()
        {
            await using var context = new AppDbContext();
            if (await context.Roles.AnyAsync().ConfigureAwait(false))
                return;

            var roles = new List<string>
            {
                Roles.Admin,
                Roles.Manager,
                Roles.User
            };

            var roleStore = new RoleStore<IdentityRole>(context);

            foreach (var roleName in roles)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == roleName)
                    .ConfigureAwait(false))
                {
                    var newRole = new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    await roleStore.CreateAsync(newRole)
                        .ConfigureAwait(false);
                }
            }

            await context.SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public static async Task InitializeDatabaseAsync()
        {
            try
            {
                await SeedRolesAsync().ConfigureAwait(false);
                await SeedAdminAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database initialization failed", ex);
            }
        }
    }
}