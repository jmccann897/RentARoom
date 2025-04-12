using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentARoom.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyIdToChatConversationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "ChatConversations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_PropertyId",
                table: "ChatConversations",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_Property_PropertyId",
                table: "ChatConversations",
                column: "PropertyId",
                principalTable: "Property",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_Property_PropertyId",
                table: "ChatConversations");

            migrationBuilder.DropIndex(
                name: "IX_ChatConversations_PropertyId",
                table: "ChatConversations");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "ChatConversations");
        }
    }
}
