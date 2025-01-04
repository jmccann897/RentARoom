using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Versioning;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
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
            IEnumerable<Property> propertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
            return View(propertyList);
        }

        public IActionResult Details(int id)
        {
            Property property = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "PropertyType,ApplicationUser,Images");
            return View(property);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        // https://stackoverflow.com/questions/72476089/how-to-implement-search-functionality
        // POST: /ShowSearchResults
        public IActionResult SearchResults(string SearchType, string SearchPhrase)
        {
            // Get all properties
            var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");

            // Filter by search phrase
            if (!string.IsNullOrEmpty(SearchPhrase))
            {
                properties = properties = _unitOfWork.Property.Find(m => m.Address.Contains(SearchPhrase) ||
                                                                m.ApplicationUserId.Contains(SearchPhrase) ||
                                                                m.Postcode.Contains(SearchPhrase) ||
                                                                m.City.Contains(SearchPhrase));
            }
            // Filter by Search Type
            if (!string.IsNullOrEmpty(SearchType))
            {
                if (SearchType.Equals("Bedroom"))
                {
                    properties = _unitOfWork.Property.Find(m => m.PropertyType.Name.Equals("Bedroom"));
                                                    
                }
                else if (SearchType.Equals("House"))
                {
                    properties = _unitOfWork.Property.Find(m => !m.PropertyType.Name.Equals("Bedroom"));
                }
            }

            // Create SearchResultsVM and populate with PropertyList and default settings for SearchAndFilterBar

            SearchResultsVM searchResultsVM = new();
            searchResultsVM.PropertyList = properties.ToList();

            SearchAndFilterBarVM searchAndFilterBarVM = new();
            searchAndFilterBarVM.Keywords = SearchPhrase;
            ViewData["SearchAndFilterBar"] = searchAndFilterBarVM; // Pass to partial view

            return View("SearchResults", searchResultsVM);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
