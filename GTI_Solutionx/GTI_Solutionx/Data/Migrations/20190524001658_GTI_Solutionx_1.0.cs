using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GTI_Solutionx.Data.Migrations
{
    public partial class GTI_Solutionx_10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wholesaler_AzImporter",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    Sku = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    ItemName = table.Column<string>(nullable: true),
                    Image1 = table.Column<string>(nullable: true),
                    Image2 = table.Column<string>(nullable: true),
                    Image3 = table.Column<string>(nullable: true),
                    Image4 = table.Column<string>(nullable: true),
                    Image5 = table.Column<string>(nullable: true),
                    Image6 = table.Column<string>(nullable: true),
                    Image7 = table.Column<string>(nullable: true),
                    Image8 = table.Column<string>(nullable: true),
                    MainImage = table.Column<string>(nullable: true),
                    WholeSale = table.Column<double>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false),
                    HTMLDescription = table.Column<string>(nullable: true),
                    ShortDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wholesaler_AzImporter", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Wholesaler_Fragrancex",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Sku = table.Column<int>(nullable: false),
                    BrandName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    Instock = table.Column<bool>(nullable: false),
                    LargeImageUrl = table.Column<string>(nullable: true),
                    MetricSize = table.Column<string>(nullable: true),
                    ParentCode = table.Column<string>(nullable: true),
                    ProductName = table.Column<string>(nullable: true),
                    RetailPriceUSD = table.Column<double>(nullable: false),
                    Size = table.Column<string>(nullable: true),
                    SmallImageURL = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    WholePriceAUD = table.Column<double>(nullable: false),
                    WholePriceCAD = table.Column<double>(nullable: false),
                    WholePriceEUR = table.Column<double>(nullable: false),
                    WholePriceGBP = table.Column<double>(nullable: false),
                    WholePriceUSD = table.Column<double>(nullable: false),
                    isInstock = table.Column<bool>(nullable: false),
                    Upc = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wholesaler_Fragrancex", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wholesaler_AzImporter");

            migrationBuilder.DropTable(
                name: "Wholesaler_Fragrancex");
        }
    }
}
