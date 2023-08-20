using Microsoft.AspNetCore.Mvc;
using PetProject.Data;
using PetProject.Models;
using System.Text.RegularExpressions;

namespace PetProject.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _context.Categories.ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name is not null && !Regex.IsMatch(category.Name, @"^[a-zA-Z]+$"))
            {
                ModelState.AddModelError("Name","'Category Name' must only consists of latin letters");
            }
            if (category.Name is not null && !char.IsUpper(category.Name[0]))
            {
                ModelState.AddModelError("Name", "'Category Name' must start with capital letter");
            }
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
