using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll().OrderBy(p => p.Title).ToList();
            return View(productList);
        }

        //public IActionResult Details()
        //{

        //}

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (product.Title is not null && !char.IsUpper(product.Title[0]))
            {
                ModelState.AddModelError("Title", "'Book Title' must start with capital letter");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product);
                _unitOfWork.Save();
                TempData["success"] = "Book created successfully";
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

            Product? productToEdit = _unitOfWork.Product.Get(p => p.Id == id);
            if (productToEdit is null)
            {
                return NotFound();
            }

            return View(productToEdit);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (product.Title is not null && !char.IsUpper(product.Title[0]))
            {
                ModelState.AddModelError("Title", "'Book Title' must start with capital letter");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Book update successfully";
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

            Product? productToDelete = _unitOfWork.Product.Get(c => c.Id == id);
            if (productToDelete is null)
            {
                return NotFound();
            }

            return View(productToDelete);
        }
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(Product product)
        {
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Book deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
