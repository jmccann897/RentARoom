using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        void Update(Image obj);
    }
}
