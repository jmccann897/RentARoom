﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class SearchResultsVM
    {
        public List<Property> PropertyList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> PropertyTypeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ApplicationUserList { get; set; }

    }
}
