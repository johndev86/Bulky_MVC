using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, "Product")
            };

            return View(orderVM);
        }

        #region

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            switch(status)
            {
                case "pending": objOrderHeaderList = objOrderHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment); break;
                case "inprocess": objOrderHeaderList = objOrderHeaderList.Where(u => u.PaymentStatus == SD.StatusInProcess); break;
                case "completed": objOrderHeaderList = objOrderHeaderList.Where(u => u.PaymentStatus == SD.StatusShipped); break;
                case "approved": objOrderHeaderList = objOrderHeaderList.Where(u => u.PaymentStatus == SD.StatusApproved); break;
                default: break;
            }

            return Json(new { data = objOrderHeaderList });
        }

        #endregion
    }
}
