using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGithubRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_GithubRepoConnecteds_GithubRepos_RepoId",
                table: "GithubRepoConnecteds");

            // 2. Drop primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_GithubRepos",
                table: "GithubRepos");

            // 3. Drop kolom RepoId
            migrationBuilder.DropColumn(
                name: "RepoId",
                table: "GithubRepos");

            // 4. Tambahkan ulang kolom tanpa identity
            migrationBuilder.AddColumn<int>(
                name: "RepoId",
                table: "GithubRepos",
                type: "int",
                nullable: false);

            // 5. Tambahkan kembali primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_GithubRepos",
                table: "GithubRepos",
                column: "RepoId");

            // 6. Tambahkan kembali foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_GithubRepoConnecteds_GithubRepos_RepoId",
                table: "GithubRepoConnecteds",
                column: "RepoId",
                principalTable: "GithubRepos",
                principalColumn: "RepoId",
                onDelete: ReferentialAction.Cascade);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GithubRepoConnecteds_GithubRepos_RepoId",
                table: "GithubRepoConnecteds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GithubRepos",
                table: "GithubRepos");

            migrationBuilder.DropColumn(
                name: "RepoId",
                table: "GithubRepos");

            migrationBuilder.AddColumn<int>(
                name: "RepoId",
                table: "GithubRepos",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GithubRepos",
                table: "GithubRepos",
                column: "RepoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GithubRepoConnecteds_GithubRepos_RepoId",
                table: "GithubRepoConnecteds",
                column: "RepoId",
                principalTable: "GithubRepos",
                principalColumn: "RepoId",
                onDelete: ReferentialAction.Cascade);
        }

    }
}
