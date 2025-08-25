using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class virtualpathrepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "GithubRepos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GithubRepos_ProjectId",
                table: "GithubRepos",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_GithubRepos_Projects_ProjectId",
                table: "GithubRepos",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GithubRepos_Projects_ProjectId",
                table: "GithubRepos");

            migrationBuilder.DropIndex(
                name: "IX_GithubRepos_ProjectId",
                table: "GithubRepos");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "GithubRepos");
        }
    }
}
