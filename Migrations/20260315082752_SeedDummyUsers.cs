using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectManagement.App.Migrations
{
    /// <inheritdoc />
    public partial class SeedDummyUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "user-admin", 0, "40b2d851-9092-42be-90f9-7a9e133b8c05", "admin@project.com", true, null, false, null, "ADMIN@PROJECT.COM", "ADMIN@PROJECT.COM", "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A==", null, false, "7649d21c-6d8c-4a3e-b873-1f1f50a31693", false, "admin@project.com" },
                    { "user-dev", 0, "222cb4d2-edc5-4d7b-8e5b-5e543cd030b9", "dev@project.com", true, null, false, null, "DEV@PROJECT.COM", "DEV@PROJECT.COM", "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A==", null, false, "8e18b76c-48c0-4384-98c1-f2f270a41704", false, "dev@project.com" },
                    { "user-viewer", 0, "cab88113-f59f-4a44-99b1-7e74457e9c1e", "viewer@project.com", true, null, false, null, "VIEWER@PROJECT.COM", "VIEWER@PROJECT.COM", "AQAAAAIAAYagAAAAEO9lG88/0zH9pXF7pGz6O5uL6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6L1L6A==", null, false, "9f29c87d-59d1-4495-a9d2-e3e381b52815", false, "viewer@project.com" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-dev");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-viewer");
        }
    }
}
