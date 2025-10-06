using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models.Emploi;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using WebApplication1.Data;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class EmploiTempsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public EmploiTempsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
            _context = db;
        }

        // GET: Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentYear = DateTime.Now.Year;
            var currentWeek = GetWeekOfYear(DateTime.Now);

            // Statistiques pour le dashboard
            ViewBag.TotalTechnicians = await _context.Personnels
                .Where(p => p.TypePersonnel == "Technician" && p.Actif)
                .CountAsync();

            ViewBag.TotalAnalysts = await _context.Personnels
                .Where(p => p.TypePersonnel == "Analyst" && p.Actif)
                .CountAsync();

            ViewBag.TotalEvidence = 245;
            ViewBag.PendingAnalysis = 23;
            ViewBag.CompletedAnalysis = 187;
            ViewBag.UrgentAnalysis = 5;

            // Statistiques emplois du temps
            ViewBag.DraftSchedules = await _context.EmploisTemps
                .Where(et => et.Statut == "Draft")
                .CountAsync();

            ViewBag.PendingApprovalSchedules = await _context.EmploisTemps
                .Where(et => et.Statut == "Pending")
                .CountAsync();

            ViewBag.ApprovedSchedules = await _context.EmploisTemps
                .Where(et => et.Statut == "Approved")
                .CountAsync();

            ViewBag.ScheduleModifications = await _context.EmploisTemps
                .Where(et => et.DateModification.HasValue &&
                            et.DateModification.Value >= DateTime.Now.AddDays(-7))
                .CountAsync();

            // Statut de la semaine courante
            var currentSchedule = await _context.EmploisTemps
                .FirstOrDefaultAsync(et => et.NumeroSemaine == currentWeek && et.Annee == currentYear);

            ViewBag.WeeklyScheduleStatus = currentSchedule?.StatutLibelle ?? "En attente de création";

            return View();
        }

        // GET: Historique avec filtres
        public async Task<IActionResult> HistoriqueEmploisTemps(int? year, string status, string creator,
            DateTime? dateFrom, DateTime? dateTo, int? coverageMin, int? coverageMax,
            int? weekFrom, int? weekTo)
        {
            var currentYear = year ?? DateTime.Now.Year;

            // Statistiques générales pour l'année
            var totalSchedules = await _context.EmploisTemps
                .Where(et => et.Annee == currentYear)
                .CountAsync();

            var approvedSchedules = await _context.EmploisTemps
                .Where(et => et.Annee == currentYear && et.Statut == "Approved")
                .CountAsync();

            var archivedSchedules = await _context.EmploisTemps
                .Where(et => et.Annee == currentYear && et.Statut == "Archived")
                .CountAsync();

            ViewBag.CurrentYear = currentYear;
            ViewBag.TotalSchedules = totalSchedules;
            ViewBag.ApprovedSchedules = approvedSchedules;
            ViewBag.ArchivedSchedules = archivedSchedules;

            // Construction de la requête avec filtres
            var query = _context.EmploisTemps
                .Include(et => et.Affectations).ThenInclude(a => a.Personnel)
                .Include(et => et.Affectations).ThenInclude(a => a.Service)
                .AsQueryable();

            // Filtrer par année si spécifiée
            if (year.HasValue)
            {
                query = query.Where(et => et.Annee == year.Value);
            }

            // Appliquer les autres filtres
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(et => et.Statut == status);
            }

            if (!string.IsNullOrEmpty(creator))
            {
                query = query.Where(et => et.CreePar.Contains(creator));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(et => et.DateCreation >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(et => et.DateCreation <= dateTo.Value.AddDays(1));
            }

            if (weekFrom.HasValue)
            {
                query = query.Where(et => et.NumeroSemaine >= weekFrom.Value);
            }

            if (weekTo.HasValue)
            {
                query = query.Where(et => et.NumeroSemaine <= weekTo.Value);
            }

            var emploisTemps = await query
                .OrderByDescending(et => et.NumeroSemaine)
                .ToListAsync();

            // Filtrer par taux de couverture (après récupération car c'est une propriété calculée)
            if (coverageMin.HasValue || coverageMax.HasValue)
            {
                emploisTemps = emploisTemps.Where(et =>
                {
                    var coverage = et.TauxCouverture;
                    return (!coverageMin.HasValue || coverage >= coverageMin.Value) &&
                           (!coverageMax.HasValue || coverage <= coverageMax.Value);
                }).ToList();
            }

            return View(emploisTemps);
        }

        // GET: Voir emploi du temps
        public async Task<IActionResult> VoirEmploiTempsSemaine(int? week, int? year)
        {
            try
            {
                var targetWeek = week ?? GetWeekOfYear(DateTime.Now);
                var targetYear = year ?? DateTime.Now.Year;

                // Toujours définir ces ViewBag même si le modèle est null
                ViewBag.WeekNumber = targetWeek;
                ViewBag.Year = targetYear;

                var emploiTemps = await _context.EmploisTemps
                    .Include(et => et.Affectations).ThenInclude(a => a.Personnel)
                    .Include(et => et.Affectations).ThenInclude(a => a.Service)
                    .Include(et => et.AbsencesConges).ThenInclude(ac => ac.Personnel)
                    .FirstOrDefaultAsync(et => et.NumeroSemaine == targetWeek && et.Annee == targetYear);

                if (emploiTemps == null)
                {
                    ViewBag.ErrorMessage = $"Aucun emploi du temps trouvé pour la semaine {targetWeek} de {targetYear}";

                    // Créer un modèle vide pour éviter les erreurs de référence null
                    var modelVide = new EmploiTemps
                    {
                        NumeroSemaine = targetWeek,
                        Annee = targetYear,
                        DateDebut = GetDateDebutSemaine(targetWeek, targetYear),
                        Statut = "NotFound",
                        Affectations = new List<AffectationPersonnel>(),
                        AbsencesConges = new List<AbsenceConge>()
                    };

                    modelVide.DateFin = modelVide.DateDebut.AddDays(4);

                    return View(modelVide);
                }

                // Définir les ViewBag avec les données de l'emploi du temps trouvé
                ViewBag.ScheduleStatus = emploiTemps.StatutLibelle;
                ViewBag.LastModified = emploiTemps.DateModification ?? emploiTemps.DateCreation;
                ViewBag.CreatedBy = emploiTemps.CreePar;
                ViewBag.ApprovedBy = emploiTemps.ApprouvePar;

                return View(emploiTemps);
            }
            catch (Exception ex)
            {
                // Log l'erreur
                System.Diagnostics.Debug.WriteLine($"Erreur dans VoirEmploiTempsSemaine: {ex.Message}");

                // Retourner une vue d'erreur avec un modèle minimal
                var targetWeek = week ?? GetWeekOfYear(DateTime.Now);
                var targetYear = year ?? DateTime.Now.Year;

                ViewBag.WeekNumber = targetWeek;
                ViewBag.Year = targetYear;
                ViewBag.ErrorMessage = $"Erreur lors du chargement de l'emploi du temps: {ex.Message}";

                var modelErreur = new EmploiTemps
                {
                    NumeroSemaine = targetWeek,
                    Annee = targetYear,
                    DateDebut = GetDateDebutSemaine(targetWeek, targetYear),
                    Statut = "Error",
                    Affectations = new List<AffectationPersonnel>(),
                    AbsencesConges = new List<AbsenceConge>()
                };

                modelErreur.DateFin = modelErreur.DateDebut.AddDays(4);

                return View(modelErreur);
            }
        }

        // GET: Créer emploi du temps
        public async Task<IActionResult> CreerEmploiTemps(int? week, int? year)
        {
            try
            {
                var targetWeek = week ?? GetWeekOfYear(DateTime.Now) + 1;
                var targetYear = year ?? DateTime.Now.Year;

                // Vérifier si un emploi du temps existe déjà
                var existingSchedule = await _context.EmploisTemps
                    .FirstOrDefaultAsync(et => et.NumeroSemaine == targetWeek && et.Annee == targetYear);

                if (existingSchedule != null)
                {
                    return RedirectToAction("ModifierEmploiTemps", new { id = existingSchedule.Id });
                }

                // Charger les données nécessaires
                await RechargerDonnees();

                ViewBag.TargetWeek = targetWeek;
                ViewBag.TargetYear = targetYear;

                // Créer un modèle vide avec les valeurs par défaut
                var model = new EmploiTemps
                {
                    NumeroSemaine = targetWeek,
                    Annee = targetYear,
                    Statut = "Draft"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Erreur lors du chargement : {ex.Message}";

                // Retourner un modèle minimal en cas d'erreur
                var model = new EmploiTemps
                {
                    NumeroSemaine = week ?? 1,
                    Annee = year ?? DateTime.Now.Year,
                    Statut = "Draft"
                };

                return View(model);
            }
        }

        // Corrections principales dans la méthode CreerEmploiTemps POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerEmploiTemps(EmploiTemps emploiTemps, string action)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== DEBUT CreerEmploiTemps POST ===");
                    System.Diagnostics.Debug.WriteLine($"Action reçue: {action}");
                    System.Diagnostics.Debug.WriteLine($"Semaine: {emploiTemps.NumeroSemaine}, Année: {emploiTemps.Annee}");

                    // Vérifier l'unicité AVANT tout traitement
                    var existingSchedule = await _context.EmploisTemps
                        .FirstOrDefaultAsync(et => et.NumeroSemaine == emploiTemps.NumeroSemaine && et.Annee == emploiTemps.Annee);

                    if (existingSchedule != null)
                    {
                        ModelState.AddModelError("", $"Un emploi du temps existe déjà pour la semaine {emploiTemps.NumeroSemaine} de {emploiTemps.Annee}");
                        await RechargerDonnees();
                        ViewBag.TargetWeek = emploiTemps.NumeroSemaine;
                        ViewBag.TargetYear = emploiTemps.Annee;
                        return View(emploiTemps);
                    }

                    // PROBLÈME PRINCIPAL : ExtraireAffectationsFormulaire ne fonctionne pas correctement
                    // Utilisons directement le Request.Form avec le bon pattern

                    var affectationsData = new List<AffectationData>();

                    // Debug complet du formulaire
                    System.Diagnostics.Debug.WriteLine("=== ANALYSE COMPLETE DU FORMULAIRE ===");
                    foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("Affectations")))
                    {
                        var value = Request.Form[key].ToString();
                        System.Diagnostics.Debug.WriteLine($"Clé: {key} = '{value}'");

                        // Pattern correct : Affectations[serviceId][dayName][shiftName]
                        var match = System.Text.RegularExpressions.Regex.Match(key, @"Affectations\[(\d+)\]\[(\w+)\]\[(\w+)\]");
                        if (match.Success && !string.IsNullOrEmpty(value))
                        {
                            int serviceId = int.Parse(match.Groups[1].Value);
                            string day = match.Groups[2].Value;
                            string shift = match.Groups[3].Value;

                            System.Diagnostics.Debug.WriteLine($"Extraction réussie - Service: {serviceId}, Jour: {day}, Équipe: {shift}, Valeur: '{value}'");

                            var personnelIds = value.Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => int.Parse(x.Trim()))
                                .ToList();

                            foreach (var personnelId in personnelIds)
                            {
                                affectationsData.Add(new AffectationData
                                {
                                    ServiceId = serviceId,
                                    PersonnelId = personnelId,
                                    Jour = day,
                                    Equipe = shift
                                });
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Total affectations extraites: {affectationsData.Count}");

                    if (!affectationsData.Any())
                    {
                        ModelState.AddModelError("", "Aucune affectation de personnel n'a été définie. Veuillez assigner du personnel avant de sauvegarder.");
                        await RechargerDonnees();
                        ViewBag.TargetWeek = emploiTemps.NumeroSemaine;
                        ViewBag.TargetYear = emploiTemps.Annee;
                        return View(emploiTemps);
                    }

                    // Calculer les dates et initialiser l'objet
                    emploiTemps.DateDebut = GetDateDebutSemaine(emploiTemps.NumeroSemaine, emploiTemps.Annee);
                    emploiTemps.DateFin = emploiTemps.DateDebut.AddDays(4);
                    emploiTemps.DateCreation = DateTime.Now;
                    emploiTemps.CreePar = User.Identity.Name ?? "Administrateur";

                    // Définir le statut selon l'action
                    switch (action?.ToLower())
                    {
                        case "draft":
                            emploiTemps.Statut = "Draft";
                            break;
                        case "pending":
                            emploiTemps.Statut = "Pending";
                            break;
                        case "approved":
                        case "approve":
                            emploiTemps.Statut = "Approved";
                            emploiTemps.DateApprobation = DateTime.Now;
                            emploiTemps.ApprouvePar = User.Identity.Name ?? "Directeur";
                            break;
                        default:
                            emploiTemps.Statut = "Draft";
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Statut défini: {emploiTemps.Statut}");

                    // ÉTAPE 1: Sauvegarder l'emploi du temps
                    _context.EmploisTemps.Add(emploiTemps);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ EmploiTemps créé avec ID: {emploiTemps.Id}");

                    // ÉTAPE 2: Créer et sauvegarder les affectations directement
                    var affectations = new List<AffectationPersonnel>();

                    foreach (var data in affectationsData)
                    {
                        // Vérifier que le personnel existe et est actif
                        var personnel = await _context.Personnels
                            .FirstOrDefaultAsync(p => p.Id == data.PersonnelId && p.ServiceId == data.ServiceId && p.Actif);

                        if (personnel == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Personnel {data.PersonnelId} introuvable ou inactif pour service {data.ServiceId}");
                            continue;
                        }

                        // Définir les heures selon l'équipe
                        TimeSpan heureDebut, heureFin;
                        if (data.Equipe == "Morning")
                        {
                            heureDebut = new TimeSpan(8, 0, 0);
                            heureFin = new TimeSpan(16, 0, 0);
                        }
                        else if (data.Equipe == "Evening")
                        {
                            heureDebut = new TimeSpan(16, 0, 0);
                            heureFin = new TimeSpan(24, 0, 0);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Équipe '{data.Equipe}' non reconnue");
                            continue;
                        }

                        var affectation = new AffectationPersonnel
                        {
                            EmploiTempsId = emploiTemps.Id,
                            PersonnelId = data.PersonnelId,
                            ServiceId = data.ServiceId,
                            Jour = data.Jour,
                            Equipe = data.Equipe,
                            HeureDebut = heureDebut,
                            HeureFin = heureFin,
                            DateCreation = DateTime.Now
                        };

                        affectations.Add(affectation);
                        System.Diagnostics.Debug.WriteLine($"✓ Affectation créée: Personnel {data.PersonnelId}, {data.Jour} {data.Equipe}");
                    }

                    if (affectations.Any())
                    {
                        _context.AffectationsPersonnel.AddRange(affectations);
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ {affectations.Count} affectations sauvegardées");
                    }

                    // Valider la transaction
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Emploi du temps créé avec succès pour la semaine {emploiTemps.NumeroSemaine}/{emploiTemps.Annee} avec {affectations.Count} affectations (Statut: {emploiTemps.StatutLibelle})";
                    return RedirectToAction("VoirEmploiTempsSemaine", new { week = emploiTemps.NumeroSemaine, year = emploiTemps.Annee });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Exception critique dans CreerEmploiTemps: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                    ModelState.AddModelError("", $"Erreur lors de la création : {ex.Message}");
                }
            }

            // En cas d'erreur, recharger les données et retourner la vue
            await RechargerDonnees();
            ViewBag.TargetWeek = emploiTemps.NumeroSemaine;
            ViewBag.TargetYear = emploiTemps.Annee;
            return View(emploiTemps);
        }

        // Classe helper pour les données d'affectation (à garder)
        public class AffectationData
        {
            public int ServiceId { get; set; }
            public int PersonnelId { get; set; }
            public string Jour { get; set; }
            public string Equipe { get; set; }
        }
        // GET: Modifier emploi du temps
        public async Task<IActionResult> ModifierEmploiTemps(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var emploiTemps = await _context.EmploisTemps
                .Include(et => et.Affectations).ThenInclude(a => a.Personnel)
                .Include(et => et.Affectations).ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(et => et.Id == id);

            if (emploiTemps == null)
            {
                return NotFound();
            }

            // Vérifier les permissions (optionnel)
            if (emploiTemps.Statut == "Approved" && !User.IsInRole("Directeur"))
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les droits pour modifier un emploi du temps approuvé.";
                return RedirectToAction("VoirEmploiTempsSemaine", new { week = emploiTemps.NumeroSemaine, year = emploiTemps.Annee });
            }

            await RechargerDonnees();

            // Préparer les données pour la vue (affectations existantes)
            ViewBag.AffectationsJson = JsonConvert.SerializeObject(
                emploiTemps.Affectations.GroupBy(a => new { a.ServiceId, a.Jour, a.Equipe })
                .ToDictionary(
                    g => $"{g.Key.ServiceId}-{g.Key.Jour}-{g.Key.Equipe}",
                    g => g.Select(a => a.PersonnelId).ToArray()
                )
            );

            return View(emploiTemps);
        }

        // POST: Modifier emploi du temps
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierEmploiTemps(int id, EmploiTemps emploiTemps, string action)
        {
            if (id != emploiTemps.Id)
            {
                return BadRequest();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var emploiTempsExistant = await _context.EmploisTemps
                        .Include(et => et.Affectations)
                        .FirstOrDefaultAsync(et => et.Id == id);

                    if (emploiTempsExistant == null)
                    {
                        return NotFound();
                    }

                    // Mettre à jour les propriétés
                    emploiTempsExistant.Commentaires = emploiTemps.Commentaires;
                    emploiTempsExistant.DateModification = DateTime.Now;
                    emploiTempsExistant.ModifiePar = User.Identity.Name ?? "Administrateur";

                    // Gérer le changement de statut
                    if (!string.IsNullOrEmpty(action))
                    {
                        switch (action)
                        {
                            case "draft":
                                emploiTempsExistant.Statut = "Draft";
                                break;
                            case "pending":
                                emploiTempsExistant.Statut = "Pending";
                                break;
                            case "approved":
                                emploiTempsExistant.Statut = "Approved";
                                emploiTempsExistant.DateApprobation = DateTime.Now;
                                emploiTempsExistant.ApprouvePar = User.Identity.Name ?? "Directeur";
                                break;
                        }
                    }

                    // Supprimer les anciennes affectations
                    var anciennesAffectations = _context.AffectationsPersonnel
                        .Where(a => a.EmploiTempsId == id);
                    _context.AffectationsPersonnel.RemoveRange(anciennesAffectations);

                    await _context.SaveChangesAsync();

                    // Ajouter les nouvelles affectations
                    await TraiterAffectationsFormulaire(id);

                    TempData["SuccessMessage"] = "Emploi du temps modifié avec succès";
                    return RedirectToAction("VoirEmploiTempsSemaine", new { week = emploiTempsExistant.NumeroSemaine, year = emploiTempsExistant.Annee });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur lors de la modification : {ex.Message}");
            }

            await RechargerDonnees();
            return View(emploiTemps);
        }

        // POST: Dupliquer emploi du temps
        [HttpPost]
        public async Task<IActionResult> DupliquerEmploiTemps(int sourceId, int targetWeek, int targetYear)
        {
            try
            {
                var source = await _context.EmploisTemps
                    .Include(et => et.Affectations)
                    .FirstOrDefaultAsync(et => et.Id == sourceId);

                if (source == null)
                {
                    return Json(new { success = false, message = "Emploi du temps source introuvable" });
                }

                // Vérifier si la cible existe déjà
                var existing = await _context.EmploisTemps
                    .FirstOrDefaultAsync(et => et.NumeroSemaine == targetWeek && et.Annee == targetYear);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Un emploi du temps existe déjà pour cette semaine" });
                }

                // Créer le nouvel emploi du temps
                var nouveau = new EmploiTemps
                {
                    NumeroSemaine = targetWeek,
                    Annee = targetYear,
                    DateDebut = GetDateDebutSemaine(targetWeek, targetYear),
                    DateCreation = DateTime.Now,
                    Commentaires = $"Dupliqué depuis la semaine {source.NumeroSemaine}/{source.Annee}",
                    CreePar = User.Identity.Name ?? "Administrateur",
                    Statut = "Draft"
                };
                nouveau.DateFin = nouveau.DateDebut.AddDays(4);

                _context.EmploisTemps.Add(nouveau);
                await _context.SaveChangesAsync();

                // Dupliquer les affectations
                foreach (var affectation in source.Affectations)
                {
                    var nouvelleAffectation = new AffectationPersonnel
                    {
                        EmploiTempsId = nouveau.Id,
                        PersonnelId = affectation.PersonnelId,
                        ServiceId = affectation.ServiceId,
                        Jour = affectation.Jour,
                        Equipe = affectation.Equipe,
                        HeureDebut = affectation.HeureDebut,
                        HeureFin = affectation.HeureFin,
                        DateCreation = DateTime.Now
                    };
                    _context.AffectationsPersonnel.Add(nouvelleAffectation);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Emploi du temps dupliqué avec succès", newId = nouveau.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur : {ex.Message}" });
            }
        }

        // POST: Changer statut
        [HttpPost]
        public async Task<IActionResult> ChangerStatut(int id, string nouveauStatut)
        {
            try
            {
                var emploiTemps = await _context.EmploisTemps.FindAsync(id);
                if (emploiTemps == null)
                {
                    return Json(new { success = false, message = "Emploi du temps introuvable" });
                }

                emploiTemps.Statut = nouveauStatut;
                emploiTemps.DateModification = DateTime.Now;
                emploiTemps.ModifiePar = User.Identity.Name ?? "Administrateur";

                if (nouveauStatut == "Approved")
                {
                    emploiTemps.DateApprobation = DateTime.Now;
                    emploiTemps.ApprouvePar = User.Identity.Name ?? "Directeur";
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Statut changé vers {emploiTemps.StatutLibelle}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur : {ex.Message}" });
            }
        }

        // DELETE: Supprimer emploi du temps
        [HttpPost]
        public async Task<IActionResult> SupprimerEmploiTemps(int id)
        {
            try
            {
                var emploiTemps = await _context.EmploisTemps.FindAsync(id);
                if (emploiTemps == null)
                {
                    return Json(new { success = false, message = "Emploi du temps introuvable" });
                }

                _context.EmploisTemps.Remove(emploiTemps);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Emploi du temps supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur : {ex.Message}" });
            }
        }

        // API: Obtenir personnel disponible
        [HttpGet]
        public async Task<IActionResult> GetPersonnelDisponible(int serviceId, string jour, string equipe, int? emploiTempsId = null)
        {
            var personnels = await _context.Personnels
                .Where(p => p.ServiceId == serviceId && p.Actif)
                .Select(p => new
                {
                    id = p.Id,
                    nom = p.NomComplet,
                    type = p.TypePersonnelLibelle,
                    email = p.Email
                })
                .ToListAsync();

            // Filtrer selon les conflits d'affectation si emploiTempsId est fourni
            if (emploiTempsId.HasValue)
            {
                var affectationsExistantes = await _context.AffectationsPersonnel
                    .Where(a => a.EmploiTempsId == emploiTempsId.Value && a.Jour == jour && a.Equipe == equipe)
                    .Select(a => a.PersonnelId)
                    .ToListAsync();

                personnels = personnels.Where(p => !affectationsExistantes.Contains(p.id)).ToList();
            }

            return Json(personnels);
        }

        // API: Statistiques pour dashboard
        [HttpGet]
        public async Task<IActionResult> GetStatistiquesDashboard()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            var stats = new
            {
                totalEmploisTemps = await _context.EmploisTemps.Where(et => et.Annee == currentYear).CountAsync(),
                brouillons = await _context.EmploisTemps.Where(et => et.Statut == "Draft").CountAsync(),
                enAttente = await _context.EmploisTemps.Where(et => et.Statut == "Pending").CountAsync(),
                approuves = await _context.EmploisTemps.Where(et => et.Statut == "Approved").CountAsync(),
                archives = await _context.EmploisTemps.Where(et => et.Statut == "Archived").CountAsync(),
                tauxCouvertureMoyen = await CalculerTauxCouvertureMoyen(),

                // Statistiques mensuelles
                emploisTempsCresCeMois = await _context.EmploisTemps
                    .Where(et => et.DateCreation.Year == currentYear && et.DateCreation.Month == currentMonth)
                    .CountAsync(),

                // Tendances
                evolutionCouverture = await CalculerEvolutionCouverture()
            };

            return Json(stats);
        }

        // MÉTHODES PRIVÉES UTILITAIRES

        // Nouvelle méthode pour extraire les données du formulaire
        private List<AffectationData> ExtraireAffectationsFormulaire()
        {
            var affectationsData = new List<AffectationData>();
            var form = Request.Form;

            System.Diagnostics.Debug.WriteLine($"=== EXTRACTION AFFECTATIONS ===");
            System.Diagnostics.Debug.WriteLine($"Nombre total de clés: {form.Keys.Count}");

            // Debug: Afficher toutes les clés du formulaire
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("Affectations"))
                {
                    System.Diagnostics.Debug.WriteLine($"Clé trouvée: {key} = '{form[key]}'");
                }
            }

            var affectationKeys = form.Keys.Where(k => k.StartsWith("Affectations[")).ToList();
            System.Diagnostics.Debug.WriteLine($"Clés d'affectation trouvées: {affectationKeys.Count}");

            foreach (string key in affectationKeys)
            {
                try
                {
                    // Parser la clé: Affectations[ServiceId][Day][Shift]
                    var match = System.Text.RegularExpressions.Regex.Match(key, @"Affectations\[(\d+)\]\[(\w+)\]\[(\w+)\]");
                    if (match.Success)
                    {
                        int serviceId = int.Parse(match.Groups[1].Value);
                        string day = match.Groups[2].Value;
                        string shift = match.Groups[3].Value;
                        string valeurFormulaire = form[key].ToString().Trim();

                        System.Diagnostics.Debug.WriteLine($"Parsé: Service={serviceId}, Jour={day}, Équipe={shift}, Valeur='{valeurFormulaire}'");

                        if (!string.IsNullOrEmpty(valeurFormulaire))
                        {
                            var personnelIds = valeurFormulaire.Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(x => int.Parse(x.Trim()))
                                .ToList();

                            foreach (var personnelId in personnelIds)
                            {
                                affectationsData.Add(new AffectationData
                                {
                                    ServiceId = serviceId,
                                    PersonnelId = personnelId,
                                    Jour = day,
                                    Equipe = shift
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur parsing clé '{key}': {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total affectations extraites: {affectationsData.Count}");
            return affectationsData;
        }

        // Nouvelle méthode pour créer les affectations
        private async Task<List<AffectationPersonnel>> CreerAffectationsPersonnel(int emploiTempsId, List<AffectationData> affectationsData)
        {
            var affectations = new List<AffectationPersonnel>();

            foreach (var data in affectationsData)
            {
                // Vérifier que le personnel existe et est actif
                var personnel = await _context.Personnels
                    .FirstOrDefaultAsync(p => p.Id == data.PersonnelId && p.ServiceId == data.ServiceId && p.Actif);

                if (personnel == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Personnel {data.PersonnelId} introuvable ou inactif pour service {data.ServiceId}");
                    continue;
                }

                // Définir les heures selon l'équipe
                TimeSpan heureDebut, heureFin;
                if (data.Equipe == "Morning")
                {
                    heureDebut = new TimeSpan(8, 0, 0);
                    heureFin = new TimeSpan(16, 0, 0);
                }
                else if (data.Equipe == "Evening")
                {
                    heureDebut = new TimeSpan(16, 0, 0);
                    heureFin = new TimeSpan(24, 0, 0);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Équipe '{data.Equipe}' non reconnue");
                    continue;
                }

                var affectation = new AffectationPersonnel
                {
                    EmploiTempsId = emploiTempsId,
                    PersonnelId = data.PersonnelId,
                    ServiceId = data.ServiceId,
                    Jour = data.Jour,
                    Equipe = data.Equipe,
                    HeureDebut = heureDebut,
                    HeureFin = heureFin,
                    DateCreation = DateTime.Now
                };

                affectations.Add(affectation);
                System.Diagnostics.Debug.WriteLine($"✓ Affectation créée: Personnel {data.PersonnelId}, {data.Jour} {data.Equipe}");
            }

            return affectations;
        }

        // Méthode TraiterAffectationsFormulaire corrigée
        private async Task TraiterAffectationsFormulaire(int emploiTempsId)
        {
            try
            {
                var form = Request.Form;
                System.Diagnostics.Debug.WriteLine($"=== DEBUT TraiterAffectationsFormulaire ===");
                System.Diagnostics.Debug.WriteLine($"EmploiTemps ID: {emploiTempsId}");
                System.Diagnostics.Debug.WriteLine($"Nombre de clés dans le formulaire: {form.Keys.Count}");

                var affectationsAjoutees = 0;
                var affectationsListe = new List<AffectationPersonnel>();

                // Debug: Afficher les clés pertinentes
                var affectationKeys = form.Keys.Where(k => k.StartsWith("Affectations[")).ToList();
                System.Diagnostics.Debug.WriteLine($"Clés d'affectation trouvées: {affectationKeys.Count}");

                foreach (string key in affectationKeys)
                {
                    System.Diagnostics.Debug.WriteLine($"Clé: {key} = '{form[key]}'");
                }

                foreach (string key in affectationKeys)
                {
                    if (key.StartsWith("Affectations[") && key.Contains("][") && key.EndsWith("]"))
                    {
                        try
                        {
                            // Extraire ServiceId, Day, Shift
                            var keyContent = key.Substring(13); // Enlever "Affectations["
                            keyContent = keyContent.Substring(0, keyContent.Length - 1); // Enlever le dernier "]"
                            var parts = keyContent.Split(new string[] { "][" }, StringSplitOptions.None);

                            if (parts.Length == 3)
                            {
                                if (!int.TryParse(parts[0], out int serviceId))
                                {
                                    System.Diagnostics.Debug.WriteLine($"ServiceId invalide: {parts[0]}");
                                    continue;
                                }

                                var day = parts[1];
                                var shift = parts[2];
                                var valeurFormulaire = form[key].ToString().Trim();

                                System.Diagnostics.Debug.WriteLine($"Traitement: Service={serviceId}, Jour={day}, Équipe={shift}");
                                System.Diagnostics.Debug.WriteLine($"Valeur: '{valeurFormulaire}'");

                                if (string.IsNullOrEmpty(valeurFormulaire))
                                {
                                    continue;
                                }

                                // Séparer les IDs du personnel
                                var personnelIds = valeurFormulaire.Split(',')
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(x => x.Trim())
                                    .ToList();

                                foreach (var personnelIdStr in personnelIds)
                                {
                                    if (int.TryParse(personnelIdStr, out int personnelId))
                                    {
                                        // Vérifier que le personnel existe
                                        var personnel = await _context.Personnels
                                            .FirstOrDefaultAsync(p => p.Id == personnelId && p.ServiceId == serviceId && p.Actif);

                                        if (personnel == null)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Personnel {personnelId} introuvable pour service {serviceId}");
                                            continue;
                                        }

                                        // Définir les heures
                                        TimeSpan heureDebut, heureFin;
                                        if (shift == "Morning")
                                        {
                                            heureDebut = new TimeSpan(8, 0, 0);
                                            heureFin = new TimeSpan(16, 0, 0);
                                        }
                                        else if (shift == "Evening")
                                        {
                                            heureDebut = new TimeSpan(16, 0, 0);
                                            heureFin = new TimeSpan(24, 0, 0);
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        // Créer l'affectation
                                        var affectation = new AffectationPersonnel
                                        {
                                            EmploiTempsId = emploiTempsId,
                                            PersonnelId = personnelId,
                                            ServiceId = serviceId,
                                            Jour = day,
                                            Equipe = shift,
                                            HeureDebut = heureDebut,
                                            HeureFin = heureFin,
                                            DateCreation = DateTime.Now
                                        };

                                        affectationsListe.Add(affectation);
                                        System.Diagnostics.Debug.WriteLine($"✓ Affectation préparée: Personnel {personnelId}, {day} {shift}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erreur sur clé '{key}': {ex.Message}");
                        }
                    }
                }

                // Ajouter toutes les affectations en une fois
                if (affectationsListe.Any())
                {
                    _context.AffectationsPersonnel.AddRange(affectationsListe);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ {affectationsListe.Count} affectations sauvegardées");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucune affectation valide trouvée");
                }

                System.Diagnostics.Debug.WriteLine($"=== FIN TraiterAffectationsFormulaire ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERREUR dans TraiterAffectationsFormulaire: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        private DateTime GetDateDebutSemaine(int numeroSemaine, int annee)
        {
            var jan1 = new DateTime(annee, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            var firstThursday = jan1.AddDays((int)daysOffset);
            var firstWeek = GetWeekOfYear(firstThursday);

            var weekNum = numeroSemaine;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3); // Revenir au lundi
        }

        private async Task<double> CalculerTauxCouvertureMoyen()
        {
            var emploisTempsAvecAffectations = await _context.EmploisTemps
                .Include(et => et.Affectations)
                .Where(et => et.Statut == "Approved" && et.Annee == DateTime.Now.Year)
                .ToListAsync();

            if (!emploisTempsAvecAffectations.Any())
                return 0;

            var totalTaux = emploisTempsAvecAffectations.Sum(et => et.TauxCouverture);
            return Math.Round(totalTaux / emploisTempsAvecAffectations.Count, 1);
        }

        // Calcul de l'évolution de la couverture sur les 6 derniers mois
        private async Task<object> CalculerEvolutionCouverture()
        {
            var sixMoisEnArriere = DateTime.Now.AddMonths(-6);

            var emploisParMois = await _context.EmploisTemps
                .Where(et => et.DateCreation >= sixMoisEnArriere && et.Statut == "Approved")
                .Include(et => et.Affectations)
                .ToListAsync();

            var evolution = emploisParMois
                .GroupBy(et => new { et.DateCreation.Year, et.DateCreation.Month })
                .Select(g => new
                {
                    mois = $"{g.Key.Month:00}/{g.Key.Year}",
                    tauxMoyen = g.Any() ? Math.Round(g.Average(et => et.TauxCouverture), 1) : 0
                })
                .OrderBy(x => x.mois)
                .ToList();

            return evolution;
        }

        private async Task RechargerDonnees()
        {
            // En ASP.NET Core, on peut utiliser IMemoryCache au lieu de HttpContext.Cache
            var services = await _context.Services.Where(s => s.Actif).ToListAsync();
            ViewBag.Services = services;

            var personnels = await _context.Personnels
                .Include(p => p.Service)
                .Where(p => p.Actif)
                .ToListAsync();
            ViewBag.Personnels = personnels;
        }

        // API pour exporter les données
        [HttpPost]
        public async Task<IActionResult> ExporterEmploisTemps(List<int> ids, string format = "excel")
        {
            try
            {
                var emploisTemps = await _context.EmploisTemps
                    .Include(et => et.Affectations).ThenInclude(a => a.Personnel)
                    .Include(et => et.Affectations).ThenInclude(a => a.Service)
                    .Where(et => ids.Contains(et.Id))
                    .ToListAsync();

                switch (format.ToLower())
                {
                    case "excel":
                        return ExporterVersExcel(emploisTemps);
                    case "pdf":
                        return ExporterVersPdf(emploisTemps);
                    case "csv":
                        return ExporterVersCsv(emploisTemps);
                    default:
                        return Json(new { success = false, message = "Format non supporté" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Méthodes d'export (implémentations basiques)
        private IActionResult ExporterVersExcel(List<EmploiTemps> emploisTemps)
        {
            // Implémentation basique CSV pour l'instant
            // Pour Excel complet, utilisez EPPlus ou ClosedXML
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Semaine,Annee,Statut,Personnel,Service,Jour,Equipe,Heures");

            foreach (var et in emploisTemps)
            {
                foreach (var affectation in et.Affectations)
                {
                    csv.AppendLine($"{et.NumeroSemaine},{et.Annee},{et.StatutLibelle}," +
                                  $"{affectation.Personnel.NomComplet},{affectation.Service.Nom}," +
                                  $"{affectation.JourLibelle},{affectation.EquipeLibelle}," +
                                  $"{affectation.DureeEnHeures}");
                }
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"EmploisTemps_{DateTime.Now:yyyyMMdd}.csv");
        }

        private IActionResult ExporterVersPdf(List<EmploiTemps> emploisTemps)
        {
            // Implémentation basique - pour un PDF complet, utilisez iTextSharp
            var html = "<html><body><h1>Emplois du Temps</h1>";

            foreach (var et in emploisTemps)
            {
                html += $"<h2>Semaine {et.NumeroSemaine} - {et.Annee}</h2>";
                html += $"<p>Statut: {et.StatutLibelle}</p>";
                html += "<table border='1'><tr><th>Personnel</th><th>Service</th><th>Jour</th><th>Équipe</th></tr>";

                foreach (var affectation in et.Affectations)
                {
                    html += $"<tr><td>{affectation.Personnel.NomComplet}</td>" +
                           $"<td>{affectation.Service.Nom}</td>" +
                           $"<td>{affectation.JourLibelle}</td>" +
                           $"<td>{affectation.EquipeLibelle}</td></tr>";
                }
                html += "</table><br/>";
            }

            html += "</body></html>";
            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(bytes, "text/html", $"EmploisTemps_{DateTime.Now:yyyyMMdd}.html");
        }

        private IActionResult ExporterVersCsv(List<EmploiTemps> emploisTemps)
        {
            return ExporterVersExcel(emploisTemps); // Même implémentation pour l'instant
        }

        // API pour actions en masse
        [HttpPost]
        public async Task<IActionResult> ActionsEnMasse(List<int> ids, string action)
        {
            try
            {
                var emploisTemps = await _context.EmploisTemps
                    .Where(et => ids.Contains(et.Id))
                    .ToListAsync();

                if (!emploisTemps.Any())
                {
                    return Json(new { success = false, message = "Aucun emploi du temps sélectionné" });
                }

                var userName = User.Identity.Name ?? "Administrateur";
                var count = 0;

                foreach (var et in emploisTemps)
                {
                    switch (action.ToLower())
                    {
                        case "archive":
                            if (et.Statut == "Approved")
                            {
                                et.Statut = "Archived";
                                et.DateModification = DateTime.Now;
                                et.ModifiePar = userName;
                                count++;
                            }
                            break;

                        case "delete":
                            if (et.Statut == "Draft" || et.Statut == "Cancelled")
                            {
                                _context.EmploisTemps.Remove(et);
                                count++;
                            }
                            break;

                        case "approve":
                            if (et.Statut == "Pending")
                            {
                                et.Statut = "Approved";
                                et.DateApprobation = DateTime.Now;
                                et.ApprouvePar = userName;
                                et.DateModification = DateTime.Now;
                                et.ModifiePar = userName;
                                count++;
                            }
                            break;

                        case "reject":
                            if (et.Statut == "Pending")
                            {
                                et.Statut = "Draft";
                                et.DateModification = DateTime.Now;
                                et.ModifiePar = userName;
                                count++;
                            }
                            break;
                    }
                }

                await _context.SaveChangesAsync();

                // Déterminer le libellé de l'action
                string actionLibelle;
                switch (action.ToLower())
                {
                    case "archive":
                        actionLibelle = "archivé(s)";
                        break;
                    case "delete":
                        actionLibelle = "supprimé(s)";
                        break;
                    case "approve":
                        actionLibelle = "approuvé(s)";
                        break;
                    case "reject":
                        actionLibelle = "rejeté(s)";
                        break;
                    default:
                        actionLibelle = "traité(s)";
                        break;
                }

                return Json(new
                {
                    success = true,
                    message = $"{count} emploi(s) du temps {actionLibelle} avec succès",
                    processed = count,
                    total = emploisTemps.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur : {ex.Message}" });
            }
        }
    }
}