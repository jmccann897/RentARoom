using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentARoom.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        // Foreign key for property
        [Display(Name = "Property Id")]
        public int PropertyId { get; set; }

        // Navigation property
        [ForeignKey("PropertyId")]
        [ValidateNever]
        public Property Property { get; set; }

    }
}
