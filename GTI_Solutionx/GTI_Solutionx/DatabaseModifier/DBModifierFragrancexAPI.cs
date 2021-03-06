﻿using System;
using System.Collections.Generic;
using System.Data;
using FrgxPublicApiSDK.Models;
using GTI_Solutionx.Models.Dashboard;

namespace DatabaseModifier
{
    public class DBModifierFragrancexAPI
    {
        public DBModifierFragrancexAPI(string path, Dictionary<int, long?> upc)
        {
            this.upc = upc;
        }

        private Dictionary<int, long?> upc { get; set; }

        public List<Product> allProducts { get; set; }

        public List<Wholesaler_Fragrancex> fragrancex = new List<Wholesaler_Fragrancex>();

        public void TableExecutor()

        {
            long? value = 0;

            int bulkSize = 1;

            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var product in allProducts)
            {
                try
                {
                    if (product != null && !dic.ContainsKey(product.ItemId))
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

                        dic.Add(fran.Sku.ToString(), fran.Sku.ToString());

                        fragrancex.Add(fran);
                    }
                }
                catch(Exception e)
                {
                    throw;
                }
            }

            //upload(uploadFragrancex, bulkSize, "dbo.Fragrancex");

        }

    }
}
