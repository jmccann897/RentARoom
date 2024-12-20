﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RentARoom.DataAccess.Data;

#nullable disable

namespace RentARoom.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241026090426_UpdatePropertyType")]
    partial class UpdatePropertyType
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RentARoom.Models.Property", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FloorArea")
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfBedrooms")
                        .HasColumnType("int");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Postcode")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("PropertyTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PropertyTypeId");

                    b.ToTable("Property");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Address = "24 Kings Row",
                            FloorArea = 83,
                            ImageUrl = "",
                            NumberOfBedrooms = 3,
                            Owner = "Clanmill Housing",
                            Postcode = "BT71 4PT",
                            Price = 400,
                            PropertyTypeId = 1
                        },
                        new
                        {
                            Id = 2,
                            Address = "17 Gortfin Street",
                            FloorArea = 151,
                            ImageUrl = "",
                            NumberOfBedrooms = 4,
                            Owner = "Private",
                            Postcode = "BT12 7BN",
                            Price = 700,
                            PropertyTypeId = 3
                        },
                        new
                        {
                            Id = 3,
                            Address = "16A Malone Road",
                            FloorArea = 57,
                            ImageUrl = "",
                            NumberOfBedrooms = 1,
                            Owner = "QUB",
                            Postcode = "BT9 5BN",
                            Price = 1000,
                            PropertyTypeId = 4
                        });
                });

            modelBuilder.Entity("RentARoom.Models.PropertyType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("PropertyType");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Terrace"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Semi Detached"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Detached"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Apartment"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Bungalow"
                        });
                });

            modelBuilder.Entity("RentARoom.Models.Property", b =>
                {
                    b.HasOne("RentARoom.Models.PropertyType", "PropertyType")
                        .WithMany()
                        .HasForeignKey("PropertyTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PropertyType");
                });
#pragma warning restore 612, 618
        }
    }
}
