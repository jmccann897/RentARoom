using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class PropertyDetailsVM
    {
        public Property Property { get; set; }
        public List<Location> UserLocations { get; set; }

        public string UserId { get; set; }
    }
}
