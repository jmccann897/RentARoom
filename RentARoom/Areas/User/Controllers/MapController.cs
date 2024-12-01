using Microsoft.AspNetCore.Mvc;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class MapController : Controller
    {
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
    }
}
