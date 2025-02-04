using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models
{
    public class PropertyView
    {
        public int Id { get; set; }
        // FK to property
        public int PropertyId { get; set; }

        // Nav property
        public Property Property { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
