using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusKeywordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatusKeywords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetStatus = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusKeywords", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StatusKeywords",
                columns: new[] { "Id", "Description", "IsActive", "Keyword", "TargetStatus" },
                values: new object[,]
                {
                    { 1, "Ubah status task menjadi Done", true, "#done", 2 },
                    { 2, "Ubah status task menjadi Done", true, "#closes", 2 },
                    { 3, "Ubah status task menjadi Done", true, "#fixes", 2 },
                    { 4, "Ubah status task menjadi InProgress", true, "#inprogress", 1 },
                    { 5, "Ubah status task menjadi ToDo", true, "#todo", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusKeywords");
        }
    }
}
