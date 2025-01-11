using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult Chat(string recipientEmail, string propertyAddress, string propertyCity, decimal propertyPrice)
        {
            ViewBag.RecipientEmail = string.IsNullOrEmpty(recipientEmail) ? string.Empty : recipientEmail;
            ViewBag.PropertyAddress = string.IsNullOrEmpty(propertyAddress) ? string.Empty : propertyAddress;
            ViewBag.PropertyCity = string.IsNullOrEmpty(propertyCity) ? string.Empty : propertyCity;
            if (!propertyPrice.Equals(null))
            {
                ViewBag.PropertyPrice = propertyPrice;
            }

            return View();
            
            
        }
    }
}
