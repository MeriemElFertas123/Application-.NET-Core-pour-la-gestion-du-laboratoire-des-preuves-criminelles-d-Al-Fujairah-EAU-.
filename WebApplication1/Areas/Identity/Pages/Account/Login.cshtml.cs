using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "L'adresse email est requise.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }

        [BindProperty]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public string? Role { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? role = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl ?? Url.Content("~/");
            Role = role;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Email, Password, RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Utilisateur {Email} connecté avec succès.", Email);

                    var user = await _userManager.FindByEmailAsync(Email);
                    if (user != null)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);

                        // Redirection basée sur le rôle
                        if (!string.IsNullOrEmpty(Role) && userRoles.Contains(Role))
                        {
                            return RedirectToRoleDashboard(Role);
                        }
                        else if (userRoles.Any())
                        {
                            var primaryRole = GetPrimaryRole(userRoles);
                            return RedirectToRoleDashboard(primaryRole);
                        }
                    }

                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Compte utilisateur {Email} verrouillé.", Email);
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                    return Page();
                }
            }

            // Si on arrive ici, quelque chose a échoué
            return Page();
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            return role.ToLower() switch
            {
                "admin" => RedirectToAction("Index", "Admin"),
                "enqueteur" => RedirectToAction("Dashboard", "Enqueteurs"),
                "analyste" => RedirectToAction("Dashboard", "Analystes"),
                "technicien" => RedirectToAction("Dashboard", "Techniciens"),
                _ => LocalRedirect(ReturnUrl ?? Url.Content("~/"))
            };
        }

        private string GetPrimaryRole(IList<string> roles)
        {
            if (roles.Contains("Admin")) return "Admin";
            if (roles.Contains("Analyste")) return "Analyste";
            if (roles.Contains("Enqueteur")) return "Enqueteur";
            if (roles.Contains("Technicien")) return "Technicien";
            return roles.FirstOrDefault() ?? "User";
        }
    }
}