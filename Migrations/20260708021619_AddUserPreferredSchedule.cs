using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chingoo.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferredSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredDayType",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "상관없음")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeSlot",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "상관없음")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredDayType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredTimeSlot",
                table: "Users");
        }
    }
}
