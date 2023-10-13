using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
using PetProject.Utility;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Make sure that only user with role admin can access all action methods of controller.
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        // Field for saving or deleting file(e.g. image) in project folder.
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // Method for displaying list of Products.
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };
            if (id is null or 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(p => p.Id == id);
                return View(productVM);
            }
        }

        // Method executes on clicking create(submit) of update(submit) button on Create or Edit web-page.
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (productVM.Product.Title is not null && !char.IsUpper(productVM.Product.Title[0]))
            {
                ModelState.AddModelError("Title", "'Book Title' must start with capital letter");
            }
            if (ModelState.IsValid)
            {
                // When user chooses file(image), it is saved to 'wwwrool\images\product' folder with unique name
                // 'fileName' and then url of image(it`s path in wwwroot folder) is saved to 'ImageUrl' of Product model.
                if (file is not null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;

                    // Creating unique name for file with image.
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    // Path to folder, where all images of Product will be stored.
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    //if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    //{
                    //    // Path to image to be deleted.
                    //    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.Trim('\\'));

                    //    // Delete image if exists.
                    //    if (System.IO.File.Exists(oldImagePath))
                    //    {
                    //        System.IO.File.Delete(oldImagePath);
                    //    }
                    //}
                    //using FileStream fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                    //file.CopyTo(fileStream);

                    //productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                // Executing when user, creating new product, not specified file input
                //if (string.IsNullOrEmpty(productVM.Product.ImageUrl))
                //{
                //    return ShowPageOnUnsuccessfullOperation(productVM);
                //}

                if (productVM.Product.Id is not 0)
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Book updated successfully";
                }
                else
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Book created successfully";
                }
                
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return ShowPageOnUnsuccessfullOperation(productVM);
            }
        }

        // Method for displaying page when some operation is unnsuccessfull.
        public IActionResult ShowPageOnUnsuccessfullOperation(ProductVM productVM)
        {
            TempData["error"] = "Error";
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
        
            return View(productVM);
        }

        
        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").OrderBy(p => p.Title).ToList();
            return Json(new { data = productList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToDelete = _unitOfWork.Product.Get(p => p.Id == id);
            if (productToDelete is null)
            {
                return Json(new { success = false, message = "Error while deleting"});
            }

            //// Path to image to be deleted.
            //var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.ImageUrl.Trim('\\'));

            //// Delete image if exists.
            //if (System.IO.File.Exists(oldImagePath))
            //{
            //    System.IO.File.Delete(oldImagePath);
            //}

            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleting successful" });
        }

        #endregion
    }
}
