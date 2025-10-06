using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using static WebApplication1.ViewModels.ArchiverAffaireViewModel;
using CloturerAffaireViewModel = WebApplication1.ViewModels.CloturerAffaireViewModel;

namespace WebApplication1.Controllers
{
    public class AffairesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AffairesController> _logger;

        public AffairesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
            _context = db;
        }

        // GET: Affaires
        public async Task<IActionResult> Index()
        {
            var affaires = await _context.Affaires.ToListAsync();
            return View(affaires);
        }

        // GET: Affaires/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var affaire = await _context.Affaires.FindAsync(id);
            if (affaire == null)
            {
                return NotFound();
            }

            return View(affaire);
        }

        // GET: Affaires/Create
        public IActionResult Create()
        {
            // Pré-remplir certaines valeurs par défaut
            var affaire = new Affaire
            {
                DateOuverture = DateTime.Now,
                Statut = StatutAffaire.Ouverte,
                Priorite = Priorite.Moyenne
            };

            return View(affaire);
        }

        // POST: Affaires/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titre,Description,DateOuverture,DateFermeture,Statut,Priorite,Lieu")] Affaire affaire)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Récupérer l'ID de l'utilisateur connecté
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        ModelState.AddModelError("", "Utilisateur non authentifié");
                        return View(affaire);
                    }

                    // Chercher l'enquêteur correspondant à l'utilisateur connecté
                    var enqueteur = await _context.Enqueteurs
                        .FirstOrDefaultAsync(e => e.UserId == currentUserId);

                    if (enqueteur == null)
                    {
                        ModelState.AddModelError("", "Aucun enquêteur trouvé pour cet utilisateur. Veuillez contacter l'administrateur.");
                        return View(affaire);
                    }

                    // Configuration de l'affaire
                    affaire.IdEnqueteurResponsable = enqueteur.idEnqueteur;
                    affaire.NomEnqueteur = enqueteur.NomComplet;
                    affaire.UserId = currentUserId;
                    affaire.NumeroAffaire = Affaire.GenererNumeroAffaire();
                    affaire.DateCreation = DateTime.Now;
                    affaire.DateModification = DateTime.Now;
                    affaire.CreeParUserId = currentUserId;

                    // Ajouter l'affaire au contexte
                    _context.Affaires.Add(affaire);

                    // Sauvegarder
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"L'affaire '{affaire.Titre}' a été créée avec succès. Numéro: {affaire.NumeroAffaire}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création de l'affaire");
                    ModelState.AddModelError("", $"Erreur lors de la création : {ex.Message}");
                }
            }

            return View(affaire);
        }
        // Méthode helper pour générer un numéro d'affaire unique
        private async Task<string> GenererNumeroAffaireUnique()
        {
            var annee = DateTime.Now.Year;
            var count = await _context.Affaires.CountAsync(a => a.DateCreation.Year == annee);
            return $"AFF-{annee}-{(count + 1):D4}"; // Ex: AFF-2025-0001
        }

        // GET: Affaires/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var affaire = await _context.Affaires.FindAsync(id);
            if (affaire == null)
            {
                return NotFound();
            }

            return View(affaire);
        }

        // POST: Affaires/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Description,DateOuverture,DateFermeture,Statut,IdEnqueteurResponsable,Priorite,Lieu")] Affaire affaire)
        {
            if (id != affaire.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(affaire);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AffaireExists(affaire.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(affaire);
        }

        // GET: Affaires/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var affaire = await _context.Affaires.FindAsync(id);
            if (affaire == null)
            {
                return NotFound();
            }

            return View(affaire);
        }

        // POST: Affaires/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var affaire = await _context.Affaires.FindAsync(id);
            if (affaire != null)
            {
                _context.Affaires.Remove(affaire);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Action AJAX pour récupérer les détails d'une affaire
        [HttpGet]
        public async Task<IActionResult> GetAffaireDetails(int id)
        {
            var affaire = await GetAffaireByIdAsync(id);
            if (affaire == null)
            {
                return Json(new { success = false, message = "Affaire non trouvée" });
            }

            var enqueteur = await GetEnqueteurByIdAsync(affaire.IdEnqueteurResponsable);

            return Json(new
            {
                success = true,
                affaire = new
                {
                    titre = affaire.Titre,
                    description = affaire.Description,
                    statut = affaire.Statut.ToString(),
                    priorite = affaire.Priorite.ToString(),
                    dateOuverture = affaire.DateOuverture.ToShortDateString(),
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Archive()
        {
            var viewModel = new ArchiverAffaireViewModel();

            // Récupérer les affaires clôturées éligibles à l'archivage
            var affairesArchivables = await GetAffairesArchivablesAsync();

            viewModel.AffairesArchivables = affairesArchivables.Select(a => new AffaireArchivableViewModel
            {
                Id = a.Id,
                Titre = a.Titre,
                DateFermeture = a.DateFermeture!.Value,
                Statut = a.Statut.ToString(),
                Priorite = a.Priorite.ToString(),
                EstSelectionne = false
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(ArchiverAffaireViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var affairesAArchiver = model.AffairesArchivables
                        .Where(a => a.EstSelectionne)
                        .Select(a => a.Id)
                        .ToList();

                    if (!affairesAArchiver.Any())
                    {
                        TempData["ErrorMessage"] = "Aucune affaire sélectionnée pour l'archivage.";
                        return RedirectToAction(nameof(Archive));
                    }

                    int nombreArchivees = 0;

                    foreach (var affaireId in affairesAArchiver)
                    {
                        var affaire = await GetAffaireByIdAsync(affaireId);
                        if (affaire != null && PeutEtreArchivee(affaire))
                        {
                            // Créer une entrée d'archive
                            var archive = new AffaireArchive
                            {
                                AffaireId = affaire.Id,
                                DateArchivage = DateTime.Now,
                                MotifArchivage = model.MotifArchivage,
                                UtilisateurArchivage = GetCurrentUserId(),
                                EmplacementStockage = GenererEmplacementStockage(affaire),
                                DonneesSerialisees = SerialiserAffaire(affaire)
                            };

                            // Sauvegarder l'archive
                          //  await SauvegarderArchiveAsync(archive);

                            // Marquer l'affaire comme archivée (ou la supprimer selon la logique)
                            await MarquerCommeArchiveeAsync(affaire);

                            nombreArchivees++;
                        }
                    }

                    TempData["SuccessMessage"] = $"{nombreArchivees} affaire(s) archivée(s) avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'archivage des affaires");
                    TempData["ErrorMessage"] = $"Erreur lors de l'archivage : {ex.Message}";
                }
            }

            // Si erreur, recharger la liste
            var affairesArchivables = await GetAffairesArchivablesAsync();
            model.AffairesArchivables = affairesArchivables.Select(a => new AffaireArchivableViewModel
            {
                Id = a.Id,
                Titre = a.Titre,
                DateFermeture = a.DateFermeture!.Value,
                Statut = a.Statut.ToString(),
                Priorite = a.Priorite.ToString(),
                EstSelectionne = false
            }).ToList();

            return View(model);
        }

        // Action AJAX pour filtrer les affaires archivables
        [HttpPost]
        public async Task<IActionResult> FiltrerAffairesArchivables(DateTime? dateDebut, DateTime? dateFin, string? priorite, string? statut)
        {
            var affaires = await GetAffairesArchivablesAsync();

            // Appliquer les filtres
            if (dateDebut.HasValue)
                affaires = affaires.Where(a => a.DateFermeture >= dateDebut.Value).ToList();

            if (dateFin.HasValue)
                affaires = affaires.Where(a => a.DateFermeture <= dateFin.Value).ToList();

            if (!string.IsNullOrEmpty(priorite))
                affaires = affaires.Where(a => a.Priorite.ToString() == priorite).ToList();

            if (!string.IsNullOrEmpty(statut))
                affaires = affaires.Where(a => a.Statut.ToString() == statut).ToList();

            var result = affaires.Select(a => new
            {
                id = a.Id,
                titre = a.Titre,
                dateFermeture = a.DateFermeture!.Value.ToShortDateString(),
                statut = a.Statut.ToString(),
                priorite = a.Priorite.ToString()
            });

            return Json(new { success = true, affaires = result });
        }

        // GET: Affaires/MesAffaires
        public async Task<IActionResult> MesAffaires()
        {
            // Récupérer l'ID de l'enquêteur connecté
            var idEnqueteur = GetCurrentUserId();

            // Récupérer les affaires dont il est responsable
            var mesAffaires = await _context.Affaires
                               .Where(a => a.IdEnqueteurResponsable == idEnqueteur)
                               .OrderByDescending(a => a.DateOuverture)
                               .ToListAsync();

            return View(mesAffaires);
        }

        #region Méthodes privées

        private async Task<List<Affaire>> GetAffairesOuvertesAsync()
        {
            return await _context.Affaires
                .Where(a => a.Statut == StatutAffaire.Ouverte || a.Statut == StatutAffaire.EnqueteActive)
                .ToListAsync();
        }

        private async Task<List<Affaire>> GetAffairesArchivablesAsync()
        {
            // Retourne les affaires clôturées depuis plus de X jours (selon votre politique)
            var dateLimit = DateTime.Now.AddDays(-30); // Exemple: 30 jours
            return await _context.Affaires
                .Where(a => a.DateFermeture.HasValue &&
                           a.DateFermeture <= dateLimit &&
                           (a.Statut == StatutAffaire.Resolue || a.Statut == StatutAffaire.NonResolue))
                .ToListAsync();
        }

        private async Task<Affaire?> GetAffaireByIdAsync(int id)
        {
            return await _context.Affaires.FindAsync(id);
        }

        private bool PeutEtreArchivee(Affaire affaire)
        {
            // Vérifier si l'affaire peut être archivée (clôturée, délai respecté, etc.)
            return affaire.DateFermeture.HasValue &&
                   (affaire.Statut == StatutAffaire.Resolue || affaire.Statut == StatutAffaire.NonResolue);
        }

        private async Task SauvegarderAffaireAsync(Affaire affaire)
        {
            _context.Update(affaire);
            await _context.SaveChangesAsync();
        }



      /*  private async Task SauvegarderArchiveAsync(AffaireArchive archive)
        {
            _context.AffairesArchives.Add(archive);
            await _context.SaveChangesAsync();
        }*/

        private async Task EnvoyerNotificationClotureAsync(Affaire affaire, RapportCloture rapport)
        {
            // Implémentation pour envoyer des notifications (email, etc.)
            // Utiliser IEmailSender ou un service de notification
            await Task.CompletedTask; // Placeholder
        }

        private string GenererEmplacementStockage(Affaire affaire)
        {
            // Génère un chemin de stockage pour l'archive
            return $"Archives/{DateTime.Now.Year}/{affaire.Id}";
        }

        private string SerialiserAffaire(Affaire affaire)
        {
            // Sérialise l'affaire en JSON pour stockage
            return System.Text.Json.JsonSerializer.Serialize(affaire);
        }

        private async Task MarquerCommeArchiveeAsync(Affaire affaire)
        {
            // Marque l'affaire comme archivée ou la supprime de la base active
            affaire.Statut = StatutAffaire.Archivee; // Assumant qu'il existe ce statut
            _context.Update(affaire);
            await _context.SaveChangesAsync();
        }

        private int GetCurrentUserId()
        {
            // Retourne l'ID de l'utilisateur connecté
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0; // Ou gérer l'erreur appropriée
        }

        private async Task<object?> GetEnqueteurByIdAsync(int id)
        {
            // Retourne les informations de l'enquêteur
            // Remplacer par votre logique d'accès aux enquêteurs
            await Task.CompletedTask;
            return new { Nom = "Enquêteur" }; // Placeholder
        }

        private bool AffaireExists(int id)
        {
            return _context.Affaires.Any(e => e.Id == id);
        }

        #endregion
    }
}