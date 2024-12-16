using Microsoft.EntityFrameworkCore.Metadata;
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
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private ApplicationDbContext _db;
        public LocationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Location obj)
        {
            var objFromDb = _db.Location.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.LocationName = obj.LocationName;
                objFromDb.Address = obj.Address;
                objFromDb.Postcode = obj.Postcode;           
                objFromDb.ApplicationUserId = obj.ApplicationUserId;
                objFromDb.City = obj.City;
               
                
            }
        }
    }
}
