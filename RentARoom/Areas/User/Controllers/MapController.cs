using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Utility;
using Property = RentARoom.Models.Property;
using Location = RentARoom.Models.Location;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class MapController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MapController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            return View();
        }
    

        #region API CALLS
        [HttpGet]
        public IActionResult GetMapProperties()
        {
   
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").ToList();
                return Json(new { data = objPropertyList });
            }
        }

        [HttpGet]
        public IActionResult GetMapLocations()
        {

            {
                List<Location> objLocationList = _unitOfWork.Location.GetAll(includeProperties: "ApplicationUser").ToList();
                return Json(new { data = objLocationList });
            }
        }

        #endregion

    }
}