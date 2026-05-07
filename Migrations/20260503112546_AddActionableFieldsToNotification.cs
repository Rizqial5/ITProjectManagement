using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class AddActionableFieldsToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "InviteeId",
                table: "WorkspaceInvites",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconCssClass",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedInviteId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                column: "ConcurrencyStamp",
                value: "d158daa5-49df-4dde-b39a-74c511bd8be5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev",
                column: "ConcurrencyStamp",
                value: "a1a6aa98-7c23-4b53-8569-d24988682c48");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer",
                column: "ConcurrencyStamp",
                value: "7c411c0c-9e87-408a-9784-f7a2d50903a7");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvites_InviteeId",
                table: "WorkspaceInvites",
                column: "InviteeId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_InviteeId",
                table: "WorkspaceInvites",
                column: "InviteeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_InviteeId",
                table: "WorkspaceInvites");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceInvites_InviteeId",
                table: "WorkspaceInvites");

            migrationBuilder.DropColumn(
                name: "InviteeId",
                table: "WorkspaceInvites");

            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeclineUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IconCssClass",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedInviteId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Notifications",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

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
    }
}
