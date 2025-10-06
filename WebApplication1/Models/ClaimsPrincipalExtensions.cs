using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace WebApplication1.Models
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("Permission", permission) ||
                   user.IsInRole("Admin"); // Les admins ont tout accès
        }
    }
}