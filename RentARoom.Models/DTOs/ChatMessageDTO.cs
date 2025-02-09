using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.DTOs
{
    public class ChatMessageDTO
    {
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
