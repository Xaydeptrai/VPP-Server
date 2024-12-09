using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vpp_server.Migrations
{
    /// <inheritdoc />
    public partial class I7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Products",
                newName: "ImageUrl1");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl2",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl3",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl4",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Catalogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl3",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl4",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Catalogs");

            migrationBuilder.RenameColumn(
                name: "ImageUrl1",
                table: "Products",
                newName: "ImageUrl");
        }
    }
}
