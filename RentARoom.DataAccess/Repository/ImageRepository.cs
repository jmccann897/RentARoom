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
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private ApplicationDbContext _db;
        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Image obj)
        {
            var objFromDb = _db.Image.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.ImageUrl = obj.ImageUrl;
                objFromDb.PropertyId = obj.PropertyId;

            }
        }
    }
}
