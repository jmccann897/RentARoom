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
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IPropertyRepository Property{ get; private set; }
        public IPropertyTypeRepository PropertyType { get; private set; }
        public ILocationRepository Location { get; private set; }
        public IImageRepository Image { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Property = new PropertyRepository(_db);
            PropertyType = new PropertyTypeRepository(_db);
            Location = new LocationRepository(_db);
            Image = new ImageRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
