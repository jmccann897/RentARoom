using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class MapSearchVM
    {
        public List<Location> LocationList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ApplicationUserList { get; set; }
    }
}
