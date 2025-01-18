using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models
{
    public class ChatMessage
    {
        public string ChatMessageId { get; set; } // PK
        public string ChatConversationId { get; set; } // FK to Conversation table
        public string SenderId { get; set; } // FK to AppUser
        public string RecipientId { get; set; } // FK to AppUser
        public string Content { get; set; } // Message content
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public ChatConversation Conversation { get; set; } // Navigation property
        public ApplicationUser Sender { get; set; } // Navigation property
        public ApplicationUser Recipient { get; set; } // Navigation property

    }
}
