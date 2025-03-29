using RentARoom.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.ViewModels
{
    public class ChatVM
    {
        public string UserId { get; set; }

        public Property? property { get; set; }
        public List<ChatConversationDTO> Conversations { get; set; } = new();
        public ApplicationUser ApplicationUser { get; set; }

        // Allow for property details - nullable
        public string? RecipientEmail { get; set; }
        public string? PropertyAddress { get; set; }
        public string? PropertyCity { get; set; }
        public decimal? PropertyPrice { get; set; }
        public int? PropertyId { get; set; }
        public List<Property> PropertyList { get; set; }
    }
}
