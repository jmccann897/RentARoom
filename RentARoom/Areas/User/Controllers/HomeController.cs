using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Versioning;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Property = RentARoom.Models.Property;
using Coordinate = RentARoom.Models.Coordinate;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITravelTimeService _travelTimeService;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ITravelTimeService travelTimeService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _travelTimeService = travelTimeService;
        }

        public IActionResult Index()
        {
            IEnumerable<Property> propertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
            return View(propertyList);
        }

        public async Task<IActionResult> Details(int id)
        {
            Property property = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "PropertyType,ApplicationUser,Images");

            // Check if user is logged in and has locations saved
            var userLocations = new List<Location>(); // Initialise as empty
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID

                if (!string.IsNullOrEmpty(userId)) // Ensure the userId is not null or empty
                {
                    // Use the Find method to get the user's saved locations
                    userLocations = _unitOfWork.Location.Find(l => l.ApplicationUserId == userId).ToList();
                }           
            }

            // Create a ViewModel to pass both property and user locations
            var viewModel = new PropertyDetailsVM
            {
                Property = property,
                UserLocations = userLocations,
                UserId = userId
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        // https://stackoverflow.com/questions/72476089/how-to-implement-search-functionality
        // POST: /ShowSearchResults
        [HttpPost]
        public IActionResult SearchResults(string SearchType, string SearchPhrase = "")
        {
            // Get all properties
            var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");

            // Filter by search phrase --> defaults to empty string so should skip if empty
            if (!string.IsNullOrEmpty(SearchPhrase))
            {
                properties = _unitOfWork.Property.Find(m => m.Address.Contains(SearchPhrase) ||
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

            var searchAndFilterBarVM = new SearchAndFilterBarVM
            {
                Keywords = SearchPhrase
            };
            ViewData["SearchAndFilterBar"] = searchAndFilterBarVM; // Pass to partial 

            return View("SearchResults", searchResultsVM);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        #region API methods
        [HttpGet]
        public async Task<IActionResult> GetTravelTime(int propertyId, string profile)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not logged in." });
            }


            var userLocations = _unitOfWork.Location.Find(l => l.ApplicationUserId == userId).ToList();
            if (!userLocations.Any())
            {
                return BadRequest(new { message = "No saved locations for the user." });
            }


            var property = _unitOfWork.Property.Get(u => u.Id == propertyId, includeProperties: "PropertyType,ApplicationUser,Images");

            if (property == null)
            {
                return BadRequest(new { message = "Invalid property." });
            }

            // Prepare the location data (user's locations and property location)
            var userCoordinates = userLocations.Select(l => new Coordinate
            { 
                Lat = l.Latitude, 
                Lon = l.Longitude 
            }).ToList();
            var propertyLocation = new Coordinate
            { 
                Lat = property.Latitude, 
                Lon = property.Longitude 
            };


            // Call the service to get travel times
            try
            {
                var result = await _travelTimeService.GetTravelTimesAndDistancesAsync(userCoordinates, propertyLocation, profile);

                Console.WriteLine(result);


                // Extract travel times and distances from the result
                var travelTimes = result.TravelTimes;
                var distances = result.Distances;

                if (travelTimes.Count == 0 || distances.Count == 0)
                {
                    return NotFound(new { message = "No travel times or distances available." });
                }

                return Json(new { success = true, travelTimes, distances });

            }
            catch (Exception ex)
            {
                // Log exception (you can implement your own logging mechanism here)
                return StatusCode(500, new { message = $"An error occurred while calculating travel times & distances: {ex.Message}" });
            }
        }

        #endregion

    }
}
