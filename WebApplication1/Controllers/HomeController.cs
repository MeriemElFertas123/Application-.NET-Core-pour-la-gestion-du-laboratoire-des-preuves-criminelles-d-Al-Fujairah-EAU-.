using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Remove this redirect - Identity will handle it automatically
            // if (user == null)
            // {
            //     return RedirectToPage("/Account/Login", new { area = "Identity" });
            // }

            if (user != null) // Only process permissions if user is authenticated
            {
                var userId = user.Id;

                // 1. Rôles de l'utilisateur
                var roles = await _userManager.GetRolesAsync(user);

                // 2. Permissions via les rôles
                var rolePermissions = _db.RolePermissions
                    .Where(rp => roles.Contains(rp.Role.Name))
                    .Select(rp => rp.Permission.permission)
                    .ToList();

                // 3. Permissions directes UserPermission
                var userPermissions = _db.UserPermissions
                    .Where(up => up.UserId == userId)
                    .Select(up => up.Permission.permission)
                    .ToList();

                // 4. Fusion
                var allPermissions = rolePermissions.Union(userPermissions).Distinct().ToList();

                ViewBag.Permissions = allPermissions;
            }
            else
            {
                ViewBag.Permissions = new List<string>();
            }

            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
