using System;
using Chingoo.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chingoo.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260713001000_AddChatMessageReadAt")]
    public partial class AddChatMessageReadAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "ChatMessages",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "ChatMessages");
        }
    }
}
