using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Models.DTOs
{
    public class ChatDataDTO
    {
        public string UserId { get; set; }
        public List<ChatConversationDTO> Conversations { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string RecipientEmail { get; set; }

    }
}
