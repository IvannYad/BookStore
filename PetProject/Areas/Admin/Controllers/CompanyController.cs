using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Utility;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Make sure that only user with role admin can access all action methods of controller.
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().OrderBy(c => c.Name).ToList();
            return View(companyList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Company company)
        {
            if (company.Name is not null && !Regex.IsMatch(company.Name, @"^[a-zA-Z]+$"))
            {
                ModelState.AddModelError("Name", "'Company Name' must only consists of latin letters");
            }
            if (company.Name is not null && !char.IsUpper(company.Name[0]))
            {
                ModelState.AddModelError("Name", "'Company Name' must start with capital letter");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Add(company);
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Company? companyToEdit = _unitOfWork.Company.Get(c => c.Id == id);
            if (companyToEdit is null)
            {
                return NotFound();
            }

            return View(companyToEdit);
        }

        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (company.Name is not null && !Regex.IsMatch(company.Name, @"^[a-zA-Z]+$"))
            {
                ModelState.AddModelError("Name", "'Company Name' must only consists of latin letters");
            }

            if (company.Name is not null && !char.IsUpper(company.Name[0]))
            {
                ModelState.AddModelError("Name", "'Company Name' must start with capital letter");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                TempData["success"] = "Company updates successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Company? companyToDelete = _unitOfWork.Company.Get(c => c.Id == id);
            if (companyToDelete is null)
            {
                return NotFound();
            }

            return View(companyToDelete);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Company companyToDelete = _unitOfWork.Company.Get(c => c.Id == id);
            if (companyToDelete is null)
            {
                return NotFound();
            }

            _unitOfWork.Company.Remove(companyToDelete);
            _unitOfWork.Save();
            TempData["success"] = "Company deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
