using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class TechniciensController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TechniciensController(ApplicationDbContext context,
                                   UserManager<ApplicationUser> userManager,
                                   RoleManager<IdentityRole> roleManager) : base(context, userManager, roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Techniciens
        public async Task<IActionResult> Index()
        {
            var techniciens = await _context.Techniciens
                .Include(t => t.User)
                .ToListAsync();
            return View(techniciens);
        }

        // GET: Techniciens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var technicien = await _context.Techniciens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technicien == null)
            {
                return NotFound();
            }

            return View(technicien);
        }

        // GET: Techniciens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Techniciens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email,MotDePasse")] Technicien technicien)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(technicien.Email) || string.IsNullOrEmpty(technicien.MotDePasse))
                {
                    ModelState.AddModelError("", "L'email et le mot de passe sont requis.");
                    return View(technicien);
                }

                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = technicien.Email,
                        Email = technicien.Email
                    };

                    var result = await _userManager.CreateAsync(user, technicien.MotDePasse);

                    if (result.Succeeded)
                    {
                        // Vérifier si le rôle existe, sinon le créer
                        if (!await _roleManager.RoleExistsAsync("Technicien"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Technicien"));
                        }

                        await _userManager.AddToRoleAsync(user, "Technicien");
                        technicien.UserId = user.Id;

                        _context.Techniciens.Add(technicien);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Technicien créé avec succès !";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erreur : " + ex.Message);
                }
            }

            return View(technicien);
        }

        // GET: Techniciens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var technicien = await _context.Techniciens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technicien == null)
            {
                return NotFound();
            }

            ViewBag.CurrentEmail = technicien.User?.Email;
            return View(technicien);
        }

        // POST: Techniciens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,UserId")] Technicien technicien, string email, string password)
        {
            if (id != technicien.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(technicien);

                    if (!string.IsNullOrEmpty(technicien.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(technicien.UserId);
                        if (user != null)
                        {
                            bool userUpdated = false;

                            if (!string.IsNullOrEmpty(email) && user.Email != email)
                            {
                                user.Email = email;
                                user.UserName = email;
                                userUpdated = true;
                            }

                            if (!string.IsNullOrEmpty(password))
                            {
                                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                                await _userManager.ResetPasswordAsync(user, token, password);
                                userUpdated = true;
                            }

                            if (userUpdated)
                            {
                                await _userManager.UpdateAsync(user);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Technicien modifié avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TechnicienExists(technicien.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erreur : " + ex.Message);
                }
            }

            var currentTechnicien = await _context.Techniciens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == technicien.Id);

            ViewBag.CurrentEmail = currentTechnicien?.User?.Email;
            return View(technicien);
        }

        // GET: Techniciens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var technicien = await _context.Techniciens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (technicien == null)
            {
                return NotFound();
            }

            return View(technicien);
        }

        // POST: Techniciens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var technicien = await _context.Techniciens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (technicien == null)
                {
                    return NotFound();
                }

                // Supprimer l'utilisateur associé s'il existe
                if (!string.IsNullOrEmpty(technicien.UserId))
                {
                    var user = await _userManager.FindByIdAsync(technicien.UserId);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                }

                // Supprimer le technicien
                _context.Techniciens.Remove(technicien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Technicien supprimé avec succès !";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors de la suppression : " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var technicien = await _context.Techniciens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (technicien == null)
            {
                TempData["Error"] = "Accès non autorisé. Vous devez être connecté en tant que technicien.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Récupération des statistiques des échantillons par statut
                var echantillonsStockes = await _context.Echantillons
                    .CountAsync(e => e.Statut == StatutEchantillon.Stocke);
                var echantillonsRecus = await _context.Echantillons
                    .CountAsync(e => e.Statut == StatutEchantillon.Recu);
                var echantillonsVerifies = await _context.Echantillons
                    .CountAsync(e => e.Statut == StatutEchantillon.Verifie);

                // Statistiques supplémentaires pour le technicien
                var totalEchantillons = await _context.Echantillons.CountAsync();
                var echantillonsUrgents = await _context.Echantillons
                    .CountAsync(e => e.Priorite == PrioriteEchantillon.Urgent &&
                                   (e.Statut == StatutEchantillon.Recu || e.Statut == StatutEchantillon.Verifie));

                // Échantillons reçus aujourd'hui
                var aujourdhui = DateTime.Today;
                var echantillonsAujourdhui = await _context.Echantillons
                    .CountAsync(e => e.DateReception.Date == aujourdhui);

                // Récupération des échantillons récents (derniers 10)
                var echantillonsRecents = await _context.Echantillons
                    .Include(e => e.Affaire)
                    .Where(e => e.Statut == StatutEchantillon.Stocke ||
                               e.Statut == StatutEchantillon.Recu ||
                               e.Statut == StatutEchantillon.Verifie)
                    .OrderByDescending(e => e.DateReception)
                    .Take(10)
                    .Select(e => new
                    {
                        e.Id,
                        e.NumeroEchantillon,
                        e.NumeroAffaire,
                        e.Type,
                        e.Statut,
                        e.Priorite,
                        e.DateReception,
                        e.Description,
                        e.LieuCollecte,
                        e.ResponsableCollecte
                    })
                    .ToListAsync();

                // Échantillons nécessitant une attention urgente
                var echantillonsUrgentsDetails = await _context.Echantillons
                    .Where(e => e.Priorite == PrioriteEchantillon.Urgent &&
                               e.Statut == StatutEchantillon.Recu)
                    .OrderBy(e => e.DateReception)
                    .Take(5)
                    .Select(e => new
                    {
                        e.NumeroEchantillon,
                        e.NumeroAffaire,
                        e.Type,
                        e.DateReception,
                        e.DateLimite
                    })
                    .ToListAsync();

                // Statistiques par type d'échantillon
                var statsParType = await _context.Echantillons
                    .Where(e => e.Statut == StatutEchantillon.Stocke ||
                               e.Statut == StatutEchantillon.Recu ||
                               e.Statut == StatutEchantillon.Verifie)
                    .GroupBy(e => e.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                // Passage des données via ViewBag
                ViewBag.EchantillonsStockes = echantillonsStockes;
                ViewBag.EchantillonsRecus = echantillonsRecus;
                ViewBag.EchantillonsVerifies = echantillonsVerifies;
                ViewBag.TotalEchantillons = totalEchantillons;
                ViewBag.EchantillonsUrgents = echantillonsUrgents;
                ViewBag.EchantillonsAujourdhui = echantillonsAujourdhui;
                ViewBag.EchantillonsRecents = echantillonsRecents;
                ViewBag.EchantillonsUrgentsDetails = echantillonsUrgentsDetails;
                ViewBag.TechnicienNom = technicien.Nom;
                ViewBag.TechnicienPrenom = technicien.Prenom;
                ViewBag.TechnicienEmail = technicien.Email;
                ViewBag.StatsParType = statsParType;

                return View(technicien);
            }
            catch (Exception ex)
            {
                // Log de l'erreur
                Console.WriteLine($"Erreur dans Dashboard: {ex.Message}");

                TempData["Error"] = "Une erreur s'est produite lors du chargement du dashboard.";

                // Retourner des valeurs par défaut en cas d'erreur
                ViewBag.EchantillonsStockes = 0;
                ViewBag.EchantillonsRecus = 0;
                ViewBag.EchantillonsVerifies = 0;
                ViewBag.TotalEchantillons = 0;
                ViewBag.EchantillonsUrgents = 0;
                ViewBag.EchantillonsAujourdhui = 0;
                ViewBag.EchantillonsRecents = new List<object>();
                ViewBag.EchantillonsUrgentsDetails = new List<object>();
                ViewBag.TechnicienNom = technicien?.Nom ?? "";
                ViewBag.TechnicienPrenom = technicien?.Prenom ?? "";
                ViewBag.TechnicienEmail = technicien?.Email ?? "";
                ViewBag.StatsParType = new List<object>();

                return View(technicien);
            }
        }

        private bool TechnicienExists(int id)
        {
            return _context.Techniciens.Any(e => e.Id == id);
        }
    }
}