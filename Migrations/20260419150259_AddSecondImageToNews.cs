using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex7.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondImageToNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL2",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL2",
                table: "News");
        }
    }
}
