using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class GithubCommitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GithubCommits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sha = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepoId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubCommits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GithubCommits_GithubRepos_RepoId",
                        column: x => x.RepoId,
                        principalTable: "GithubRepos",
                        principalColumn: "RepoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GithubCommits_TaskItems_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GithubCommits_RepoId",
                table: "GithubCommits",
                column: "RepoId");

            migrationBuilder.CreateIndex(
                name: "IX_GithubCommits_Sha",
                table: "GithubCommits",
                column: "Sha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GithubCommits_TaskId",
                table: "GithubCommits",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GithubCommits");
        }
    }
}
