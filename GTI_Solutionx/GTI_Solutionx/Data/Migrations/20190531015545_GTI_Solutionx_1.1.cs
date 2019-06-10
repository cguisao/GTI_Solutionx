using Microsoft.EntityFrameworkCore.Migrations;

namespace GTI_Solutionx.Data.Migrations
{
    public partial class GTI_Solutionx_11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "marketPlace",
                table: "Amazon",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "marketPlace",
                table: "Amazon");
        }
    }
}
