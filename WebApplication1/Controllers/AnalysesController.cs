using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Analyse;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Analyste")]
    public class AnalysesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalysesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager):base(context,userManager,roleManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========== TABLEAU DE BORD ANALYSTE ==========
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new AnalystesDashboardViewModel
            {
                // Échantillons assignés à cet analyste
                EchantillonsAssignes = await _context.Echantillons
                    .Where(e => e.AnalysteId == analyste.Id &&
                               e.Statut == StatutEchantillon.Stocke)
                    .Include(e => e.Analyste)
                    .OrderBy(e => e.Priorite)
                    .ThenBy(e => e.DateLimite)
                    .ToListAsync(),

                // Analyses en cours
                AnalysesEnCours = await _context.Echantillons
                    .Where(e => e.AnalysteId == analyste.Id &&
                               e.Statut == StatutEchantillon.EnAnalyse)
                    .Include(e => e.Analyste)
                    .ToListAsync(),

                // Analyses terminées
                AnalysesTerminees = await _context.Echantillons
                    .Where(e => e.AnalysteId == analyste.Id &&
                               e.Statut == StatutEchantillon.AnalyseTerminee)
                    .Include(e => e.Analyste)
                    .OrderByDescending(e => e.DateFinAnalyse)
                    .Take(10)
                    .ToListAsync(),

                // Statistiques
                NombreAssignes = await _context.Echantillons.CountAsync(e => e.AnalysteId == analyste.Id &&
                                                                          e.Statut == StatutEchantillon.Stocke),
                NombreEnCours = await _context.Echantillons.CountAsync(e => e.AnalysteId == analyste.Id &&
                                                                         e.Statut == StatutEchantillon.EnAnalyse),
                NombreTerminees = await _context.Echantillons.CountAsync(e => e.AnalysteId == analyste.Id &&
                                                                           e.Statut == StatutEchantillon.AnalyseTerminee),
                NombreAValider = await _context.Echantillons.CountAsync(e => e.AnalysteId == analyste.Id &&
                                                                          e.Statut == StatutEchantillon.EnValidation)
            };

            return View(viewModel);
        }

        // ========== MES ÉCHANTILLONS ASSIGNÉS ==========
        public async Task<IActionResult> MesEchantillons()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var echantillons = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id &&
                           e.Statut == StatutEchantillon.Stocke)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .OrderBy(e => e.Priorite)
                .ThenBy(e => e.DateLimite)
                .ToListAsync();

            return View(echantillons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemarrerAnalyse(int echantillonId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .FirstOrDefaultAsync(e => e.Id == echantillonId && e.AnalysteId == analyste.Id); // Utiliser Id

            if (echantillon == null)
            {
                TempData["ErrorMessage"] = "Échantillon non trouvé ou non assigné à vous.";
                return RedirectToAction("MesEchantillons");
            }

            if (echantillon.Statut != StatutEchantillon.Stocke)
            {
                TempData["ErrorMessage"] = "Cet échantillon ne peut pas être analysé dans son état actuel.";
                return RedirectToAction("MesEchantillons");
            }

            try
            {
                // S'assurer que l'échantillon est assigné à cet analyste
                if (echantillon.AnalysteId != analyste.Id)
                {
                    echantillon.AnalysteId = analyste.Id;
                }

                // Démarrer l'analyse
                echantillon.Statut = StatutEchantillon.EnAnalyse;
                echantillon.DateDebutAnalyse = DateTime.Now;

                // Créer les analyses par défaut si elles n'existent pas
                if (!echantillon.Analyses.Any())
                {
                    await CreerAnalysesParDefaut(echantillon, analyste.Id);
                }
                else
                {
                    // Mettre à jour le statut des analyses existantes
                    foreach (var analyse in echantillon.Analyses)
                    {
                        analyse.Statut = StatutAnalyse.EnCours;
                        analyse.DateAnalyse = DateTime.Now;
                        // S'assurer que l'AnalysteId est défini
                        if (analyse.AnalysteId == 0)
                        {
                            analyse.AnalysteId = analyste.Id;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Analyse démarrée avec succès pour l'échantillon {echantillon.NumeroEchantillon}";
                return RedirectToAction("EnCours");
            }
            catch (Exception ex)
            {
                // Afficher l'exception interne pour le débogage
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                TempData["ErrorMessage"] = $"Erreur lors du démarrage de l'analyse : {errorMessage}";
                return RedirectToAction("MesEchantillons");
            }
        }


        private async Task CreerAnalysesParDefaut(Echantillon echantillon, int analysteId)
        {
            var analysesTypes = new[]
            {
        new {
            Nom = "Analyse Biologique",
            Type = "Analyse Biologique",
            Methode = "Spectrométrie de masse",
            Observations = "Analyse biologique standard en cours"
        },
        new {
            Nom = "Analyse Physico-Chimique",
            Type = "Analyse Physico-Chimique",
            Methode = "Chromatographie liquide",
            Observations = "Analyse physico-chimique initiée"
        },
        new {
            Nom = "Analyse Numérique",
            Type = "Analyse Numérique",
            Methode = "Analyse statistique",
            Observations = "Traitement des données en cours"
        }
    };

            foreach (var type in analysesTypes)
            {
                var analyse = new Analyse
                {
                    Nom = type.Nom,
                    EchantillonId = echantillon.Id,
                    AnalysteId = analysteId,
                    TypeAnalyse = type.Type,
                    Methode = type.Methode,
                    DateAnalyse = DateTime.Now,
                    Statut = StatutAnalyse.EnCours,
                    Resultats = "En attente de résultats",
                    Conclusion = "",
                    Observations = type.Observations,
                    NomFichier = "",
                    FichierContentType = "",
                    FichierContenu = Array.Empty<byte>(), // Tableau de bytes vide
                    EstValide = false
                };

                _context.Analyses.Add(analyse);
            }

            await _context.SaveChangesAsync();
        }

        // ========== ANALYSES EN COURS ==========
        public async Task<IActionResult> EnCours()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var analysesEnCours = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id &&
                           e.Statut == StatutEchantillon.EnAnalyse)
                .Include(e => e.Analyste)
                .OrderBy(e => e.Priorite)
                .ThenBy(e => e.DateLimite)
                .ToListAsync();

            return View(analysesEnCours);
        }

        [HttpGet]
        public async Task<IActionResult> SaisirResultats(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var echantillon = await _context.Echantillons
                .AsNoTracking()
                .Include(e => e.Analyses)
                .FirstOrDefaultAsync(e => e.Id == id && e.AnalysteId == analyste.Id);

            if (echantillon == null)
            {
                TempData["ErrorMessage"] = "Échantillon non trouvé.";
                return RedirectToAction("MesEchantillons");
            }

            if (echantillon.Statut != StatutEchantillon.EnAnalyse)
            {
                TempData["ErrorMessage"] = "L'échantillon n'est pas en cours d'analyse.";
                return RedirectToAction("EnCours");
            }

            // Tri des analyses après chargement
            if (echantillon.Analyses != null)
            {
                echantillon.Analyses = echantillon.Analyses.OrderBy(a => a.Id).ToList();
            }

            // Création des analyses par défaut si nécessaire
            if (!echantillon.Analyses.Any())
            {
                await CreerAnalysesParDefaut(echantillon);
            }

            return View(echantillon);
        }

        // ========== TRAITEMENT DES RÉSULTATS (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoumettreResultats(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            // Chargement avec tracking pour modification
            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .FirstOrDefaultAsync(e => e.Id == id && e.AnalysteId == analyste.Id);

            if (echantillon == null)
            {
                TempData["ErrorMessage"] = "Échantillon non trouvé.";
                return RedirectToAction("EnCours");
            }

            try
            {
                bool hasChanges = false;

                // Traitement de chaque analyse
                for (int i = 0; i < echantillon.Analyses.Count; i++)
                {
                    var analyse = echantillon.Analyses.ElementAt(i);
                    var file = Request.Form.Files[$"Analyses[{i}].FichierContenu"];

                    // Gestion du fichier uploadé
                    if (file != null && file.Length > 0)
                    {
                        // Validation du fichier (5MB max)
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            TempData["ErrorMessage"] = $"Le fichier {file.FileName} dépasse 5MB.";
                            return View(echantillon);
                        }

                        // Validation du type de fichier
                        var allowedTypes = new[] { "application/pdf", "application/msword",
                                         "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
                        if (!allowedTypes.Contains(file.ContentType))
                        {
                            TempData["ErrorMessage"] = $"Type de fichier non autorisé : {file.FileName}";
                            return View(echantillon);
                        }

                        // Lecture du fichier
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            analyse.FichierContenu = memoryStream.ToArray();
                        }

                        analyse.NomFichier = Path.GetFileName(file.FileName);
                        analyse.FichierContentType = file.ContentType;
                        analyse.Resultats = $"Fichier : {analyse.NomFichier}";
                        hasChanges = true;
                    }

                    // Mise à jour du statut si nécessaire
                    if (!string.IsNullOrEmpty(analyse.Resultats) || analyse.FichierContenu != null)
                    {
                        analyse.Statut = StatutAnalyse.Terminee;
                        analyse.DateAnalyse = DateTime.Now;
                        hasChanges = true;
                    }
                }

                if (!hasChanges)
                {
                    TempData["WarningMessage"] = "Aucune modification détectée.";
                    return View(echantillon);
                }

                // Vérification si toutes les analyses sont complètes
                bool toutesCompletes = echantillon.Analyses.All(a =>
                    a.Statut == StatutAnalyse.Terminee &&
                    (!string.IsNullOrEmpty(a.Resultats) || a.FichierContenu != null));

                if (toutesCompletes)
                {
                    echantillon.Statut = StatutEchantillon.AnalyseTerminee;
                    echantillon.DateFinAnalyse = DateTime.Now;
                    TempData["SuccessMessage"] = "Toutes les analyses sont terminées !";
                }
                else
                {
                    TempData["SuccessMessage"] = "Résultats partiels enregistrés.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(toutesCompletes ? "MesResultats" : "EnCours");
            }
            catch (Exception ex)
            {
                // Journalisation de l'erreur
                System.Diagnostics.Trace.TraceError($"Erreur SaisirResultats : {ex}");
                TempData["ErrorMessage"] = "Une erreur technique est survenue.";
                return View(echantillon);
            }
        }

        private async Task CreerAnalysesParDefaut(Echantillon echantillon)
        {
            var analysesTypes = new[]
            {
        "Analyse Biologique",
        "Analyse Physico-Chimique",
        "Analyse Numérique"
    };

            foreach (var type in analysesTypes)
            {
                var analyse = new Analyse
                {
                    Nom = type,
                    EchantillonId = echantillon.Id,
                    AnalysteId = echantillon.AnalysteId ?? 0, // Problème ici si null
                    TypeAnalyse = type,
                    DateAnalyse = DateTime.Now,
                    Statut = StatutAnalyse.EnCours
                };

                _context.Analyses.Add(analyse);
            }

            await _context.SaveChangesAsync();
        }
        // Méthode pour les résultats partiels
        public async Task<IActionResult> ResultatsPartiels()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var resultatsPartiels = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id &&
                           e.Statut == StatutEchantillon.EnAnalyse)
                .Include(e => e.Analyses)
                .OrderByDescending(e => e.DateFinAnalyse)
                .ToListAsync();

            return View(resultatsPartiels);
        }

        // Méthode pour les analyses à valider
        public async Task<IActionResult> AValider()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var aValider = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id &&
                           e.Statut == StatutEchantillon.AnalyseTerminee)
                .Include(e => e.Analyses)
                .OrderBy(e => e.DateFinAnalyse)
                .ToListAsync();

            return View(aValider);
        }

        // Méthode pour valider un résultat
        [HttpPost]
        public async Task<IActionResult> ValiderResultat(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .FirstOrDefaultAsync(e => e.Id == id && e.AnalysteId == analyste.Id);

            if (echantillon != null && echantillon.Statut == StatutEchantillon.AnalyseTerminee)
            {
                echantillon.Statut = StatutEchantillon.Valide;

                // Marquer toutes les analyses comme validées
                foreach (var analyse in echantillon.Analyses)
                {
                    analyse.Statut = StatutAnalyse.Validee;
                    analyse.EstValide = true;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Résultats validés avec succès.";
            }
            else
            {
                TempData["ErrorMessage"] = "Impossible de valider cet échantillon.";
            }

            return RedirectToAction("AValider");
        }

        // ========== MES RÉSULTATS ==========
        public async Task<IActionResult> MesResultats()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var resultats = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id &&
                           (e.Statut == StatutEchantillon.AnalyseTerminee ||
                            e.Statut == StatutEchantillon.EnValidation ||
                            e.Statut == StatutEchantillon.Valide))
                .Include(e => e.Analyses)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .OrderByDescending(e => e.DateFinAnalyse ?? e.DateDebutAnalyse ?? e.DateReception)
                .ToListAsync();

            return View(resultats);
        }

        public async Task<IActionResult> TelechargerRapport(int analyseId)
        {
            var analyse = await _context.Analyses.FindAsync(analyseId);
            if (analyse?.FichierContenu == null)
            {
                TempData["ErrorMessage"] = "Aucun fichier trouvé pour cette analyse";
                return RedirectToAction("MesResultats");
            }

            return File(analyse.FichierContenu, analyse.FichierContentType, analyse.NomFichier);
        }

        // Méthode pour afficher les détails complets d'un résultat
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .FirstOrDefaultAsync(e => e.Id == id && e.AnalysteId == analyste.Id);

            if (echantillon == null)
            {
                TempData["ErrorMessage"] = "Échantillon non trouvé ou non autorisé.";
                return RedirectToAction("MesResultats");
            }

            // Vérifier que l'échantillon a des résultats
            if (echantillon.Statut == StatutEchantillon.Stocke ||
                echantillon.Statut == StatutEchantillon.EnAnalyse)
            {
                TempData["WarningMessage"] = "Cet échantillon n'a pas encore de résultats disponibles.";
                return RedirectToAction("MesResultats");
            }

            return View(echantillon);
        }

        // ========== GÉNÉRER RAPPORT TECHNIQUE ==========
        [HttpGet]
        public async Task<IActionResult> GenererRapport(int echantillonId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .Include(e => e.Analyste)
                .FirstOrDefaultAsync(e => e.Id == echantillonId && e.AnalysteId == analyste.Id);

            if (echantillon == null || echantillon.Statut != StatutEchantillon.AnalyseTerminee)
            {
                TempData["ErrorMessage"] = "Impossible de générer le rapport pour cet échantillon.";
                return RedirectToAction("MesResultats");
            }

            var viewModel = new RapportTechniqueViewModel
            {
                Echantillon = echantillon,
                Analyses = echantillon.Analyses.ToList(),
                Analyste = analyste
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenererRapportPDF(int echantillonId)
        {
            try
            {
                var rapport = await GenererRapportTechniquePDF(echantillonId);
                return File(rapport, "application/pdf", $"Rapport_Technique_{echantillonId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors de la génération du rapport: " + ex.Message;
                return RedirectToAction("MesResultats");
            }
        }

        // ========== HISTORIQUE DES ANALYSES ==========
        public async Task<IActionResult> Historique()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var historique = await _context.Echantillons
                .Where(e => e.AnalysteId == analyste.Id)
                .Include(e => e.Analyses)
                .Include(e => e.Stockage)
                .OrderByDescending(e => e.DateFinAnalyse ?? e.DateDebutAnalyse ?? e.DateReception)
                .ToListAsync();

            return View(historique);
        }

        public async Task<IActionResult> MesRapports()
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyste = await _context.Analystes
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (analyste == null)
            {
                TempData["ErrorMessage"] = "Profil analyste non trouvé.";
                return RedirectToAction("Index", "Home");
            }

            var rapports = await _context.Analyses
                .Where(a => a.AnalysteId == analyste.Id && a.FichierContenu != null)
                .Include(a => a.Echantillon)
                .OrderByDescending(a => a.DateAnalyse)
                .ToListAsync();

            return View(rapports);
        }

        [HttpPost]
        public async Task<IActionResult> SupprimerRapport(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var analyse = await _context.Analyses
                .Include(a => a.Echantillon)
                .ThenInclude(e => e.Analyste)
                .FirstOrDefaultAsync(a => a.Id == id && a.Echantillon.Analyste.UserId == currentUserId);

            if (analyse == null)
            {
                return Json(new { success = false, message = "Rapport non trouvé." });
            }

            try
            {
                _context.Analyses.Remove(analyse);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== MÉTHODES PRIVÉES ==========
        private async Task<byte[]> GenererRapportTechniquePDF(int echantillonId)
        {
            var echantillon = await _context.Echantillons
                .Include(e => e.Analyses)
                .Include(e => e.Analyste)
                .FirstOrDefaultAsync(e => e.Id == echantillonId);

            // Implémentation basique - vous devrez installer une librairie PDF comme QuestPDF ou iTextSharp 8
            using (var stream = new MemoryStream())
            {
                // Exemple basique - à adapter avec votre librairie PDF préférée
                var document = new System.Text.StringBuilder();
                document.AppendLine("RAPPORT TECHNIQUE D'ANALYSE");
                document.AppendLine($"Échantillon: {echantillon.NumeroEchantillon}");
                document.AppendLine($"Analyste: {echantillon.Analyste.nom}");
                document.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy}");

                // Convertir en bytes (dans un vrai scénario, utilisez une vraie librairie PDF)
                return System.Text.Encoding.UTF8.GetBytes(document.ToString());
            }
        }
    }
}