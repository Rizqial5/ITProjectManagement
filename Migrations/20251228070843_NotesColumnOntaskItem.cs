using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class NotesColumnOntaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotesHtml",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotesHtml",
                table: "TaskItems");
        }
    }
}
