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
        public IActionResult BasicChat()
        {

            return View();
        }
    }
}
