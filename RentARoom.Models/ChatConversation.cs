using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models
{
    public class ChatConversation
    {
        public string ChatConversationId { get; set; } // PK
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? DeletedBy { get; set; } // JSON string representing user IDs who deleted the conversation
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatConversationParticipant> Participants { get; set; } = new List<ChatConversationParticipant>();
        public int? PropertyId { get; set; } // Nullable foreign key

        [ForeignKey("PropertyId")]
        public Property? Property { get; set; } // Nullable navigation property

    }
}
