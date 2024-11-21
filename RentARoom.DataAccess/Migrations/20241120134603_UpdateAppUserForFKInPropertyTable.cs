using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppUserForFKInPropertyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_AspNetUsers_UserId",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Property",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Property_UserId",
                table: "Property",
                newName: "IX_Property_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "ApplicationUserId",
                value: "69b2f21d-9f6a-4756-a4ee-bbe873232f41");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_AspNetUsers_ApplicationUserId",
                table: "Property",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Property_AspNetUsers_ApplicationUserId",
                table: "Property");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Property",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Property_ApplicationUserId",
                table: "Property",
                newName: "IX_Property_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: "69b2f21d - 9f6a - 4756 - a4ee - bbe873232f41");

            migrationBuilder.AddForeignKey(
                name: "FK_Property_AspNetUsers_UserId",
                table: "Property",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
