using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
using PetProject.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderViewModel = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(h => h.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(d => d.OrderHeaderId == orderId, includeProperties: "Product")
            };
          
            
            return View(OrderViewModel);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail(int orderId)
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(h => h.Id == OrderViewModel.OrderHeader.Id);

            // Updating properties of OrderHeader.
            orderHeaderFromDB.Name = OrderViewModel.OrderHeader.Name;
            orderHeaderFromDB.PhoneNumber = OrderViewModel.OrderHeader.PhoneNumber;
            orderHeaderFromDB.StreetAddress = OrderViewModel.OrderHeader.StreetAddress;
            orderHeaderFromDB.City = OrderViewModel.OrderHeader.City;
            orderHeaderFromDB.State = OrderViewModel.OrderHeader.State;
            orderHeaderFromDB.PostalCode = OrderViewModel.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderViewModel.OrderHeader.Carrier))
                orderHeaderFromDB.Carrier = OrderViewModel.OrderHeader.Carrier;
            if (!string.IsNullOrEmpty(OrderViewModel.OrderHeader.TrackingNumber))
                orderHeaderFromDB.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
            _unitOfWork.Save();

            TempData["success"] = "Order Deatails Updated Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDB.Id });
        }

        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = _unitOfWork.OrderHeader.GetAll(h => h.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusPending);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusProcessing);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusApproved);
                    break;
                case "all":
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
