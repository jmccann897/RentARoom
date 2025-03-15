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
    public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
    {
        private ApplicationDbContext _db;
        public ChatMessageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string conversationId)
        {
            return await _db.ChatMessages
            .Where(msg => msg.ChatConversationId == conversationId)
            .OrderByTimestamp()
            .ToListAsync();
        }
    }

    public static class QueryExtensions
    {
        public static IQueryable<ChatMessage> OrderByTimestamp(this IQueryable<ChatMessage> query)
        {
            return query.OrderBy(m => m.Timestamp);
        }
    }

}
