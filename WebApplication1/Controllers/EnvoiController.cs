using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Models.Envoi;
using WebApplication1.Models.Stockage;
using WebApplication1.Data;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class EnvoiController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnvoiController> _logger;

        public EnvoiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager, ILogger<EnvoiController> logger):base(context,userManager,roleManager)
        {
            _context = context;
            _logger = logger;
        }

        // Vue de préparation d'envoi existante
        public async Task<ActionResult> PreparerEnvoi(int? affaireId)
        {
            var model = new PreparationEnvoiViewModel();

            ViewBag.Affaires = (await _context.Affaires
                .Select(a => new
                {
                    Id = a.Id,
                    Titre = a.Titre
                })
                .ToListAsync())
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"AFF-{a.Id:D4} - {a.Titre}"
                })
                .ToList();

            // Charger les types d'analyse
            ViewBag.TypesAnalyse = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Toxicologique", Text = "Analyse Toxicologique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Chimique", Text = "Analyse Chimique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Biologique", Text = "Analyse Biologique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Physique", Text = "Analyse Physique" }
            };

            if (affaireId.HasValue)
            {
                model.AffaireId = affaireId.Value;

                // Récupérer les échantillons disponibles
                var echantillonsData = await _context.Echantillons
                    .Where(e => e.AffaireId == affaireId.Value)
                    .Select(e => new
                    {
                        Id = e.Id,
                        Code = e.NumeroEchantillon,
                        Description = e.Description,
                        Type = e.Type,
                        DejaEnvoye = _context.EnvoiComplets
                            .Any(ec => ec.EchantillonId == e.Id && ec.StatutEchantillon != "Refusé")
                    })
                    .ToListAsync();

                model.EchantillonsDisponibles = echantillonsData
                    .Select(e => new EchantillonDisponibleModel
                    {
                        Id = e.Id,
                        Code = e.Code,
                        Description = e.Description,
                        Type = e.Type.ToString(),
                        DejaEnvoye = e.DejaEnvoye
                    })
                    .ToList();
            }

            return View(model);
        }

        // CORRECTION POUR L'ACTION PreparerEnvoi dans EnvoiController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreparerEnvoi(PreparationEnvoiViewModel model, bool estEnvoi = false)
        {
            try
            {
                // AJOUT DE LOGS DE DEBUG
                _logger.LogInformation("=== DEBUG: Début PreparerEnvoi ===");
                _logger.LogInformation($"AffaireId: {model.AffaireId}");
                _logger.LogInformation($"TypeAnalyseDemandee: {model.TypeAnalyseDemandee}");
                _logger.LogInformation($"EstEnvoi: {estEnvoi}");
                _logger.LogInformation($"EchantillonsSelectionnes Count: {model.EchantillonsSelectionnes?.Count ?? 0}");
                _logger.LogInformation($"EchantillonsDetailsJson: {model.EchantillonsDetailsJson ?? "NULL"}");
                _logger.LogInformation($"QRCodesJson: {model.QRCodesJson ?? "NULL"}");

                // Vérifications de base
                if (model.AffaireId <= 0)
                {
                    _logger.LogWarning("Affaire non sélectionnée");
                    ModelState.AddModelError("", "Veuillez sélectionner une affaire.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.TypeAnalyseDemandee))
                {
                    _logger.LogWarning("Type d'analyse non sélectionné");
                    ModelState.AddModelError("", "Veuillez sélectionner un type d'analyse.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                // CORRECTION: Vérifier d'abord si les échantillons sont sélectionnés
                if (model.EchantillonsSelectionnes == null || !model.EchantillonsSelectionnes.Any())
                {
                    _logger.LogWarning("Aucun échantillon sélectionné");
                    ModelState.AddModelError("", "Aucun échantillon sélectionné.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                // CORRECTION: Vérifier les JSON avant de les désérialiser
                if (string.IsNullOrEmpty(model.EchantillonsDetailsJson))
                {
                    _logger.LogWarning("EchantillonsDetailsJson est vide");
                    ModelState.AddModelError("", "Aucun détail d'échantillon fourni. Veuillez sauvegarder les détails de chaque échantillon.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.QRCodesJson))
                {
                    _logger.LogWarning("QRCodesJson est vide");
                    ModelState.AddModelError("", "Codes QR manquants. Veuillez vous assurer que tous les échantillons ont un QR code généré.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                // Désérialiser les données JSON avec gestion d'erreurs améliorée
                Dictionary<string, JsonElement> echantillonsDetails;
                Dictionary<string, string> qrCodes;

                try
                {
                    echantillonsDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(model.EchantillonsDetailsJson);
                    qrCodes = JsonSerializer.Deserialize<Dictionary<string, string>>(model.QRCodesJson);

                    _logger.LogInformation($"Désérialisation réussie: {echantillonsDetails.Count} détails, {qrCodes.Count} QR codes");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erreur de désérialisation JSON");
                    ModelState.AddModelError("", "Erreur dans le format des données. Veuillez réessayer.");
                    await RechargerDonnees(model);
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur générale lors de la désérialisation");
                    ModelState.AddModelError("", "Erreur technique. Veuillez réessayer.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                // CORRECTION: Vérification améliorée des données
                if (echantillonsDetails == null || !echantillonsDetails.Any())
                {
                    _logger.LogWarning("echantillonsDetails vide après désérialisation");
                    ModelState.AddModelError("", "Aucun détail d'échantillon valide trouvé.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                // Vérifier que tous les échantillons sélectionnés ont leurs détails
                var echantillonsManquants = new List<int>();
                foreach (var echantillonId in model.EchantillonsSelectionnes)
                {
                    var echantillonIdStr = echantillonId.ToString();
                    if (!echantillonsDetails.ContainsKey(echantillonIdStr) || !qrCodes.ContainsKey(echantillonIdStr))
                    {
                        echantillonsManquants.Add(echantillonId);
                    }
                }

                if (echantillonsManquants.Any())
                {
                    _logger.LogWarning($"Échantillons manquants: {string.Join(", ", echantillonsManquants)}");
                    ModelState.AddModelError("", $"Détails manquants pour les échantillons: {string.Join(", ", echantillonsManquants)}. Veuillez sauvegarder les détails de tous les échantillons sélectionnés.");
                    await RechargerDonnees(model);
                    return View(model);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var envoiComptesCreés = new List<EnvoiComplet>();

                        // Pour chaque échantillon sélectionné, créer un enregistrement EnvoiComplet
                        foreach (var echantillonId in model.EchantillonsSelectionnes)
                        {
                            var echantillonIdStr = echantillonId.ToString();

                            _logger.LogInformation($"Traitement échantillon {echantillonId}");

                            // Vérifier que l'échantillon existe
                            var echantillonExistant = await _context.Echantillons
                                .FirstOrDefaultAsync(e => e.Id == echantillonId && e.AffaireId == model.AffaireId);

                            if (echantillonExistant == null)
                            {
                                _logger.LogWarning($"Échantillon {echantillonId} introuvable");
                                throw new InvalidOperationException($"Échantillon {echantillonId} introuvable");
                            }

                            // CORRECTION: Extraction sécurisée des données JSON
                            var detailsJson = echantillonsDetails[echantillonIdStr].GetRawText();
                            var details = JsonSerializer.Deserialize<JsonElement>(detailsJson);

                            // Validation des propriétés requises
                            if (!details.TryGetProperty("Poids", out var poidsElement) ||
                                !poidsElement.TryGetDecimal(out var poids) ||
                                poids <= 0)
                            {
                                throw new InvalidOperationException($"Poids invalide pour l'échantillon {echantillonId}");
                            }

                            var envoiComplet = new EnvoiComplet
                            {
                                AffaireId = model.AffaireId,
                                TypeAnalyseDemandee = model.TypeAnalyseDemandee,
                                DateEnvoiPrevue = model.DateEnvoiPrevue,
                                DateEnvoiEffective = estEnvoi ? DateTime.Now : DateTime.MinValue,
                                StatutEnvoi = estEnvoi ? "Envoyé" : "Préparé",
                                ObservationsEnvoi = model.Observations ?? "",

                                // Informations de l'échantillon
                                EchantillonId = echantillonId,
                                CodeQR = qrCodes[echantillonIdStr],
                                Poids = poids,
                                ConditionsStockage = details.TryGetProperty("ConditionsStockage", out var conditionsProperty) ?
                                    conditionsProperty.GetString() ?? "" : "",
                                Couleur = details.TryGetProperty("Couleur", out var couleurProperty) ?
                                    couleurProperty.GetString() ?? "" : "",
                                Emballage = details.TryGetProperty("Emballage", out var emballageProperty) ?
                                    emballageProperty.GetString() ?? "" : "",
                                Emplacement = details.TryGetProperty("Emplacement", out var emplacementProperty) ?
                                    emplacementProperty.GetString() ?? "" : "",
                                ObservationsEchantillon = details.TryGetProperty("Observations", out var obsProperty) ?
                                    obsProperty.GetString() ?? "" : "",
                                StatutEchantillon = estEnvoi ? "Envoyé" : "Préparé",
                                DatePreparation = DateTime.Now,
                                DateReception = null,
                                DateVerification = null,
                                VerifiePar = "",
                                NotesVerification = ""
                            };

                            _context.EnvoiComplets.Add(envoiComplet);
                            envoiComptesCreés.Add(envoiComplet);

                            _logger.LogInformation($"EnvoiComplet créé pour échantillon {echantillonId}");
                        }

                        // Sauvegarder d'abord pour obtenir les IDs
                        await _context.SaveChangesAsync();

                        // Ensuite, mettre à jour les échantillons si c'est un envoi direct
                        if (estEnvoi)
                        {
                            for (int i = 0; i < model.EchantillonsSelectionnes.Count; i++)
                            {
                                var echantillonId = model.EchantillonsSelectionnes[i];
                                var echantillon = await _context.Echantillons
                                    .FirstOrDefaultAsync(e => e.Id == echantillonId);

                                if (echantillon != null)
                                {
                                    echantillon.Statut = StatutEchantillon.Envoye;
                                    echantillon.EnvoiCompletId = envoiComptesCreés[i].Id;
                                    echantillon.DejaEnvoye = true;
                                    _logger.LogInformation($"Échantillon {echantillonId} marqué comme envoyé");
                                }
                            }

                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commitée avec succès");

                        var message = estEnvoi
                            ? $"{model.EchantillonsSelectionnes.Count} échantillon(s) envoyé(s) avec succès."
                            : $"{model.EchantillonsSelectionnes.Count} échantillon(s) préparé(s) avec succès.";

                        TempData["SuccessMessage"] = message;
                        _logger.LogInformation($"Opération réussie: {message}");

                        return RedirectToAction("MesEnvois");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Erreur lors du traitement, transaction annulée");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur générale dans PreparerEnvoi");

                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Une erreur est survenue lors du traitement: {errorMessage}");

                await RechargerDonnees(model);
                return View(model);
            }
        }

        // Action pour marquer un échantillon comme reçu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarquerRecu(int envoiEchantillonId)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == envoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                if (envoiComplet.StatutEchantillon != "Envoyé")
                {
                    return Json(new { success = false, message = "L'échantillon n'est pas dans un état permettant la réception" });
                }

                envoiComplet.StatutEchantillon = "Reçu";
                envoiComplet.DateReception = DateTime.Now;

                // Mettre à jour aussi les échantillons associés si ils existent
                foreach (var echantillon in envoiComplet.Echantillons)
                {
                    echantillon.Statut = StatutEchantillon.Recu;
                    if (echantillon.DateReception == default(DateTime))
                    {
                        echantillon.DateReception = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Échantillon marqué comme reçu avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la mise à jour : " + ex.Message });
            }
        }

        // Action pour afficher la modal de vérification
        public async Task<ActionResult> VerifierEchantillon(int id)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoiComplet == null)
                {
                    ViewBag.ErrorMessage = "Échantillon introuvable";
                    return PartialView("_VerificationPartial", null);
                }

                var model = new VerificationEchantillonViewModel
                {
                    EnvoiEchantillonId = id,
                    EchantillonInfo = new EnvoiEchantillonReceptionDto
                    {
                        Id = envoiComplet.Id,
                        CodeQR = envoiComplet.CodeQR,
                        NumeroAffaire = $"AFF-{envoiComplet.Affaire.Id:D4}",
                        NomEnqueteur = envoiComplet.Affaire.NomEnqueteur,
                        DescriptionEchantillon = envoiComplet.ObservationsEchantillon ?? "Description non disponible",
                        Poids = envoiComplet.Poids,
                        Couleur = envoiComplet.Couleur,
                        ConditionsStockage = envoiComplet.ConditionsStockage,
                        Emballage = envoiComplet.Emballage
                    },
                    CriteresVerification = new List<CritereVerification>
                    {
                        new CritereVerification { Nom = "Poids", ValeurAttendue = $"{envoiComplet.Poids}g" },
                        new CritereVerification { Nom = "Couleur", ValeurAttendue = envoiComplet.Couleur ?? "Non spécifié" },
                        new CritereVerification { Nom = "Emballage", ValeurAttendue = envoiComplet.Emballage },
                        new CritereVerification { Nom = "Conditions", ValeurAttendue = envoiComplet.ConditionsStockage }
                    }
                };

                return PartialView("_VerificationEchantillon", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement : " + ex.Message;
                return PartialView("_VerificationPartial", null);
            }
        }

        // Action pour traiter la vérification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> TraiterVerification(VerificationEchantillonViewModel model)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == model.EnvoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Mettre à jour le statut de l'échantillon dans EnvoiComplet
                        envoiComplet.StatutEchantillon = model.EstAccepte ? "Conforme" : "Non-conforme";
                        envoiComplet.DateVerification = DateTime.Now;
                        envoiComplet.VerifiePar = User.Identity?.Name ?? "Système";
                        envoiComplet.NotesVerification = model.NotesVerification;

                        // Mettre à jour les échantillons associés
                        foreach (var echantillon in envoiComplet.Echantillons)
                        {
                            echantillon.Statut = model.EstAccepte ? StatutEchantillon.Verifie : StatutEchantillon.EnValidation;
                            if (string.IsNullOrEmpty(echantillon.NotesSupplementaires))
                            {
                                echantillon.NotesSupplementaires = model.NotesVerification;
                            }
                            else
                            {
                                echantillon.NotesSupplementaires += Environment.NewLine + "Vérification: " + model.NotesVerification;
                            }
                        }

                        // Sauvegarder les détails de vérification si la table existe
                        if (model.CriteresVerification != null)
                        {
                            foreach (var critere in model.CriteresVerification.Where(c => c.EstVerifie))
                            {
                                var verification = new VerificationEchantillon
                                {
                                    EnvoiEchantillonId = model.EnvoiEchantillonId,
                                    Critere = critere.Nom,
                                    ValeurAttendue = critere.ValeurAttendue,
                                    ValeurConstatee = critere.ValeurConstatee,
                                    EstConforme = critere.EstConforme,
                                    Observations = critere.Observations,
                                    DateVerification = DateTime.Now,
                                    VerifiePar = User.Identity?.Name ?? "Système"
                                };

                                _context.VerificationEchantillons.Add(verification);
                            }
                        }

                        // Si refusé, créer un rapport de non-conformité
                        if (!model.EstAccepte)
                        {
                            var numeroRapport = $"RNC-{DateTime.Now:yyyyMMdd}-{model.EnvoiEchantillonId:D4}";

                            var rapport = new RapportNonConformite
                            {
                                EnvoiEchantillonId = model.EnvoiEchantillonId,
                                NumeroRapport = numeroRapport,
                                DateRapport = DateTime.Now,
                                RedacteurRapport = User.Identity?.Name ?? "Système",
                                DescriptionNonConformite = model.CriteresVerification != null ?
                                    string.Join("; ", model.CriteresVerification
                                        .Where(c => c.EstVerifie && !c.EstConforme)
                                        .Select(c => $"{c.Nom}: attendu '{c.ValeurAttendue}', constaté '{c.ValeurConstatee}'")) :
                                    "Non-conformité détectée",
                                ActionRecommandee = model.ActionSiRefus ?? "Contact",
                                NotesDetaillees = model.NotesRefus,
                                StatutRapport = "En cours"
                            };

                            _context.RapportsNonConformite.Add(rapport);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var message = model.EstAccepte
                            ? "Échantillon accepté et marqué comme conforme."
                            : "Échantillon refusé et rapport de non-conformité généré.";

                        return Json(new { success = true, message = message, redirect = Url.Action("Reception") });
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
                return Json(new { success = false, message = "Erreur lors du traitement de la vérification : " + ex.Message });
            }
        }

        // Action pour obtenir les détails d'un échantillon en JSON
        public async Task<JsonResult> DetailsEchantillon(int id)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoiComplet == null)
                {
                    return Json(new { error = "Échantillon introuvable" });
                }

                var details = new
                {
                    Id = envoiComplet.Id,
                    CodeQR = envoiComplet.CodeQR,
                    NumeroAffaire = $"AFF-{envoiComplet.Affaire.Id:D4}",
                    NomEnqueteur = envoiComplet.Affaire.NomEnqueteur,
                    DateEnvoi = envoiComplet.DateEnvoiEffective,
                    DateReception = envoiComplet.DateReception,
                    DescriptionEchantillon = envoiComplet.ObservationsEchantillon ?? "Description non disponible",
                    TypeAnalyse = envoiComplet.TypeAnalyseDemandee,
                    Poids = envoiComplet.Poids,
                    Couleur = envoiComplet.Couleur,
                    ConditionsStockage = envoiComplet.ConditionsStockage,
                    Emballage = envoiComplet.Emballage,
                    Emplacement = envoiComplet.Emplacement,
                    StatutEchantillon = envoiComplet.StatutEchantillon,
                    Observations = envoiComplet.ObservationsEchantillon,
                    NotesVerification = envoiComplet.NotesVerification,
                    NombreEchantillons = envoiComplet.Echantillons.Count,
                    EchantillonsDetails = envoiComplet.Echantillons.Select(e => new
                    {
                        Id = e.Id,
                        NumeroEchantillon = e.NumeroEchantillon,
                        Type = e.Type.ToString(),
                        Statut = e.Statut.ToString(),
                        Priorite = e.Priorite.ToString(),
                        Description = e.Description,
                        LieuCollecte = e.LieuCollecte,
                        ResponsableCollecte = e.ResponsableCollecte
                    }).ToList()
                };

                return Json(details);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Erreur lors du chargement : " + ex.Message });
            }
        }

        // Action pour procéder au stockage d'un échantillon vérifié
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcederAuStockage(int envoiEchantillonId, string notesStockage = null)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == envoiEchantillonId);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Échantillon non trouvé" });
                }

                if (envoiComplet.StatutEchantillon != "Conforme" && envoiComplet.StatutEchantillon != "Vérifié")
                {
                    return Json(new { success = false, message = "L'échantillon doit être conforme avant le stockage." });
                }

                envoiComplet.StatutEchantillon = "Stocké";
                if (!string.IsNullOrEmpty(notesStockage))
                {
                    envoiComplet.ObservationsEchantillon += $" | Stockage: {notesStockage}";
                }

                // Mettre à jour les échantillons associés
                foreach (var echantillon in envoiComplet.Echantillons)
                {
                    echantillon.Statut = StatutEchantillon.Stocke;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Échantillon transféré au stockage avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors du transfert au stockage : " + ex.Message });
            }
        }

        // Action pour rechercher un échantillon par QR Code
        [HttpPost]
        public async Task<JsonResult> RechercherParQR(string qrCode)
        {
            try
            {
                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .FirstOrDefaultAsync(ec => ec.CodeQR == qrCode);

                if (envoiComplet == null)
                {
                    return Json(new { success = false, message = "Aucun échantillon trouvé avec ce QR Code." });
                }

                var result = new EnvoiEchantillonReceptionDto
                {
                    Id = envoiComplet.Id,
                    CodeQR = envoiComplet.CodeQR,
                    NumeroAffaire = $"AFF-{envoiComplet.Affaire.Id:D4}",
                    NomEnqueteur = envoiComplet.Affaire.NomEnqueteur,
                    DescriptionEchantillon = envoiComplet.ObservationsEchantillon ?? "Description non disponible",
                    StatutEchantillon = envoiComplet.StatutEchantillon,
                    DateEnvoi = envoiComplet.DateEnvoiEffective,
                    DateReception = envoiComplet.DateReception
                };

                return Json(new { success = true, echantillon = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la recherche." });
            }
        }

        // Action pour obtenir les statistiques de réception
        public async Task<JsonResult> StatistiquesReception()
        {
            try
            {
                var stats = new
                {
                    EnAttente = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Reçu" || ec.StatutEchantillon == "Envoyé"),
                    Verifies = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Vérifié" || ec.StatutEchantillon == "Conforme"),
                    Refuses = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Non-conforme"),
                    Stockes = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Stocké"),
                    Total = await _context.EnvoiComplets.CountAsync()
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Erreur lors du calcul des statistiques" });
            }
        }

        // Méthode utilitaire pour recharger les données du formulaire
        private async Task RechargerDonnees(PreparationEnvoiViewModel model)
        {
            ViewBag.Affaires = (await _context.Affaires
                .Select(a => new
                {
                    Id = a.Id,
                    Titre = a.Titre
                })
                .ToListAsync())
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"AFF-{a.Id:D4} - {a.Titre}"
                })
                .ToList();

            ViewBag.TypesAnalyse = new[]
            {
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Toxicologique", Text = "Analyse Toxicologique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Chimique", Text = "Analyse Chimique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Biologique", Text = "Analyse Biologique" },
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "Physique", Text = "Analyse Physique" }
            };

            if (model.AffaireId > 0)
            {
                var echantillonsData = await _context.Echantillons
                    .Where(e => e.AffaireId == model.AffaireId)
                    .Select(e => new
                    {
                        Id = e.Id,
                        Code = e.NumeroEchantillon,
                        Description = e.Description,
                        Type = e.Type,
                        DejaEnvoye = _context.EnvoiComplets
                            .Any(ec => ec.EchantillonId == e.Id && ec.StatutEchantillon != "Refusé")
                    })
                    .ToListAsync();

                model.EchantillonsDisponibles = echantillonsData
                    .Select(e => new EchantillonDisponibleModel
                    {
                        Id = e.Id,
                        Code = e.Code,
                        Description = e.Description,
                        Type = e.Type.ToString(),
                        DejaEnvoye = e.DejaEnvoye
                    })
                    .ToList();
            }
        }

        // Action pour lister tous les envois
        public async Task<ActionResult> MesEnvois()
        {
            try
            {
                var envois = await _context.EnvoiComplets
                    .Include(e => e.Echantillons)
                    .Include(e => e.Affaire)
                    .OrderByDescending(e => e.DateEnvoiEffective)
                    .ToListAsync();

                return View(envois);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement des envois : " + ex.Message;
                return View(new List<EnvoiComplet>());
            }
        }

        // Action pour voir les détails d'un envoi
        public async Task<ActionResult> DetailsEnvoi(int id)
        {
            try
            {
                var envoi = await _context.EnvoiComplets
                    .Include(e => e.Affaire)
                    .Include(e => e.Echantillons)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (envoi == null)
                {
                    return NotFound();
                }

                return View(envoi);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement des détails : " + ex.Message;
                return View();
            }
        }

        // Action pour voir le rapport de refus
        public async Task<ActionResult> VoirRapportRefus(int envoiEchantillonId)
        {
            try
            {
                var rapport = await _context.RapportsNonConformite
                    .FirstOrDefaultAsync(r => r.EnvoiEchantillonId == envoiEchantillonId);

                if (rapport == null)
                {
                    return Json(new { success = false, message = "Aucun rapport trouvé" });
                }

                var details = new
                {
                    NumeroRapport = rapport.NumeroRapport,
                    DateRapport = rapport.DateRapport,
                    RedacteurRapport = rapport.RedacteurRapport,
                    DescriptionNonConformite = rapport.DescriptionNonConformite,
                    ActionRecommandee = rapport.ActionRecommandee,
                    NotesDetaillees = rapport.NotesDetaillees,
                    StatutRapport = rapport.StatutRapport
                };

                return Json(details);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors du chargement du rapport" });
            }
        }

        // Action pour télécharger un rapport de non-conformité
        public async Task<ActionResult> TelechargerRapportNonConformite(int id)
        {
            try
            {
                var rapport = await _context.RapportsNonConformite
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rapport == null)
                {
                    return NotFound();
                }

                var envoiComplet = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .FirstOrDefaultAsync(ec => ec.Id == rapport.EnvoiEchantillonId);

                // Générer le contenu HTML du rapport
                var html = $@"
                <html>
                <head>
                    <title>Rapport de Non-Conformité {rapport.NumeroRapport}</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .section {{ margin-bottom: 20px; }}
                        .label {{ font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>RAPPORT DE NON-CONFORMITÉ</h1>
                        <h2>{rapport.NumeroRapport}</h2>
                    </div>
                    <div class='content'>
                        <div class='section'>
                            <div class='label'>Date du Rapport:</div>
                            <div>{rapport.DateRapport:dd/MM/yyyy HH:mm}</div>
                        </div>
                        <div class='section'>
                            <div class='label'>Rédacteur:</div>
                            <div>{rapport.RedacteurRapport}</div>
                        </div>";

                if (envoiComplet != null)
                {
                    html += $@"
                        <div class='section'>
                            <div class='label'>Affaire:</div>
                            <div>AFF-{envoiComplet.Affaire.Id:D4} - {envoiComplet.Affaire.Titre}</div>
                        </div>
                        <div class='section'>
                            <div class='label'>Échantillon:</div>
                            <div>{envoiComplet.ObservationsEchantillon} (QR: {envoiComplet.CodeQR})</div>
                        </div>";
                }

                html += $@"
                        <div class='section'>
                            <div class='label'>Description de la Non-Conformité:</div>
                            <div>{rapport.DescriptionNonConformite}</div>
                        </div>
                        <div class='section'>
                            <div class='label'>Action Recommandée:</div>
                            <div>{rapport.ActionRecommandee}</div>
                        </div>
                        <div class='section'>
                            <div class='label'>Notes Détaillées:</div>
                            <div>{rapport.NotesDetaillees}</div>
                        </div>
                    </div>
                </body>
                </html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"Erreur lors de la génération du rapport : {ex.Message}", "text/plain");
            }
        }

        // Action pour afficher l'historique des vérifications
        public async Task<ActionResult> HistoriqueVerifications(int envoiEchantillonId)
        {
            try
            {
                var verifications = await _context.VerificationEchantillons
                    .Where(v => v.EnvoiEchantillonId == envoiEchantillonId)
                    .OrderByDescending(v => v.DateVerification)
                    .ToListAsync();

                return PartialView("_HistoriqueVerifications", verifications);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement de l'historique : " + ex.Message;
                return PartialView("_HistoriqueVerifications", new List<VerificationEchantillon>());
            }
        }

        // Action pour actualiser les données de réception
        public async Task<JsonResult> ActualiserReceptions()
        {
            try
            {
                var stats = new
                {
                    EnAttente = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Reçu" || ec.StatutEchantillon == "Envoyé"),
                    Verifies = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Vérifié" || ec.StatutEchantillon == "Conforme"),
                    Refuses = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Non-conforme"),
                    Stockes = await _context.EnvoiComplets.CountAsync(ec => ec.StatutEchantillon == "Stocké")
                };

                return Json(new { success = true, statistiques = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de l'actualisation" });
            }
        }

        // Action pour scanner un QR code (placeholder pour intégration future)
        public ActionResult ScannerQR(string codeQR)
        {
            // Cette méthode peut être étendue pour intégrer une vraie fonctionnalité de scan
            // Pour l'instant, elle redirige vers la recherche
            return RedirectToAction("Reception", new { filtres = new FiltresReception { Recherche = codeQR } });
        }

        // Action pour exporter les données de réception
        public async Task<ActionResult> ExporterReceptions(FiltresReception filtres = null)
        {
            try
            {
                // Construction de la requête similaire à Reception
                var query = _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons);

                // Appliquer les mêmes filtres que Reception
                if (filtres != null)
                {
                    if (!string.IsNullOrEmpty(filtres.Recherche))
                    {
                        query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.Affaire.Titre.Contains(filtres.Recherche) ||
                                                 ec.CodeQR.Contains(filtres.Recherche));
                    }

                    if (!string.IsNullOrEmpty(filtres.Statut))
                    {
                        switch (filtres.Statut)
                        {
                            case "pending":
                                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.StatutEchantillon == "Reçu" || ec.StatutEchantillon == "Envoyé");
                                break;
                            case "verified":
                                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.StatutEchantillon == "Vérifié" || ec.StatutEchantillon == "Conforme");
                                break;
                            case "rejected":
                                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.StatutEchantillon == "Non-conforme");
                                break;
                            case "stored":
                                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.StatutEchantillon == "Stocké");
                                break;
                        }
                    }

                    if (filtres.DateDebut.HasValue)
                    {
                        query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.DateEnvoiEffective >= filtres.DateDebut.Value);
                    }

                    if (filtres.DateFin.HasValue)
                    {
                        query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<EnvoiComplet, ICollection<Echantillon>>)query.Where(ec => ec.DateEnvoiEffective <= filtres.DateFin.Value);
                    }
                }

                var donnees = await query
                    .OrderByDescending(ec => ec.DateEnvoiEffective)
                    .Select(ec => new
                    {
                        CodeQR = ec.CodeQR,
                        NumeroAffaire = "AFF-" + ec.Affaire.Id.ToString("D4"),
                        NomEnqueteur = ec.Affaire.NomEnqueteur ?? "Non spécifié",
                        TypeAnalyse = ec.TypeAnalyseDemandee,
                        Poids = ec.Poids,
                        Couleur = ec.Couleur,
                        Emballage = ec.Emballage,
                        StatutEchantillon = ec.StatutEchantillon,
                        DateEnvoi = ec.DateEnvoiEffective,
                        DateReception = ec.DateReception,
                        DateVerification = ec.DateVerification,
                        VerifiePar = ec.VerifiePar
                    })
                    .ToListAsync();

                // Générer un CSV simple
                var csv = "CodeQR,NumeroAffaire,NomEnqueteur,TypeAnalyse,Poids,Couleur,Emballage,StatutEchantillon,DateEnvoi,DateReception,DateVerification,VerifiePar\n";

                foreach (var item in donnees)
                {
                    csv += $"{item.CodeQR},{item.NumeroAffaire},{item.NomEnqueteur},{item.TypeAnalyse},{item.Poids},{item.Couleur},{item.Emballage},{item.StatutEchantillon},{item.DateEnvoi:yyyy-MM-dd},{item.DateReception:yyyy-MM-dd},{item.DateVerification:yyyy-MM-dd},{item.VerifiePar}\n";
                }

                var fileName = $"Receptions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors de l'export : " + ex.Message;
                return RedirectToAction("Reception");
            }
        }

        // Action pour afficher l'historique complet
        public async Task<ActionResult> Historique()
        {
            try
            {
                var historique = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons)
                    .OrderByDescending(ec => ec.DateEnvoiEffective)
                    .Take(100) // Limiter aux 100 derniers pour les performances
                    .ToListAsync();

                return View(historique);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erreur lors du chargement de l'historique : " + ex.Message;
                return View(new List<EnvoiComplet>());
            }
        }

        // Action pour supprimer un envoi (si autorisé)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SupprimerEnvoi(int id)
        {
            try
            {
                var envoi = await _context.EnvoiComplets
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoi == null)
                {
                    return Json(new { success = false, message = "Envoi introuvable" });
                }

                // Vérifier si l'envoi peut être supprimé (par exemple, seulement si statut = "Préparé")
                if (envoi.StatutEnvoi != "Préparé")
                {
                    return Json(new { success = false, message = "Seuls les envois en préparation peuvent être supprimés" });
                }

                // Remettre à jour les échantillons associés
                foreach (var echantillon in envoi.Echantillons)
                {
                    echantillon.Statut = StatutEchantillon.EnAttente;
                    echantillon.EnvoiCompletId = null;
                }

                _context.EnvoiComplets.Remove(envoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Envoi supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la suppression : " + ex.Message });
            }
        }

        // Action pour modifier un envoi en préparation
        public async Task<ActionResult> ModifierEnvoi(int id)
        {
            try
            {
                var envoi = await _context.EnvoiComplets
                    .Include(ec => ec.Affaire)
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoi == null)
                {
                    return NotFound();
                }

                if (envoi.StatutEnvoi != "Préparé")
                {
                    TempData["ErrorMessage"] = "Seuls les envois en préparation peuvent être modifiés.";
                    return RedirectToAction("MesEnvois");
                }

                // Créer le modèle pour la modification
                var model = new PreparationEnvoiViewModel
                {
                    AffaireId = envoi.AffaireId,
                    TypeAnalyseDemandee = envoi.TypeAnalyseDemandee,
                    DateEnvoiPrevue = envoi.DateEnvoiPrevue,
                    Observations = envoi.ObservationsEnvoi,
                    // Ajouter les échantillons sélectionnés, etc.
                };

                // Recharger les listes déroulantes
                await RechargerDonnees(model);

                return View("PreparerEnvoi", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors du chargement : " + ex.Message;
                return RedirectToAction("MesEnvois");
            }
        }

        // Action pour confirmer un envoi préparé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConfirmerEnvoi(int id)
        {
            try
            {
                var envoi = await _context.EnvoiComplets
                    .Include(ec => ec.Echantillons)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (envoi == null)
                {
                    return Json(new { success = false, message = "Envoi introuvable" });
                }

                if (envoi.StatutEnvoi != "Préparé")
                {
                    return Json(new { success = false, message = "Seuls les envois préparés peuvent être confirmés" });
                }

                envoi.StatutEnvoi = "Envoyé";
                envoi.StatutEchantillon = "Envoyé";
                envoi.DateEnvoiEffective = DateTime.Now;

                // Mettre à jour les échantillons associés
                foreach (var echantillon in envoi.Echantillons)
                {
                    echantillon.Statut = StatutEchantillon.Envoye;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Envoi confirmé avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la confirmation : " + ex.Message });
            }
        }
    }
}