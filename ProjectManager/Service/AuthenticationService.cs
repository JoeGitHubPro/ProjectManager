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

        private UserManager<IdentityUser> CreateUserManager(AppDbContext context)
        {
            return new UserManager<IdentityUser>(
                new UserStore<IdentityUser>(context),
                Options.Create(new IdentityOptions()),
                new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                null
            );
        }

        public async Task<IdentityResult> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
        {
            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    if (await userManager.FindByNameAsync(username) != null)
                        return IdentityResult.Failed(new IdentityError { Description = "Username is already taken." });

                    if (await userManager.FindByEmailAsync(email) != null)
                        return IdentityResult.Failed(new IdentityError { Description = "Email is already registered." });

                    var user = new IdentityUser { UserName = username, Email = email, PhoneNumber = phoneNumber };
                    var result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                        return result;

                    var roleResult = await userManager.AddToRoleAsync(user, role);
                    return roleResult.Succeeded ? IdentityResult.Success : roleResult;
                }
            }
        }


        public async Task<IdentityResultViewModel> LogInAsync(string usernameOrEmail, string password)
        {
            var user = GetUser(usernameOrEmail);
            if (user is null)
                return new IdentityResultViewModel { IsSuccessed = false, Errors = new IdentityError { Code = "101", Description = "Email or username not found!" } };

            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    bool isPasswordValid;
                    try
                    {
                        isPasswordValid = await userManager.CheckPasswordAsync(user, password);
                    }
                    catch (Exception)
                    {
                        return new IdentityResultViewModel { IsSuccessed = false, Errors = new IdentityError { Code = "101", Description = "Email or username not found!" } };
                    }

                    if (!isPasswordValid)
                        return new IdentityResultViewModel { IsSuccessed = false, Errors = new IdentityError { Code = "101", Description = "Email or username not found!" } };

                    var roles = await userManager.GetRolesAsync(user);

                    return new IdentityResultViewModel
                    {
                        UserName = user.UserName,
                        IsSuccessed = true,
                        Role = roles.SingleOrDefault() ?? string.Empty // Now role is just string
                    };
                }
            }
        }

        public string GetUserId(string usernameOrEmail)
        {
            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    IdentityUser? user;
                    if (IsValidEmail(usernameOrEmail))
                    {
                        user = userManager.FindByEmailAsync(usernameOrEmail).Result;
                    }
                    else
                    {
                        user = userManager.FindByNameAsync(usernameOrEmail).Result;
                    }

                    return user?.Id ?? string.Empty;
                }
            }
        }

        public IdentityUser GetUser(string usernameOrEmail)
        {
            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    if (IsValidEmail(usernameOrEmail))
                    {
                        return userManager.FindByEmailAsync(usernameOrEmail).Result;
                    }
                    else
                    {
                        return userManager.FindByNameAsync(usernameOrEmail).Result;
                    }
                }
            }
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    var users = await userManager.Users.ToListAsync();

                    var result = new List<UserViewModel>();

                    foreach (var user in users)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        result.Add(new UserViewModel
                        {
                            UserName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Roles = string.Join(",", roles)
                        });
                    }

                    return result;
                }
            }
        }


        public async Task<bool> UserInRoleAsync(string usernameOrEmail, string role)
        {
            var user = GetUser(usernameOrEmail);
            if (user is null) return false;

            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    return await userManager.IsInRoleAsync(user, role);
                }
            }
        }


        public async Task<bool> EditUserAsync(UserViewModel user)
        {
            using (var context = new AppDbContext())
            {
                using (var userManager = CreateUserManager(context))
                {
                    var editUser = await userManager.FindByIdAsync(user.UserId); // Get user by ID

                    if (editUser == null)
                        return false; // User does not exist

                    // Check if email is already used by another user (excluding the current user)
                    var emailExists = await userManager.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.UserId);
                    if (emailExists)
                        throw new InvalidOperationException("Email is already taken by another user.");

                    // Check if username is already used by another user (excluding the current user)
                    var usernameExists = await userManager.Users.AnyAsync(u => u.UserName == user.UserName && u.Id != user.UserId);
                    if (usernameExists)
                        throw new InvalidOperationException("Username is already taken by another user.");

                    // Update user details
                    editUser.UserName = user.UserName;
                    editUser.Email = user.Email;
                    editUser.PhoneNumber = user.PhoneNumber;

                    // Update roles
                    var currentRoles = await userManager.GetRolesAsync(editUser);
                    await userManager.RemoveFromRolesAsync(editUser, currentRoles);
                    await userManager.AddToRoleAsync(editUser, user.Roles);

                    // Update password if a new password is provided
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        // Remove the current password
                        var removePasswordResult = await userManager.RemovePasswordAsync(editUser);
                        if (!removePasswordResult.Succeeded)
                        {
                            throw new InvalidOperationException("Failed to remove the current password.");
                        }

                        // Add the new password
                        var addPasswordResult = await userManager.AddPasswordAsync(editUser, user.Password);
                        if (!addPasswordResult.Succeeded)
                        {
                            throw new InvalidOperationException("Failed to set the new password.");
                        }
                    }

                    context.Update(editUser);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
        }



        public async Task<IdentityResultViewModel> DeleteUserAsync(string email)
        {
            using (var context = new AppDbContext())
            {
                IdentityUser? user;
                using (var userManager = CreateUserManager(context))
                {
                    user = await userManager.FindByEmailAsync(email);
                    if (user is null)
                        return new IdentityResultViewModel { IsSuccessed = false };
                }

                context.Users.Remove(user);

                try
                {
                    await context.SaveChangesAsync();
                    return new IdentityResultViewModel { IsSuccessed = true };
                }
                catch (DbUpdateException)
                {
                    return new IdentityResultViewModel
                    {
                        IsSuccessed = false,
                        Errors = new IdentityError
                        {
                            Code = "103",
                            Description = "Cannot delete this user because they are assigned as a cashier in existing orders. Please reassign or delete the related orders before proceeding."
                        }
                    };
                }
            }
        }

        private static bool IsValidEmail(string email)
        {
            try { _ = new MailAddress(email); return true; }
            catch { return false; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
        }

        ~AuthenticationService() => Dispose(false);
    }
}
