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

            modelBuilder.Entity<PropertyType>().HasData(
                new Models.PropertyType { Id = 1, Name = "Terrace" },
                new Models.PropertyType { Id = 2, Name = "Semi Detached" },
                new Models.PropertyType { Id = 3, Name = "Detached" },
                new Models.PropertyType { Id = 4, Name = "Apartment" },
                new Models.PropertyType { Id = 5, Name = "Bungalow" },
                new Models.PropertyType { Id = 6, Name = "Bedroom" }
               );
        
            modelBuilder.Entity<Property>().HasData(
                new Models.Property { Id = 1, Postcode = "BT71 4PT", Address = "24 Kings Row", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId=1, Price=800, NumberOfBedrooms=3, NumberOfBathrooms=2, FloorArea=83, ImageUrl="", City ="Belfast"},
                new Models.Property { Id = 2, Postcode = "BT12 7BN", Address = "17 Gortfin Street", ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", PropertyTypeId=3, Price = 700, NumberOfBedrooms = 4, NumberOfBathrooms = 2, FloorArea = 151, ImageUrl = "", City = "Belfast" },
                new Models.Property { Id = 3, Postcode = "BT9 5BN", Address = "16A Malone Road", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId=4, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, FloorArea = 57, ImageUrl = "", City = "Belfast" },
                new Models.Property { Id = 4, Postcode = "BT12 3AB", Address = "9 Sandymount Mews", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId = 6, Price = 400, NumberOfBedrooms = 1, NumberOfBathrooms = 1, FloorArea = 15, ImageUrl = "", City = "Belfast" },
                new Models.Property { Id = 5, Postcode = "BT9 5EJ", Address = "12 Hillside Drive", ApplicationUserId = "b27826ed-35dd-474d-ae38-ecd0cdc89264", PropertyTypeId = 6, Price = 700, NumberOfBedrooms = 1, NumberOfBathrooms = 1, FloorArea = 20, ImageUrl = "", City = "Belfast" },
                new Models.Property { Id = 6, Postcode = "BT6 8EX", Address = "31 FLorida Drive", ApplicationUserId = "69b2f21d-9f6a-4756-a4ee-bbe873232f41", PropertyTypeId = 6, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, FloorArea = 30, ImageUrl = "", City = "Belfast" }
                );
        }
    }
}
