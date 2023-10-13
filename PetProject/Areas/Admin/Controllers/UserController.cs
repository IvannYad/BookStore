using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetProject.DataAccess.Data;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RoleManagment(string? userId)
        {
            if (userId is null)
                return NotFound();

            var user = _context.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId);
            if (user is null)
                return NotFound();
            var roleId = _context.UserRoles.FirstOrDefault(t => t.UserId == userId).RoleId;
            var roleManagmentVM = new RoleManagmentVM()
            {
                User = user,
                RoleList = _context.Roles.Select(r => new SelectListItem() { Text = r.Name, Value = r.Name }),
                CompanyList = _context.Companies.Select(c => new SelectListItem() { Text = c.Name, Value = c.Id.ToString() })
            };
            roleManagmentVM.User.RoleName = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;
            return View(roleManagmentVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            string roleId = _context.UserRoles.FirstOrDefault(t => t.UserId == roleManagmentVM.User.Id).RoleId;
            string oldRole = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;

            if (roleManagmentVM.User.RoleName != oldRole)
            {
                // Role was updated.
                var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.User.Id);

                if (user is null)
                    return NotFound();
                
                if (roleManagmentVM.User.RoleName == SD.Role_Company)
                {
                    user.CompanyId = roleManagmentVM.User.CompanyId;
                }

                if (oldRole == SD.Role_Company)
                {
                    user.CompanyId = null;
                }

                _context.SaveChanges();
                _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, roleManagmentVM.User.RoleName).GetAwaiter().GetResult();
            }

            return RedirectToAction("Index");
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
