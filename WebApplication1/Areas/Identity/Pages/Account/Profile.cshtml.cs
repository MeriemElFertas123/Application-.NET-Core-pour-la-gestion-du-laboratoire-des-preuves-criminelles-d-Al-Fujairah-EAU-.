using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Id { get; set; }

            [Required(ErrorMessage = "Le nom est requis")]
            [Display(Name = "Nom")]
            public string nom { get; set; }

            [Required(ErrorMessage = "Le prénom est requis")]
            [Display(Name = "Prénom")]
            public string prenom { get; set; }

            [Required(ErrorMessage = "L'email est requis")]
            [EmailAddress(ErrorMessage = "Format d'email invalide")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone(ErrorMessage = "Format de téléphone invalide")]
            [Display(Name = "Téléphone")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Profil utilisateur introuvable.";
                return RedirectToPage("/Home/Index");
            }

            Input = new InputModel
            {
                Id = user.Id,
                nom = user.UserName.Contains("_") ? user.UserName.Split('_')[0] : "", // Adapté selon votre logique
                prenom = user.UserName.Contains("_") ? user.UserName.Split('_')[1] : "", // Adapté selon votre logique
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToPage();
            }

            // Mise à jour des informations utilisateur
            user.Email = Input.Email;
            user.PhoneNumber = Input.PhoneNumber;
            user.UserName = $"{Input.nom}_{Input.prenom}"; // Adapté selon votre logique

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil mis à jour avec succès.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}