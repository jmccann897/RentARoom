using System.ComponentModel.DataAnnotations;

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
        public string Owner { get; set; }

        [Display(Name ="Property Type")]
        public PropertyType PropertyType { get; set; }

        [Display(Name = "Rent")]
        [Range(1,10000)]
        public int Price { get; set; }

        [Display(Name = "Number of Bedrooms")]
        [Range(1, 100)]
        public int NumberOfBedrooms { get; set; }

        [Display(Name = "Floor Area")]
        [Range(1, 1000000)]
        public int FloorArea { get; set; }
        public string ImageUrl { get; set; }
       


        //default constructor
        public Property()
        {
                    
        }
    }
}
