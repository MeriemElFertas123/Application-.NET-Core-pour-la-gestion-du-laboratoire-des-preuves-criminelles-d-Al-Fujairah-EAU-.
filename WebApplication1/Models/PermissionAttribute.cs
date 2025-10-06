using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Filters
{
    public class PermissionAttribute : IAsyncActionFilter
    {
        private readonly string _requiredPermission;

        public PermissionAttribute(string requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await CheckUserPermissionAsync(userId, _requiredPermission))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["AccessDenied"] =
                        $"Vous n'avez pas les permissions nécessaires pour accéder à cette fonctionnalité ({_requiredPermission}).";
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            await next();
        }

        private async Task<bool> CheckUserPermissionAsync(string userId, string permission)
        {
            using var db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());

            // Vérifier si l'utilisateur est admin
            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                var userRoles = await db.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var roleNames = await db.Roles
                    .Where(r => userRoles.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync();

                if (roleNames.Contains("Admin"))
                    return true;
            }

            // Permissions directes
            var hasDirectPermission = await db.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.Permission.permission == permission);

            if (hasDirectPermission)
                return true;

            // Permissions via rôles
            var userRoleIds = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var hasRolePermission = await db.RolePermissions
                .AnyAsync(rp => userRoleIds.Contains(rp.RoleId) && rp.Permission.permission == permission);

            return hasRolePermission;
        }
    }

    public class RefreshPermissionsAttribute : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated && context.Controller is Controller controller)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var permissions = await GetUserPermissionsAsync(userId);
                controller.ViewBag.UserPermissions = permissions;
            }

            await next();
        }

        private async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var permissions = new List<string>();

            using var db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());

            // Permissions directes
            var userPermissions = await db.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.permission)
                .ToListAsync();

            permissions.AddRange(userPermissions);

            // Permissions via rôles
            var userRoles = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (userRoles.Any())
            {
                var rolePermissions = await db.RolePermissions
                    .Where(rp => userRoles.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.permission)
                    .ToListAsync();

                permissions.AddRange(rolePermissions);
            }

            // Admin a tous les droits
            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                var roleNames = await db.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                if (roleNames.Contains("Admin"))
                {
                    var allPermissions = await db.Permissions.Select(p => p.permission).ToListAsync();
                    permissions.AddRange(allPermissions);
                }
            }

            return permissions.Distinct().ToList();
        }
    }
}