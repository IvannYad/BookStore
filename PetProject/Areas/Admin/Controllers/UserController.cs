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
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
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

            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties:"Company");
            if (user is null)
                return NotFound();
            
            var roleManagmentVM = new RoleManagmentVM()
            {
                User = user,
                RoleList = _roleManager.Roles.Select(r => new SelectListItem() { Text = r.Name, Value = r.Name }),
                CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem() { Text = c.Name, Value = c.Id.ToString() })
            };
            roleManagmentVM.User.RoleName = _userManager.GetRolesAsync(user)
                .GetAwaiter().GetResult().FirstOrDefault();
            return View(roleManagmentVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser
                .Get(u => u.Id == roleManagmentVM.User.Id))
                .GetAwaiter().GetResult().FirstOrDefault();

            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.User.Id);

            if (roleManagmentVM.User.RoleName != oldRole)
            {
                // Role was updated.
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

                _unitOfWork.ApplicationUser.Update(user);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, roleManagmentVM.User.RoleName).GetAwaiter().GetResult();
            }
            else
            {
                if (oldRole == SD.Role_Company && user.CompanyId != roleManagmentVM.User.CompanyId)
                {
                    user.CompanyId = roleManagmentVM.User.CompanyId;
                    _unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }

        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();
            
            foreach (var user in userList)
            {
                user.RoleName = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                
                if (user.Company is null)
                    user.Company = new Company() { Name = "" };
            }
            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string userId)
        {
            var userFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
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

            _unitOfWork.ApplicationUser.Update(userFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation successful" });
        }

        #endregion
    }
}
