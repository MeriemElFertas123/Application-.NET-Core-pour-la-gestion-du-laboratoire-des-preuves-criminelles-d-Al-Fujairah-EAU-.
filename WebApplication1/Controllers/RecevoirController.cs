using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.Envoi;
using WebApplication1.Data;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RecevoirController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public RecevoirController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager):base(context,userManager,roleManager)
        {
            _context = context;
        }

        // GET: Reception
        public async Task<IActionResult> Index(FiltresReception filtres = null)
        {
            var model = new ReceptionViewModel
            {
                Filtres = filtres ?? new FiltresReception()
            };

            try
            {
                var query = _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(model.Filtres.Recherche))
                {
                    query = query.Where(ec =>
                        ec.CodeQR.Contains(model.Filtres.Recherche) ||
                        ec.Affaire.Titre.Contains(model.Filtres.Recherche) ||
                        ec.ObservationsEchantillon.Contains(model.Filtres.Recherche));
                }

                if (!string.IsNullOrEmpty(model.Filtres.Statut))
                {
                    switch (model.Filtres.Statut)
                    {
                        case "pending":
                            query = query.Where(ec => ec.StatutEchantillon == "Reçu" || ec.StatutEchantillon == "Envoyé");
                            break;
                        case "verified":
                            query = query.Where(ec => ec.StatutEchantillon == "Accepté" || ec.StatutEchantillon == "Vérifié");
                            break;
                        case "rejected":
                            query = query.Where(ec => ec.StatutEchantillon == "Refusé");
                            break;
                        case "stored":
                            query = query.Where(ec => ec.StatutEchantillon == "Stocké");
                            break;
                    }
                }

                if (model.Filtres.DateDebut.HasValue)
                {
                    query = query.Where(ec => ec.DateEnvoiEffective >= model.Filtres.DateDebut.Value);
                }

                if (model.Filtres.DateFin.HasValue)
                {
                    query = query.Where(ec => ec.DateEnvoiEffective <= model.Filtres.DateFin.Value);
                }

                var queryResults = await query
                    .OrderByDescending(ec => ec.DateEnvoiEffective)
                    .Select(ec => new
                    {
                        ec.Id,
                        ec.EchantillonId,
                        ec.CodeQR,
                        AffaireId = ec.Affaire.Id,
                        NomEnqueteur = ec.Affaire.NomEnqueteur,
                        DescriptionEchantillon = ec.ObservationsEchantillon,
                        ec.TypeAnalyseDemandee,
                        ec.Poids,
                        ec.Couleur,
                        ec.ConditionsStockage,
                        ec.Emballage,
                        ec.StatutEchantillon,
                        Priorite = ec.TypeAnalyseDemandee.Contains("Urgent") ? "urgent" :
                                 ec.TypeAnalyseDemandee.Contains("Faible") ? "low" : "normal",
                        ec.DateEnvoiEffective,
                        ec.DateReception,
                        ec.DateVerification,
                        ec.VerifiePar,
                        ec.ObservationsEchantillon
                    })
                    .ToListAsync();

                model.EchantillonsRecus = queryResults.Select(x => new EnvoiEchantillonReceptionDto
                {
                    Id = x.Id,
                    EchantillonId = x.EchantillonId,
                    CodeQR = x.CodeQR,
                    NumeroAffaire = $"AFF-{x.AffaireId:D4}",
                    NomEnqueteur = x.NomEnqueteur ?? "Non spécifié",
                    DescriptionEchantillon = x.DescriptionEchantillon ?? "Description non disponible",
                    TypeAnalyse = x.TypeAnalyseDemandee,
                    Poids = x.Poids,
                    Couleur = x.Couleur,
                    ConditionsStockage = x.ConditionsStockage,
                    Emballage = x.Emballage,
                    StatutEchantillon = x.StatutEchantillon ?? "En cours",
                    Priorite = x.Priorite,
                    DateEnvoi = x.DateEnvoiEffective,
                    DateReception = x.DateReception,
                    DateVerification = x.DateVerification,
                    VerifiePar = x.VerifiePar,
                    Observations = x.ObservationsEchantillon
                }).ToList();

                model.Statistiques = new StatistiquesReception
                {
                    EnAttente = model.EchantillonsRecus.Count(e => e.StatutEchantillon == "Reçu" || e.StatutEchantillon == "Envoyé"),
                    Verifies = model.EchantillonsRecus.Count(e => e.StatutEchantillon == "Accepté" || e.StatutEchantillon == "Vérifié"),
                    Refuses = model.EchantillonsRecus.Count(e => e.StatutEchantillon == "Refusé"),
                    Stockes = model.EchantillonsRecus.Count(e => e.StatutEchantillon == "Stocké")
                };
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement des données : " + ex.Message;
                model.EchantillonsRecus = new List<EnvoiEchantillonReceptionDto>();
                model.Statistiques = new StatistiquesReception();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraiterVerification(VerificationCorrespondanceViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Données invalides" });
                }

                var envoiComplet = await _context.EnvoiComplets
                    .FirstOrDefaultAsync(ec => ec.Id == model.EnvoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                // Vérification des critères
                bool tousConformes = true;
                var nonConformites = new List<string>();
                var observations = new List<string>();

                foreach (var critere in model.CriteresVerification)
                {
                    if (critere.Obligatoire && string.IsNullOrEmpty(critere.ValeurConstatee))
                    {
                        tousConformes = false;
                        nonConformites.Add($"{critere.Nom}: valeur manquante");
                        continue;
                    }

                    bool conforme = false;

                    switch (critere.Nom)
                    {
                        case "Poids":
                            if (double.TryParse(critere.ValeurConstatee, out double poidsConstate))
                            {
                                conforme = poidsConstate >= critere.ToleranceMin && poidsConstate <= critere.ToleranceMax;
                            }
                            break;

                        case "Couleur":
                            conforme = critere.ValeurConstatee == critere.ValeurAttendue || critere.ValeurConstatee == "Autre";
                            break;

                        case "Emballage":
                        case "État général":
                            conforme = critere.ValeurConstatee == "Conforme" || critere.ValeurConstatee == "Bon état";
                            break;

                        default:
                            conforme = true;
                            break;
                    }

                    if (!conforme)
                    {
                        tousConformes = false;
                        nonConformites.Add($"{critere.Nom}: {critere.ValeurConstatee} (attendu: {critere.ValeurAttendue})");
                    }

                    if (!string.IsNullOrEmpty(critere.Observations))
                    {
                        observations.Add($"{critere.Nom}: {critere.Observations}");
                    }
                }

                // Mise à jour du statut
                envoiComplet.StatutEchantillon = tousConformes ? "Accepté" : "Refusé";
                envoiComplet.DateVerification = DateTime.Now;
                envoiComplet.VerifiePar = User.Identity.Name ?? "Système";
                envoiComplet.ObservationsEchantillon = string.Join("; ", observations);

                if (!string.IsNullOrEmpty(model.ObservationsGenerales))
                {
                    envoiComplet.ObservationsEchantillon += (observations.Any() ? "; " : "") + model.ObservationsGenerales;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = tousConformes
                        ? "Échantillon accepté avec succès"
                        : "Échantillon refusé - " + string.Join(", ", nonConformites),
                    statut = envoiComplet.StatutEchantillon
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Erreur technique",
                    error = ex.Message
                });
            }
        }

        // POST: Procéder au stockage (modifié pour accepter "Accepté" aussi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcederAuStockage(int envoiEchantillonId)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .FirstOrDefaultAsync(ec => ec.Id == envoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                // CHANGEMENT : Accepter aussi le statut "Accepté"
                if (envoiComplet.StatutEchantillon != "Accepté" &&
                    envoiComplet.StatutEchantillon != "Conforme" &&
                    envoiComplet.StatutEchantillon != "Vérifié")
                {
                    return Json(new { success = false, message = "Seuls les échantillons acceptés peuvent être stockés" });
                }

                envoiComplet.StatutEchantillon = "Stocké";
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Échantillon transféré au stockage avec succès"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        // GET: Charger la vérification de correspondance
        [HttpGet]
        public async Task<IActionResult> VerifierEchantillon(int id)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                if (envoiComplet.StatutEchantillon != "Envoyé")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cet échantillon ne peut pas être vérifié dans son état actuel"
                    });
                }

                var viewModel = new VerificationCorrespondanceViewModel
                {
                    EnvoiEchantillonId = envoiComplet.Id,
                    EchantillonInfo = new EnvoiEchantillonReceptionDto
                    {
                        Id = envoiComplet.Id,
                        CodeQR = envoiComplet.CodeQR,
                        NumeroAffaire = $"AFF-{envoiComplet.AffaireId:D4}",
                        NomEnqueteur = envoiComplet.Affaire?.NomEnqueteur ?? "Non spécifié",
                        DescriptionEchantillon = envoiComplet.ObservationsEchantillon,
                        TypeAnalyse = envoiComplet.TypeAnalyseDemandee,
                        Poids = envoiComplet.Poids,
                        Couleur = envoiComplet.Couleur,
                        Emballage = envoiComplet.Emballage,
                        ConditionsStockage = envoiComplet.ConditionsStockage,
                        StatutEchantillon = envoiComplet.StatutEchantillon,
                        DateEnvoi = envoiComplet.DateEnvoiEffective,
                        Priorite = envoiComplet.TypeAnalyseDemandee?.Contains("Urgent") == true ? "urgent" : "normal"
                    },
                    CriteresVerification = new List<CritereVerificationCorrespondance>
                    {
                        new CritereVerificationCorrespondance
                        {
                            Nom = "Poids",
                            Icone = "fa-weight",
                            ValeurAttendue = $"{envoiComplet.Poids}g ± 2g",
                            TypeVerification = "numeric",
                            ToleranceMin = (double)(envoiComplet.Poids - 2),
                            ToleranceMax = (double)(envoiComplet.Poids + 2),
                            Obligatoire = true
                        },
                        new CritereVerificationCorrespondance
                        {
                            Nom = "Couleur",
                            Icone = "fa-palette",
                            ValeurAttendue = envoiComplet.Couleur ?? "Non spécifié",
                            TypeVerification = "select",
                            OptionsDisponibles = new List<string> { "Blanc", "Blanc cassé", "Beige", "Jaune", "Marron", "Autre" },
                            Obligatoire = true
                        },
                        new CritereVerificationCorrespondance
                        {
                            Nom = "Emballage",
                            Icone = "fa-box",
                            ValeurAttendue = envoiComplet.Emballage ?? "Non spécifié",
                            TypeVerification = "radio",
                            OptionsDisponibles = new List<string> { "Conforme", "Non conforme", "Endommagé" },
                            Obligatoire = true
                        },
                        new CritereVerificationCorrespondance
                        {
                            Nom = "État général",
                            Icone = "fa-eye",
                            ValeurAttendue = "Bon état, pas de contamination",
                            TypeVerification = "radio",
                            OptionsDisponibles = new List<string> { "Bon état", "Acceptable", "Mauvais état" },
                            Obligatoire = true
                        }
                    }
                };

                return PartialView("_VerificationCorrespondance", viewModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors du chargement: " + ex.Message });
            }
        }

        // POST: Marquer un échantillon comme reçu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerRecu(int envoiEchantillonId)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .FirstOrDefaultAsync(ec => ec.Id == envoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                if (envoiComplet.StatutEchantillon != "Envoyé")
                {
                    return Json(new { success = false, message = "Cet échantillon ne peut pas être marqué comme reçu" });
                }

                return Json(new
                {
                    success = true,
                    requiresVerification = true,
                    message = "Veuillez d'abord vérifier la correspondance de l'échantillon",
                    echantillonId = envoiEchantillonId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        // GET: Voir les détails d'un échantillon
        [HttpGet]
        public async Task<IActionResult> DetailsEchantillon(int id)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoiComplet == null)
                {
                    return Json(new { error = "Échantillon non trouvé" });
                }

                var details = new
                {
                    Id = envoiComplet.Id,
                    CodeQR = envoiComplet.CodeQR,
                    NumeroAffaire = $"AFF-{envoiComplet.AffaireId:D4}",
                    NomEnqueteur = envoiComplet.Affaire?.NomEnqueteur,
                    DescriptionEchantillon = envoiComplet.ObservationsEchantillon,
                    TypeAnalyse = envoiComplet.TypeAnalyseDemandee,
                    Poids = envoiComplet.Poids,
                    Couleur = envoiComplet.Couleur,
                    Emballage = envoiComplet.Emballage,
                    ConditionsStockage = envoiComplet.ConditionsStockage,
                    StatutEchantillon = envoiComplet.StatutEchantillon,
                    DateEnvoi = envoiComplet.DateEnvoiEffective,
                    DateReception = envoiComplet.DateReception,
                    DateVerification = envoiComplet.DateVerification,
                    VerifiePar = envoiComplet.VerifiePar,
                    Observations = envoiComplet.ObservationsEchantillon
                };

                return Json(details);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Erreur: " + ex.Message });
            }
        }

        // GET: Afficher l'historique des réceptions
        [HttpGet]
        public async Task<IActionResult> Historique(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                dateDebut = dateDebut ?? DateTime.Now.AddDays(-30);
                dateFin = dateFin ?? DateTime.Now;

                var historique = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Where(ec => ec.DateReception.HasValue &&
                                ec.DateReception >= dateDebut &&
                                ec.DateReception <= dateFin)
                    .OrderByDescending(ec => ec.DateReception)
                    .Select(ec => new EnvoiEchantillonReceptionDto
                    {
                        Id = ec.Id,
                        CodeQR = ec.CodeQR,
                        NumeroAffaire = $"AFF-{ec.AffaireId:D4}",
                        NomEnqueteur = ec.Affaire.NomEnqueteur,
                        DescriptionEchantillon = ec.ObservationsEchantillon,
                        TypeAnalyse = ec.TypeAnalyseDemandee,
                        StatutEchantillon = ec.StatutEchantillon,
                        DateEnvoi = ec.DateEnvoiEffective,
                        DateReception = ec.DateReception,
                        DateVerification = ec.DateVerification,
                        VerifiePar = ec.VerifiePar
                    })
                    .ToListAsync();

                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;
                ViewBag.NombreTotal = historique.Count;

                return View(historique);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement de l'historique: " + ex.Message;
                return View(new List<EnvoiEchantillonReceptionDto>());
            }
        }

        private string GetValeurAttendue(EnvoiComplet envoiComplet, string critereName)
        {
            switch (critereName)
            {
                case "Poids":
                    return $"{envoiComplet.Poids}g ± 2g";
                case "Couleur":
                    return envoiComplet.Couleur ?? "Non spécifié";
                case "Emballage":
                    return envoiComplet.Emballage ?? "Non spécifié";
                case "État général":
                    return "Bon état, pas de contamination";
                default:
                    return "Non spécifié";
            }
        }

        private string DeterminerActionRecommandee(List<string> nonConformites)
        {
            var actions = new List<string>();

            foreach (var nc in nonConformites)
            {
                if (nc.Contains("Poids"))
                {
                    actions.Add("Revérifier le poids avec une balance calibrée");
                }
                else if (nc.Contains("Couleur"))
                {
                    actions.Add("Documenter la différence de couleur avec photos");
                }
                else if (nc.Contains("Emballage"))
                {
                    actions.Add("Examiner l'intégrité de l'emballage et documenter les dommages");
                }
                else if (nc.Contains("État"))
                {
                    actions.Add("Évaluer l'impact sur la qualité de l'échantillon");
                }
                else
                {
                    actions.Add("Documenter la non-conformité observée");
                }
            }

            actions.Add("Contacter l'enquêteur responsable");
            actions.Add("Évaluer la nécessité d'un nouvel échantillon");

            return string.Join("; ", actions.Distinct());
        }
    }
}