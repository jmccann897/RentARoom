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

        void Save();
    }
}