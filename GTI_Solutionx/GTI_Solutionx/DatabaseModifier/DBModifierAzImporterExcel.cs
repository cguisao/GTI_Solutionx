using GTI_Solutionx.Models.Dashboard;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DatabaseModifier
{
    public class DBModifierAzImporterExcel : Database, IDatabaseModifier
    {
        public DBModifierAzImporterExcel(string _path, Dictionary<string, Wholesaler_AzImporter> _azImportItems)
        {
            path = _path;
            azImportItems = _azImportItems;
        }

        private string path { set; get; }

        private Dictionary<string, Wholesaler_AzImporter> azImportItems { get; set; }

        public List<Wholesaler_AzImporter> azImport = new List<Wholesaler_AzImporter>();
        
        public DataTable CreateTable()
        {
            DataTable azImporterTable = new DataTable("AzImporter");

            ColumnMaker(azImporterTable, "ItemID", "System.Int32");
            ColumnMaker(azImporterTable, "Category", "System.String");
            ColumnMaker(azImporterTable, "HTMLDescription", "System.String");
            ColumnMaker(azImporterTable, "Image1", "System.String");
            ColumnMaker(azImporterTable, "Image2", "System.String");
            ColumnMaker(azImporterTable, "Image3", "System.String");
            ColumnMaker(azImporterTable, "Image4", "System.String");
            ColumnMaker(azImporterTable, "Image5", "System.String");
            ColumnMaker(azImporterTable, "Image6", "System.String");
            ColumnMaker(azImporterTable, "Image7", "System.String");
            ColumnMaker(azImporterTable, "Image8", "System.String");
            ColumnMaker(azImporterTable, "itemName", "System.String");
            ColumnMaker(azImporterTable, "MainImage", "System.String");
            ColumnMaker(azImporterTable, "Quantity", "System.Int32");
            ColumnMaker(azImporterTable, "ShortDescription", "System.String");
            ColumnMaker(azImporterTable, "Sku", "System.String");
            ColumnMaker(azImporterTable, "Weight", "System.Int32");
            ColumnMaker(azImporterTable, "WholeSale", "System.Double");

            return azImporterTable;
        }

        private void TableCreator()
        {
            FileInfo file = new FileInfo(path);
            
            int exception = 0;

            try
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        Wholesaler_AzImporter az = new Wholesaler_AzImporter();
                        exception++;
                        if(exception == 10)
                        {

                        }
                        if (row != 1)
                        {
                            az.id = row - 1;
                            az.Sku = worksheet.Cells[row, 1].Value?.ToString().ToUpper();
                            az.Category = worksheet.Cells[row, 2].Value?.ToString();
                            az.ItemName = worksheet.Cells[row, 3].Value?.ToString();
                            az.Image1 = worksheet.Cells[row, 4].Value?.ToString();
                            az.Image2 = worksheet.Cells[row, 5].Value?.ToString();
                            az.Image3 = worksheet.Cells[row, 6].Value?.ToString();
                            az.Image4 = worksheet.Cells[row, 7].Value?.ToString();
                            az.Image5 = worksheet.Cells[row, 8].Value?.ToString();
                            az.Image6 = worksheet.Cells[row, 9].Value?.ToString();
                            az.Image7 = worksheet.Cells[row, 10].Value?.ToString();
                            az.Image8 = worksheet.Cells[row, 11].Value?.ToString();
                            az.MainImage = worksheet.Cells[row, 12].Value?.ToString();
                            az.WholeSale = Convert.ToDouble(worksheet.Cells[row, 13].Value?.ToString());
                            az.Quantity = Convert.ToInt32(worksheet.Cells[row, 14]?.Value);
                            az.ShortDescription = worksheet.Cells[row, 25].Value?.ToString();
                            az.Weight = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(worksheet.Cells[row, 15]?.Value)));
                            az.HTMLDescription = worksheet.Cells[row, 24].Value?.ToString();

                            if(!azImportItems.TryAdd(az.Sku, az))
                            {
                                azImportItems[az.Sku].Quantity = az.Quantity;
                            }
                            else
                            {
                                azImportItems.TryAdd(az.Sku, az);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void TableExecutor()
        {
            TableCreator();

            DataTable uploadAzImporter = CreateTable();

            int bulkSize = 1;

            int exception = 0;

            foreach(var az in azImportItems)
            {
                exception++;

                Wholesaler_AzImporter azImporter = new Wholesaler_AzImporter();

                azImporter.id = bulkSize;
                azImporter.Sku = az.Value.Sku;
                azImporter.Category = az.Value.Category;
                azImporter.ItemName = az.Value.ItemName;
                azImporter.Image1 = az.Value.Image1;
                azImporter.Image2 = az.Value.Image2;
                azImporter.Image3 = az.Value.Image3;
                azImporter.Image4 = az.Value.Image4;
                azImporter.Image5 = az.Value.Image5;
                azImporter.Image6 = az.Value.Image6;
                azImporter.Image7 = az.Value.Image7;
                azImporter.Image8 = az.Value.Image8;
                azImporter.MainImage = az.Value.MainImage;
                azImporter.WholeSale = Convert.ToDouble(az.Value.WholeSale);
                azImporter.Quantity = Convert.ToInt32(az.Value.Quantity);
                azImporter.ShortDescription = az.Value.ShortDescription;
                azImporter.Weight = az.Value.Weight;
                azImporter.HTMLDescription = az.Value.HTMLDescription;

                azImport.Add(azImporter);

                bulkSize++;
                
            }
        }
    }
}
