using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class LocationVM
    {
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ApplicationUserList { get; set; }
    }
}
