using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.DTOs
{
    public class PropertyDTO
    {
        // If Id is 0, this means a new property; otherwise, update the existing property.
        public int Id { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [MaxLength(8)]
        public string Postcode { get; set; }

        [Range(1, 10000)]
        public int Price { get; set; }

        [Range(1, 100)]
        public int NumberOfBedrooms { get; set; }

        [Range(1, 100)]
        public int NumberOfBathrooms { get; set; }

        [Range(0, 100)]
        public int NumberOfEnsuites { get; set; }

        [Range(1, 1000000)]
        public int FloorArea { get; set; }

        public string City { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // FKs
        [Required]
        public string ApplicationUserId { get; set; }  // FK to User
        [Required]
        public int PropertyTypeId { get; set; }

        public int ViewCount { get; set; }

    }
}
