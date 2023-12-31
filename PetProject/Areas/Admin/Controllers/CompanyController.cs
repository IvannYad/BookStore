﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Services.Interfaces;
using PetProject.Utility;
using PetProject.Utility.CustomExceptions;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Make sure that only user with role admin can access all action methods of controller.
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Company> _validator;

        public CompanyController(IUnitOfWork unitOfWork, IValidator<Company> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
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
            try
            {
                _validator.Validate(company);
            }
            catch (CompanyValidationException ex)
            {
                ModelState.AddModelError(ex.Field, ex.Message);
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
            try
            {
                _validator.Validate(company);
            }
            catch (CompanyValidationException ex)
            {
                ModelState.AddModelError(ex.Field, ex.Message);
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                TempData["success"] = "Company updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().OrderBy(p => p.Name).ToList();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToDelete = _unitOfWork.Company.Get(p => p.Id == id);
            if (companyToDelete is null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleting successfull" });
        }

        #endregion
    }
}
