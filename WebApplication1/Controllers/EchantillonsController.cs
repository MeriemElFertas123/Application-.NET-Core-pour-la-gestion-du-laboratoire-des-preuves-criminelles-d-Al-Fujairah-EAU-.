using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Stockage;
using WebApplication1.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    public class EchantillonsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EchantillonsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment):base(context,userManager,roleManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Echantillons
        public async Task<IActionResult> Index()
        {
            if (!HasPermission("Afficher Preuve"))
            {
                TempData["ErrorMessage"] = "Accès refusé.";
                return RedirectToAction("Index", "Home");
            }

            var echantillons = await _context.Echantillons
                .Include(e => e.Affaire)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .ToListAsync();

            return View(echantillons);
        }

        // GET: Echantillons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "L'identifiant de l'échantillon est requis";
                return RedirectToAction(nameof(Index));
            }

            var echantillon = await _context.Echantillons
                .Include(e => e.Affaire)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (echantillon == null)
            {
                TempData["ErrorMessage"] = "Échantillon introuvable";
                return RedirectToAction(nameof(Index));
            }

            return View(echantillon);
        }

        // GET: Echantillons/Create
        // GET: Echantillons/Create
        public async Task<IActionResult> Create()
        {
            if (!HasPermission("ajouter Preuve"))
            {
                TempData["AccessDenied"] = "Vous n'avez pas la permission d'ajouter des échantillons";
                return RedirectToAction("Index", "Home");
            }

            await PrepareCreateViewBags();

            var echantillon = new Echantillon
            {
                DateReception = DateTime.Now,
                Statut = StatutEchantillon.EnAttente,
                Priorite = PrioriteEchantillon.Normal
            };

            return View(echantillon);
        }

        private async Task PrepareCreateViewBags()
        {
            ViewBag.AffaireId = new SelectList(
                await _context.Affaires
                    .Where(a => a.Statut != StatutAffaire.Resolue && a.Statut != StatutAffaire.NonResolue)
                    .OrderBy(a => a.Titre)
                    .ToListAsync(),
                "Id",
                "Titre");

            // Supprimé: ViewBag.AnalysteId et ViewBag.StockageId
        }

        private async Task PrepareEditViewBags(Echantillon echantillon)
        {
            ViewBag.AffaireId = new SelectList(
                await _context.Affaires
                    .Where(a => a.Statut != StatutAffaire.Resolue && a.Statut != StatutAffaire.NonResolue)
                    .OrderBy(a => a.Titre)
                    .ToListAsync(),
                "Id",
                "Titre",
                echantillon.AffaireId);

            // Supprimé: ViewBag.AnalysteId et ViewBag.StockageId
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Echantillon echantillon, IFormFile fichier)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (fichier != null && fichier.Length > 0)
                    {
                        if (!IsValidFile(fichier))
                        {
                            ModelState.AddModelError("Fichier", "Type de fichier non autorisé");
                            await PrepareCreateViewBags();
                            return View(echantillon);
                        }
                        await ProcessUploadedFile(echantillon, fichier);
                    }

                    _context.Echantillons.Add(echantillon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Échantillon créé avec succès!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erreur: {ex.Message}");
                }
            }

            await PrepareCreateViewBags();
            return View(echantillon);
        }

        // GET: Echantillons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var echantillon = await _context.Echantillons.FindAsync(id);
            if (echantillon == null)
            {
                return NotFound();
            }

            await PrepareEditViewBags(echantillon);
            return View(echantillon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Echantillon echantillon, IFormFile fichier)
        {
            if (id != echantillon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (fichier != null && fichier.Length > 0)
                    {
                        if (!IsValidFile(fichier))
                        {
                            ModelState.AddModelError("Fichier", "Type de fichier non autorisé");
                            await PrepareEditViewBags(echantillon);
                            return View(echantillon);
                        }
                        await ProcessUploadedFile(echantillon, fichier);
                    }

                    _context.Update(echantillon);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Échantillon modifié avec succès!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EchantillonExists(echantillon.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erreur: {ex.Message}");
                }
            }

            await PrepareEditViewBags(echantillon);
            return View(echantillon);
        }

        // GET: Echantillons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var echantillon = await _context.Echantillons
                .Include(e => e.Affaire)
                .Include(e => e.Analyste)
                .Include(e => e.Stockage)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (echantillon == null)
            {
                return NotFound();
            }

            return View(echantillon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var echantillon = await _context.Echantillons.FindAsync(id);

            if (echantillon != null)
            {
                // Supprimer le fichier physique s'il existe
                if (!string.IsNullOrEmpty(echantillon.CheminFichier))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, echantillon.CheminFichier.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Echantillons.Remove(echantillon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Échantillon supprimé avec succès!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Télécharger le fichier attaché
        public async Task<IActionResult> DownloadFile(int id)
        {
            var echantillon = await _context.Echantillons.FindAsync(id);
            if (echantillon == null || string.IsNullOrEmpty(echantillon.CheminFichier))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, echantillon.CheminFichier.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, echantillon.TypeMime ?? "application/octet-stream", echantillon.NomFichier);
        }

        #region Helper Methods

     
        private static bool IsValidFile(IFormFile file)
        {
            const long maxSize = 50 * 1024 * 1024; // 50MB
            if (file.Length > maxSize) return false;

            var allowedTypes = new[]
            {
                "image/jpeg", "image/png", "application/pdf",
                "video/mp4", "audio/mpeg", "text/plain"
            };

            return allowedTypes.Contains(file.ContentType.ToLower());
        }

        private async Task ProcessUploadedFile(Echantillon echantillon, IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "echantillons");

            // Créer le dossier s'il n'existe pas
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            var relativePath = $"/uploads/echantillons/{fileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            echantillon.NomFichier = file.FileName;
            echantillon.TypeMime = file.ContentType;
            echantillon.TailleFichier = file.Length;
            echantillon.CheminFichier = relativePath;

            // Stocker le contenu en base si le fichier est petit (< 1MB)
            if (file.Length < 1048576)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                echantillon.ContenuFichier = memoryStream.ToArray();
            }
        }

        private async Task<bool> EchantillonExists(int id)
        {
            return await _context.Echantillons.AnyAsync(e => e.Id == id);
        }

        #endregion
    }
}