﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DatabaseModifier;
using EFCore.BulkExtensions;
using GTI_Solutionx.Data;
using GTI_Solutionx.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GTI_Solutions.Controllers
{
    public class AzImporterController : Controller
    {
        public ApplicationDbContext _context;

        public AzImporterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            ViewBag.TimeStamp = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.Wholesalers;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            return View(_context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Shipping()
        {
            return View(_context.Shipping);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");
            try
            {
                var azImportItems = _context.Wholesaler_AzImporter.ToDictionary(x => x.Sku, y => y);

                foreach (var az in azImportItems)
                {
                    az.Value.Quantity = 0;
                }

                DBModifierAzImporterExcel AzImporter = new DBModifierAzImporterExcel(path, azImportItems);

                AzImporter.TableExecutor();

                // delete everything from the db, then update

                using (var tran = _context.Database.BeginTransaction())
                {
                    await _context.BulkDeleteAsync(_context.Wholesaler_AzImporter.ToList());
                    tran.Commit();
                }

                using (var tran = _context.Database.BeginTransaction())
                {
                    await _context.BulkInsertOrUpdateAsync(AzImporter.azImport);
                    tran.Commit();
                }

                ServiceTimeStamp service = new ServiceTimeStamp();

                service.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                        , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                service.Wholesalers = Wholesalers.AzImporter.ToString();

                service.type = "Excel";

                _context.ServiceTimeStamp.Add(service);

                _context.SaveChanges();

            }
            catch(Exception e)
            {
                SetUIValues(path);

                ViewData["Error"] = e.Message.ToString();

                return View(_context.ServiceTimeStamp
                            .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
            }

            SetUIValues(path);

            ViewData["Success"] = "Database updated successfully at " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                    , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            return View(_context.ServiceTimeStamp
                        .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                            .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        private void SetUIValues(string path)
        {
            System.IO.File.Delete(path);

            ViewBag.TimeStamp = _context.ServiceTimeStamp
            .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
            .LastOrDefault()?.TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.Wholesalers;
        }

        [HttpPost]
        public Task<IActionResult> UpdateAzImports()
        {
            //var client = new System.Net.WebClient();
            string url = @"http://www.azimporter.com/datafeed/dailydatafeed2.csv";
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "test.csv");

            //client.DownloadFile(url, path);

            var memory = new MemoryStream();

            using (var client = new WebClient())
            {
                var content = client.DownloadData(url);

                string csvDocument = Encoding.ASCII.GetString(content);

                var file = new FileInfo(path);

                using (FileStream fs = file.Create())
                {
                    //Add some information to the file.
                    fs.Write(content, 0, csvDocument.Length);
                }

                var lines = System.IO.File.ReadAllLines(path, Encoding.UTF8).Select(a => a.Split(','));

                List<Wholesaler_AzImporter> items = new List<Wholesaler_AzImporter>();
                int skipFirst = 1;

                foreach (var line in lines)
                {    
                    if (line.Length == 17)
                    {
                        int curr = 1;
                        Wholesaler_AzImporter azImporter = new Wholesaler_AzImporter();
                        foreach (var col in line)
                        {
                            // skip the first line because it is the title of the csv file
                            if (skipFirst == 1)
                                break;
                            // The first item is always the sku
                            if(curr == 1)
                            {
                                azImporter.Sku = col.Replace('"', ' ');
                            }
                            // loop until the first lines contains HTTP

                            // Once it does not have HTTP and it can be converted into a double
                            // we have the wholesale

                            // The next should the quantity and it can be turned into an integer

                            // Add the azImporter to the list
                            if (curr == 17)
                            {
                                items.Add(azImporter);
                            }

                            curr++;
                        }
                        skipFirst++;
                    }
                }

                //List<AzImporter> items = new List<AzImporter>();
                //int fieldCount = 0;
                //string[] headers;

                //using (TextFieldParser parser = new TextFieldParser(@"c:\temp\test.csv"))
                //{
                //    parser.TextFieldType = FieldType.Delimited;
                //    parser.SetDelimiters(",");
                //    while (!parser.EndOfData)
                //    {
                //        //Process row
                //        string[] fields = parser.ReadFields();
                //        foreach (string field in fields)
                //        {
                //            //TODO: Process field
                //        }
                //    }
                //}

                //using (CsvReader csv =  new CsvReader(file.OpenText()))
                //{
                //    fieldCount = csv.FieldCount;

                //    headers = csv.GetFieldHeaders();
                //    while (csv.ReadNextRecord())
                //    {
                //        for (int i = 0; i < fieldCount; i++)
                //            Console.Write(string.Format("{0} = {1};",
                //                          headers[i], csv[i]));
                //        Console.WriteLine();
                //    }
                //}

                using (var workbook = new XLWorkbook())
                {
                    //rowCount = 1;
                    Wholesaler_AzImporter azImporter = new Wholesaler_AzImporter();
                    //foreach (var line in csv.GetRecords<AzImporter>())
                    //{


                    //    colCount = 1;
                    //    //foreach (var col in line)
                    //    //{
                    //    //    if (rowCount != 1)
                    //    //    {
                    //    //        if(colCount == 1)
                    //    //        {
                    //    //            azImporter.Sku = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if(colCount == 2)
                    //    //        {
                    //    //            azImporter.Category = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 3)
                    //    //        {
                    //    //            azImporter.ItemName = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 4)
                    //    //        {
                    //    //            azImporter.Image1 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 5)
                    //    //        {
                    //    //            azImporter.Image2 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 6)
                    //    //        {
                    //    //            azImporter.Image3 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 7)
                    //    //        {
                    //    //            azImporter.Image4 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 8)
                    //    //        {
                    //    //            azImporter.Image5 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 9)
                    //    //        {
                    //    //            azImporter.Image6 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 10)
                    //    //        {
                    //    //            azImporter.Image7 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 11)
                    //    //        {
                    //    //            azImporter.Image8 = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 12)
                    //    //        {
                    //    //            azImporter.MainImage = col.Replace('"', ' ');
                    //    //        }
                    //    //        else if (colCount == 13)
                    //    //        {
                    //    //            azImporter.WholeSale = Convert.ToDouble(col.Replace('"', ' '));
                    //    //        }
                    //    //        else if (colCount == 14)
                    //    //        {
                    //    //            azImporter.Quantity = Convert.ToInt32(col.Replace('"', ' '));
                    //    //        }
                    //    //        else if (colCount == 15)
                    //    //        {
                    //    //            azImporter.Weight = Convert.ToInt32(col.Replace('"', ' '));
                    //    //        }
                    //    //        else if (colCount == 16)
                    //    //        {
                    //    //            azImporter.HTMLDescription = col.Replace('"', ' ');
                    //    //        }
                    //    //    }

                    //    //    colCount++;
                    //    //}
                    //    rowCount++;
                    //    items.Add(azImporter);
                    //}
                }
            }

            memory.Position = 0;


            //return new FileContentResult(bytes, MimeMapping.GetMimeMapping(f));

            return null;
        }

        [HttpPost]
        public IActionResult Shipping(int Weight, double Price)
        {
            try
            {
                Shipping shipping = new Shipping();
                shipping.ItemPrice = Price;
                shipping.weightId = Weight;

                if (_context.Shipping.Any(x => x.weightId == Weight))
                {
                    _context.Shipping.Update(shipping);

                    _context.SaveChanges();
                }
                else
                {
                    _context.Shipping.Add(shipping);

                    _context.SaveChanges();
                }
            }
            catch(Exception e)
            {
                ViewData["Error"] = e.Message.ToString();
                return View(_context.Shipping);
            }

            ViewData["Success"] = "Database updated successfully!!";

            return View(_context.Shipping);
        }
    }
}