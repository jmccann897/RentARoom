using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Identity.Client;
using RentARoom.Models;

namespace RentARoom.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        //constructor which passes options to base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
              
        }
        public DbSet<Property> Property { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Property>().HasData(
                new Models.Property { Id = 1, Postcode = "BT71 4PT", Address = "24 Kings Row", Owner = "Clanmill Housing", PropertyType = PropertyType.Terrace, Price=400, NumberOfBedrooms=3, FloorArea=83, ImageUrl=""},
                new Models.Property { Id = 2, Postcode = "BT12 7BN", Address = "17 Gortfin Street", Owner = "Private", PropertyType = PropertyType.Detached, Price = 700, NumberOfBedrooms = 4, FloorArea = 151, ImageUrl = "" },
                new Models.Property { Id = 3, Postcode = "BT9 5BN", Address = "16A Malone Road", Owner = "QUB", PropertyType = PropertyType.Apartment, Price = 1000, NumberOfBedrooms = 1, FloorArea = 57, ImageUrl = "" }
                );
        }
    }
}
