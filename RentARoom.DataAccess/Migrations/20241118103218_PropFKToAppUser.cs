using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PropFKToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "Property");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Property",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: "69b2f21d-9f6a-4756-a4ee-bbe873232f41");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserId",
                value: "b27826ed-35dd-474d-ae38-ecd0cdc89264");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                column: "UserId",
                value: "69b2f21d-9f6a-4756-a4ee-bbe873232f41");

            migrationBuilder.CreateIndex(
                name: "IX_Property_UserId",
                table: "Property",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_AspNetUsers_UserId",
                table: "Property",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_AspNetUsers_UserId",
                table: "Property");

            migrationBuilder.DropIndex(
                name: "IX_Property_UserId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Property");

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "Property",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "Owner",
                value: "Clanmill Housing");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2,
                column: "Owner",
                value: "Private");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3,
                column: "Owner",
                value: "QUB");
        }
    }
}
