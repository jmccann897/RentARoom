﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IPropertyRepository Property { get; }
        IPropertyTypeRepository PropertyType { get; }
        ILocationRepository Location { get;  }
        IImageRepository Image { get; }
        IPropertyViewRepository PropertyView { get; }
        IChatConversationRepository ChatConversations { get; }
        IChatMessageRepository ChatMessages { get; }
        IChatConversationParticipantRepository ChatConversationParticipants { get; }

        void Save();

        Task SaveAsync();
    }
}
