using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBedroomToPropertyTypeAndBathroomsToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfBathrooms",
                table: "Property",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NumberOfBathrooms", "Price" },
                values: new object[] { 2, 800 });

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                column: "NumberOfBathrooms",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                column: "NumberOfBathrooms",
                value: 2);

            migrationBuilder.InsertData(
                table: "PropertyType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "Bedroom" });

            migrationBuilder.InsertData(
                table: "Property",
                columns: new[] { "Id", "Address", "ApplicationUserId", "City", "FloorArea", "ImageUrl", "NumberOfBathrooms", "NumberOfBedrooms", "Postcode", "Price", "PropertyTypeId" },
                values: new object[,]
                {
                    { 4, "9 Sandymount Mews", "69b2f21d-9f6a-4756-a4ee-bbe873232f41", "Belfast", 15, "", 1, 1, "BT12 3AB", 400, 6 },
                    { 5, "12 Hillside Drive", "b27826ed-35dd-474d-ae38-ecd0cdc89264", "Belfast", 20, "", 1, 1, "BT9 5EJ", 700, 6 },
                    { 6, "31 FLorida Drive", "69b2f21d-9f6a-4756-a4ee-bbe873232f41", "Belfast", 30, "", 2, 1, "BT6 8EX", 1000, 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PropertyType",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "NumberOfBathrooms",
                table: "Property");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 400);
        }
    }
}
