using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Location",
                columns: new[] { "Id", "Address", "ApplicationUserId", "City", "Latitude", "LocationName", "Longitude", "Postcode" },
                values: new object[,]
                {
                    { 1, "16A Malone Road", "b27826ed-35dd-474d-ae38-ecd0cdc89264", "Belfast", 54.594109000000003, "QUB Computer Science Building", -5.9157710000000003, "BT9 5BN" },
                    { 2, "60-66 Dublin Rd", "b27826ed-35dd-474d-ae38-ecd0cdc89264", "Belfast", 54.588756699999998, "Tesco Express", -5.9476385000000001, "BT2 7HP" },
                    { 3, "Stranmillis Gardens", "b27826ed-35dd-474d-ae38-ecd0cdc89264", "Belfast", 54.580800600000003, "QUB PEC", -5.9321733999999999, "BT9 5EX" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Location_ApplicationUserId",
                table: "Location",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}
