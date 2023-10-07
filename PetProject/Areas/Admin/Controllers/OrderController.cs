using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Models.ViewModels;
using PetProject.Utility;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
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
        [ActionName("Details")]
        public IActionResult DetailsPayNow()
        {
            OrderViewModel.OrderHeader = _unitOfWork.OrderHeader
                .Get(h => h.Id == OrderViewModel.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderViewModel.OrderDetails = _unitOfWork.OrderDetail
                .GetAll(d => d.OrderHeaderId == OrderViewModel.OrderHeader.Id, includeProperties: "Product");

            string domain = "https://localhost:7176/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/Details?orderId={OrderViewModel.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            // Stripe logic.
            foreach (var item in OrderViewModel.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(h => h.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                // Order placed bu a company.
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            return View(orderHeaderId);
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

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderViewModel.OrderHeader.Id, SD.StatusProcessing);
            _unitOfWork.Save();
            TempData["success"] = "Order Deatails Updated Successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(h => h.Id == OrderViewModel.OrderHeader.Id);

            orderHeaderFromDB.Carrier = OrderViewModel.OrderHeader.Carrier;
            orderHeaderFromDB.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
            orderHeaderFromDB.OrderStatus = SD.StatusShipped;
            orderHeaderFromDB.ShippingDate = DateTime.Now;
            if (orderHeaderFromDB.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDB.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(h => h.Id == OrderViewModel.OrderHeader.Id);
            if (orderHeaderFromDB.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDB.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCanceled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCanceled, SD.StatusCanceled);
            }
            
            _unitOfWork.Save();
            TempData["success"] = "Order canceled Successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
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
