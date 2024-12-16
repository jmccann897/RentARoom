using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Identity.Client;
using RentARoom.Models;

namespace RentARoom.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //constructor which passes options to base class
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
              
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Property> Property { get; set; }
        public DbSet<PropertyType> PropertyType { get; set; }
        public DbSet<Location> Location { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //needed for identities to build properly

            //https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many
            //https://stackoverflow.com/questions/62110667/cannot-resolve-symbol-hasrequired-entity-framework-core
            modelBuilder.Entity<Property>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(t => t.Properties)
                .HasForeignKey(m => m.ApplicationUserId)
                .IsRequired();

            // Seed Property Types
            modelBuilder.Entity<PropertyType>().HasData(
                new Models.PropertyType { Id = 1, Name = "Terrace" },
                new Models.PropertyType { Id = 2, Name = "Semi Detached" },
                new Models.PropertyType { Id = 3, Name = "Detached" },
                new Models.PropertyType { Id = 4, Name = "Apartment" },
                new Models.PropertyType { Id = 5, Name = "Bungalow" },
                new Models.PropertyType { Id = 6, Name = "Bedroom" }
               );
        
            // Seed Properties
            modelBuilder.Entity<Property>().HasData(
                new Models.Property { Id = 1, Postcode = "BT71 4PT", Address = "24 Kings Row", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId=1, Price=800, NumberOfBedrooms=3, NumberOfBathrooms=2, NumberOfEnsuites = 0, FloorArea =83, ImageUrl="", City ="Belfast", Latitude= 54.54107, Longitude= -6.69998 },
                new Models.Property { Id = 2, Postcode = "BT12 7BN", Address = "17 Gortfin Street", ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", PropertyTypeId=3, Price = 700, NumberOfBedrooms = 4, NumberOfBathrooms = 2, NumberOfEnsuites = 1, FloorArea = 151, ImageUrl = "", City = "Belfast", Latitude = 54.622865, Longitude = -5.951895 },
                new Models.Property { Id = 3, Postcode = "BT9 5BN", Address = "16A Malone Road", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId=4, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, NumberOfEnsuites = 2, FloorArea = 57, ImageUrl = "", City = "Belfast" , Latitude = 54.594109, Longitude = -5.915771 },
                new Models.Property { Id = 4, Postcode = "BT12 3AB", Address = "9 Sandymount Mews", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId = 6, Price = 400, NumberOfBedrooms = 1, NumberOfBathrooms = 1, NumberOfEnsuites = 0, FloorArea = 15, ImageUrl = "", City = "Belfast", Latitude = 54.544020, Longitude = -6.014020 },
                new Models.Property { Id = 5, Postcode = "BT9 5EJ", Address = "12 Hillside Drive", ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", PropertyTypeId = 6, Price = 700, NumberOfBedrooms = 1, NumberOfBathrooms = 1, NumberOfEnsuites = 1, FloorArea = 20, ImageUrl = "", City = "Belfast", Latitude = 54.573028, Longitude = -5.937106 },
                new Models.Property { Id = 6, Postcode = "BT6 8EX", Address = "31 FLorida Drive", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId = 6, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, NumberOfEnsuites = 1, FloorArea = 30, ImageUrl = "", City = "Belfast", Latitude = 54.600098, Longitude = -5.885410 }
                );

            // Seed Locations
            modelBuilder.Entity<Location>().HasData(
               new Models.Location { Id = 1, LocationName = "QUB Computer Science Building", Postcode = "BT9 5BN", Address = "16A Malone Road", City = "Belfast", Latitude = 54.594109, Longitude = -5.915771, ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264" },
               new Models.Location { Id = 2, LocationName = "Tesco Express", Postcode = "BT2 7HP", Address = "60-66 Dublin Rd",  City = "Belfast", Latitude = 54.5887567, Longitude = -5.9476385, ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", },
               new Models.Location { Id = 3, LocationName = "QUB PEC", Postcode = "BT9 5EX", Address = "Stranmillis Gardens", City = "Belfast", Latitude = 54.5808006, Longitude = -5.9321734, ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", }
               );
        }
    }
}
