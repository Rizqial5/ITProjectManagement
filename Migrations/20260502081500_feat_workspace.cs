using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class feat_workspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspaces_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceMembers",
                columns: table => new
                {
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMembers", x => new { x.WorkspaceId, x.UserId });
                    table.ForeignKey(
                        name: "FK_WorkspaceMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkspaceMembers_Workspaces_WorkspaceId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WorkspaceId",
                table: "Projects",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserId",
                table: "WorkspaceMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OwnerUserId",
                table: "Workspaces",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Workspaces_WorkspaceId",
                table: "Projects",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Workspaces_WorkspaceId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "WorkspaceMembers");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Projects_WorkspaceId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Projects");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin",
                column: "ConcurrencyStamp",
                value: "40b2d851-9092-42be-90f9-7a9e133b8c05");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev",
                column: "ConcurrencyStamp",
                value: "222cb4d2-edc5-4d7b-8e5b-5e543cd030b9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer",
                column: "ConcurrencyStamp",
                value: "cab88113-f59f-4a44-99b1-7e74457e9c1e");
        }
    }
}
