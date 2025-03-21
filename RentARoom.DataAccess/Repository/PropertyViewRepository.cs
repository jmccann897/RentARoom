﻿using Microsoft.EntityFrameworkCore.Metadata;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Repository
{
    public class PropertyViewRepository : Repository<PropertyView>, IPropertyViewRepository
    {
        private ApplicationDbContext _db;
        public PropertyViewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
