using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Property",
                columns: new[] { "Id", "Address", "Owner", "Postcode" },
                values: new object[,]
                {
                    { 1, "24 Kings Row", "Clanmill Housing", "BT71 4PT" },
                    { 2, "17 Gortfin Street", "Private", "BT12 7BN" },
                    { 3, "16A Malone Road", "QUB", "BT9 5BN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Property");
        }
    }
}
