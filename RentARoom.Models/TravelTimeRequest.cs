using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentARoom.Models
{
    public class TravelTimeRequest
    {
        public string Profile { get; set; } // "driving-car", "foot-walking", etc.
        public double originLat { get; set; }
        public double originLon { get; set; }
        public double destLat { get; set; }
        public double destLon { get; set; }
    }
}
