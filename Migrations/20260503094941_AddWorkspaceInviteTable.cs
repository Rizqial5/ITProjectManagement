using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceInviteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                column: "ConcurrencyStamp",
                value: "4b2c0bd5-e45e-4353-9e1b-bfaa9a7331b6");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev",
                column: "ConcurrencyStamp",
                value: "a5d39a32-f3b9-4226-829b-8113e7570254");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer",
                column: "ConcurrencyStamp",
                value: "0aa31198-5c45-4a4b-a667-9d8677c49f9e");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                column: "ConcurrencyStamp",
                value: "cf977cfe-5180-4158-b281-b0d8e99a2617");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev",
                column: "ConcurrencyStamp",
                value: "d6aa7c70-85a3-4ce1-8836-04494aafa8fb");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer",
                column: "ConcurrencyStamp",
                value: "7abdf088-8bff-42c1-8844-43dbab4be39b");
        }
    }
}
