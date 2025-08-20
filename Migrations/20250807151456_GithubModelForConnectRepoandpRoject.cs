using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class GithubModelForConnectRepoandpRoject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GithubRepos",
                columns: table => new
                {
                    RepoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubRepos", x => x.RepoId);
                });

            migrationBuilder.CreateTable(
                name: "GithubRepoConnecteds",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubRepoConnecteds", x => new { x.ProjectId, x.UserId, x.RepoId });
                    table.ForeignKey(
                        name: "FK_GithubRepoConnecteds_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GithubRepoConnecteds_GithubRepos_RepoId",
                        column: x => x.RepoId,
                        principalTable: "GithubRepos",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GithubRepoConnecteds_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GithubRepoConnecteds_RepoId",
                table: "GithubRepoConnecteds",
                column: "RepoId");

            migrationBuilder.CreateIndex(
                name: "IX_GithubRepoConnecteds_UserId",
                table: "GithubRepoConnecteds",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GithubRepoConnecteds");

            migrationBuilder.DropTable(
                name: "GithubRepos");
        }
    }
}
