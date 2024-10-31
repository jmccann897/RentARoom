using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using System.Diagnostics;

namespace RentARoom.Controllers
{
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
            IEnumerable<Property> propertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType");
            return View(propertyList);
        }

        public IActionResult Details(int id)
        {
            Property property = _unitOfWork.Property.Get(u => u.Id == id,includeProperties: "PropertyType");
            return View(property);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        //https://stackoverflow.com/questions/72476089/how-to-implement-search-functionality
        // POST: /ShowSearchResults
        public IActionResult SearchResults(String SearchPhrase)
        {
            //ViewData["CurrentFilter"] = SearchPhrase;

            var properties = _unitOfWork.Property.GetAll();

            if (!string.IsNullOrEmpty(SearchPhrase))
            {
                properties = _unitOfWork.Property.Find(m => m.Address.Contains(SearchPhrase)
                                                     || m.Owner.Contains(SearchPhrase)
                                                     || m.Postcode.Contains(SearchPhrase)) ;
            }

            return View("SearchResults",properties.ToList());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
