using DatabaseModifier;
using GTI_Solutionx.Models.Dashboard;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ExcelModifier
{
    public class AmazonDBUploader : WholesaleHelper, IExcelExtension
    {
        private Dictionary<string, Wholesaler_AzImporter> azImporter1;
        private Dictionary<int, Wholesaler_Fragrancex> fragrancex1;
        private Dictionary<object, PerfumeWorldWide> perfumeWorldWide1;
        private Dictionary<string, Amazon> amazon;
        private Dictionary<int, double> shipping;

        public AmazonDBUploader(string _path, Dictionary<string, Wholesaler_AzImporter> _azImporter, Dictionary<int, Wholesaler_Fragrancex> _fragracex
            , List<Amazon> _amazon, Dictionary<int, double> _shipping, MarketPlace _marketPlace)
        {
            path = _path;
            fragrancexList = _fragracex;
            azImporterList = _azImporter;
            //perfumeWorldWideList = _perfumeWorldWide;
            amazonList = _amazon;
            ShippingList = _shipping;
            amazonPrintList = new List<Amazon>();
            marketPlace = _marketPlace;
            setList();
        }

        public AmazonDBUploader(string path, Dictionary<string, Wholesaler_AzImporter> azImporter1, Dictionary<int, Wholesaler_Fragrancex> fragrancex1, Dictionary<object, PerfumeWorldWide> perfumeWorldWide1, Dictionary<string, Amazon> amazon, Dictionary<int, double> shipping)
        {
            this.path = path;
            this.azImporter1 = azImporter1;
            this.fragrancex1 = fragrancex1;
            this.perfumeWorldWide1 = perfumeWorldWide1;
            this.amazon = amazon;
            this.shipping = shipping;
        }

        //public List<Amazon> amazon = 

        public void ExcelGenerator()
        {
            FileInfo file = new FileInfo(path);
            long? skuID;
            int execption = 0;
            try
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    int row = 0;

                    for (row = 1; row <= rowCount + 1; row++)
                    {
                        execption++;

                        if (row == 1)
                        {
                            worksheet.Cells[row, 1].Value = "sku";
                            worksheet.Cells[row, 2].Value = "product-id";
                            worksheet.Cells[row, 3].Value = "product-id-type";
                            worksheet.Cells[row, 4].Value = "price";
                            worksheet.Cells[row, 5].Value = "minimum-seller-allowed-price";
                            worksheet.Cells[row, 6].Value = "maximum-seller-allowed-price";
                            worksheet.Cells[row, 7].Value = "item-condition";
                            worksheet.Cells[row, 8].Value = "quantity";
                            worksheet.Cells[row, 9].Value = "add-delete";
                            worksheet.Cells[row, 10].Value = "will-ship-internationally";
                            worksheet.Cells[row, 11].Value = "expedited-shipping";
                            worksheet.Cells[row, 12].Value = "standard-plus";
                            worksheet.Cells[row, 13].Value = "item-note";
                            worksheet.Cells[row, 14].Value = "binding";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Value?.ToString()))
                            {
                                // if the first row is a perfume/Cologne
                                string rowSku = worksheet.Cells[row, 1].Value.ToString();
                                long? digitSku = DigitGetter(rowSku);
                                //double rowPrice = Convert.ToDouble(worksheet.Cells[row, price].Value);
                                string asin = worksheet.Cells[row, 2].Value.ToString();

                                if (isInDB(asin))
                                {
                                    amazonPrintList.RemoveAll(x => x.Asin == asin);
                                }
                                else
                                {
                                    double sellingPrice = 0.0;
                                    // Add to the dictionary
                                    if (isFragrancex(digitSku))
                                    {
                                        if (!isInDB(asin))
                                        {
                                            Amazon amazon = new Amazon();
                                            amazon.id = amazonList.Count + 1;
                                            amazon.Asin = asin;
                                            skuID = DigitGetter(rowSku);
                                            //if(skuID == 0)
                                            amazon.sku = skuID.ToString();
                                            amazon.price = Convert.ToDouble(worksheet.Cells[row, 3].Value);
                                            amazon.wholesaler = Wholesalers.Fragrancex.ToString();
                                            amazon.blackList = false;
                                            amazon.marketPlace = marketPlace.ToString();
                                            amazonList.Add(amazon);
                                        }
                                    }
                                    else if (isAzImporter(rowSku))
                                    {
                                        if (!isInDB(asin))
                                        {
                                            Amazon amazon = new Amazon();

                                            amazon.id = amazonList.Count + 1;
                                            amazon.Asin = asin;
                                            sellingPrice = getSellingPrice();
                                            amazon.sku = azImporter.Sku.ToUpper();
                                            amazon.price = Convert.ToDouble(worksheet.Cells[row, 3].Value);
                                            amazon.wholesaler = Wholesalers.AzImporter.ToString();
                                            amazon.blackList = false;
                                            amazon.marketPlace = marketPlace.ToString();
                                            amazonList.Add(amazon);
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }

                    worksheet.DeleteRow(2, rowCount);

                    row = 2;

                    foreach (Amazon list in amazonPrintList.Where(x => x.blackList == false 
                        && x.marketPlace == marketPlace.ToString()).OrderBy(x => x.wholesaler))
                    {
                        Random rnd = new Random();
                        Random rnd2 = new Random();
                        double rand3 = Convert.ToDouble(rnd2.Next(1, 99)) / 100;
                        worksheet.Cells[row, 1].Value = list.sku + " " + rnd.Next(1, 49999);
                        worksheet.Cells[row, 2].Value = list.Asin;
                        worksheet.Cells[row, 3].Value = 1;
                        worksheet.Cells[row, 4].Value = list.price + rand3;
                        worksheet.Cells[row, 5].Value = "delete";
                        worksheet.Cells[row, 6].Value = "delete";
                        worksheet.Cells[row, 7].Value = 11;
                        worksheet.Cells[row, 8].Value = 0;
                        worksheet.Cells[row, 9].Value = "a";
                        worksheet.Cells[row, 10].Value = "n";
                        worksheet.Cells[row, 14].Value = "unknown_binding";
                        row++;
                    }

                    package.Save();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool isInDB(string asin)
        {
            if(amazonList.Any(x => x.Asin == asin && x.marketPlace == marketPlace.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private void setList()
        {
            foreach(var item in amazonList)
            {
                amazonPrintList.Add(item);
            }
        }

        public string getSellingPrice(long? skuID)
        {
            double sellingPrice = 0;

            double summer = 0.0;

            fragrancexPrices.TryGetValue(Convert.ToInt32(skuID), out sellingPrice);

            if (sellingPrice == 0)
            {
                return "0.0";
            }

            double innerPrice = Convert.ToDouble(sellingPrice);

            // profit 20% by default
            summer = innerPrice + (innerPrice * 20) / 100;

            // shipping
            summer = summer + 6;

            // Amazon Fee 20%
            summer = summer + (summer * 20) / 100;

            return summer.ToString();
        }

        public string path { get; set; }

        public Dictionary<int, double> fragrancexPrices { get; set; }

        public List<Amazon> amazonList { get; set; }

        public MarketPlace marketPlace { get; set; }

        public List<Amazon> amazonPrintList { get; set; }
        
    }
}
