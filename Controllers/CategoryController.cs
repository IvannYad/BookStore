using Microsoft.AspNetCore.Mvc;

namespace PetProject.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
