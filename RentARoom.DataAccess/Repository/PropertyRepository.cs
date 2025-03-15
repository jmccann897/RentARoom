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
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        private ApplicationDbContext _db;
        public PropertyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Property obj)
        {
            var objFromDb = _db.Property.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Address = obj.Address;
                objFromDb.Postcode = obj.Postcode;
                objFromDb.Price = obj.Price;
                objFromDb.NumberOfBedrooms = obj.NumberOfBedrooms;
                objFromDb.NumberOfBathrooms = obj.NumberOfBathrooms;
                objFromDb.NumberOfEnsuites = obj.NumberOfEnsuites;
                objFromDb.FloorArea = obj.FloorArea;
                objFromDb.City = obj.City;
                objFromDb.Latitude = obj.Latitude;
                objFromDb.Longitude = obj.Longitude;
                objFromDb.PropertyTypeId = obj.PropertyTypeId;
                objFromDb.ApplicationUserId = obj.ApplicationUserId;
            }
        }
    }
}
