using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AssignController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AssignController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
            _context = db;
        }



        // GET: Assign
        public async Task<IActionResult> Index()
        {
            var viewModel = new AssignAffaireViewModel
            {
                AffairesDisponibles = await _context.Affaires
                    .Where(a => a.IdEnqueteurResponsable == 0 || a.IdEnqueteurResponsable == null)
                    .OrderBy(a => a.Titre)
                    .ToListAsync(),

                Enqueteurs = await _context.Enqueteurs
                    .OrderBy(e => e.nom)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AssignAffaireViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.AffairesSelectionnees == null || !viewModel.AffairesSelectionnees.Any())
                    {
                        ModelState.AddModelError("AffairesSelectionnees", "Veuillez sélectionner au moins une affaire");
                        return View(await ReloadViewModel(viewModel));
                    }

                    var affairesAAssigner = await _context.Affaires
                        .Where(a => viewModel.AffairesSelectionnees.Contains(a.Id))
                        .ToListAsync();

                    var enqueteur = await _context.Enqueteurs.FindAsync(viewModel.EnqueteurId);
                    if (enqueteur == null)
                    {
                        ModelState.AddModelError("EnqueteurId", "L'enquêteur sélectionné n'existe pas");
                        return View(await ReloadViewModel(viewModel));
                    }

                    foreach (var affaire in affairesAAssigner)
                    {
                        affaire.IdEnqueteurResponsable = viewModel.EnqueteurId;
                        _context.Update(affaire);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{affairesAAssigner.Count} affaire(s) assignée(s) avec succès à {enqueteur.nom}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Une erreur s'est produite lors de l'affectation : " + ex.Message);
                }
            }

            return View(await ReloadViewModel(viewModel));
        }

        // GET: Assign/History
        public async Task<IActionResult> History()
        {
            var affairesAssignees = await _context.Affaires
                .Where(a => a.IdEnqueteurResponsable != 0 && a.IdEnqueteurResponsable != null)
                .OrderByDescending(a => a.DateOuverture)
                .ToListAsync();

            ViewBag.Enqueteurs = await _context.Enqueteurs.ToListAsync();
            return View(affairesAssignees);
        }

        // GET: Assign/Reassign/5
        public async Task<IActionResult> Reassign(int id)
        {
            var affaire = await _context.Affaires.FindAsync(id);
            if (affaire == null)
            {
                return NotFound();
            }

            var viewModel = new AssignAffaireViewModel
            {
                AffairesSelectionnees = new List<int> { id },
                AffairesDisponibles = new List<Affaire> { affaire },
                Enqueteurs = await _context.Enqueteurs.OrderBy(e => e.nom).ToListAsync()
            };

            return View("Index", viewModel);
        }

        // POST: Assign/Unassign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id)
        {
            try
            {
                var affaire = await _context.Affaires.FindAsync(id);
                if (affaire != null)
                {
                    affaire.IdEnqueteurResponsable = 0;
                    _context.Update(affaire);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Affaire désassignée avec succès";
                }
                else
                {
                    TempData["ErrorMessage"] = "Affaire non trouvée";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur lors de la désassignation : " + ex.Message;
            }

            return RedirectToAction("History");
        }

        private async Task<AssignAffaireViewModel> ReloadViewModel(AssignAffaireViewModel viewModel)
        {
            viewModel.AffairesDisponibles = await _context.Affaires
                .Where(a => a.IdEnqueteurResponsable == 0 || a.IdEnqueteurResponsable == null)
                .OrderBy(a => a.Titre)
                .ToListAsync();

            viewModel.Enqueteurs = await _context.Enqueteurs
                .OrderBy(e => e.nom)
                .ToListAsync();

            return viewModel;
        }
    }
}