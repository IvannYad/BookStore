using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetProject.DataAccess.Data;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Utility;
using SQLitePCL;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Make sure that only user with role admin can access all action methods of controller.
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RoleManagment()
        {
            return View();
        }


        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _context.ApplicationUsers.Include(u => u.Company).OrderBy(p => p.Name).ToList();
            var roles = _context.Roles.ToList();
            var userRoles = _context.UserRoles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.RoleName = roles.FirstOrDefault(r => r.Id == roleId).Name;
                
                if (user.Company is null)
                    user.Company = new Company() { Name = "" };
            }
            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string userId)
        {
            var userFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (userFromDb is null)
            {
                return Json(new { success = false, message = "Error while Locking/unlocking" });
            }

            if (userFromDb.LockoutEnd is not null && userFromDb.LockoutEnd >= DateTime.Now)
            {
                // User is currently locked and we need to unlock them.
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _context.SaveChanges();
            return Json(new { success = true, message = "Operation successful" });
        }

        #endregion
    }
}
