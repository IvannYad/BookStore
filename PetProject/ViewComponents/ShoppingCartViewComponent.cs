using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Utility;
using System.Security.Claims;

namespace PetProject.ViewComponents
{
    public class ShoppingCartViewComponent: ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is not null)
            {
                // Setting number of products is cart of user current session.
                if (HttpContext.Session.GetInt32(SD.SessionCart) is null)
                {
                    // If session is null got to db and change value.
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value).Count());
                }
                
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
            
        }
    }
}
