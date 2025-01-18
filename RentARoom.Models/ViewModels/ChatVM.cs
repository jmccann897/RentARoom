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
        public List<string> ConversationIds { get; set; }

        public Property? property { get; set; }
    }
}
