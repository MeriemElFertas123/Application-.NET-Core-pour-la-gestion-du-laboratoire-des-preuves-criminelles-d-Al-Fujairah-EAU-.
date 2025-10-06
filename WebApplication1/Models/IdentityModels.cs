using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nom { get; set; }  // Nullable
        public string? Prenom { get; set; }  // Nullable
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
