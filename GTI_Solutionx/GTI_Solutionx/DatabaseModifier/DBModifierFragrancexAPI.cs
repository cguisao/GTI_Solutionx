using System;
using System.Collections.Generic;
using System.Data;
using FrgxPublicApiSDK.Models;
using GTI_Solutionx.Models.Dashboard;

namespace DatabaseModifier
{
    public class DBModifierFragrancexAPI : DBModifierFragrancexExcel, IDatabaseModifier
    {
        public DBModifierFragrancexAPI(string path, Dictionary<int, long?> upc) : base(path, upc)
        {
            this.upc = upc;
        }

        private Dictionary<int, long?> upc { get; set; }

        public List<Product> allProducts { get; set; }

        public List<Wholesaler_Fragrancex> fragrancex = new List<Wholesaler_Fragrancex>();

        public override void TableExecutor()

        {
            long? value = 0;

            int bulkSize = 1;

            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var product in allProducts)
            {
                try
                {
                    dic.Add(product.ItemId, product.ProductName);
                    if (product != null)
                    {
                        Wholesaler_Fragrancex fran = new Wholesaler_Fragrancex();

                        fran.id = bulkSize + 1;
                        fran.Sku = Convert.ToInt32(product.ItemId);
                        fran.BrandName = product.BrandName;
                        fran.Description = product.Description;
                        fran.Gender = product.Gender;
                        fran.Instock = product.Instock;
                        fran.LargeImageUrl = product.LargeImageUrl;
                        fran.MetricSize = product.MetricSize;
                        fran.ParentCode = product.ParentCode;
                        fran.ProductName = product.ProductName;
                        fran.RetailPriceUSD = product.RetailPriceUSD;
                        fran.Size = product.Size;
                        fran.SmallImageURL = product.SmallImageUrl;
                        fran.Type = product.Type;
                        fran.WholePriceAUD = product.WholesalePriceAUD;
                        fran.WholePriceCAD = product.WholesalePriceCAD;
                        fran.WholePriceEUR = product.WholesalePriceEUR;
                        fran.WholePriceGBP = product.WholesalePriceGBP;
                        fran.WholePriceUSD = product.WholesalePriceUSD;

                        if (upc.TryGetValue(Convert.ToInt32(product.ItemId), out value))
                        {
                            fran.Upc = value;
                        }

                        fragrancex.Add(fran);
                    }
                }
                catch
                {
                    throw;
                }
            }

            //upload(uploadFragrancex, bulkSize, "dbo.Fragrancex");

        }

        public DataTable CreateTable()
        {
            throw new NotImplementedException();
        }
    }
}
