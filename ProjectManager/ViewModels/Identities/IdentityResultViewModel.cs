using Microsoft.AspNetCore.Identity;

namespace ProjectManager.ViewModels.Identities
{
    public class IdentityResultViewModel
    {
        public string? UserName { get; set; }
        public bool IsSuccessed { get; set; }
        public IdentityError? Errors { get; set; }
        public string? Role { get; set; } = null;


    }
}
