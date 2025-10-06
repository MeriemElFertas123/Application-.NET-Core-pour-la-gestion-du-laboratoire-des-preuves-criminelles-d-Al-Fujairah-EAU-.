using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.Analyse;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Controllers
{
    public class RapportsController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public RapportsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
            : base(db, userManager, roleManager)
        {
            _db = db;
        }

        // GET: Rapports
        public async Task<IActionResult> Index(string filtreStatut = null, string filtreType = null, string filtrePeriode = null)
        {
            var query = _db.Analyses.Include(a => a.Echantillon).AsQueryable();

            // Filtrage par statut
            if (!string.IsNullOrEmpty(filtreStatut))
            {
                if (Enum.TryParse(filtreStatut, out StatutAnalyse statut))
                {
                    query = query.Where(a => a.Statut == statut);
                }
            }

            // Filtrage par type
            if (!string.IsNullOrEmpty(filtreType))
            {
                query = query.Where(a => a.TypeAnalyse == filtreType);
            }

            // Filtrage par période (à implémenter selon vos besoins)
            if (!string.IsNullOrEmpty(filtrePeriode))
            {
                var now = DateTime.Now;
                switch (filtrePeriode.ToLower())
                {
                    case "today":
                        query = query.Where(a => a.DateAnalyse == now.Date);
                        break;
                    case "week":
                        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                        query = query.Where(a => a.DateAnalyse >= startOfWeek);
                        break;
                   
                }
            }

            var analyses = await query
                .OrderByDescending(a => a.DateAnalyse)
                .ToListAsync();

            var viewModel = analyses
                .Select(a => new RapportViewModel
                {
                    Id = a.Id,
                    Nom = a.Nom,
                    TypeAnalyse = a.TypeAnalyse,
                    DateAnalyse = (DateTime)a.DateAnalyse,
                    Statut = (StatutAnalyse)a.Statut,
                    EstValide = (bool)a.EstValide,
                    EchantillonNom = a.Echantillon?.NumeroEchantillon ?? "N/A"
                });

            // Préparation des ViewBag pour les dropdowns
            ViewBag.FiltreStatut = new SelectList(Enum.GetNames(typeof(StatutAnalyse)));
            ViewBag.FiltreType = new SelectList(await _db.Analyses.Select(a => a.TypeAnalyse).Distinct().ToListAsync());
            ViewBag.FiltrePeriode = new SelectList(new[] { "today", "week", "month", "quarter", "year" });

            return View(viewModel);
        }

        // GET: Rapports/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var analyse = await _db.Analyses
                .Include(a => a.Echantillon)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analyse == null)
            {
                return NotFound();
            }

            return View(analyse);
        }

        // GET: Rapports/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var analyse = await _db.Analyses.FindAsync(id);

            if (analyse?.FichierContenu == null)
            {
                return NotFound();
            }

            return File(analyse.FichierContenu, analyse.FichierContentType, analyse.NomFichier);
        }
    }
}