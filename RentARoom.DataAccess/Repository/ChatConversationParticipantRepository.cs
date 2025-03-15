using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Repository
{
    public class ChatConversationParticipantRepository : Repository<ChatConversationParticipant>, IChatConversationParticipantRepository
    {
        private ApplicationDbContext _db;
        public ChatConversationParticipantRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId)
        {
            return await _db.ChatConversationParticipants
                .AnyAsync(participant => participant.UserId == userId && participant.ChatConversationId == conversationId);
        }

    }
}
