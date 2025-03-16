using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using Location = RentARoom.Models.Location;
using RentARoom.Models.ViewModels;
using System.Security.Claims;
using RentARoom.Services.IServices;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class MapController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapService _mapService;

        public MapController(IUnitOfWork unitOfWork, IMapService mapService)
        {
            _unitOfWork = unitOfWork;
            _mapService = mapService;
        }


        public async Task<IActionResult> MapSearch()
        {
            // Map should be populated with all properties for all users
            // Map should populate with common locations set by admin
            // Map should allow user to create location to add to the map and list container below map
            MapSearchVM mapSearchVM = new()
            {
                // get user --> to check their saved locations
                // get list of locations --> placeholder Getall
                //LocationList = _unitOfWork.Location.GetAll(includeProperties: "ApplicationUser").ToList()
                //List<Location> objLocationList = _unitOfWork.Location.Find(x => x.ApplicationUserId == User.Identity.Name).ToList();
                LocationList = await _mapService.GetMapLocations()

            };

            // Add in Search and filter model
            SearchAndFilterBarVM searchAndFilterBarVM = new();
            ViewData["SearchAndFilterBar"] = searchAndFilterBarVM; // Pass to partial view
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
            var result = _mapService.SaveNewLocation(locationVM, userId);
            return result ? Json(new { success = true, message = "Location saved successfully" })
                : BadRequest(new { success = false, message = "Failed to save Location" });
        }


        [HttpGet("User/Map/GetMapProperties")]
        public IActionResult GetMapProperties()
        {
            var properties = _mapService.GetMapProperties().Result ;
            return Json(new { data = properties });
        }

        [HttpGet("User/Map/GetMapLocations")]
        public IActionResult GetMapLocations()
        {
            var locations = _mapService.GetMapLocations().Result;
            return Json(new { data = locations });
        }

        [HttpGet("User/Map/GetLocation/{id}")]
        public IActionResult GetLocation(int id)
        {
            var location = _mapService.GetLocationById(id);
            return location != null ? Ok(location) : NotFound(new { success = false, message = "Location not found" });
        }

        [HttpPut("User/Map/Edit/{id}")]
        public IActionResult Edit(int id, [FromBody] Location updatedLocation)
        {
            if (updatedLocation == null || id != updatedLocation.Id)
            {
                return BadRequest(new { success = false, message = "Invalid location data or ID mismatch." });
            }

            var result = _mapService.EditLocation(id, updatedLocation);
            return result ? Ok(new { success = true, message = "Location updated successfully" })
                : NotFound(new { success = false, message = "Location not found" });
        }


        [HttpDelete("User/Map/DeleteLocation/{id}")]
        public IActionResult DeleteLocation(int id)
        {
            var result = _mapService.DeleteLocation(id);
            return result ? Ok(new { success = true, message = "Location deleted successfully" })
                : NotFound(new { success = false, message = "Location not found" });
        }

        #endregion

    }
}