using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class PropertyVM
    {
        public Property Property { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> PropertyTypeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ApplicationUserList { get; set; }
        public IEnumerable<string> ImageUrls { get; set; } = Enumerable.Empty<string>();
    }
}
