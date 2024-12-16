using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentARoom.Models
{
    public class Location
    {
        //class fields
        public int Id { get; set; }
        [Required]
        public string LocationName { get; set; }
        
        [Required]
        public string Address { get; set; }
        [Required]
        [MaxLength(8)]
        public string Postcode { get; set; }
        public string City { get; set; }

        // Foreign key for ApplicationUser
        [Display(Name = "Application User Id")]
        public string ApplicationUserId { get; set; }

        // Navigation property
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        // https://stackoverflow.com/questions/28068123/double-or-decimal-for-latitude-longitude-values-in-c-sharp
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        //default constructor
        public Location()
        {           
        }
    }
}
