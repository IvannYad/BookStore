using Microsoft.AspNetCore.Mvc;
using PetProject.DataAccess.Repository;
using PetProject.Models;

namespace PetProject.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public OrderController(UnitOfWork unitOfWork)
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
        public IActionResult GetAll()
        {
            List<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
