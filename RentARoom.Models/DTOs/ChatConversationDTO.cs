using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.DTOs
{
    public class ChatConversationDTO
    {
        public string ChatConversationId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientUserName { get; set; }
        
        public string LastMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }

        public int? PropertyId { get; set; } 
        public string PropertyAddress { get; set; } 
        public string PropertyImageUrl { get; set; }
    }
}
