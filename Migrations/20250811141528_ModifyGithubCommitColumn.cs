using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class ModifyGithubCommitColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GithubCommits_TaskItems_TaskId",
                table: "GithubCommits");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "GithubCommits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GithubCommits_TaskItems_TaskId",
                table: "GithubCommits",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GithubCommits_TaskItems_TaskId",
                table: "GithubCommits");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "GithubCommits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GithubCommits_TaskItems_TaskId",
                table: "GithubCommits",
                column: "TaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
