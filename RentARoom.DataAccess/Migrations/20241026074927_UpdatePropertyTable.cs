using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Postcode",
                table: "Property",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "FloorArea",
                table: "Property",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Property",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBedrooms",
                table: "Property",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Property",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PropertyType",
                table: "Property",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FloorArea", "ImageUrl", "NumberOfBedrooms", "Price", "PropertyType" },
                values: new object[] { 83, "", 3, 400, 0 });

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FloorArea", "ImageUrl", "NumberOfBedrooms", "Price", "PropertyType" },
                values: new object[] { 151, "", 4, 700, 2 });

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FloorArea", "ImageUrl", "NumberOfBedrooms", "Price", "PropertyType" },
                values: new object[] { 57, "", 1, 1000, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FloorArea",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "NumberOfBedrooms",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "Property");

            migrationBuilder.AlterColumn<string>(
                name: "Postcode",
                table: "Property",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);
        }
    }
}
