using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostCode { get; set; }

        // Navigation property for related Properties
        // not virtual to avoid lazy loading
        // initialises on get so can't be null by default - see https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public ICollection<Property> Properties { get; } = new List<Property>();

        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatConversationParticipant> ChatConversationParticipants { get; set; } = new List<ChatConversationParticipant>();

    }
}
