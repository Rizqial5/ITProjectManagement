using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class AddFullInvitationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    InviterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InviteeEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceInvites_AspNetUsers_InviterUserId",
                        column: x => x.InviterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkspaceInvites_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvites_InviterUserId",
                table: "WorkspaceInvites",
                column: "InviterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvites_WorkspaceId",
                table: "WorkspaceInvites",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "WorkspaceInvites");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                column: "ConcurrencyStamp",
                value: "4722f6ce-3f29-4496-be2d-6c98670ce59b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev",
                column: "ConcurrencyStamp",
                value: "22cdf904-d7a9-49dd-a3db-3db5fffb5d35");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer",
                column: "ConcurrencyStamp",
                value: "737d48c4-e63b-41f9-9cde-8afa10288507");
        }
    }
}
