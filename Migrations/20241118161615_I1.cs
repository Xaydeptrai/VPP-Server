using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vpp_server.Migrations
{
    /// <inheritdoc />
    public partial class I1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "869e032d-8a9e-4012-a711-25fcf6b62c07");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e191cd7-879e-4450-8773-c06f4423be8d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "f6d9090b-88be-46d1-acd3-f1915d2effce", null, "Admin", "ADMIN" },
                    { "fefde393-5cda-4ae4-826c-d749e8ebf0ea", null, "Customer", "CUSTOMER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f6d9090b-88be-46d1-acd3-f1915d2effce");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fefde393-5cda-4ae4-826c-d749e8ebf0ea");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "869e032d-8a9e-4012-a711-25fcf6b62c07", null, "Customer", "CUSTOMER" },
                    { "8e191cd7-879e-4450-8773-c06f4423be8d", null, "Admin", "ADMIN" }
                });
        }
    }
}
