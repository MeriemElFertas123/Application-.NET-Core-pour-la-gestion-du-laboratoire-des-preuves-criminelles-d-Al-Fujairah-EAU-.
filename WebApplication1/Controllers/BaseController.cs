using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _db;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext context;

        public BaseController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
       
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var userPermissions = GetUserPermissions(userId).Result;

                ViewBag.UserPermissions = userPermissions;

                // Optionnel : Définir le breadcrumb
                SetBreadcrumb(context);
            }

            base.OnActionExecuting(context);
        }

        private async Task<List<string>> GetUserPermissions(string userId)
        {
            var permissions = new List<string>();

            try
            {
                // Permissions attribuées directement
                var userPermissions = await _db.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.Permission.permission)
                    .ToListAsync();

                permissions.AddRange(userPermissions);

                // Permissions via rôles
                var user = await _userManager.FindByIdAsync(userId);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Any())
                {
                    var roleIds = _db.Roles
                        .Where(r => roles.Contains(r.Name))
                        .Select(r => r.Id)
                        .ToList();

                    var rolePermissions = await _db.RolePermissions
                        .Where(rp => roleIds.Contains(rp.RoleId))
                        .Select(rp => rp.Permission.permission)
                        .ToListAsync();

                    permissions.AddRange(rolePermissions);
                }

                // Admin → tous les droits
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var allPermissions = await _db.Permissions
                        .Select(p => p.permission)
                        .ToListAsync();
                    permissions.AddRange(allPermissions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des permissions: {ex.Message}");
            }

            return permissions.Distinct().ToList();
        }

        protected bool HasPermission(string permission)
        {
            if (ViewBag.UserPermissions != null)
            {
                var userPermissions = (List<string>)ViewBag.UserPermissions;
                return userPermissions.Contains(permission) || User.IsInRole("Admin");
            }
            return User.IsInRole("Admin");
        }

        protected async Task RefreshPermissions()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var userPermissions = await GetUserPermissions(userId);
                ViewBag.UserPermissions = userPermissions;
            }
        }

        protected void SetBreadcrumb(ActionExecutingContext context)
        {
            var breadcrumbItems = new List<dynamic>();

            var controllerName = context.RouteData.Values["controller"].ToString();
            var actionName = context.RouteData.Values["action"].ToString();

            switch (controllerName.ToLower())
            {
                case "preuve":
                    breadcrumbItems.Add(new { Text = "Gestion des Preuves", Url = Url.Action("Index", "Preuve"), IsActive = false });
                    if (actionName.ToLower() == "create")
                        breadcrumbItems.Add(new { Text = "Ajouter Preuve", Url = "", IsActive = true });
                    else if (actionName.ToLower() == "edit")
                        breadcrumbItems.Add(new { Text = "Modifier Preuve", Url = "", IsActive = true });
                    else if (actionName.ToLower() == "delete")
                        breadcrumbItems.Add(new { Text = "Supprimer Preuve", Url = "", IsActive = true });
                    break;

                case "analyse":
                    breadcrumbItems.Add(new { Text = "Gestion des Analyses", Url = Url.Action("Index", "Analyse"), IsActive = false });
                    if (actionName.ToLower() == "create")
                        breadcrumbItems.Add(new { Text = "Nouvelle Analyse", Url = "", IsActive = true });
                    else if (actionName.ToLower() == "edit")
                        breadcrumbItems.Add(new { Text = "Modifier Analyse", Url = "", IsActive = true });
                    break;

                case "enquete":
                    breadcrumbItems.Add(new { Text = "Gestion des Enquêtes", Url = Url.Action("Index", "Enquete"), IsActive = false });
                    if (actionName.ToLower() == "create")
                        breadcrumbItems.Add(new { Text = "Nouvelle Enquête", Url = "", IsActive = true });
                    break;

                case "permissions":
                    breadcrumbItems.Add(new { Text = "Administration", Url = Url.Action("Index", "Home"), IsActive = false });
                    if (actionName.ToLower() == "attribuer")
                        breadcrumbItems.Add(new { Text = "Attribuer Permissions", Url = "", IsActive = true });
                    break;
            }

            ViewBag.BreadcrumbItems = breadcrumbItems;
        }

        public class RolePermissionViewModel
        {
            public string SelectedRoleId { get; set; }
            public IEnumerable<IdentityRole> Roles { get; set; }
            public List<PermissionItem> AvailablePermissions { get; set; }
        }

        public class PermissionItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }

        public class UserPermissionSummary
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public int PermissionCount { get; set; }
        }
    }
}
