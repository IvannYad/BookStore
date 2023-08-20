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
            List<Category> categoryList = _context.Categories.OrderBy(c => c.DisplayOrder).ToList();
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

        public IActionResult Edit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Category? categoryToEdit = _context.Categories.Find(id);
            if (categoryToEdit is null)
            {
                return NotFound();
            }

            return View(categoryToEdit);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name is not null && !Regex.IsMatch(category.Name, @"^[a-zA-Z]+$"))
            {
                ModelState.AddModelError("Name", "'Category Name' must only consists of latin letters");
            }

            if (category.Name is not null && !char.IsUpper(category.Name[0]))
            {
                ModelState.AddModelError("Name", "'Category Name' must start with capital letter");
            }

            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
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

            Category? categoryToDelete = _context.Categories.Find(id);
            if (categoryToDelete is null)
            {
                return NotFound();
            }

            return View(categoryToDelete);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category categoryToDelete = _context.Categories.Find(id);
            if (categoryToDelete is null)
            {
                return NotFound();
            }

            _context.Categories.Remove(categoryToDelete);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
