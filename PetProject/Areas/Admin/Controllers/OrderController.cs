using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using PetProject.Utility;
using System.Diagnostics;

namespace PetProject.Areas.Admin.Controllers
{
    [Area("admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        // Method for retrieving all entities from db, and returning JSON file with data entites, that is 
        // displayed in DataTable on Index page.
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

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
