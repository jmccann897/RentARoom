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
            IEnumerable<Property> propertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser");
            return View(propertyList);
        }

        public IActionResult Details(int id)
        {
            Property property = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "PropertyType,ApplicationUser");
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
            var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser");

            // Search Type
            if (!string.IsNullOrEmpty(SearchType))
            {
                if (SearchType.Equals("Bedroom"))
                {
                    properties = _unitOfWork.Property.Find(m => m.PropertyType.Name.Equals("Bedroom")
                                                        && (m.Address.Contains(SearchPhrase)
                                                        || m.ApplicationUserId.Contains(SearchPhrase)
                                                        || m.Postcode.Contains(SearchPhrase)
                                                        || m.City.Contains(SearchPhrase)));
                }
                else if (SearchType.Equals("House"))
                {
                    properties = _unitOfWork.Property.Find(m => !m.PropertyType.Name.Equals("Bedroom")
                                                        && (m.Address.Contains(SearchPhrase)
                                                        || m.ApplicationUserId.Contains(SearchPhrase)
                                                        || m.Postcode.Contains(SearchPhrase)
                                                        || m.City.Contains(SearchPhrase)));
                }
            }

            // Create SearchResultsVM and populate with PropertyList and default settings for SearchAndFilterBar

            SearchResultsVM searchResultsVM = new();
            searchResultsVM.PropertyList = properties.ToList();
            searchResultsVM.Keywords = SearchPhrase;

            return View("SearchResults", searchResultsVM);


            //    // nested ternary
            //    properties = properties.Where(m =>
            //        SearchType.Equals("bedroom") ? m.PropertyType.Name.Equals("Bedroom") : // searchBedroom
            //        SearchType.Equals("house") ? !m.PropertyType.Name.Equals("Bedroom") : // searchHouse = everything but bedroom
            //        true // searchAll
            //        );
            //}

            //// Search Phrase logic
            //if (!string.IsNullOrEmpty(SearchPhrase))
            //{
            //    properties = _unitOfWork.Property.Find(m => m.Address.Contains(SearchPhrase)
            //                                            || m.ApplicationUserId.Contains(SearchPhrase)
            //                                            || m.Postcode.Contains(SearchPhrase)
            //                                            || m.City.Contains(SearchPhrase));
            //}




        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
