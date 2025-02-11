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
        
        public string LastMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }
}
