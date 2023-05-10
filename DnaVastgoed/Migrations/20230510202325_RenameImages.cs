using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnaVastgoed.Migrations
{
    public partial class RenameImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DnaPropertyImage_Properties_DnaPropertyId",
                table: "DnaPropertyImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DnaPropertyImage",
                table: "DnaPropertyImage");

            migrationBuilder.RenameTable(
                name: "DnaPropertyImage",
                newName: "PropertyImages");

            migrationBuilder.RenameIndex(
                name: "IX_DnaPropertyImage_DnaPropertyId",
                table: "PropertyImages",
                newName: "IX_PropertyImages_DnaPropertyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PropertyImages",
                table: "PropertyImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_Properties_DnaPropertyId",
                table: "PropertyImages",
                column: "DnaPropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_Properties_DnaPropertyId",
                table: "PropertyImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PropertyImages",
                table: "PropertyImages");

            migrationBuilder.RenameTable(
                name: "PropertyImages",
                newName: "DnaPropertyImage");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyImages_DnaPropertyId",
                table: "DnaPropertyImage",
                newName: "IX_DnaPropertyImage_DnaPropertyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DnaPropertyImage",
                table: "DnaPropertyImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DnaPropertyImage_Properties_DnaPropertyId",
                table: "DnaPropertyImage",
                column: "DnaPropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }
    }
}
