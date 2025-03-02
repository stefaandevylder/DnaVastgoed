using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnaVastgoed.Migrations
{
    public partial class SuspendedImmovlanProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImmovlanSuspended",
                table: "Properties",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImmovlanSuspended",
                table: "Properties");
        }
    }
}
