using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
using System.Security.Claims;

namespace PetProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new() 
            { 
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, includeProperties: "Product").ToList()
            };

            foreach (var shoppingCartItem in ShoppingCartVM.ShoppingCartList)
            {
                shoppingCartItem.Price = GetPriceBasedOnQuantity(shoppingCartItem);
                ShoppingCartVM.OrderTotal += shoppingCartItem.Count * shoppingCartItem.Price;
            }

            return View(ShoppingCartVM);
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            return shoppingCart.Count switch
            {
                <= 50 => shoppingCart.Product.Price,
                <= 100 => shoppingCart.Product.Price50,
                _ => shoppingCart.Product.Price100
            };
        }
    }
}
