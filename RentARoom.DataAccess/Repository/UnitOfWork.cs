using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
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
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Property = new PropertyRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
