using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models
{
    public class ChatConversationParticipant
    {
        public string ChatConversationId { get; set; } // FK to ChatConversations
        public string UserId { get; set; } // FK to AppUser

        public bool IsActive { get; set; } = true; // Indicates if user  is still part of the conversation

        public ChatConversation ChatConversation { get; set; } // Navigation property
        public ApplicationUser User { get; set; } // Navigation property

    }
}
