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
    public class PropertyTypeRepository : Repository<PropertyType>, IPropertyTypeRepository
    {
        private ApplicationDbContext _db;
        public PropertyTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        
        public void Update(PropertyType obj)
        {
            _db.PropertyType.Update(obj); ;
        }

        
    }
}
