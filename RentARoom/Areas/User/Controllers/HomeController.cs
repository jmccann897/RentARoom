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
            //Property property = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "PropertyType,ApplicationUser,Images,PropertyViews");

            Property property = await _propertyService.GetPropertyAsync(id);

            if (property == null) { return NotFound(); }

            // Log the property view
            //var propertyView = new PropertyView
            //{
            //    PropertyId = id,
            //    ViewedAt = DateTime.UtcNow
            //};

            //// Save view to DB
            //_unitOfWork.PropertyView.Add(propertyView);
            //_unitOfWork.Save();

            //// Get updated view count
            //int totalViews = _unitOfWork.PropertyView.Find(v => v.PropertyId == id).Count();

            //// Check if user is logged in and has locations saved
            //var userLocations = new List<Location>(); // Initialise as empty
            //string userId = "";
            //if (User.Identity.IsAuthenticated)
            //{
            //    userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID

            //    if (!string.IsNullOrEmpty(userId)) // Ensure the userId is not null or empty
            //    {
            //        // Use the Find method to get the user's saved locations
            //        userLocations = _unitOfWork.Location.Find(l => l.ApplicationUserId == userId).ToList();
            //    }           
            //}

            //// Create a ViewModel to pass both property and user locations
            //var viewModel = new PropertyDetailsVM
            //{
            //    Property = property,
            //    UserLocations = userLocations,
            //    UserId = userId,
            //    TotalViews = totalViews
            //};

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
        public async Task<IActionResult> SearchResults(string searchType, string searchPhrase = "")
        {
            // Get all properties
            //var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");

            var properties = await _propertyService.SearchPropertiesAsync(searchType, searchPhrase);

            var searchResultsVM = new SearchResultsVM
            {
                PropertyList = properties.ToList()
            };

            var searchAndFilterBarVM = new SearchAndFilterBarVM
            {
                Keywords = searchPhrase
            };
            ViewData["SearchAndFilterBar"] = searchAndFilterBarVM;
            return View("SearchResults", searchResultsVM);

            //// Filter by search phrase --> defaults to empty string so should skip if empty
            //if (!string.IsNullOrEmpty(searchPhrase))
            //{
            //    properties = _unitOfWork.Property.Find(m => m.Address.Contains(searchPhrase) ||
            //                                                    m.ApplicationUserId.Contains(searchPhrase) ||
            //                                                    m.Postcode.Contains(searchPhrase) ||
            //                                                    m.City.Contains(searchPhrase));
            //}
            //// Filter by Search Type
            //if (!string.IsNullOrEmpty(searchType))
            //{
            //    if (searchType.Equals("Bedroom"))
            //    {
            //        properties = _unitOfWork.Property.Find(m => m.PropertyType.Name.Equals("Bedroom"));
                                                    
            //    }
            //    else if (searchType.Equals("House"))
            //    {
            //        properties = _unitOfWork.Property.Find(m => !m.PropertyType.Name.Equals("Bedroom"));
            //    }
            //}

            // Create SearchResultsVM and populate with PropertyList and default settings for SearchAndFilterBar

            //SearchResultsVM searchResultsVM = new();
            //searchResultsVM.PropertyList = properties.ToList();

            //var searchAndFilterBarVM = new SearchAndFilterBarVM
            //{
            //    Keywords = searchPhrase
            //};
            //ViewData["SearchAndFilterBar"] = searchAndFilterBarVM; // Pass to partial 

            //return View("SearchResults", searchResultsVM);
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


            //var userLocations = _unitOfWork.Location.Find(l => l.ApplicationUserId == userId).ToList();
            //if (!userLocations.Any())
            //{
            //    return BadRequest(new { message = "No saved locations for the user." });
            //}


            //var property = _unitOfWork.Property.Get(u => u.Id == propertyId, includeProperties: "PropertyType,ApplicationUser,Images");

            //if (property == null)
            //{
            //    return BadRequest(new { message = "Invalid property." });
            //}

            //// Prepare the location data (user's locations and property location)
            //var userCoordinates = userLocations.Select(l => new Coordinate
            //{ 
            //    Lat = l.Latitude, 
            //    Lon = l.Longitude 
            //}).ToList();
            //var propertyLocation = new Coordinate
            //{ 
            //    Lat = property.Latitude, 
            //    Lon = property.Longitude 
            //};


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
                return StatusCode(500, new { message = $"An error occurred while calculating travel times & distances: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetViewsPerDay(int propertyId)
        {
            //var viewsPerDay = _unitOfWork.PropertyView
            //    .Find(pv => pv.PropertyId == propertyId)
            //    .GroupBy(pv => pv.ViewedAt.Date)
            //    .Select(g => new
            //    {
            //        Date = g.Key.ToString("yyyy-MM-dd"),
            //        Views = g.Count()
            //    })
            //    .OrderBy(v => v.Date)
            //    .ToList();

            var viewsPerDay = _propertyViewService.GetViewsPerDay(propertyId);

            return Json(viewsPerDay);
        }


        #endregion

    }
}
