using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Utility;
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
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int? id)
        {
            if (id is null)
                return NotFound();

            Product product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "Category,ProductImages");
            if (product is null)
                return NotFound();

            ShoppingCart shoppingCart = new() { Product = product, Count = 1, ProductId = product.Id };
            return View(shoppingCart);
        }

        [HttpPost]
        // [Autorize] attribure make it forbidden for unauthenticated(not-logined) users to add items to cart.
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            if (shoppingCart is null)
                return NotFound();

            // Getting Id of user, associated with current execution action.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;

            var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.ApplicationUserId == userId && c.ProductId == shoppingCart.ProductId);

            
            if (cartFromDb is not null)
            {
                // If we already have order of given user with given product, update order, rather than create new.
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                // Add new shopping cart record.
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();
            
            // Setting number of products is cart of user current session.
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId).Count());

            TempData["success"] = "Cart updated successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Filter(string bookName
            , string author
            , string category
            , double fromPrice
            , double toPrice)
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            if (bookName is not null && !string.IsNullOrEmpty(bookName!.Trim()))
                productList = productList.Where(p => p.Title.ToLower().Contains(bookName.ToLower()));
            if (author != "All")
                productList = productList.Where(p => p.Author.Contains(author));
            if (category != "All")
                productList = productList.Where(p => p.Category.Name.Contains(category));
            if (fromPrice != -1)
                productList = productList.Where(p => p.Price100 >= fromPrice);
            if (toPrice != -1)
                productList = productList.Where(p => p.Price100 <= toPrice);

            return View(nameof(Index), productList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}