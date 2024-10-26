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
        public DbSet<PropertyType> PropertyType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PropertyType>().HasData(
                new Models.PropertyType { Id = 1, Name = "Terrace" },
                new Models.PropertyType { Id = 2, Name = "Semi Detached" },
                new Models.PropertyType { Id = 3, Name = "Detached" },
                new Models.PropertyType { Id = 4, Name = "Apartment" },
                new Models.PropertyType { Id = 5, Name = "Bungalow" }
               );
        
            modelBuilder.Entity<Property>().HasData(
                new Models.Property { Id = 1, Postcode = "BT71 4PT", Address = "24 Kings Row", Owner = "Clanmill Housing", PropertyTypeId=1, Price=400, NumberOfBedrooms=3, FloorArea=83, ImageUrl=""},
                new Models.Property { Id = 2, Postcode = "BT12 7BN", Address = "17 Gortfin Street", Owner = "Private", PropertyTypeId=3, Price = 700, NumberOfBedrooms = 4, FloorArea = 151, ImageUrl = "" },
                new Models.Property { Id = 3, Postcode = "BT9 5BN", Address = "16A Malone Road", Owner = "QUB", PropertyTypeId=4, Price = 1000, NumberOfBedrooms = 1, FloorArea = 57, ImageUrl = "" }
                );
        }
    }
}
