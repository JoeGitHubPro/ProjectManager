using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManager.Data;
using ProjectManager.ViewModels.Identities;
using System.Net.Mail;

namespace ProjectManager.Service
{
    public class AuthenticationService : IDisposable
    {
        private bool _disposed = false;
        private readonly AppDbContext _context;

        public AuthenticationService()
        {
            _context = new AppDbContext();
        }

        private UserManager<IdentityUser> CreateUserManager()
        {
            return new UserManager<IdentityUser>(
                new UserStore<IdentityUser>(_context),
                Options.Create(new IdentityOptions()),
                new PasswordHasher<IdentityUser>(),
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                null);
        }

        public async Task<IdentityResult> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
        {
            using var userManager = CreateUserManager();

            if (await userManager.FindByNameAsync(username) != null)
                return IdentityResult.Failed(new IdentityError { Description = "Username is already taken." });

            if (await userManager.FindByEmailAsync(email) != null)
                return IdentityResult.Failed(new IdentityError { Description = "Email is already registered." });

            var user = new IdentityUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return result;

            var roleResult = await userManager.AddToRoleAsync(user, role);
            return roleResult.Succeeded ? IdentityResult.Success : roleResult;
        }

        public async Task<IdentityResultViewModel> LogInAsync(string usernameOrEmail, string password)
        {
            using var userManager = CreateUserManager();
            var user = await GetUserAsync(usernameOrEmail);

            if (user is null)
                return new IdentityResultViewModel
                {
                    IsSuccessed = false,
                    Errors = new IdentityError
                    {
                        Code = "101",
                        Description = "Email or username not found!"
                    }
                };

            try
            {
                var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
                if (!isPasswordValid)
                    return new IdentityResultViewModel
                    {
                        IsSuccessed = false,
                        Errors = new IdentityError
                        {
                            Code = "101",
                            Description = "Invalid credentials"
                        }
                    };

                var roles = await userManager.GetRolesAsync(user);

                return new IdentityResultViewModel
                {
                    UserName = user.UserName,
                    IsSuccessed = true,
                    Role = roles.FirstOrDefault() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                return new IdentityResultViewModel
                {
                    IsSuccessed = false,
                    Errors = new IdentityError
                    {
                        Code = "500",
                        Description = $"Login error: {ex.Message}"
                    }
                };
            }
        }

        public async Task<string> GetUserIdAsync(string usernameOrEmail)
        {
            using var userManager = CreateUserManager();
            var user = await GetUserAsync(usernameOrEmail);
            return user?.Id ?? string.Empty;
        }

        public async Task<IdentityUser> GetUserAsync(string usernameOrEmail)
        {
            using var userManager = CreateUserManager();

            return IsValidEmail(usernameOrEmail)
                ? await userManager.FindByEmailAsync(usernameOrEmail)
                : await userManager.FindByNameAsync(usernameOrEmail);
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            using var userManager = CreateUserManager();
            var users = await userManager.Users.ToListAsync();

            var result = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(new UserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<bool> UserInRoleAsync(string usernameOrEmail, string role)
        {
            using var userManager = CreateUserManager();
            var user = await GetUserAsync(usernameOrEmail);
            return user != null && await userManager.IsInRoleAsync(user, role);
        }
        public async Task<List<IdentityUser>> GetUsersInRoleAsync(string roleName)
        {
            using var userManager = CreateUserManager();

            var usersInRole = new List<IdentityUser>();

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, roleName))
                {
                    usersInRole.Add(user);
                }
            }

            return usersInRole;
        }

        public async Task<bool> EditUserAsync(UserViewModel user)
        {
            using var userManager = CreateUserManager();
            var existingUser = await userManager.FindByIdAsync(user.UserId);

            if (existingUser == null)
                return false;

            // Check for duplicate email
            var emailUser = await userManager.FindByEmailAsync(user.Email);
            if (emailUser != null && emailUser.Id != user.UserId)
                throw new InvalidOperationException("Email is already taken by another user.");

            // Check for duplicate username
            var usernameUser = await userManager.FindByNameAsync(user.UserName);
            if (usernameUser != null && usernameUser.Id != user.UserId)
                throw new InvalidOperationException("Username is already taken by another user.");

            // Update user properties
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            // Update roles
            var currentRoles = await userManager.GetRolesAsync(existingUser);
            await userManager.RemoveFromRolesAsync(existingUser, currentRoles);
            await userManager.AddToRoleAsync(existingUser, user.Roles);

            // Update password if provided
            if (!string.IsNullOrEmpty(user.Password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(existingUser);
                var result = await userManager.ResetPasswordAsync(existingUser, token, user.Password);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Password update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var updateResult = await userManager.UpdateAsync(existingUser);
            return updateResult.Succeeded;
        }

        public async Task<IdentityResultViewModel> DeleteUserAsync(string email)
        {
            using var userManager = CreateUserManager();
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
                return new IdentityResultViewModel { IsSuccessed = false };

            try
            {
                var result = await userManager.DeleteAsync(user);
                return new IdentityResultViewModel
                {
                    IsSuccessed = result.Succeeded,
                    Errors = result.Succeeded ? null : result.Errors.FirstOrDefault()
                };
            }
            catch (DbUpdateException ex)
            {
                return new IdentityResultViewModel
                {
                    IsSuccessed = false,
                    Errors = new IdentityError
                    {
                        Code = "103",
                        Description = "Cannot delete user because of existing references in the database."
                    }
                };
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        ~AuthenticationService() => Dispose(false);
    }
}