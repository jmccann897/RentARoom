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


        //default constructor
        public Property()
        {
                    
        }
    }
}
