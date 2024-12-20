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
    [Migration("20241026074927_UpdatePropertyTable")]
    partial class UpdatePropertyTable
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

                    b.Property<int>("PropertyType")
                        .HasColumnType("int");

                    b.HasKey("Id");

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
                            PropertyType = 0
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
                            PropertyType = 2
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
                            PropertyType = 3
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
