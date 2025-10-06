using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class PermissionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public PermissionsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
            : base(db, userManager, roleManager)
        {
            _context = db;
        }

        // GET: Permissions/Attribuer
        public async Task<IActionResult> Attribuer(string userId = null)
        {
            if (!HasPermission("AttribuerDroits"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les permissions nécessaires pour accéder à cette page.";
                return RedirectToAction("Index", "Home");
            }

            var allUsers = await _userManager.Users
                            .Where(u => u.Email != "admin@gmail.com")
                            .ToListAsync();

            var model = new AttributionViewModel
            {
                SelectedUserId = userId,
                Users = new SelectList(allUsers, "Id", "UserName", userId),
                AvailablePermissions = await _db.Permissions
                    .Select(p => new WebApplication1.Models.PermissionItem
                    {
                        Id = p.Id,
                        Name = p.permission,
                        IsSelected = userId != null && _db.UserPermissions
                            .Any(up => up.UserId == userId && up.PermissionId == p.Id)
                    }).ToListAsync()
            };

            return View(model);
        }

        // POST: Permissions/Attribuer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attribuer(AttributionViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"=== DEBUG ATTRIBUER ===");
            System.Diagnostics.Debug.WriteLine($"SelectedUserId: {model.SelectedUserId}");

            if (!HasPermission("AttribuerDroits"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les permissions nécessaires.";
                return RedirectToAction("Index", "Home");
            }

            // Validation des données
            if (string.IsNullOrEmpty(model.SelectedUserId))
            {
                TempData["ErrorMessage"] = "Veuillez sélectionner un utilisateur.";
                await ReloadViewModel(model);
                return View(model);
            }

            if (model.AvailablePermissions == null || !model.AvailablePermissions.Any())
            {
                TempData["ErrorMessage"] = "Aucune permission disponible.";
                await ReloadViewModel(model);
                return View(model);
            }

            var selectedPermissions = model.AvailablePermissions.Where(p => p.IsSelected).ToList();
            if (!selectedPermissions.Any())
            {
                TempData["ErrorMessage"] = "Veuillez sélectionner au moins une permission.";
                await ReloadViewModel(model);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.SelectedUserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                await ReloadViewModel(model);
                return View(model);
            }

            try
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Supprimer les permissions existantes
                        var existingUserPermissions = await _db.UserPermissions
                            .Where(up => up.UserId == model.SelectedUserId)
                            .ToListAsync();

                        System.Diagnostics.Debug.WriteLine($"Existing permissions to remove: {existingUserPermissions.Count}");

                        if (existingUserPermissions.Any())
                        {
                            _db.UserPermissions.RemoveRange(existingUserPermissions);
                            await _db.SaveChangesAsync();
                            System.Diagnostics.Debug.WriteLine("Existing permissions removed");
                        }

                        // 2. Ajouter les nouvelles permissions
                        var newUserPermissions = new List<UserPermission>();

                        foreach (var perm in selectedPermissions)
                        {
                            var permissionExists = await _db.Permissions
                                .AnyAsync(p => p.Id == perm.Id);

                            if (!permissionExists)
                            {
                                System.Diagnostics.Debug.WriteLine($"Permission {perm.Id} not found in database");
                                continue;
                            }

                            var userPermission = new UserPermission
                            {
                                UserId = model.SelectedUserId,
                                PermissionId = perm.Id
                            };

                            newUserPermissions.Add(userPermission);
                        }

                        if (newUserPermissions.Any())
                        {
                            _db.UserPermissions.AddRange(newUserPermissions);
                            System.Diagnostics.Debug.WriteLine($"Adding {newUserPermissions.Count} new permissions");
                        }

                        // 3. Sauvegarder les nouvelles permissions
                        var savedCount = await _db.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"SaveChanges returned: {savedCount} rows affected");

                        // 4. Gérer les permissions par rôle
                        await UpdateRolePermissions(model.SelectedUserId, selectedPermissions);

                        // 5. Valider la transaction
                        await transaction.CommitAsync();
                        System.Diagnostics.Debug.WriteLine("Transaction committed successfully");

                        TempData["SuccessMessage"] = $"Permissions attribuées avec succès à l'utilisateur {user.UserName}.";
                        return RedirectToAction("Attribuer", new { userId = model.SelectedUserId });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR in transaction: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OUTER ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                var errorMessage = "Une erreur s'est produite lors de l'attribution des permissions.";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Détail: {ex.InnerException.Message}";
                }

                TempData["ErrorMessage"] = errorMessage;
            }

            await ReloadViewModel(model);
            return View(model);
        }

        // Méthode helper pour gérer les permissions de rôle
        private async Task UpdateRolePermissions(string userId, List<WebApplication1.Models.PermissionItem>
 selectedPermissions)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userRoles = await _userManager.GetRolesAsync(user);
                System.Diagnostics.Debug.WriteLine($"User roles: {string.Join(", ", userRoles)}");

                foreach (var roleName in userRoles)
                {
                    var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                    if (role != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing role: {roleName} (ID: {role.Id})");

                        foreach (var perm in selectedPermissions)
                        {
                            var rolePermissionExists = await _db.RolePermissions
                                .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id.ToString());

                            if (!rolePermissionExists)
                            {
                                var rolePermission = new RolePermission
                                {
                                    RoleId = role.Id,
                                    PermissionId = perm.Id.ToString()
                                };

                                _db.RolePermissions.Add(rolePermission);
                                System.Diagnostics.Debug.WriteLine($"Added RolePermission: RoleId={rolePermission.RoleId}, PermissionId={rolePermission.PermissionId}");
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("RolePermissions saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating role permissions: {ex.Message}");
            }
        }

        // Méthode helper pour recharger les données du ViewModel
        private async Task ReloadViewModel(AttributionViewModel model)
        {
            var allUsers = await _userManager.Users
                .Where(u => u.Email != "admin@gmail.com")
                .ToListAsync();

            model.Users = new SelectList(allUsers, "Id", "UserName", model.SelectedUserId);

            if (model.AvailablePermissions == null)
            {
                model.AvailablePermissions = await _db.Permissions
                    .Select(p => new WebApplication1.Models.PermissionItem
                    {
                        Id = p.Id,
                        Name = p.permission,
                        IsSelected = model.SelectedUserId != null && _db.UserPermissions
                            .Any(up => up.UserId == model.SelectedUserId && up.PermissionId == p.Id)
                    }).ToListAsync();
            }
        }

        // GET: Permissions/GetUserPermissions
        public async Task<JsonResult> GetUserPermissions(string userId)
        {
            if (!HasPermission("AttribuerDroits"))
            {
                return Json(new { success = false, message = "Accès refusé" });
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "ID utilisateur requis" });
            }

            try
            {
                var userPermissions = await _db.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.PermissionId)
                    .ToListAsync();

                return Json(new { success = true, permissions = userPermissions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la récupération des permissions: " + ex.Message });
            }
        }

        // GET: Permissions/AttribuerParRole
        public async Task<IActionResult> AttribuerParRole()
        {
            if (!HasPermission("AttribuerDroits"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les permissions nécessaires.";
                return RedirectToAction("Index", "Home");
            }

            var model = new RolePermissionViewModel
            {
                Roles = await _db.Roles.ToListAsync(),
                AvailablePermissions = await _db.Permissions
                    .Select(p => new PermissionItem
                    {
                        Id = p.Id,
                        Name = p.permission,
                        IsSelected = false
                    }).ToListAsync()
            };

            return View(model);
        }

        // POST: Permissions/AttribuerParRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttribuerParRole(RolePermissionViewModel model)
        {
            if (!HasPermission("AttribuerDroits"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les permissions nécessaires.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var transaction = await _db.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Supprimer les permissions existantes pour ce rôle
                            var existingRolePermissions = _db.RolePermissions
                                .Where(rp => rp.RoleId == model.SelectedRoleId);
                            _db.RolePermissions.RemoveRange(existingRolePermissions);

                            // Ajouter les nouvelles permissions
                            foreach (var perm in model.AvailablePermissions.Where(p => p.IsSelected))
                            {
                                _db.RolePermissions.Add(new RolePermission
                                {
                                    RoleId = model.SelectedRoleId,
                                    PermissionId = perm.Id.ToString()
                                });
                            }

                            await _db.SaveChangesAsync();
                            await transaction.CommitAsync();

                            TempData["SuccessMessage"] = "Permissions attribuées au rôle avec succès.";
                            return RedirectToAction("AttribuerParRole");
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Une erreur s'est produite lors de l'attribution des permissions: " + ex.Message;
                }
            }

            // Recharger les données en cas d'erreur
            model.Roles = await _db.Roles.ToListAsync();
            model.AvailablePermissions = await _db.Permissions
                .Select(p => new PermissionItem
                {
                    Id = p.Id,
                    Name = p.permission,
                    IsSelected = false
                }).ToListAsync();

            return View(model);
        }

        // GET: Permissions/Index
        public async Task<IActionResult> Index()
        {
            if (!HasPermission("AttribuerDroits"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les permissions nécessaires.";
                return RedirectToAction("Index", "Home");
            }

            var usersWithPermissions = await _db.Users
                .Select(u => new UserPermissionSummary
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PermissionCount = u.UserPermissions.Count()
                }).ToListAsync();

            return View(usersWithPermissions);
        }
    }
}
   