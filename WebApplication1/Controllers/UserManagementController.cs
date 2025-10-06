using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Data;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class UserManagementController : BaseController
    {
        public UserManagementController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db, userManager, roleManager)
        {
        }

        // GET: UserManagement
        public IActionResult Index()
        {
            return View();
        }
    }
}