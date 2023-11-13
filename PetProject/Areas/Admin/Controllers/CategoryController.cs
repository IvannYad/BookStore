using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Data;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Services.Interfaces;
using PetProject.Utility;
using PetProject.Utility.CustomExceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Make sure that only user with role admin can access all action methods of controller.
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Category> _validator;

        public CategoryController(IUnitOfWork unitOfWork, IValidator<Category> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _unitOfWork.Category.GetAll().OrderBy(c => c.DisplayOrder).ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            try
            {
                _validator.Validate(category);
            }
            catch (CategoryValidationException ex)
            {
                ModelState.AddModelError(ex.Field, ex.Message);
            }
            
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Category? categoryToEdit = _unitOfWork.Category.Get(c => c.Id == id);
            if (categoryToEdit is null)
            {
                return NotFound();
            }

            return View(categoryToEdit);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            try
            {
                _validator.Validate(category);
            }
            catch (CategoryValidationException ex)
            {
                ModelState.AddModelError(ex.Field, ex.Message);
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Category? categoryToDelete = _unitOfWork.Category.Get(c => c.Id == id);
            if (categoryToDelete is null)
            {
                return NotFound();
            }

            return View(categoryToDelete);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category categoryToDelete = _unitOfWork.Category.Get(c => c.Id == id);
            if (categoryToDelete is null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(categoryToDelete);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
