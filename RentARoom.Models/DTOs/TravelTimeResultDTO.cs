using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.DTOs
{
    public class TravelTimeResultDTO
    {
        public List<double> TravelTimes { get; set; }
        public List<double> Distances { get; set; }
    }
}
