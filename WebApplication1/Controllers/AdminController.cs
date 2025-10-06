using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{

    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
        }

        // Méthode pour récupérer les permissions - appelée avant chaque action
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && User.IsInRole("Admin"))
            {
                // Si c'est un admin, donner tous les droits
                ViewBag.UserPermissions = new List<string>
                {
                    "AjouterEnqueteur",
                    "ModifierEnqueteur",
                    "SupprimerEnqueteur",
                    "AfficherEnqueteur",
                    "AjouterAnalyste",
                    "ModifierAnalyste",
                    "SupprimerAnalyste",
                    "AfficherAnalyste",
                    "ajouter Preuve",
                    "modifier Preuve",
                    "supprimer Preuve",
                    "Stocker",
                    "Afficher Preuve",
                    "GenererRapport",
                    "ArchiverPreuve",
                    "AttribuerDroits",
                    "ValidationAnalyse",
                    "AfficherAnalyste",
                    "ModifierProfil",
                    "Verifier la Correspondance",
                    "Assigner Affaire",
                    "VisualiserTtesAffaires",
                    "Cloturer Affaire",
                    "ArchiverAffaire",
                    "Creer Affaire",
                    "rechercher Affaire",
                    "Modifier Affaire",
                    "Consulter Rapports",
                    "recherche avancée",
                    "Preparer Envoi",
                    "consulter les preuves",
                    "Consulter l'historique des envois",
                    "Receptionner les echantillons",
                    "Creer Zone de stockage",
                    "Consulter Le Plan Stockage",
                    "Transferer Vers Analyste",
                    "signer et Valider rapport",
                    "consulter les resultats",
                    "Notifier les enqueteurs"
                };
            }
            else if (user != null)
            {
                var userId = user.Id;

                var userRoles = await _userManager.GetRolesAsync(user);
                var roleIds = _db.Roles
                    .Where(r => userRoles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToList();

                var rolePermissions = await _db.RolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.permission)
                    .ToListAsync();

                var userPermissions = await _db.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.Permission.permission)
                    .ToListAsync();

                var allPermissions = rolePermissions
                    .Union(userPermissions)
                    .Distinct()
                    .ToList();

                ViewBag.UserPermissions = allPermissions;
            }

            await next();
        }

        // GET: Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _db.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_db.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,nom,prenom,email,DateCreation,DerniereConnexion,Statut,UserId")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _db.Add(admin);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_db.Users, "Id", "Email", admin.UserId);
            return View(admin);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _db.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_db.Users, "Id", "Email", admin.UserId);
            return View(admin);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,nom,prenom,email,DateCreation,DerniereConnexion,Statut,UserId")] Admin admin)
        {
            if (id != admin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(admin);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_db.Users, "Id", "Email", admin.UserId);
            return View(admin);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _db.Admins
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _db.Admins.FindAsync(id);
            _db.Admins.Remove(admin);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _db.Admins.Any(e => e.Id == id);
        }
    }
}