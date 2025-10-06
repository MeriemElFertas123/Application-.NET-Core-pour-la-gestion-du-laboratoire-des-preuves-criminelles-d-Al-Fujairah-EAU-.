using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Models.Analyse;
using WebApplication1.Models.Stockage;
using WebApplication1.ViewModels;
using WebApplication1.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class RetourController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RetourController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(context, userManager, roleManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Retour/ConsulterResultats
        public async Task<IActionResult> ConsulterResultats()
        {
            string userId = _userManager.GetUserId(User);

            var enqueteurConnecte = await _context.Enqueteurs
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (enqueteurConnecte == null)
                return View(new List<Analyse>());

            int idEnqueteur = enqueteurConnecte.idEnqueteur;

            var affaireIds = await _context.Affaires
                .Where(a => a.IdEnqueteurResponsable == idEnqueteur)
                .Select(a => a.Id)
                .ToListAsync();

            var echantillonIds = await _context.Echantillons
                .Where(e => affaireIds.Contains(e.AffaireId.Value))
                .Select(e => e.Id)
                .ToListAsync();

            var analyses = await _context.Analyses
                .Include(a => a.Echantillon)
                .Where(a => echantillonIds.Contains((int)a.EchantillonId))
                .ToListAsync();

            return View(analyses);
        }

        public async Task<IActionResult> TelechargerResultat(int id)
        {
            var analyse = await _context.Analyses.FindAsync(id);

            if (analyse == null || analyse.FichierContenu == null)
                return NotFound();

            return File(analyse.FichierContenu, analyse.FichierContentType, analyse.NomFichier);
        }

        // GET: Retour/CloturerAffaire
        public async Task<IActionResult> CloturerAffaire(string statut = "", string priorite = "", DateTime? dateOuverture = null, int? enqueteur = null, string searchText = "")
        {
            string userId = _userManager.GetUserId(User);
            var enqueteurConnecte = await _context.Enqueteurs
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (enqueteurConnecte == null)
            {
                var viewModelEmpty = new CloturerAffaireViewModel
                {
                    Affaires = new List<Affaire>(),
                    Enqueteurs = new SelectList(new List<Enqueteur>(), "idEnqueteur", "nom"),
                    TotalAffaires = 0,
                    AffairesPretes = 0,
                    AffairesEnCours = 0,
                    AffairesBloquees = 0
                };
                return View(viewModelEmpty);
            }

            int idEnqueteur = enqueteurConnecte.idEnqueteur;

            var query = _context.Affaires
                .Where(a => a.IdEnqueteurResponsable == idEnqueteur &&
                       (a.Statut == StatutAffaire.Ouverte || a.Statut == StatutAffaire.EnqueteActive) &&
                       a.DateFermeture == null);

            if (!string.IsNullOrEmpty(statut) && Enum.TryParse<StatutAffaire>(statut, out var statutEnum))
            {
                query = query.Where(a => a.Statut == statutEnum);
            }

            if (!string.IsNullOrEmpty(priorite) && Enum.TryParse<Priorite>(priorite, out var prioriteEnum))
            {
                query = query.Where(a => a.Priorite == prioriteEnum);
            }

            if (dateOuverture.HasValue)
            {
                var dateOnly = dateOuverture.Value.Date;
                query = query.Where(a => a.DateOuverture.Date == dateOnly);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(a => a.Titre.Contains(searchText) ||
                                   a.Description.Contains(searchText) ||
                                   a.NumeroAffaire.Contains(searchText));
            }

            var affaires = await query
                .Include(a => a.Echantillons)
                    .ThenInclude(e => e.Analyses)
                .OrderByDescending(a => a.DateOuverture)
                .ToListAsync();

            foreach (var affaire in affaires)
            {
                var enqueteurInfo = await _context.Enqueteurs.FindAsync(affaire.IdEnqueteurResponsable);
                if (enqueteurInfo != null)
                {
                    affaire.NomEnqueteur = enqueteurInfo.nom;
                }
            }

            var enqueteurs = await _context.Enqueteurs.ToListAsync();

            var viewModel = new CloturerAffaireViewModel
            {
                Affaires = affaires,
                Enqueteurs = new SelectList(enqueteurs, "idEnqueteur", "nom"),
                TotalAffaires = affaires.Count,
                AffairesPretes = affaires.Count(a => PeutEtreClôturee(a)),
                AffairesEnCours = affaires.Count(a => !PeutEtreClôturee(a)),
                AffairesBloquees = 0,
                Statut = statut,
                Priorite = priorite,
                DateOuverture = dateOuverture,
                Enqueteur = enqueteur,
                SearchText = searchText
            };

            return View(viewModel);
        }

        // POST: Retour/ClotureDefinitive
        [HttpPost]
        public async Task<IActionResult> ClotureDefinitive(int affaireId, string motifCloture, string commentaireCloture)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                var enqueteurConnecte = await _context.Enqueteurs
                    .FirstOrDefaultAsync(e => e.UserId == userId);

                var affaire = await _context.Affaires
                    .Include(a => a.Echantillons)
                        .ThenInclude(e => e.Analyses)
                    .FirstOrDefaultAsync(a => a.Id == affaireId &&
                                            a.IdEnqueteurResponsable == enqueteurConnecte.idEnqueteur);

                if (affaire == null)
                {
                    return Json(new { success = false, message = "Affaire non trouvée ou accès non autorisé" });
                }

                if (!PeutEtreClôturee(affaire))
                {
                    return Json(new { success = false, message = "Cette affaire ne peut pas encore être clôturée" });
                }

                StatutAffaire nouveauStatut;
                if (motifCloture.Contains("resolue") || motifCloture.Contains("condamnation") || motifCloture.Contains("acquittement"))
                {
                    nouveauStatut = StatutAffaire.Resolue;
                }
                else if (motifCloture.Contains("non_resolue") || motifCloture.Contains("preuves") || motifCloture.Contains("prescription"))
                {
                    nouveauStatut = StatutAffaire.NonResolue;
                }
                else
                {
                    nouveauStatut = StatutAffaire.Resolue;
                }

                affaire.Statut = nouveauStatut;
                affaire.DateFermeture = DateTime.Now;

                var historique = new HistoriqueAffaire
                {
                    AffaireId = affaire.Id,
                    Action = "Clôture définitive",
                    MotifCloture = motifCloture,
                    Commentaire = commentaireCloture,
                    DateAction = DateTime.Now,
                    UserId = userId
                };

                _context.HistoriqueAffaires.Add(historique);

                var echantillons = await _context.Echantillons
                    .Where(e => e.AffaireId == affaire.Id)
                    .ToListAsync();

                foreach (var echantillon in echantillons)
                {
                    echantillon.Statut = StatutEchantillon.Archive;
                    echantillon.DateArchivage = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var numeroReference = $"CLO-{DateTime.Now.Year}-{affaire.Id:000}";

                return Json(new
                {
                    success = true,
                    message = "Affaire clôturée avec succès",
                    numeroReference = numeroReference,
                    dateCloture = DateTime.Now.ToString("dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la clôture : " + ex.Message });
            }
        }

        // GET: Retour/VerifierPrerequisCloture - VERSION CORRIGÉE
        [HttpGet]
        public async Task<IActionResult> VerifierPrerequisCloture(int affaireId)
        {
            try
            {
                var affaire = await _context.Affaires
                    .Include(a => a.Echantillons)
                        .ThenInclude(e => e.Analyses)
                    .FirstOrDefaultAsync(a => a.Id == affaireId);

                if (affaire == null)
                {
                    return Json(new { success = false, message = "Affaire non trouvée" });
                }

                var prerequisInfo = GetPrerequisCloture(affaire);

                // Calculer si l'affaire peut être clôturée
                bool peutEtreCloturee = prerequisInfo.All(p => p.Statut == "completed");

                return Json(new
                {
                    success = true,
                    prerequis = prerequisInfo,
                    peutEtreCloturee = peutEtreCloturee
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Erreur lors de la vérification: " + ex.Message
                });
            }
        }

        // GET: Retour/GetAffaireDetails
        [HttpGet]
        public async Task<IActionResult> GetAffaireDetails(int affaireId)
        {
            var affaire = await _context.Affaires
                .Include(a => a.Echantillons)
                    .ThenInclude(e => e.Analyses)
                .FirstOrDefaultAsync(a => a.Id == affaireId);

            if (affaire == null)
            {
                return Json(new { success = false });
            }

            var enqueteur = await _context.Enqueteurs.FindAsync(affaire.IdEnqueteurResponsable);

            var preuves = affaire.Echantillons.Select(e => new
            {
                id = e.Id,
                description = e.Description,
                type = e.Type.ToString(),
                statut = e.Statut.ToString()
            }).ToList();

            var analyses = affaire.Echantillons
                .SelectMany(e => e.Analyses.Select(a => new
                {
                    id = a.Id,
                    type = a.TypeAnalyse,
                    statut = a.Statut.ToString(),
                    dateCreation = a.DateAnalyse,
                    echantillonId = e.Id
                }))
                .ToList();

            return Json(new
            {
                success = true,
                affaire = new
                {
                    id = affaire.Id,
                    titre = affaire.Titre,
                    description = affaire.Description,
                    numeroAffaire = affaire.NumeroAffaire,
                    statut = affaire.Statut.ToString(),
                    priorite = affaire.Priorite.ToString(),
                    dateOuverture = affaire.DateOuverture.ToString("dd/MM/yyyy"),
                    lieu = affaire.Lieu,
                    enqueteur = enqueteur != null ? enqueteur.nom : "Non assigné"
                },
                preuves = preuves,
                analyses = analyses,
                peutEtreCloturee = PeutEtreClôturee(affaire)
            });
        }

        // Méthodes privées helper
        private bool PeutEtreClôturee(Affaire affaire)
        {
            if (affaire.Statut != StatutAffaire.Ouverte && affaire.Statut != StatutAffaire.EnqueteActive)
                return false;

            if (affaire.DateFermeture != null)
                return false;

            if (affaire.Echantillons == null)
            {
                var echantillons = _context.Echantillons.Where(e => e.AffaireId == affaire.Id).ToList();
                affaire.Echantillons = echantillons;
            }

            if (!affaire.Echantillons.Any())
                return true;

            foreach (var echantillon in affaire.Echantillons)
            {
                if (echantillon.Analyses == null)
                {
                    echantillon.Analyses = _context.Analyses.Where(a => a.EchantillonId == echantillon.Id).ToList();
                }

                if (echantillon.Analyses.Any() && echantillon.Analyses.Any(a => a.Statut != StatutAnalyse.Validee))
                    return false;
            }

            return true;
        }

        // VERSION CORRIGÉE avec classe spécifique
        public class PrerequisCloture
        {
            public string Titre { get; set; }
            public string Statut { get; set; }
            public string Description { get; set; }
        }

        private List<PrerequisCloture> GetPrerequisCloture(Affaire affaire)
        {
            var prerequis = new List<PrerequisCloture>();

            // 1. Vérifier les analyses terminées
            bool toutesAnalysesTerminees = true;
            int nombreAnalyses = 0;
            int analysesValidees = 0;

            if (affaire.Echantillons != null && affaire.Echantillons.Any())
            {
                foreach (var echantillon in affaire.Echantillons)
                {
                    if (echantillon.Analyses != null && echantillon.Analyses.Any())
                    {
                        nombreAnalyses += echantillon.Analyses.Count();
                        analysesValidees += echantillon.Analyses.Count(a => a.Statut == StatutAnalyse.Validee);

                        if (echantillon.Analyses.Any(a => a.Statut != StatutAnalyse.Validee))
                        {
                            toutesAnalysesTerminees = false;
                        }
                    }
                }
            }

            prerequis.Add(new PrerequisCloture
            {
                Titre = "Analyses terminées",
                Statut = toutesAnalysesTerminees ? "completed" : "pending",
                Description = nombreAnalyses > 0 ?
                    $"{analysesValidees}/{nombreAnalyses} analyses validées" :
                    "Aucune analyse requise"
            });

            // 2. Vérifier la documentation complète
            bool documentationComplete = true;
            int documentsManquants = 0;

            if (affaire.Echantillons != null && affaire.Echantillons.Any())
            {
                foreach (var echantillon in affaire.Echantillons)
                {
                    if (echantillon.Analyses != null && echantillon.Analyses.Any())
                    {
                        foreach (var analyse in echantillon.Analyses)
                        {
                            if (analyse.FichierContenu == null && string.IsNullOrEmpty(analyse.Resultats))
                            {
                                documentationComplete = false;
                                documentsManquants++;
                            }
                        }
                    }
                }
            }

            prerequis.Add(new PrerequisCloture
            {
                Titre = "Documentation complète",
                Statut = documentationComplete ? "completed" : "pending",
                Description = documentationComplete ?
                    "Tous les rapports d'analyses sont disponibles" :
                    $"{documentsManquants} rapport(s) manquant(s)"
            });

            // 3. Vérifier le statut approprié
            bool statutOK = affaire.Statut == StatutAffaire.Ouverte || affaire.Statut == StatutAffaire.EnqueteActive;

            prerequis.Add(new PrerequisCloture
            {
                Titre = "Statut approprié pour clôture",
                Statut = statutOK ? "completed" : "blocked",
                Description = statutOK ?
                    $"Affaire en statut '{affaire.Statut}' - peut être clôturée" :
                    $"Affaire en statut '{affaire.Statut}' - ne peut pas être clôturée"
            });

            // 4. Vérifier qu'elle n'est pas déjà fermée
            bool pasDejeFermee = affaire.DateFermeture == null;

            prerequis.Add(new PrerequisCloture
            {
                Titre = "Affaire non clôturée",
                Statut = pasDejeFermee ? "completed" : "blocked",
                Description = pasDejeFermee ?
                    "L'affaire n'a pas encore été clôturée" :
                    $"Affaire déjà clôturée le {affaire.DateFermeture?.ToString("dd/MM/yyyy")}"
            });

            return prerequis;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}