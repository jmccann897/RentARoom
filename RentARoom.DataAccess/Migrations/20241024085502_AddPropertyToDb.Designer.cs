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
    [Migration("20241024085502_AddPropertyToDb")]
    partial class AddPropertyToDb
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

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Postcode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Property");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Address = "24 Kings Row",
                            Owner = "Clanmill Housing",
                            Postcode = "BT71 4PT"
                        },
                        new
                        {
                            Id = 2,
                            Address = "17 Gortfin Street",
                            Owner = "Private",
                            Postcode = "BT12 7BN"
                        },
                        new
                        {
                            Id = 3,
                            Address = "16A Malone Road",
                            Owner = "QUB",
                            Postcode = "BT9 5BN"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
