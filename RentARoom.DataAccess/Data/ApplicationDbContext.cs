using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RentARoom.Models;

namespace RentARoom.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Property> Property { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>().HasData(
                new Models.Property { Id = 1, Postcode = "BT71 4PT", Address = "24 Kings Row", Owner = "Clanmill Housing" },
                new Models.Property { Id = 2, Postcode = "BT12 7BN", Address = "17 Gortfin Street", Owner = "Private" },
                new Models.Property { Id = 3, Postcode = "BT9 5BN", Address = "16A Malone Road", Owner = "QUB" }
                );
        }
    }
}
