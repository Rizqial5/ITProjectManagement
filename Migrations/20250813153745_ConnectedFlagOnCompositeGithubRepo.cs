using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class ConnectedFlagOnCompositeGithubRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Connected",
                table: "GithubRepoConnecteds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectedDate",
                table: "GithubRepoConnecteds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DisconnectedDate",
                table: "GithubRepoConnecteds",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Connected",
                table: "GithubRepoConnecteds");

            migrationBuilder.DropColumn(
                name: "ConnectedDate",
                table: "GithubRepoConnecteds");

            migrationBuilder.DropColumn(
                name: "DisconnectedDate",
                table: "GithubRepoConnecteds");
        }
    }
}
