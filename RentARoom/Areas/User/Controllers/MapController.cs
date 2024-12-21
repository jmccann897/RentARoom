using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using Property = RentARoom.Models.Property;
using Location = RentARoom.Models.Location;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentARoom.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using RentARoom.Models;
using RentARoom.DataAccess.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using RentARoom.Utility;
using System.Security.Claims;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class MapController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public MapController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            // Should the api key be hidden - no as no charge associated to simple embed request
            // https://stackoverflow.com/questions/38153734/do-i-need-to-hide-api-key-when-using-google-maps-js-api-if-so-how


            // Need to dynamically change q parameter to property address, add the embedded map as a partial to details pages
            // Need to update the view model in associated details pages and create partial for map view

            return View();
        }

        public IActionResult MapSearch()
        {
            // Either Google Maps JavaScript API with hidden api key
            // OR
            // Leaflet.js with OpenStreetMap tiles


            // Map should be populated with all properties for all users
            // Map should populate with common locations set by admin
            // Map should allow user to create location to add to the map and list container below map

            MapSearchVM mapSearchVM = new()
            {

                // get user --> to check their saved locations

                // get list of locations --> placeholder Getall

                LocationList = _unitOfWork.Location.GetAll(includeProperties: "ApplicationUser").ToList()
                //List<Location> objLocationList = _unitOfWork.Location.Find(x => x.ApplicationUserId == User.Identity.Name).ToList();

            };
            return View(mapSearchVM);
        }

        #region API methods

        [HttpPost("User/Map/SaveNewLocation")]
        public IActionResult SaveNewLocation([FromBody] LocationVM locationVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid location data.");
            }

            // Get the current logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }

            var newLocation = new Location
                {
                    LocationName = locationVM.LocationName,
                    Address = locationVM.Address,
                    City = locationVM.City,
                    Postcode = locationVM.Postcode,
                    Latitude = double.TryParse(locationVM.Latitude,out var lat) ? lat :0,
                    Longitude = double.TryParse(locationVM.Longitude, out var lng) ? lng : 0,
                ApplicationUserId = userId // Associate with logged-in user
                };

                _unitOfWork.Location.Add(newLocation);
                _unitOfWork.Save();

                return Json(new { success = true, message="Location saved successfully" });
        }


        [HttpGet("User/Map/GetMapProperties")]
        public IActionResult GetMapProperties()
        {
   
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").ToList();
                return Json(new { data = objPropertyList });
            }
        }

        [HttpGet("User/Map/GetMapLocations")]
        public IActionResult GetMapLocations()
        {

            {
                List<Location> objLocationList = _unitOfWork.Location.GetAll(includeProperties: "ApplicationUser").ToList();
                return Json(new { data = objLocationList });
            }
        }

        [HttpGet("User/Map/GetLocation/{id}")]
        public IActionResult GetLocation(int id)
        {

            {
                var location = _unitOfWork.Location.Get(l => l.Id == id);
                if (location == null)
                {
                    return NotFound();
                }
                return Ok(location);
            }
        }

        [HttpPut("User/Map/Edit/{id}")]
        public IActionResult Edit(int id, [FromBody] Location updatedLocation)
        {
            if (updatedLocation == null || id != updatedLocation.Id)
            {
                return BadRequest(new { success = false, message = "Invalid location data or ID mismatch." });
            }

            var existingLocation = _unitOfWork.Location.Get(l => l.Id == id);
            if (existingLocation == null)
            {
                return NotFound(new { success = false, message = "Location not found." });
            }

            // Update properties
            existingLocation.LocationName = updatedLocation.LocationName;
            existingLocation.Address = updatedLocation.Address;
            existingLocation.City = updatedLocation.City;
            existingLocation.Postcode = updatedLocation.Postcode;
            existingLocation.Latitude = updatedLocation.Latitude;
            existingLocation.Longitude = updatedLocation.Longitude;

            // Save changes
            _unitOfWork.Location.Update(existingLocation);
            _unitOfWork.Save();

            return Ok(new { success = true, message = "Location updated successfully", data = existingLocation });
        }


        [HttpDelete("User/Map/DeleteLocation/{id}")]
        public IActionResult DeleteLocation(int id)
        {
            var location = _unitOfWork.Location.Get(l => l.Id == id);

            if (location == null)
            {
                TempData["error"] = "Location not found";
                return NotFound(new { success = false, message = "Location not found." });
            }

            _unitOfWork.Location.Remove(location);
            _unitOfWork.Save();

            //TempData["success"] = "Location successfully deleted."; // bit odd as page doesnt reload on success so get notification late
            return Ok(new { success = true, message = "Location deleted successfully." });
        }

        #endregion

    }
}