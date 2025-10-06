// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Analyse;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class EnqueteursController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EnqueteursController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var dashboard = new DashboardEnqueteur();

            try
            {
                var enqueteurConnecte = await _db.Enqueteurs
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (enqueteurConnecte == null)
                {
                    ViewBag.ErrorMessage = "Aucun profil enquêteur trouvé pour cet utilisateur.";
                    return View(GetEmptyDashboard());
                }

                int idEnqueteur = enqueteurConnecte.idEnqueteur;

                var affaires = await _db.Affaires
                    .Where(a => a.IdEnqueteurResponsable == idEnqueteur)
                    .ToListAsync();

                var echantillons = await _db.Echantillons
                    .Include(e => e.Affaire)
                    .Where(e => e.Affaire.IdEnqueteurResponsable == idEnqueteur)
                    .ToListAsync();

                var analyses = await _db.Analyses
                    .Include(a => a.Echantillon)
                    .Include(a => a.Echantillon.Affaire)
                    .Where(a => a.Echantillon.Affaire.IdEnqueteurResponsable == idEnqueteur)
                    .ToListAsync();

                var envois = await _db.EnvoiComplets
                    .Where(e => e.Echantillons.Any(ech => ech.Affaire.IdEnqueteurResponsable == idEnqueteur))
                    .ToListAsync();

                dashboard.TotalAffaires = affaires.Count;
                dashboard.TotalEchantillons = echantillons.Count;
                dashboard.TotalEnvoisLabo = envois.Count;

                // Initialiser les dictionnaires
                dashboard.AffairesParStatut = new Dictionary<StatutAffaire, int>();
                dashboard.AffairesParPriorite = new Dictionary<Priorite, int>();
                dashboard.EchantillonsParStatut = new Dictionary<StatutEchantillon, int>();
                dashboard.EchantillonsParType = new Dictionary<TypeEchantillon, int>();
                dashboard.EchantillonsParPriorite = new Dictionary<PrioriteEchantillon, int>();

                // Remplir les statistiques
                foreach (StatutAffaire statut in Enum.GetValues(typeof(StatutAffaire)))
                    dashboard.AffairesParStatut[statut] = affaires.Count(a => a.Statut == statut);

                foreach (Priorite priorite in Enum.GetValues(typeof(Priorite)))
                    dashboard.AffairesParPriorite[priorite] = affaires.Count(a => a.Priorite == priorite);

                foreach (StatutEchantillon statut in Enum.GetValues(typeof(StatutEchantillon)))
                    dashboard.EchantillonsParStatut[statut] = echantillons.Count(e => e.Statut == statut);

                foreach (TypeEchantillon type in Enum.GetValues(typeof(TypeEchantillon)))
                    dashboard.EchantillonsParType[type] = echantillons.Count(e => e.Type == type);

                foreach (PrioriteEchantillon priorite in Enum.GetValues(typeof(PrioriteEchantillon)))
                    dashboard.EchantillonsParPriorite[priorite] = echantillons.Count(e => e.Priorite == priorite);

                dashboard.AnalysesTerminees = analyses.Count(a =>
                    a.Statut == StatutAnalyse.Terminee || a.Statut == StatutAnalyse.Validee);
                dashboard.AnalysesEnCours = analyses.Count(a =>
                    a.Statut == StatutAnalyse.EnCours || a.Statut == StatutAnalyse.Reportee);

                dashboard.AffairesRecentes = affaires
                    .OrderByDescending(a => a.DateOuverture)
                    .Take(5)
                    .ToList();

                dashboard.EchantillonsRecents = echantillons
                    .OrderByDescending(e => e.DateReception)
                    .Take(5)
                    .ToList();

                return View(dashboard);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement du dashboard : " + ex.Message;
                return View(GetEmptyDashboard());
            }
        }

        private DashboardEnqueteur GetEmptyDashboard()
        {
            var dashboard = new DashboardEnqueteur
            {
                TotalAffaires = 0,
                TotalEchantillons = 0,
                TotalEnvoisLabo = 0,
                AnalysesTerminees = 0,
                AnalysesEnCours = 0,
                AffairesParStatut = new Dictionary<StatutAffaire, int>(),
                AffairesParPriorite = new Dictionary<Priorite, int>(),
                EchantillonsParStatut = new Dictionary<StatutEchantillon, int>(),
                EchantillonsParType = new Dictionary<TypeEchantillon, int>(),
                EchantillonsParPriorite = new Dictionary<PrioriteEchantillon, int>(),
                AffairesRecentes = new List<Affaire>(),
                EchantillonsRecents = new List<Echantillon>()
            };

            foreach (StatutAffaire statut in Enum.GetValues(typeof(StatutAffaire)))
                dashboard.AffairesParStatut[statut] = 0;

            foreach (Priorite priorite in Enum.GetValues(typeof(Priorite)))
                dashboard.AffairesParPriorite[priorite] = 0;

            foreach (StatutEchantillon statut in Enum.GetValues(typeof(StatutEchantillon)))
                dashboard.EchantillonsParStatut[statut] = 0;

            foreach (TypeEchantillon type in Enum.GetValues(typeof(TypeEchantillon)))
                dashboard.EchantillonsParType[type] = 0;

            foreach (PrioriteEchantillon priorite in Enum.GetValues(typeof(PrioriteEchantillon)))
                dashboard.EchantillonsParPriorite[priorite] = 0;

            return dashboard;
        }

        // GET: Enqueteurs
        public async Task<IActionResult> Index(string gradeFiltre)
        {
            var liste = _db.Enqueteurs.Include(e => e.User).AsQueryable();

            var grades = await _db.Enqueteurs
                .Select(e => e.grade)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.Grades = grades;
            ViewBag.SelectedGrade = gradeFiltre;

            if (!string.IsNullOrEmpty(gradeFiltre))
            {
                liste = liste.Where(e => e.grade == gradeFiltre);
            }

            return View(await liste.ToListAsync());
        }

        // GET: Enqueteurs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enqueteur = await _db.Enqueteurs
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.idEnqueteur == id);

            if (enqueteur == null)
            {
                return NotFound();
            }

            return View(enqueteur);
        }

        // GET: Enqueteurs/Create
        public async Task<IActionResult> Create()
        {
            await EnsureRoleExists();
            await PopulateDropDownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enqueteur enqueteur, string email, string password)
        {
            // Supprimer la validation pour UserId qui sera assigné plus tard
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("preuves");

            // Validation manuelle des champs email et password
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("email", "L'email est requis.");
            }
            else if (!IsValidEmail(email))
            {
                ModelState.AddModelError("email", "L'email n'est pas valide.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("password", "Le mot de passe est requis.");
            }
            else if (password.Length < 6)
            {
                ModelState.AddModelError("password", "Le mot de passe doit contenir au moins 6 caractères.");
            }

            // Vérifier si l'email existe déjà
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("email", "Cet email est déjà utilisé.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // 1. Créer l'utilisateur Identity
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var userResult = await _userManager.CreateAsync(user, password);

                    if (!userResult.Succeeded)
                    {
                        foreach (var error in userResult.Errors)
                        {
                            ModelState.AddModelError("", $"Erreur utilisateur : {error.Description}");
                            System.Diagnostics.Debug.WriteLine($"Erreur création utilisateur: {error.Code} - {error.Description}");
                        }
                        await transaction.RollbackAsync();
                        await PopulateDropDownsAsync();
                        return View(enqueteur);
                    }

                    // 2. Assigner le rôle Enqueteur
                    await EnsureRoleExists();
                    var roleResult = await _userManager.AddToRoleAsync(user, "Enqueteur");

                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", $"Erreur rôle : {error.Description}");
                            System.Diagnostics.Debug.WriteLine($"Erreur assignation rôle: {error.Code} - {error.Description}");
                        }
                        await transaction.RollbackAsync();
                        await PopulateDropDownsAsync();
                        return View(enqueteur);
                    }

                    // 3. Créer l'enquêteur et lier avec l'utilisateur
                    enqueteur.UserId = user.Id;
                    enqueteur.preuves = new List<Echantillon>();

                    _db.Enqueteurs.Add(enqueteur);
                    await _db.SaveChangesAsync();

                    // 4. Confirmer la transaction
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "L'enquêteur a été créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Rollback en cas d'erreur
                    await transaction.RollbackAsync();

                    // Log détaillé de l'erreur
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    System.Diagnostics.Debug.WriteLine($"Erreur complète création enquêteur: {ex}");

                    ModelState.AddModelError("", $"Erreur lors de l'enregistrement : {errorMessage}");
                    TempData["ErrorMessage"] = $"Erreur lors de l'enregistrement : {errorMessage}";
                }
            }

            // En cas d'erreur, recharger les données pour la vue
            await PopulateDropDownsAsync();
            return View(enqueteur);
        }

        // GET: Enqueteurs/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enqueteur = await _db.Enqueteurs
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.idEnqueteur == id);

            if (enqueteur == null)
            {
                return NotFound();
            }

            ViewBag.CurrentEmail = enqueteur.User?.Email;
            await PopulateDropDownsAsync(enqueteur.grade, enqueteur.service);

            return View(enqueteur);
        }

        // POST: Enqueteurs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Enqueteur enqueteur, string email, string password)
        {
            if (id != enqueteur.idEnqueteur)
            {
                return NotFound();
            }

            // Désactiver la validation pour les propriétés non modifiées par le formulaire
            ModelState.Remove("User");
            ModelState.Remove("preuves");

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(enqueteur);

                    if (!string.IsNullOrEmpty(enqueteur.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(enqueteur.UserId);
                        if (user != null)
                        {
                            bool userUpdated = false;

                            if (!string.IsNullOrEmpty(email) && user.Email != email)
                            {
                                // Vérifier si le nouvel email n'est pas déjà utilisé
                                var existingUser = await _userManager.FindByEmailAsync(email);
                                if (existingUser != null && existingUser.Id != user.Id)
                                {
                                    TempData["ErrorMessage"] = "Cet email est déjà utilisé par un autre utilisateur.";
                                    var currentEnqueteur = await _db.Enqueteurs
                                        .Include(e => e.User)
                                        .FirstOrDefaultAsync(e => e.idEnqueteur == enqueteur.idEnqueteur);

                                    ViewBag.CurrentEmail = currentEnqueteur?.User?.Email;
                                    await PopulateDropDownsAsync(enqueteur.grade, enqueteur.service);
                                    return View(enqueteur);
                                }

                                user.Email = email;
                                user.UserName = email;
                                userUpdated = true;
                            }

                            if (!string.IsNullOrEmpty(password))
                            {
                                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                                var resetResult = await _userManager.ResetPasswordAsync(user, token, password);

                                if (!resetResult.Succeeded)
                                {
                                    foreach (var error in resetResult.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }

                                    var currentEnqueteur = await _db.Enqueteurs
                                        .Include(e => e.User)
                                        .FirstOrDefaultAsync(e => e.idEnqueteur == enqueteur.idEnqueteur);

                                    ViewBag.CurrentEmail = currentEnqueteur?.User?.Email;
                                    await PopulateDropDownsAsync(enqueteur.grade, enqueteur.service);
                                    return View(enqueteur);
                                }
                                userUpdated = true;
                            }

                            if (userUpdated)
                            {
                                var updateResult = await _userManager.UpdateAsync(user);
                                if (!updateResult.Succeeded)
                                {
                                    foreach (var error in updateResult.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }

                                    var currentEnqueteur = await _db.Enqueteurs
                                        .Include(e => e.User)
                                        .FirstOrDefaultAsync(e => e.idEnqueteur == enqueteur.idEnqueteur);

                                    ViewBag.CurrentEmail = currentEnqueteur?.User?.Email;
                                    await PopulateDropDownsAsync(enqueteur.grade, enqueteur.service);
                                    return View(enqueteur);
                                }
                            }
                        }
                    }

                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Enquêteur modifié avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnqueteurExists(enqueteur.idEnqueteur))
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

            var currentEnq = await _db.Enqueteurs
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.idEnqueteur == enqueteur.idEnqueteur);

            ViewBag.CurrentEmail = currentEnq?.User?.Email;
            await PopulateDropDownsAsync(enqueteur.grade, enqueteur.service);

            return View(enqueteur);
        }

        // GET: Enqueteurs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enqueteur = await _db.Enqueteurs
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.idEnqueteur == id);

            if (enqueteur == null)
            {
                return NotFound();
            }

            ViewBag.NbPreuves = await _db.Echantillons.CountAsync(p => p.CreateurId == id.ToString());
            ViewBag.NbAffaire = await _db.Affaires.CountAsync(e => e.IdEnqueteurResponsable == id);

            return View(enqueteur);
        }

        // POST: Enqueteurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var enqueteur = await _db.Enqueteurs
                    .Include(e => e.User)
                    .Include(e => e.preuves)
                    .FirstOrDefaultAsync(e => e.idEnqueteur == id);

                if (enqueteur == null)
                {
                    return NotFound();
                }

                // Supprimer les preuves associées
                if (enqueteur.preuves != null && enqueteur.preuves.Any())
                {
                    var preuves = enqueteur.preuves.ToList();
                    foreach (var preuve in preuves)
                    {
                        _db.Echantillons.Remove(preuve);
                    }
                }

                // Supprimer l'enquêteur
                _db.Enqueteurs.Remove(enqueteur);

                // Supprimer l'utilisateur Identity associé si il existe
                if (!string.IsNullOrEmpty(enqueteur.UserId))
                {
                    var user = await _userManager.FindByIdAsync(enqueteur.UserId);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Enquêteur et toutes ses données associées supprimés avec succès";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Erreur lors de la suppression : " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #region Méthodes privées

        private bool EnqueteurExists(int id)
        {
            return _db.Enqueteurs.Any(e => e.idEnqueteur == id);
        }

        private void PopulateDropDowns(string selectedGrade = null, string selectedService = null)
        {
            var grades = new List<string>
            {
                "Inspecteur", "Inspecteur Principal", "Commissaire",
                "Commissaire Principal", "Contrôleur Général"
            };

            var services = new List<string>
            {
                "Police Judiciaire", "Sûreté Nationale", "Police Administrative",
                "Renseignements Généraux", "Police Scientifique",
                "Brigade Criminelle", "Cybercriminalité"
            };

            ViewBag.GradeList = new SelectList(grades, selectedGrade);
            ViewBag.ServiceList = new SelectList(services, selectedService);
        }

        private async Task PopulateDropDownsAsync(string selectedGrade = null, string selectedService = null)
        {
            PopulateDropDowns(selectedGrade, selectedService);
            await Task.CompletedTask;
        }

        private async Task EnsureRoleExists()
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync("Enqueteur"))
                {
                    System.Diagnostics.Debug.WriteLine("Création du rôle Enqueteur...");
                    var role = new IdentityRole("Enqueteur");
                    var result = await _roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        System.Diagnostics.Debug.WriteLine("Rôle Enqueteur créé avec succès");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Erreur lors de la création du rôle:");
                        foreach (var error in result.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {error.Description}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Le rôle Enqueteur existe déjà");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception lors de la vérification du rôle: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}