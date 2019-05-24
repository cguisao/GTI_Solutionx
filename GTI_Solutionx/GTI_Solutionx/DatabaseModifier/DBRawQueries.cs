using GTI_Solutionx.Models.Dashboard;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;

namespace DatabaseModifier
{
    public class DBRawQueries : Database
    {
        public long? getUpc2(string itemID)
        {
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables();
            long upc = 0L;

            IConfiguration Configuration;
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            string connectionstring = Configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection sourceConnection = new SqlConnection(connectionstring))
            {
                sourceConnection.Open();

                SqlCommand commandSourceData = new SqlCommand(
                "SELECT * FROM UPC WHERE " + "itemID = " + "'" + itemID + "'", sourceConnection);

                SqlDataReader reader = commandSourceData.ExecuteReader();
                
                while (reader.Read())
                {
                    upc = Convert.ToInt64(reader[1]);
                }

                sourceConnection.Close();
            }

            return upc;

        }

        public Wholesaler_Fragrancex getFragrancex2(string itemID)
        {
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables();

            IConfiguration Configuration;

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();

            string connectionstring = Configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection sourceConnection = new SqlConnection(connectionstring))
            {
                sourceConnection.Open();

                SqlCommand commandSourceData = new SqlCommand(
                "SELECT * FROM Fragrancex WHERE "
                + "itemID = " + "'" + itemID + "'", sourceConnection);

                SqlDataReader reader = commandSourceData.ExecuteReader();

                Wholesaler_Fragrancex fragrancex = new Wholesaler_Fragrancex();

                while (reader.Read())
                {
                    fragrancex.Sku = Convert.ToInt32(reader[0]);
                    fragrancex.BrandName = reader[1].ToString();
                    fragrancex.Description = reader[2].ToString();
                    fragrancex.Gender = reader[3].ToString();
                    fragrancex.WholePriceUSD = Convert.ToInt32(reader[17]);
                }

                sourceConnection.Close();

                return fragrancex;
            }
        }
    }
}
