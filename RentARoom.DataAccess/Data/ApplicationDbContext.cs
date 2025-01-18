using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Identity.Client;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using System.Reflection.Emit;

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
        public DbSet<Image> Image { get; set; }

        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatConversationParticipant> ChatConversationParticipants { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //needed for identities to build properly

            //https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many
            //https://stackoverflow.com/questions/62110667/cannot-resolve-symbol-hasrequired-entity-framework-core

            // One-to-many relationship between Property and ApplicationUser
            // Each Property is associated with one ApplicationUser (owner), 
            // and each ApplicationUser can own many Properties.
            modelBuilder.Entity<Property>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(t => t.Properties)
                .HasForeignKey(m => m.ApplicationUserId)
                .IsRequired();

            // One-to-many relationship between Property and Image
            // Each Property can have many Images, and each Image is associated with one Property.
            modelBuilder.Entity<Property>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Property)
                .HasForeignKey(i => i.PropertyId);

            // Many-to-many relationship between ChatConversation and ApplicationUser
            // Via ChatConversationParticipant join table
            // Each ChatConversation can have many ApplicationUsers, and each ApplicationUser can participate in many ChatConversations.
            modelBuilder.Entity<ChatConversationParticipant>()
                .HasKey(cp => new { cp.ChatConversationId, cp.UserId }); // Composite key for the join table

            // Relationship between ChatConversationParticipant and ChatConversation
            // Each ChatConversationParticipant is associated with one ChatConversation,
            // and each ChatConversation can have many ChatConversationParticipants.
            modelBuilder.Entity<ChatConversationParticipant>()
                .HasOne(cp => cp.ChatConversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ChatConversationId);

            // Relationship between ChatConversationParticipant and ApplicationUser
            // Each ChatConversationParticipant is associated with one ApplicationUser (the participant),
            // and each ApplicationUser can have many ChatConversationParticipants.
            modelBuilder.Entity<ChatConversationParticipant>()
                .HasOne(cp => cp.User)
                .WithMany(u => u.ChatConversationParticipants)
                .HasForeignKey(cp => cp.UserId);

            // One-to-many relationship between ChatMessage and ChatConversation
            // Each ChatMessage belongs to one ChatConversation, and each ChatConversation can have many ChatMessages.
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(cm => cm.ChatConversationId);

            // One-to-many relationship between ChatMessage and ApplicationUser (Sender)
            // Each ChatMessage is sent by one ApplicationUser (Sender), and each ApplicationUser can send many ChatMessages.
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent delete cascade for user
            
            // One-to-many relationship between ChatMessage and ApplicationUser (Recipient)
            // Each ChatMessage is sent to one ApplicationUser (Recipient), and each ApplicationUser can receive many ChatMessages.
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Recipient)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(cm => cm.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent delete cascade for user
        }

        // Helper methods to check existence and add data
        private async Task<bool> PropertyTypeExists() => await this.PropertyType.AnyAsync();
        private async Task<bool> PropertiesExist() => await this.Property.AnyAsync();
        private async Task<bool> LocationsExist() => await this.Location.AnyAsync();

        private async Task AddPropertyTypesAsync(PropertyType[] propertyTypes)
        {
            this.PropertyType.AddRange(propertyTypes);
            await this.SaveChangesAsync();
        }

        private async Task AddPropertiesAsync(Property[] properties)
        {
            this.Property.AddRange(properties);
            await this.SaveChangesAsync();
        }

        private async Task AddLocationsAsync(Location[] locations)
        {
            this.Location.AddRange(locations);
            await this.SaveChangesAsync();
        }

        public async Task SeedDataAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Seed roles
            var roles = new[] { "Agent", "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed users
            var adminEmail = "testadmin@gmail.com";
            var agentEmail = "testagent@gmail.com";
            var userEmail = "testuser@gmail.com";

            var testAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (testAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    Name = adminEmail,
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = false
                };
                var result = await userManager.CreateAsync(adminUser, "Password.01");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.UpdateAsync(adminUser);
                }
                testAdmin = adminUser; // Assign the user after it's created
            }

            var testAgent = await userManager.FindByEmailAsync(agentEmail);
            if (testAgent == null)
            {
                var agentUser = new ApplicationUser
                {
                    Name = agentEmail,
                    UserName = agentEmail,
                    Email = agentEmail,
                    EmailConfirmed = false
                };
                var result = await userManager.CreateAsync(agentUser, "Password.01");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agentUser, "Agent");
                    await userManager.UpdateAsync(agentUser);
                }
                testAgent = agentUser;
            }
           
            var testUser = await userManager.FindByEmailAsync(userEmail);
            if (testUser == null)
            {
                var normalUser = new ApplicationUser
                {
                    Name = userEmail,
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = false
                };
                var result = await userManager.CreateAsync(normalUser, "Password.01");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                    await userManager.UpdateAsync(normalUser);
                }
                testUser = normalUser;
            }

            var adminId = testAdmin?.Id;
            var agentId = testAgent?.Id;
            var userId = testUser?.Id;


            // Seed PropertyTypes
            if (!await PropertyTypeExists())
            {
                var propertyTypes = new[]
                {
                    new PropertyType { Name = "Terrace" },
                    new PropertyType { Name = "Semi Detached" },
                    new PropertyType { Name = "Detached" },
                    new PropertyType { Name = "Apartment" },
                    new PropertyType { Name = "Bungalow" },
                    new PropertyType { Name = "Bedroom" }
                };

                await AddPropertyTypesAsync(propertyTypes);
            }

            // Seed Properties
            if (!await PropertiesExist())
            {
                var properties = new[]
                {
                    new Models.Property { Postcode = "BT71 4PT", Address = "24 Kings Row", ApplicationUserId = agentId, PropertyTypeId = 1, Price = 800, NumberOfBedrooms = 3, NumberOfBathrooms = 2, NumberOfEnsuites = 0, FloorArea = 83, City = "Belfast", Latitude = 54.54107, Longitude = -6.69998 },
                    new Models.Property { Postcode = "BT12 7BN", Address = "17 Gortfin Street", ApplicationUserId = adminId, PropertyTypeId = 3, Price = 700, NumberOfBedrooms = 4, NumberOfBathrooms = 2, NumberOfEnsuites = 1, FloorArea = 151, City = "Belfast", Latitude = 54.622865, Longitude = -5.951895 },
                    new Models.Property { Postcode = "BT9 5BN", Address = "16A Malone Road", ApplicationUserId = agentId, PropertyTypeId = 4, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, NumberOfEnsuites = 2, FloorArea = 57, City = "Belfast", Latitude = 54.594109, Longitude = -5.915771 },
                    new Models.Property { Postcode = "BT12 3AB", Address = "9 Sandymount Mews", ApplicationUserId = agentId, PropertyTypeId = 6, Price = 400, NumberOfBedrooms = 1, NumberOfBathrooms = 1, NumberOfEnsuites = 0, FloorArea = 15, City = "Belfast", Latitude = 54.544020, Longitude = -6.014020 },
                    new Models.Property { Postcode = "BT9 5EJ", Address = "12 Hillside Drive", ApplicationUserId = adminId, PropertyTypeId = 6, Price = 700, NumberOfBedrooms = 1, NumberOfBathrooms = 1, NumberOfEnsuites = 1, FloorArea = 20, City = "Belfast", Latitude = 54.573028, Longitude = -5.937106 },
                    new Models.Property { Postcode = "BT6 8EX", Address = "31 FLorida Drive", ApplicationUserId = agentId, PropertyTypeId = 6, Price = 1000, NumberOfBedrooms = 1, NumberOfBathrooms = 2, NumberOfEnsuites = 1, FloorArea = 30, City = "Belfast", Latitude = 54.600098, Longitude = -5.885410 }
                };
                await AddPropertiesAsync(properties);
            }

            // Seed Locations
            if (!await LocationsExist())
            {
                var locations = new[]
                {
                    new Models.Location { LocationName = "QUB Computer Science Building", Postcode = "BT9 5BN", Address = "16A Malone Road", City = "Belfast", Latitude = 54.594109, Longitude = -5.915771, ApplicationUserId = adminId },
                    new Models.Location { LocationName = "Tesco Express", Postcode = "BT2 7HP", Address = "60-66 Dublin Rd", City = "Belfast", Latitude = 54.5887567, Longitude = -5.9476385, ApplicationUserId = adminId, },
                    new Models.Location { LocationName = "QUB PEC", Postcode = "BT9 5EX", Address = "Stranmillis Gardens", City = "Belfast", Latitude = 54.5808006, Longitude = -5.9321734, ApplicationUserId = adminId, }
                };
                await AddLocationsAsync(locations);
            }
        }
    }
}