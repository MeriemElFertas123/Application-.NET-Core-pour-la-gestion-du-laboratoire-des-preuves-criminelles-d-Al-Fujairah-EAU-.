using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WebApplication1.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        public string Message { get; set; }

        public IActionResult OnGet(string returnUrl = null)
        {
            // Définir l'URL de retour par défaut
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Si l'utilisateur n'est pas connecté, rediriger directement
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { ReturnUrl });
            }

            // Message personnalisé basé sur le rôle ou les informations utilisateur
            if (User.IsInRole("Admin"))
            {
                Message = "Vous êtes sur le point de quitter l'interface d'administration.";
            }
            else if (User.IsInRole("Technicien"))
            {
                Message = "Votre session de laboratoire va être fermée.";
            }

            _logger.LogInformation("Utilisateur {UserId} sur la page de déconnexion",
                User.Identity.Name ?? "Inconnu");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userName = User.Identity.Name;
                    var userRoles = User.Claims
                        .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                        .Select(c => c.Value);

                    _logger.LogInformation("Déconnexion de l'utilisateur {UserName} avec les rôles: {Roles}",
                        userName, string.Join(", ", userRoles));

                    // Effacer tous les cookies d'authentification
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Effacer la session si elle existe
                    if (HttpContext.Session != null)
                    {
                        HttpContext.Session.Clear();
                    }

                    // Ajouter un message de succès
                    TempData["SuccessMessage"] = "Déconnexion réussie. À bientôt !";

                    _logger.LogInformation("Déconnexion réussie pour l'utilisateur {UserName}", userName);
                }
                else
                {
                    _logger.LogWarning("Tentative de déconnexion d'un utilisateur non authentifié");
                }

                // Rediriger vers l'URL de retour ou la page de connexion
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                return RedirectToPage("/Account/Login");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");

                TempData["ErrorMessage"] = "Une erreur s'est produite lors de la déconnexion.";
                return RedirectToPage("/Account/Login");
            }
        }

        /// <summary>
        /// Action pour la déconnexion forcée (par exemple, en cas de session expirée)
        /// </summary>
        public async Task<IActionResult> OnGetForceLogoutAsync(string reason = null)
        {
            _logger.LogWarning("Déconnexion forcée. Raison: {Reason}", reason ?? "Non spécifiée");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (HttpContext.Session != null)
            {
                HttpContext.Session.Clear();
            }

            var message = reason switch
            {
                "expired" => "Votre session a expiré. Veuillez vous reconnecter.",
                "security" => "Déconnexion pour des raisons de sécurité.",
                "admin" => "Déconnexion demandée par un administrateur.",
                _ => "Vous avez été déconnecté."
            };

            TempData["InfoMessage"] = message;
            return RedirectToPage("/Account/Login");
        }
    }
}

// Alternative: Si vous préférez utiliser un Controller au lieu d'une Razor Page
/*
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Logout(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Home");
            
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Message = "Vous êtes sur le point de quitter l'interface d'administration.";
            }
            else if (User.IsInRole("Technicien"))
            {
                ViewBag.Message = "Votre session de laboratoire va être fermée.";
            }

            _logger.LogInformation("Utilisateur {UserId} sur la page de déconnexion", 
                User.Identity.Name ?? "Inconnu");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userName = User.Identity.Name;
                    _logger.LogInformation("Déconnexion de l'utilisateur {UserName}", userName);

                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    
                    if (HttpContext.Session != null)
                    {
                        HttpContext.Session.Clear();
                    }

                    TempData["SuccessMessage"] = "Déconnexion réussie. À bientôt !";
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                
                return RedirectToAction("Login");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");
                TempData["ErrorMessage"] = "Une erreur s'est produite lors de la déconnexion.";
                return RedirectToAction("Login");
            }
        }
    }
}
*/