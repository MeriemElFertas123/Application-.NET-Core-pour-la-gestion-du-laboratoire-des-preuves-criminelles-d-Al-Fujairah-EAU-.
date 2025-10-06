using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Envoi;
using WebApplication1.Models.Stockage;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class StockageController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public StockageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
            : base(context, userManager, roleManager)
        {
            _context = context;
        }

        // GET: Stockage/Stocker/5
        public async Task<IActionResult> Stocker(int id)
        {
            var envoiComplet = await _context.EnvoiComplets.FindAsync(id);
            if (envoiComplet == null)
            {
                TempData["ErrorMessage"] = "Échantillon non trouvé.";
                return RedirectToAction("Index");
            }

            if (envoiComplet.StatutEchantillon != "Accepté")
            {
                TempData["ErrorMessage"] = "Cet échantillon n'est pas prêt pour le stockage.";
                return RedirectToAction("Index");
            }

            var echantillonTemp = new Echantillon
            {
                Id = envoiComplet.Id,
                NumeroEchantillon = envoiComplet.CodeQR,
                NumeroAffaire = envoiComplet.Affaire?.NumeroAffaire ?? "N/A",
                Description = envoiComplet.ObservationsEchantillon,
                DateReception = envoiComplet.DateReception ?? DateTime.Now,
                Type = GetTypeFromAnalyse(envoiComplet.TypeAnalyseDemandee),
                Statut = StatutEchantillon.Recu,
                ConditionsStockage = envoiComplet.ConditionsStockage
            };

            var model = new StockerEchantillonViewModel
            {
                Echantillon = echantillonTemp,
                ZonesDisponibles = await GetZonesCompatibles(echantillonTemp.Type),
                EmplacementsLibres = await GetEmplacementsLibres()
            };

            return View(model);
        }

        // POST: Stockage/Stocker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stocker(StockerEchantillonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ZonesDisponibles = await GetZonesCompatibles(model.Echantillon.Type);
                model.EmplacementsLibres = await GetEmplacementsLibres();
                return View(model);
            }

            try
            {
                if (!await IsEmplacementLibre(model.ZoneSelectionnee, model.EmplacementSelectionne))
                {
                    ModelState.AddModelError("", "L'emplacement sélectionné n'est plus disponible.");
                    model.ZonesDisponibles = await GetZonesCompatibles(model.Echantillon.Type);
                    model.EmplacementsLibres = await GetEmplacementsLibres();
                    return View(model);
                }

                var envoiComplet = await _context.EnvoiComplets.FindAsync(model.Echantillon.Id);
                if (envoiComplet == null)
                {
                    TempData["ErrorMessage"] = "Échantillon non trouvé.";
                    return RedirectToAction("Index");
                }

                var stockage = new Stockage
                {
                    EchantillonId = envoiComplet.Id,
                    Zone = model.ZoneSelectionnee,
                    Emplacement = model.EmplacementSelectionne,
                    DateStockage = DateTime.Now,
                    TechnicienId = GetCurrentUserId(),
                    Statut = StatutStockage.Stocke
                };

                _context.Stockages.Add(stockage);
                envoiComplet.StatutEchantillon = "Stocké";

                var emplacement = await _context.Emplacements.FirstOrDefaultAsync(e =>
                    e.Zone == model.ZoneSelectionnee &&
                    e.Numero == model.EmplacementSelectionne);

                if (emplacement != null)
                {
                    emplacement.EstOccupe = true;
                    emplacement.EchantillonId = envoiComplet.Id;
                    emplacement.DateOccupation = DateTime.Now;
                }

                var zone = await _context.ZoneStockages.FindAsync(model.ZoneSelectionnee);
                if (zone != null)
                {
                    zone.Occupe++;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Échantillon {envoiComplet.CodeQR} stocké avec succès à l'emplacement {model.ZoneSelectionnee}-{model.EmplacementSelectionne}";

                return RedirectToAction("Details", new { id = stockage.EchantillonId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors du stockage : " + ex.Message;
                model.ZonesDisponibles = await GetZonesCompatibles(model.Echantillon.Type);
                model.EmplacementsLibres = await GetEmplacementsLibres();
                return View(model);
            }
        }

        // GET: Stockage/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var stockage = await _context.Stockages.FirstOrDefaultAsync(s => s.EchantillonId == id);
            if (stockage == null)
            {
                TempData["ErrorMessage"] = "Enregistrement de stockage non trouvé.";
                return RedirectToAction("Index");
            }

            var envoiComplet = await _context.EnvoiComplets.FindAsync(id);
            if (envoiComplet != null)
            {
                stockage.Echantillon = new Echantillon
                {
                    Id = envoiComplet.Id,
                    NumeroEchantillon = envoiComplet.CodeQR,
                    NumeroAffaire = envoiComplet.Affaire?.NumeroAffaire ?? "N/A",
                    Description = envoiComplet.ObservationsEchantillon,
                    DateReception = envoiComplet.DateReception ?? DateTime.Now,
                    Type = GetTypeFromAnalyse(envoiComplet.TypeAnalyseDemandee),
                    ConditionsStockage = envoiComplet.ConditionsStockage
                };
            }

            return View(stockage);
        }

        // GET: Stockage/PlanStockage
        public async Task<IActionResult> PlanStockage()
        {
            var model = new PlanStockageViewModel
            {
                Zones = await _context.ZoneStockages
                    .Include(z => z.Emplacements)
                    .OrderBy(z => z.Code)
                    .ToListAsync(),
                Filtres = new FiltresStockage(),
                Statistiques = await GetStatistiquesStockage()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> LibererEmplacement(string emplacement)
        {
            var parts = emplacement.Split('-');
            if (parts.Length != 2)
                return Json(new { success = false, message = "Emplacement invalide" });

            var zone = parts[0];
            var numero = parts[1];

            try
            {
                var emplacementEntity = await _context.Emplacements
                    .FirstOrDefaultAsync(e => e.Zone == zone && e.Numero == numero);

                if (emplacementEntity == null)
                    return Json(new { success = false, message = "Emplacement non trouvé" });

                if (!emplacementEntity.EstOccupe)
                    return Json(new { success = false, message = "L'emplacement est déjà libre" });

                emplacementEntity.EstOccupe = false;
                emplacementEntity.EchantillonId = null;
                emplacementEntity.DateOccupation = null;

                var zoneEntity = await _context.ZoneStockages.FirstOrDefaultAsync(z => z.Code == zone);
                if (zoneEntity != null)
                {
                    zoneEntity.Occupe--;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Emplacement libéré avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        // GET: Stockage/Rechercher
        public async Task<IActionResult> Rechercher(string terme, string zone, string statut, string type)
        {
            var resultats = await RechercherEchantillons(terme, zone, statut, type);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ResultatsRecherche", resultats);
            }

            var model = new RechercheStockageViewModel
            {
                Terme = terme,
                ZoneSelectionnee = zone,
                StatutSelectionne = statut,
                TypeSelectionne = type,
                Resultats = resultats,
                Zones = await GetZonesStockage(),
                Statuts = GetStatutsStockage(),
                Types = GetTypesEchantillon()
            };

            return View(model);
        }

        // POST: Stockage/PreparerAnalyse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreparerAnalyse(int[] echantillonIds)
        {
            if (echantillonIds == null || !echantillonIds.Any())
            {
                TempData["ErrorMessage"] = "Aucun échantillon sélectionné.";
                return RedirectToAction("Index");
            }

            try
            {
                foreach (var id in echantillonIds)
                {
                    var echantillon = await _context.Echantillons.FindAsync(id);
                    if (echantillon != null && echantillon.Statut == StatutEchantillon.Stocke)
                    {
                        echantillon.Statut = StatutEchantillon.EnAnalyse;

                        var stockage = await _context.Stockages.FirstOrDefaultAsync(s => s.EchantillonId == id);
                        if (stockage != null)
                        {
                            stockage.Statut = StatutStockage.PretPourAnalyse;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{echantillonIds.Length} échantillon(s) préparé(s) pour l'analyse.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors de la préparation : " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Stockage/GetEmplacements (AJAX) - VERSION CORRIGÉE
        public async Task<JsonResult> GetEmplacements(string zone)
        {
            try
            {
                if (string.IsNullOrEmpty(zone))
                {
                    System.Diagnostics.Debug.WriteLine("Zone vide ou null");
                    return Json(new List<object>());
                }

                System.Diagnostics.Debug.WriteLine($"Recherche emplacements pour zone: {zone}");

                var emplacements = await _context.Emplacements
                    .Where(e => e.Zone == zone)
                    .OrderBy(e => e.Numero)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Emplacements bruts trouvés: {emplacements.Count}");

                var result = emplacements.Select(e => new {
                    Value = e.Numero,
                    Text = $"{e.Numero} {(e.EstOccupe ? "(Occupé)" : "(Libre)")}",
                    Disabled = e.EstOccupe,
                    EstOccupe = e.EstOccupe,
                    EchantillonId = e.EchantillonId,
                    DateOccupation = e.DateOccupation
                }).ToList();

                // Log détaillé pour debugging
                System.Diagnostics.Debug.WriteLine($"Zone: {zone}, Emplacements formatés: {result.Count}");
                foreach (var emp in result.Take(5)) // Afficher les 5 premiers
                {
                    System.Diagnostics.Debug.WriteLine($"  - Numero:{emp.Value}, EstOccupe:{emp.EstOccupe}, Text:{emp.Text}");
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur dans GetEmplacements: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }

        // GET: Stockage/VerifierDisponibilite (AJAX) - NOUVELLE MÉTHODE
        public async Task<JsonResult> VerifierDisponibilite(string zone, string emplacement)
        {
            try
            {
                if (string.IsNullOrEmpty(zone) || string.IsNullOrEmpty(emplacement))
                {
                    return Json(new
                    {
                        Disponible = false,
                        Message = "Zone et emplacement requis"
                    });
                }

                var emplacementEntity = await _context.Emplacements
                    .FirstOrDefaultAsync(e => e.Zone == zone && e.Numero == emplacement);

                if (emplacementEntity == null)
                {
                    return Json(new
                    {
                        Disponible = false,
                        Message = $"Emplacement {zone}-{emplacement} non trouvé dans la base de données"
                    });
                }

                bool estDisponible = !emplacementEntity.EstOccupe;

                string message = estDisponible
                    ? $"Emplacement {zone}-{emplacement} est libre et disponible"
                    : $"Emplacement {zone}-{emplacement} est déjà occupé" +
                      (emplacementEntity.EchantillonId.HasValue
                          ? $" par l'échantillon ID: {emplacementEntity.EchantillonId}"
                          : "");

                // Log pour debugging
                System.Diagnostics.Debug.WriteLine($"Vérification {zone}-{emplacement}: EstOccupe={emplacementEntity.EstOccupe}, EchantillonId={emplacementEntity.EchantillonId}");

                return Json(new
                {
                    Disponible = estDisponible,
                    Message = message,
                    Zone = zone,
                    Emplacement = emplacement,
                    DateOccupation = emplacementEntity.DateOccupation
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Disponible = false,
                    Message = $"Erreur lors de la vérification: {ex.Message}"
                });
            }
        }

        // GET: Stockage/GetStatistiques (AJAX)
        public async Task<JsonResult> GetStatistiques()
        {
            var statistiques = await GetStatistiquesStockage();
            return Json(statistiques);
        }

        #region Méthodes privées

        private async Task<List<EnvoiEchantillonReceptionDto>> GetEchantillonsEnAttente()
        {
            return await _context.EnvoiComplets
                .Where(e => e.StatutEchantillon == "Accepté")
                .Include(e => e.Affaire)
                .Select(e => new EnvoiEchantillonReceptionDto
                {
                    Id = e.Id,
                    EchantillonId = e.EchantillonId,
                    CodeQR = e.CodeQR,
                    NumeroAffaire = e.Affaire != null ? e.Affaire.NumeroAffaire : "N/A",
                    NomEnqueteur = e.Affaire != null ? e.Affaire.NomEnqueteur : "N/A",
                    DescriptionEchantillon = e.ObservationsEchantillon,
                    TypeAnalyse = e.TypeAnalyseDemandee,
                    Poids = e.Poids,
                    Couleur = e.Couleur,
                    ConditionsStockage = e.ConditionsStockage,
                    Emballage = e.Emballage,
                    StatutEchantillon = e.StatutEchantillon,
                    Priorite = "Normale",
                    DateEnvoi = e.DateEnvoiEffective,
                    DateReception = e.DateReception,
                    DateVerification = e.DateVerification,
                    VerifiePar = e.VerifiePar,
                    Observations = e.ObservationsEchantillon
                })
                .OrderByDescending(e => e.DateReception)
                .ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            var totalEnvoiComplets = await _context.EnvoiComplets.CountAsync();
            var acceptesCount = await _context.EnvoiComplets.CountAsync(e => e.StatutEchantillon == "Accepté");

            System.Diagnostics.Debug.WriteLine($"Total EnvoiComplets: {totalEnvoiComplets}");
            System.Diagnostics.Debug.WriteLine($"Échantillons acceptés: {acceptesCount}");

            var model = new StockageViewModel
            {
                EchantillonsEnAttente = await GetEchantillonsEnAttente(),
                ZonesStockage = await GetZonesStockage(),
                Statistiques = await GetStatistiquesStockage()
            };

            System.Diagnostics.Debug.WriteLine($"Échantillons en attente récupérés: {model.EchantillonsEnAttente.Count}");

            return View(model);
        }

        private async Task<List<ZoneStockage>> GetZonesStockage()
        {
            return await _context.ZoneStockages
                .OrderBy(z => z.Code)
                .ToListAsync();
        }

        private async Task<StatistiquesStockage> GetStatistiquesStockage()
        {
            var zones = await _context.ZoneStockages
                .Include(z => z.Emplacements)
                .ToListAsync();

            foreach (var zone in zones)
            {
                zone.Occupe = zone.Emplacements.Count(e => e.EstOccupe);
                zone.Capacite = zone.Emplacements.Count;
            }

            return new StatistiquesStockage
            {
                TotalCapacite = zones.Sum(z => z.Capacite),
                TotalOccupe = zones.Sum(z => z.Occupe),
                EchantillonsEnAttente = (await GetEchantillonsEnAttente()).Count,
                TauxOccupation = zones.Sum(z => z.Capacite) > 0 ?
                    Math.Round((double)zones.Sum(z => z.Occupe) / zones.Sum(z => z.Capacite) * 100, 1) : 0
            };
        }

        private async Task<List<ZoneStockage>> GetZonesCompatibles(TypeEchantillon type)
        {
            var toutesZones = await _context.ZoneStockages
                .Include(z => z.Emplacements)
                .Where(z => z.Capacite > z.Occupe)
                .ToListAsync();

            switch (type)
            {
                case TypeEchantillon.Sang:
                case TypeEchantillon.Urine:
                    return toutesZones.Where(z => z.Temperature <= -20).ToList();
                case TypeEchantillon.Solide:
                    return toutesZones.Where(z => z.Temperature >= -20).ToList();
                default:
                    return toutesZones;
            }
        }

        private async Task<List<Emplacement>> GetEmplacementsLibres()
        {
            return await _context.Emplacements
                .Where(e => !e.EstOccupe)
                .OrderBy(e => e.Zone)
                .ThenBy(e => e.Numero)
                .ToListAsync();
        }

        private async Task<bool> IsEmplacementLibre(string zone, string numero)
        {
            var emplacement = await _context.Emplacements.FirstOrDefaultAsync(e =>
                e.Zone == zone && e.Numero == numero);
            return emplacement != null && !emplacement.EstOccupe;
        }

        private async Task<List<Emplacement>> GetTousEmplacements()
        {
            return await _context.Emplacements
                .Include(e => e.Echantillon)
                .OrderBy(e => e.Zone)
                .ThenBy(e => e.Numero)
                .ToListAsync();
        }

        private async Task<List<Echantillon>> RechercherEchantillons(string terme, string zone, string statut, string type)
        {
            var query = _context.Echantillons.AsQueryable();

            if (!string.IsNullOrEmpty(terme))
            {
                query = query.Where(e =>
                    e.NumeroEchantillon.Contains(terme) ||
                    e.NumeroAffaire.Contains(terme) ||
                    e.Description.Contains(terme));
            }

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<TypeEchantillon>(type, out var typeEnum))
            {
                query = query.Where(e => e.Type == typeEnum);
            }

            if (!string.IsNullOrEmpty(statut) && Enum.TryParse<StatutEchantillon>(statut, out var statutEnum))
            {
                query = query.Where(e => e.Statut == statutEnum);
            }

            return await query.OrderByDescending(e => e.DateReception).ToListAsync();
        }

        private List<string> GetStatutsStockage()
        {
            return Enum.GetNames(typeof(StatutStockage)).ToList();
        }

        private List<string> GetTypesEchantillon()
        {
            return Enum.GetNames(typeof(TypeEchantillon)).ToList();
        }

        private string GetCurrentUserId()
        {
            return User.Identity.Name;
        }

        private TypeEchantillon GetTypeFromAnalyse(string typeAnalyse)
        {
            if (string.IsNullOrEmpty(typeAnalyse))
                return TypeEchantillon.Solide;

            switch (typeAnalyse.ToLower())
            {
                case "analyse chimique":
                case "analyse toxicologique":
                    return TypeEchantillon.Liquide;
                case "analyse biologique":
                case "analyse adn":
                    return TypeEchantillon.Sang;
                case "analyse balistique":
                case "analyse d'empreintes":
                    return TypeEchantillon.Solide;
                default:
                    return TypeEchantillon.Solide;
            }
        }

        #endregion

        public IActionResult CreateZone()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateZone([Bind("Code,Nom,Temperature,Capacite")] ZoneStockage zone)
        {
            if (ModelState.IsValid)
            {
                zone.Occupe = 0;
                _context.ZoneStockages.Add(zone);
                await _context.SaveChangesAsync();
                return RedirectToAction("PlanStockage");
            }
            return View(zone);
        }

        // GET: Stockage/AttributionAnalyse
        public async Task<IActionResult> AttributionAnalyse()
        {
            var model = new AttributionAnalyseViewModel
            {
                EchantillonsDisponibles = await GetEchantillonsDisponiblesPourAnalyse(),
                AnalystesDisponibles = await GetAnalystesDisponibles(),
                TypesAnalyseDisponibles = GetTypesAnalyseDisponibles(),
                NiveauxPriorite = GetNiveauxPriorite()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttributionAnalyse(AttributionAnalyseViewModel model)
        {
            try
            {
                // Log pour déboguer
                System.Diagnostics.Debug.WriteLine($"POST reçu - AnalysteId: {model.AnalysteId}");
                System.Diagnostics.Debug.WriteLine($"TypeAnalyse: {model.TypeAnalyseRequis}");
                System.Diagnostics.Debug.WriteLine($"Priorité: {model.PrioriteAnalyse}");

                // Recharger les listes déroulantes en cas d'erreur
                await RechargerListesDeroulantes(model);

                // Validation manuelle des champs requis
                if (model.AnalysteId <= 0)
                {
                    ModelState.AddModelError("AnalysteId", "Veuillez sélectionner un analyste.");
                }

                if (string.IsNullOrEmpty(model.TypeAnalyseRequis))
                {
                    ModelState.AddModelError("TypeAnalyseRequis", "Veuillez sélectionner un type d'analyse.");
                }

                if (string.IsNullOrEmpty(model.PrioriteAnalyse))
                {
                    ModelState.AddModelError("PrioriteAnalyse", "Veuillez sélectionner une priorité.");
                }

                // Vérifier les échantillons sélectionnés
                var echantillonsSelectionnes = model.EchantillonsDisponibles?.Where(e => e.EstSelectionne).ToList() ?? new List<EchantillonPourAttribution>();

                System.Diagnostics.Debug.WriteLine($"Échantillons sélectionnés: {echantillonsSelectionnes.Count}");

                if (!echantillonsSelectionnes.Any())
                {
                    ModelState.AddModelError("", "Veuillez sélectionner au moins un échantillon.");
                }

                // Si erreurs de validation, retourner la vue avec les erreurs
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Vérifier que l'analyste existe
                var analyste = await _context.Analystes.FindAsync(model.AnalysteId);
                if (analyste == null)
                {
                    ModelState.AddModelError("AnalysteId", "Analyste non trouvé.");
                    return View(model);
                }

                string currentUserId = GetCurrentUserId();
                int attributionsCreees = 0;

                // *** TRANSACTION POUR S'ASSURER DE LA COHÉRENCE ***
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Traitement des attributions
                        foreach (var echantillonSelectionne in echantillonsSelectionnes)
                        {
                            System.Diagnostics.Debug.WriteLine($"Traitement échantillon ID: {echantillonSelectionne.Id}");

                            // *** RÉCUPÉRATION AVEC TRACKING EXPLICITE ***
                            var echantillon = await _context.Echantillons
                                .Include(e => e.Stockage)
                                .FirstOrDefaultAsync(e => e.Id == echantillonSelectionne.Id);

                            if (echantillon == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"ERREUR: Échantillon {echantillonSelectionne.Id} non trouvé en base");
                                continue;
                            }

                            System.Diagnostics.Debug.WriteLine($"Échantillon trouvé: {echantillon.NumeroEchantillon}, Statut actuel: {echantillon.Statut}");

                            if (echantillon.Statut != StatutEchantillon.Stocke)
                            {
                                System.Diagnostics.Debug.WriteLine($"ERREUR: Échantillon {echantillon.NumeroEchantillon} n'est pas en statut Stocké (statut: {echantillon.Statut})");
                                continue;
                            }

                            // *** ASSIGNATION DE L'ANALYSTE - POINT CRITIQUE ***
                            System.Diagnostics.Debug.WriteLine($"AVANT assignation - AnalysteId: {echantillon.AnalysteId}");

                            echantillon.AnalysteId = model.AnalysteId;
                            echantillon.Statut = StatutEchantillon.EnAnalyse;

                            System.Diagnostics.Debug.WriteLine($"APRÈS assignation - AnalysteId: {echantillon.AnalysteId}");

                            // Marquer l'entité comme modifiée explicitement
                            _context.Entry(echantillon).Property(e => e.AnalysteId).IsModified = true;
                            _context.Entry(echantillon).Property(e => e.Statut).IsModified = true;

                            // Créer l'attribution
                            var attribution = new AttributionAnalyse
                            {
                                EchantillonId = echantillon.Id,
                                AnalysteId = model.AnalysteId,
                                TypeAnalyseRequis = model.TypeAnalyseRequis,
                                PrioriteAnalyse = model.PrioriteAnalyse,
                                InstructionsSpeciales = model.InstructionsSpeciales,
                                DateAttribution = DateTime.Now,
                                AttributePar = currentUserId,
                                NotificationEnvoyee = false
                            };

                            _context.AttributionAnalyses.Add(attribution);

                            // Mettre à jour le stockage si existe
                            if (echantillon.Stockage != null)
                            {
                                echantillon.Stockage.Statut = StatutStockage.PretPourAnalyse;
                                _context.Entry(echantillon.Stockage).Property(s => s.Statut).IsModified = true;
                            }

                            attributionsCreees++;
                            System.Diagnostics.Debug.WriteLine($"Attribution créée pour échantillon {echantillon.NumeroEchantillon}");
                        }

                        // Mettre à jour la charge de l'analyste
                        analyste.ChargeActuelle += attributionsCreees;
                        _context.Entry(analyste).Property(a => a.ChargeActuelle).IsModified = true;

                        System.Diagnostics.Debug.WriteLine($"Sauvegarde des {attributionsCreees} attributions...");

                        // *** SAUVEGARDER TOUTES LES MODIFICATIONS ***
                        var saveResult = await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"SaveChanges résultat: {saveResult} enregistrements modifiés");

                        // Valider la transaction
                        await transaction.CommitAsync();
                        System.Diagnostics.Debug.WriteLine("Transaction validée avec succès");

                        // *** VÉRIFICATION POST-SAUVEGARDE ***
                        foreach (var echantillonSelectionne in echantillonsSelectionnes)
                        {
                            var verification = await _context.Echantillons
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == echantillonSelectionne.Id);

                            if (verification != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"VÉRIFICATION: Échantillon {verification.NumeroEchantillon} - AnalysteId: {verification.AnalysteId}, Statut: {verification.Statut}");
                            }
                        }

                        TempData["SuccessMessage"] =
                            $"{attributionsCreees} échantillon(s) attribué(s) avec succès à {analyste.NomComplet} pour {model.TypeAnalyseRequis}.";

                        return RedirectToAction("AttributionAnalyse");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"ERREUR dans la transaction: {ex.Message}");
                        throw; // Re-lancer pour être capturé par le catch externe
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERREUR GÉNÉRALE lors de l'attribution : {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace : {ex.StackTrace}");

                TempData["ErrorMessage"] = "Erreur lors de l'attribution : " + ex.Message;

                await RechargerListesDeroulantes(model);
                return View(model);
            }
        }

        // Méthode utilitaire pour recharger les listes déroulantes
        private async Task RechargerListesDeroulantes(AttributionAnalyseViewModel model)
        {
            model.EchantillonsDisponibles = await GetEchantillonsDisponiblesPourAnalyse();
            model.AnalystesDisponibles = await GetAnalystesDisponibles();
            model.TypesAnalyseDisponibles = GetTypesAnalyseDisponibles();
            model.NiveauxPriorite = GetNiveauxPriorite();
        }

        private async Task<List<EchantillonPourAttribution>> GetEchantillonsDisponiblesPourAnalyse()
        {
            try
            {
                var echantillons = await _context.Echantillons
                    .Where(e => e.Statut == StatutEchantillon.Stocke) // Utiliser le bon enum
                    .Include(e => e.Stockage)
                    .OrderByDescending(e => e.Priorite) // Utiliser PrioriteEchantillon
                    .ThenBy(e => e.DateReception)
                    .Select(e => new EchantillonPourAttribution
                    {
                        Id = e.Id,
                        NumeroEchantillon = e.NumeroEchantillon,
                        NumeroAffaire = e.NumeroAffaire ?? "N/A",
                        TypeEchantillon = e.Type.ToString(),
                        Zone = e.Stockage != null ? e.Stockage.Zone : "Non stocké",
                        Emplacement = e.Stockage != null ? e.Stockage.Emplacement : "Non défini",
                        Statut = e.Statut.ToString(),
                        Priorite = e.Priorite.ToString(),
                        DateReception = e.DateReception,
                        EstSelectionne = false // Important : initialiser à false
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Échantillons stockés trouvés: {echantillons.Count}");

                // Log détaillé des premiers échantillons
                foreach (var ech in echantillons.Take(3))
                {
                    System.Diagnostics.Debug.WriteLine($"  - ID: {ech.Id}, Numéro: {ech.NumeroEchantillon}, Zone: {ech.Zone}");
                }

                return echantillons;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERREUR dans GetEchantillonsDisponiblesPourAnalyse: {ex.Message}");
                return new List<EchantillonPourAttribution>();
            }
        }

        private async Task<List<SelectListItem>> GetAnalystesDisponibles()
        {
            return await _context.Analystes
                .Where(a => a.statut == "actif")
                .OrderBy(a => a.ChargeActuelle)
                .ThenBy(a => a.nom)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.prenom + " " + a.nom + " - " + a.specialite + " (" + a.ChargeActuelle + " en cours)"
                })
                .ToListAsync();
        }

        private List<SelectListItem> GetTypesAnalyseDisponibles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Analyse chimique", Text = "Analyse chimique" },
                new SelectListItem { Value = "Analyse biologique", Text = "Analyse biologique" },
                new SelectListItem { Value = "Analyse toxicologique", Text = "Analyse toxicologique" },
                new SelectListItem { Value = "Analyse ADN", Text = "Analyse ADN" },
                new SelectListItem { Value = "Analyse balistique", Text = "Analyse balistique" },
                new SelectListItem { Value = "Analyse d'empreintes", Text = "Analyse d'empreintes" },
                new SelectListItem { Value = "Analyse documentaire", Text = "Analyse documentaire" }
            };
        }

        private List<SelectListItem> GetNiveauxPriorite()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Urgente", Text = "Urgente (24h)" },
                new SelectListItem { Value = "Élevée", Text = "Élevée (48h)" },
                new SelectListItem { Value = "Normale", Text = "Normale (1 semaine)" },
                new SelectListItem { Value = "Faible", Text = "Faible (2 semaines)" }
            };
        }

        [Authorize(Roles = "Analyste")]
        public async Task<IActionResult> Consulter()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");

                var analyste = await _context.Analystes.FirstOrDefaultAsync(a => a.UserId == userId);

                if (analyste == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Aucun analyste trouvé pour UserId: {userId}");
                    return NotFound($"Aucun analyste associé à ce compte. UserId: {userId}.");
                }

                var echantillons = await _context.Echantillons
                    .Where(e => e.AnalysteId == analyste.Id)
                    .Select(e => new EchantillonAConsulter
                    {
                        Id = e.Id,
                        NumeroEchantillon = e.NumeroEchantillon,
                        Type = e.Type.ToString(),
                        Statut = e.Statut.ToString(),
                        Priorite = e.Priorite.ToString(),
                        DateReception = e.DateReception,
                        Zone = e.Stockage != null ? e.Stockage.Zone : "Non stocké",
                        Emplacement = e.Stockage != null ? e.Stockage.Emplacement : "Non défini"
                    })
                    .OrderByDescending(e => e.DateReception)
                    .ToListAsync();

                var model = new ConsultationAnalysteViewModel
                {
                    NomAnalyste = $"{analyste.nom} {analyste.prenom}",
                    Echantillons = echantillons
                };

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur dans Consulter(): {ex}");
                return View("Error", new { Message = ex.Message });
            }
        }
    }
}