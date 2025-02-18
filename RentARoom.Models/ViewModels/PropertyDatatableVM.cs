using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class PropertyDatatableVM
    {
        public List<Property> PropertyList { get; set; }
        
        public ApplicationUser ApplicationUser { get; set; }
    }
}
