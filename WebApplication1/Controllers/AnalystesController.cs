using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class AnalystesController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalystesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
            _db = db;
            _userManager = userManager;
        }



        // GET: Analystes
        public async Task<IActionResult> Index(string specialiteFiltre)
        {
            var liste = _db.Analystes.Include(a => a.User).AsQueryable();

            var specialites = await _db.Analystes
                .Select(a => a.specialite)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.Specialites = specialites;
            ViewBag.SelectedSpecialite = specialiteFiltre;

            if (!string.IsNullOrEmpty(specialiteFiltre))
            {
                liste = liste.Where(a => a.specialite == specialiteFiltre);
            }

            return View(await liste.ToListAsync());
        }

        // GET: Analystes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyste = await _db.Analystes
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analyste == null)
            {
                return NotFound();
            }

            return View(analyste);
        }

        private List<SelectListItem> GetSpecialiteList()
        {
            try
            {
                return new List<SelectListItem>
        {
            new SelectListItem { Value = "ADN", Text = "ADN" },
            new SelectListItem { Value = "Empreintes", Text = "Empreintes" },
            new SelectListItem { Value = "Balistique", Text = "Balistique" },
            new SelectListItem { Value = "Chimie", Text = "Chimie" },
            new SelectListItem { Value = "Toxicologie", Text = "Toxicologie" },
            new SelectListItem { Value = "Numérique", Text = "Numérique" },
            new SelectListItem { Value = "Incendie", Text = "Incendie" },
            new SelectListItem { Value = "Traceologie", Text = "Traceologie" },
            new SelectListItem { Value = "Documents", Text = "Documents" }
        };
            }
            catch (Exception ex)
            {
                // Log l'erreur et retourne une liste vide
                Console.WriteLine($"Erreur lors du chargement des spécialités: {ex.Message}");
                return new List<SelectListItem>();
            }
        }

        // GET: Analystes/Create
        public IActionResult Create()
            {
                ViewBag.SpecialiteList = GetSpecialiteList();
                return View();
            }

        // POST: Analystes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("idAnalyste,nom,prenom,specialite,statut")] Analyste analyste, string email, string password)
        {
            // ⚡ INITIALISATION GARANTIE de la liste
            ViewBag.SpecialiteList = GetSpecialiteList();

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "L'email et le mot de passe sont requis.");
                    return View(analyste);
                }

                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email
                    };

                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Analyste");
                        analyste.UserId = user.Id;
                        analyste.statut = analyste.statut ?? "actif";

                        _db.Analystes.Add(analyste);
                        await _db.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Analyste créé avec succès !";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erreur : " + ex.Message);
                }
            }

            return View(analyste);
        }

            // GET: Analystes/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var analyste = await _db.Analystes
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (analyste == null)
                {
                    return NotFound();
                }

                ViewBag.CurrentEmail = analyste.User?.Email;

                return View(analyste);
            }

            // POST: Analystes/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("idAnalyste,nom,prenom,specialite,statut,UserId")] Analyste analyste, string email, string password)
            {
                if (id != analyste.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _db.Update(analyste);

                        if (!string.IsNullOrEmpty(analyste.UserId))
                        {
                            var user = await _userManager.FindByIdAsync(analyste.UserId);
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

                        await _db.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Analyste modifié avec succès !";
                        return RedirectToAction(nameof(Index));
                    }

                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Erreur : " + ex.Message);
                    }
                }

                var currentAnalyste = await _db.Analystes
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == analyste.Id);

                ViewBag.CurrentEmail = currentAnalyste?.User?.Email;

                return View(analyste);
            }

            // GET: Analystes/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var analyste = await _db.Analystes.FindAsync(id);
                if (analyste == null)
                {
                    return NotFound();
                }

                ViewBag.NbPreuves = await _db.Echantillons.CountAsync(p => p.AnalysteId == id);

                return View(analyste);
            }

            // POST: Analystes/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    var analyste = await _db.Analystes
                        .Include(a => a.EchantillonsAssignees)
                        .Include(a => a.rapports)
                        .Include(a => a.User)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (analyste == null)
                    {
                        return NotFound();
                    }

                    string userId = analyste.UserId;

                    // 1. Supprimer les preuves assignées
                    if (analyste.EchantillonsAssignees?.Any() == true)
                    {
                        _db.Echantillons.RemoveRange(analyste.EchantillonsAssignees);
                    }


                    // 3. Supprimer l'analyste
                    _db.Analystes.Remove(analyste);

                    // 4. Supprimer manuellement toutes les données Identity si l'utilisateur existe
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await DeleteUserFromIdentityAsync(userId);
                    }

                    // Sauvegarder tous les changements
                    await _db.SaveChangesAsync();

                    // Confirmer la transaction
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Analyste, compte utilisateur et toutes les données associées supprimés avec succès";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Méthode helper pour supprimer complètement un utilisateur d'Identity
            private async Task DeleteUserFromIdentityAsync(string userId)
            {
                try
                {
                    // Supprimer de AspNetUserRoles
                    var userRoles = _db.UserRoles.Where(ur => ur.UserId == userId);
                    if (userRoles.Any())
                    {
                        _db.UserRoles.RemoveRange(userRoles);
                    }

                    // Supprimer de AspNetUserClaims
                    var userClaims = _db.UserClaims.Where(uc => uc.UserId == userId);
                    if (userClaims.Any())
                    {
                        _db.UserClaims.RemoveRange(userClaims);
                    }

                    // Supprimer de AspNetUserLogins
                    var userLogins = _db.UserLogins.Where(ul => ul.UserId == userId);
                    if (userLogins.Any())
                    {
                        _db.UserLogins.RemoveRange(userLogins);
                    }

                    // Supprimer de AspNetUserTokens
                    var userTokens = _db.UserTokens.Where(ut => ut.UserId == userId);
                    if (userTokens.Any())
                    {
                        _db.UserTokens.RemoveRange(userTokens);
                    }

                    // Supprimer de AspNetUsers
                    var user = await _db.Users.FindAsync(userId);
                    if (user != null)
                    {
                        _db.Users.Remove(user);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur lors de la suppression des données Identity : {ex.Message}");
                }
            }
        // GET: Analystes/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Récupérer l'ID de l'analyste connecté
                var currentUserId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Trouver l'analyste correspondant à l'utilisateur connecté
                var analyste = await _db.Analystes
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.UserId == currentUserId);

                if (analyste == null)
                {
                    TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                    return RedirectToAction("Index", "Home");
                }

                // Calculer les statistiques
                var echantillonsAssignes = await _db.Echantillons
                    .CountAsync(e => e.AnalysteId == analyste.Id &&
                                   e.Statut == StatutEchantillon.EnAnalyse);

                var analysesTerminees = await _db.Echantillons
                    .CountAsync(e => e.AnalysteId == analyste.Id &&
                                   e.Statut == StatutEchantillon.AnalyseTerminee);

                var analysesEnCours = await _db.Echantillons
                    .CountAsync(e => e.AnalysteId == analyste.Id &&
                                   (e.Statut == StatutEchantillon.EnAnalyse ||
                                    e.Statut == StatutEchantillon.EnAnalyse));

                var analysesEnAttenteValidation = await _db.Echantillons
                    .CountAsync(e => e.AnalysteId == analyste.Id &&
                                   e.Statut == StatutEchantillon.EnAttente);

                // Préparer les données pour la vue
                ViewBag.EchantillonsAssignes = echantillonsAssignes;
                ViewBag.AnalysesEnCours = analysesEnCours;
                ViewBag.AnalysesTerminees = analysesTerminees;
                ViewBag.AnalysesEnAttenteValidation = analysesEnAttenteValidation;

                // Passer l'analyste à la vue
                var analystesList = new List<Analyste> { analyste };
                return View(analystesList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors du chargement du tableau de bord : " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
    }
