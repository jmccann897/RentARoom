using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PropertyType",
                table: "Property",
                newName: "PropertyTypeId");

            migrationBuilder.CreateTable(
                name: "PropertyType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyType", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "PropertyTypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                column: "PropertyTypeId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                column: "PropertyTypeId",
                value: 4);

            migrationBuilder.InsertData(
                table: "PropertyType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Terrace" },
                    { 2, "Semi Detached" },
                    { 3, "Detached" },
                    { 4, "Apartment" },
                    { 5, "Bungalow" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Property_PropertyTypeId",
                table: "Property",
                column: "PropertyTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_PropertyType_PropertyTypeId",
                table: "Property",
                column: "PropertyTypeId",
                principalTable: "PropertyType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_PropertyType_PropertyTypeId",
                table: "Property");

            migrationBuilder.DropTable(
                name: "PropertyType");

            migrationBuilder.DropIndex(
                name: "IX_Property_PropertyTypeId",
                table: "Property");

            migrationBuilder.RenameColumn(
                name: "PropertyTypeId",
                table: "Property",
                newName: "PropertyType");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "PropertyType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                column: "PropertyType",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                column: "PropertyType",
                value: 3);
        }
    }
}
