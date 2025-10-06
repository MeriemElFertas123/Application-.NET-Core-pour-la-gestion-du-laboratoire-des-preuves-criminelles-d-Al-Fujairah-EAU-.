using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class RechercheController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RechercheController> _logger;

        public RechercheController(ApplicationDbContext context, ILogger<RechercheController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> QuickSearch(string searchType, string searchTerm)
        {
            _logger.LogInformation($"QuickSearch appelé - Type: {searchType}, Terme: {searchTerm}");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["ErrorMessage"] = "Veuillez entrer un terme de recherche";
                return RedirectToAction("Index", "Home");
            }

            searchTerm = searchTerm.Trim();

            try
            {
                switch (searchType?.ToLower())
                {
                    case "affaire":
                        return await SearchAffaire(searchTerm);

                    case "echantillon":
                        return await SearchEchantillon(searchTerm);

                    case "qr":
                        return await SearchByQRCode(searchTerm);

                    default:
                        TempData["ErrorMessage"] = "Type de recherche non implémenté : " + searchType;
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche");
                TempData["ErrorMessage"] = $"Erreur lors de la recherche : {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<IActionResult> SearchAffaire(string numeroAffaire)
        {
            _logger.LogInformation($"Recherche affaire avec numéro: {numeroAffaire}");

            try
            {
                Affaire? affaire = null;

                // Si l'utilisateur entre un ID numérique, chercher directement par ID
                if (int.TryParse(numeroAffaire, out int affaireId))
                {
                    affaire = await _context.Affaires
                        .Include(a => a.Echantillons)
                        .FirstOrDefaultAsync(a => a.Id == affaireId);
                }

                // Si pas trouvé par ID, recherche par numéro d'affaire exact
                if (affaire == null)
                {
                    affaire = await _context.Affaires
                        .Include(a => a.Echantillons)
                        .FirstOrDefaultAsync(a => a.NumeroAffaire == numeroAffaire);
                }

                // Si pas trouvé, recherche partielle
                if (affaire == null)
                {
                    affaire = await _context.Affaires
                        .Include(a => a.Echantillons)
                        .FirstOrDefaultAsync(a => a.NumeroAffaire != null &&
                                                 a.NumeroAffaire.Contains(numeroAffaire));
                }

                if (affaire == null)
                {
                    _logger.LogWarning($"Aucune affaire trouvée pour: {numeroAffaire}");
                    TempData["ErrorMessage"] = $"Aucune affaire trouvée avec le numéro : {numeroAffaire}";

                    if (User.IsInRole("Enqueteur"))
                        return RedirectToAction("MesAffaires", "Affaires");
                    else
                        return RedirectToAction("Index", "Affaires");
                }

                // VÉRIFICATION DES PERMISSIONS CORRIGÉE
                if (User.IsInRole("Enqueteur"))
                {
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    // DEBUG: Log pour voir les IDs comparés
                    _logger.LogInformation($"Vérification permissions - UserId: {currentUserId}, Affaire.UserId: {affaire.UserId}");

                    if (affaire.UserId != currentUserId)
                    {
                        _logger.LogWarning($"Accès refusé à l'affaire {affaire.Id} pour l'utilisateur {currentUserId}");
                        TempData["ErrorMessage"] = "Vous n'avez pas accès à cette affaire";

                        // Rediriger vers MesAffaires avec un message d'erreur
                        return RedirectToAction("MesAffaires", "Affaires");
                    }
                }

                // SI TOUT EST OK, REDIRIGER VERS LES DÉTAILS
                _logger.LogInformation($"Affaire trouvée, redirection vers Details ID: {affaire.Id}");
                return RedirectToAction("Details", "Affaires", new { id = affaire.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans SearchAffaire");
                throw;
            }
        }
        private async Task<IActionResult> SearchEchantillon(string searchTerm)
        {
            _logger.LogInformation($"Recherche échantillon: {searchTerm}");

            try
            {
                Echantillon? echantillon = null;

                // Essayer de parser comme ID
                if (int.TryParse(searchTerm, out int echantillonId))
                {
                    echantillon = await _context.Echantillons
                        .Include(e => e.Affaire)
                        .FirstOrDefaultAsync(e => e.Id == echantillonId);
                }

                // Si pas trouvé, chercher par code
                if (echantillon == null)
                {
                    echantillon = await _context.Echantillons
                        .Include(e => e.Affaire)
                        .FirstOrDefaultAsync(e => e.NumeroEchantillon != null &&
                                                e.NumeroEchantillon.Contains(searchTerm));
                }

                if (echantillon == null)
                {
                    TempData["ErrorMessage"] = $"Aucun échantillon trouvé avec : {searchTerm}";
                    return RedirectToAction("Index", "Echantillons");
                }

                return RedirectToAction("Details", "Echantillons", new { id = echantillon.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans SearchEchantillon");
                throw;
            }
        }

        private async Task<IActionResult> SearchByQRCode(string qrCode)
        {
            _logger.LogInformation($"Recherche par QR code: {qrCode}");

            try
            {
                var echantillon = await _context.Echantillons
                    .Include(e => e.Affaire)
                    .FirstOrDefaultAsync(e => e.QRCode == qrCode);

                if (echantillon == null)
                {
                    TempData["ErrorMessage"] = $"Aucun échantillon trouvé pour le code QR : {qrCode}";
                    return RedirectToAction("Index", "Echantillons");
                }

                return RedirectToAction("Details", "Echantillons", new { id = echantillon.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans SearchByQRCode");
                throw;
            }
        }

        [HttpGet]
        public IActionResult AdvancedSearch()
        {
            return View();
        }

        // Action de test pour vérifier que le contrôleur fonctionne
        [HttpGet]
        public IActionResult Test()
        {
            return Content("Le contrôleur Recherche fonctionne correctement !");
        }
    }
}