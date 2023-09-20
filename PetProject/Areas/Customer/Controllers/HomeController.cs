using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PetProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int? id)
        {
            if (id is null)
                return NotFound();

            Product product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "Category");
            if (product is null)
                return NotFound();

            ShoppingCart shoppingCart = new() { Product = product, Count = 1, ProductId = product.Id };
            return View(shoppingCart);
        }

        [HttpPost]
        // [Autorize] attribure make it forbidden for unautorized(not-logined) users to add items to cart.
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            if (shoppingCart is null)
                return NotFound();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            // Getting Id of user, associated with current execution action.
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;

            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}