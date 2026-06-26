using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chingoo.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoardType",
                table: "Posts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardType",
                table: "Posts");
        }
    }
}
