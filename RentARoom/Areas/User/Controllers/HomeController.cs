using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using Property = RentARoom.Models.Property;
using RentARoom.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITravelTimeService _travelTimeService;
        private readonly IHubContext<PropertyViewHub> _propertyViewHub;
        private readonly IPropertyService _propertyService;
        private readonly IPropertyViewService _propertyViewService;
        private readonly ILocationService _locationService;

        public HomeController(
            ILogger<HomeController> logger, 
            IUnitOfWork unitOfWork,
            ITravelTimeService travelTimeService,
            IHubContext<PropertyViewHub> propertyViewHub,
            IPropertyService propertyService, IPropertyViewService propertyViewService, ILocationService locationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _travelTimeService = travelTimeService;
            _propertyViewHub = propertyViewHub;
            _propertyService = propertyService;
            _propertyViewService = propertyViewService;
            _locationService = locationService;
         }

        public IActionResult Index()
        {
            //IEnumerable<Property> propertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
            var propertyList = _propertyService.GetAllProperties();
            return View(propertyList);
        }

        public async Task<IActionResult> Details(int id)
        {
            Property property = await _propertyService.GetPropertyAsync(id);

            if (property == null) { return NotFound(); }

            int totalViews = await _propertyViewService.LogPropertyViewAsync(id);

            // Notify all clients about the new view count
            await _propertyViewHub.Clients.All.SendAsync("UpdateViewCount", id, totalViews);

            var viewModel = new PropertyDetailsVM
            {
                Property = property,
                TotalViews = totalViews
            };

            // Get the user ID from the authenticated user
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var userLocations = _locationService.GetUserLocations(userId);

                // Add user locations to the view model
                viewModel.UserLocations = userLocations.ToList();  // Convert to list
                viewModel.UserId = userId;
            }
            else
            {
                // If user is not logged in, initialize an empty list for user locations
                viewModel.UserLocations = new List<Location>();
            }
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        // https://stackoverflow.com/questions/72476089/how-to-implement-search-functionality
        // POST: /ShowSearchResults
        [HttpPost]
        public async Task<IActionResult> SearchResults(string searchType, string searchPhrase = "", string sortBy = "")
        {
            var properties = await _propertyService.SearchPropertiesAsync(searchType, searchPhrase);

            // Apply sorting
            properties = sortBy switch
            {
                "PriceLowHigh" => properties.OrderBy(p => p.Price),
                "PriceHighLow" => properties.OrderByDescending(p => p.Price),
                _ => properties
            };

            var searchResultsVM = new SearchResultsVM
            {
                PropertyList = properties.ToList(),
                Keywords = searchPhrase,
                SortBy = sortBy,
                SearchType = searchType
            };
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

            // Call the service to get travel times
            try
            {
                var result = await _travelTimeService.GetTravelTimeAsync(userId, propertyId, profile);
                Console.WriteLine(result);

                // Check if the travel times and distances are valid collections
                if (result.TravelTimes == null || result.Distances == null || result.TravelTimes.Count == 0 || result.Distances.Count == 0)
                {
                    return NotFound(new { message = "No travel times or distances available." });
                }
                return Json(new { success = true, travelTimes = result.TravelTimes, distances = result.Distances });

            }
            catch (Exception ex)
            {
                // Log exception (you can implement your own logging mechanism here)
                return StatusCode(500, new { message = $"An error occurred while calculating travel times and distances: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetViewsPerDay(int propertyId)
        {
            var viewsPerDay = _propertyViewService.GetViewsPerDay(propertyId);
            return Json(viewsPerDay);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestionsAsync(string searchType, string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return NoContent();

            if (searchType != "House" && searchType != "Bedroom")
            {
                searchType = "All";
            }
            // Filter locations by city/postcode/address matching the term
            var matchingProperties = await _propertyService.SearchPropertiesAsync(searchType, userInput);

            var suggestions = matchingProperties
                .Select(p => new[] { p.City, p.Postcode, p.Address })
                .SelectMany(x => x)
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.Contains(userInput, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Take(10);

           
            return Json(suggestions);
        }

        #endregion

    }
}
