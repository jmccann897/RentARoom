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
    public class ChatConversationRepository : Repository<ChatConversation>, IChatConversationRepository
    {
        private ApplicationDbContext _db;
        public ChatConversationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId)
        {
            return await _db.ChatConversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<string>> GetConversationIdsByUserIdAsync(string userId)
        {
            return await _db.ChatConversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Select(c => c.ChatConversationId)
                .ToListAsync();
        }
    }
}
