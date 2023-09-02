using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
using System.Text.RegularExpressions;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll().OrderBy(p => p.Title).ToList();
            
            return View(productList);
        }

        //public IActionResult Details()
        //{

        //}

        // Combined Create and Update.
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

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // Delete old image.
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.Trim('\\'));

                        // Delete image if exists.
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using FileStream fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                    file.CopyTo(fileStream);

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                // Executing when user, creating new product, not specified file input
                if (string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    return ShowPageOnUnsuccessfullOperation(productVM);
                }

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

        public IActionResult Delete(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            ProductVM productToDeleteVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = _unitOfWork.Product.Get(p => p.Id == id)
            };

            if (productToDeleteVM.Product is null)
            {
                return NotFound();
            }

            return View(productToDeleteVM);
        }
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePost(ProductVM productVM)
        {
            _unitOfWork.Product.Remove(productVM.Product);
            _unitOfWork.Save();
            TempData["success"] = "Book deleted successfully";
            return RedirectToAction("Index");
        }

        // Mathod for displaying page when some operation is unnsuccessfull.
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
    }
}
