using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentARoom.Models
{
    public class Property
    {
        //class fields
        public int Id { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [MaxLength(8)]
        public string Postcode { get; set; }

        [Display(Name = "Rent")]
        [Range(1,10000)]
        public int Price { get; set; }

        [Display(Name = "Number of Bedrooms")]
        [Range(1, 100)]
        public int NumberOfBedrooms { get; set; }

        [Display(Name = "Number of Bathrooms")]
        [Range(1, 100)]
        public int NumberOfBathrooms { get; set; }

        [Display(Name = "Floor Area")]
        [Range(1, 1000000)]
        public int FloorArea { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }

        [Display(Name = "Property Type")]
        public int PropertyTypeId { get; set; }
        [ForeignKey("PropertyTypeId")]
        [ValidateNever]
        public PropertyType PropertyType { get; set; }
        public String City { get; set; }

        // Foreign key for ApplicationUser
        [Display(Name = "Application User Id")]
        public string ApplicationUserId { get; set; }

        // Navigation property
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        //default constructor
        public Property()
        {
                    
        }
    }
}
