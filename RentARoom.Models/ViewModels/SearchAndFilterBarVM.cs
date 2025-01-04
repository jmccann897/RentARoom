using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class SearchAndFilterBarVM
    {

        // Search and Filter Bar 
        public string Keywords { get; set; }
        public string SortBy { get; set; }
        public string? PropertyType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
