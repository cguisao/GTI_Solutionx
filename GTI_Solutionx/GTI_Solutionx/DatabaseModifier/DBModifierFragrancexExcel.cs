using GTI_Solutionx.Models.Dashboard;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DatabaseModifier
{
    public class DBModifierFragrancexExcel
    {
        public DBModifierFragrancexExcel(string path, Dictionary<int, long?> upc)
        {
            this.path = path;
            this.upc = upc;
        }

        public List<Wholesaler_Fragrancex> fragancexList = new List<Wholesaler_Fragrancex>();

        private string path { get; set; }

        private Dictionary<int, long?> upc { get; set; }
        
        public virtual void TableExecutor()
        {
            
            List<UPC> list = new List<UPC>();

            FileInfo file = new FileInfo(path);
            
            int exception = 0;

            try
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    long? value = 0;
                    int itemID = 0, description = 0, price = 0, brandName = 0,
                        image = 0, type = 0;
                        
                    for (int row = 1; row <= rowCount; row++)
                    {
                        // Map titles
                            
                        if(row == 1)
                        {
                            for (int column = 1; column <= worksheet.Dimension.Columns; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("sku"))
                                {
                                    itemID = column;
                                }
                                else if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("html"))
                                {
                                    description = column;
                                }
                                else if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("variant price"))
                                {
                                    price = column;
                                }
                                else if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("vendor"))
                                {
                                    brandName = column;
                                }
                                else if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("image src"))
                                {
                                    image = column;
                                }
                                else if (worksheet.Cells[row, column].Value.ToString().ToLower().Contains("type"))
                                {
                                    type = column;
                                }
                            }
                        }
                        else
                        {
                            exception++;

                            Wholesaler_Fragrancex fran = new Wholesaler_Fragrancex();

                            fran.id = exception;
                            fran.Sku = Convert.ToInt32(worksheet.Cells[row, itemID].Value?.ToString());
                            fran.BrandName = worksheet.Cells[row, brandName].Value?.ToString();
                            fran.Description = worksheet.Cells[row, description].Value?.ToString();
                            fran.Gender = null;
                            fran.Instock = true;
                            fran.LargeImageUrl = worksheet.Cells[row, image].Value?.ToString();
                            fran.MetricSize = null;
                            fran.ParentCode = null;
                            fran.ProductName = null;
                            fran.RetailPriceUSD = 0.0;
                            fran.Size = null;
                            fran.SmallImageURL = null;
                            fran.Type = worksheet.Cells[row, type].Value?.ToString();
                            fran.WholePriceAUD = 0.0;
                            fran.WholePriceCAD = 0.0;
                            fran.WholePriceEUR = 0.0;
                            fran.WholePriceGBP = 0.0;
                            fran.WholePriceUSD = Convert.ToDouble(worksheet.Cells[row, price].Value?.ToString());

                            if (upc.TryGetValue(Convert.ToInt32(Convert.ToInt32(worksheet.Cells[row, itemID].Value?.ToString())), out value))
                            {
                                fran.Upc = value;
                            }

                            fragancexList.Add(fran);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
