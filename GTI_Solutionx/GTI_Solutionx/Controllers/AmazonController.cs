using DatabaseModifier;
using EFCore.BulkExtensions;
using ExcelModifier;
using GTI_Solutionx.Code;
using GTI_Solutionx.Data;
using GTI_Solutionx.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GTI_Solutionx.Controllers
{
    [Authorize(Roles = "Admin, user")]
    public class AmazonController : Controller
    {
        public ApplicationDbContext _context;
        
        public AmazonController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Upload()
        {
            UploadUI();

            return View();
        }

        public IActionResult BlackList()
        {
            return View(_context.Amazon.ToList().Where(x => x.blackList == true));
        }

        public IActionResult Index()
        {
            ViewBag.TimeStampFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.typeAzFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.typeAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampPerfumeWorldWide = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.typePerfumeWorldWide = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.type;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            Profile profile = new Profile();

            return View(_context.Profile.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string file, MarketPlace marketPlace)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");
            try
            {
                var azImporter = _context.Wholesaler_AzImporter.ToDictionary(x => x.Sku, x => x);

                //var perfumeWorldWide = _context.PerfumeWorldWide.ToDictionary(x => x.sku, x => x);

                var fragrancex = _context.Wholesaler_Fragrancex.ToDictionary(x => x.Sku, x => x);

                var amazon = _context.Amazon.Where(x => x.marketPlace == marketPlace.ToString()).ToList();

                var shipping = _context.Shipping.ToDictionary(x => x.weightId, x => x.ItemPrice);

                var amazonNumber = _context.Amazon.Count();

                AmazonDBUploader amazonDBUploader = new AmazonDBUploader(path, azImporter, fragrancex
                    , amazon, shipping, marketPlace, amazonNumber);

                try
                {
                    amazonDBUploader.ExcelGenerator();
                }
                catch (Exception e)
                {
                    System.IO.File.Delete(path);
                    ViewData["Error"] = e.Message.ToString();

                    return View();
                }
                try
                {
                    using (var tran = _context.Database.BeginTransaction())
                    {
                        await _context.BulkInsertAsync(amazonDBUploader.amazonList);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {
                    System.IO.File.Delete(path);
                    ViewData["Error"] = e.Message.ToString();
                    return View();
                }

                var memory = new MemoryStream();

                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;

                FileStreamResult returnFile =
                    File(memory, Helper.GetContentType(path), "Amazon"
                        + "_Converted_" + DateTime.Today.GetDateTimeFormats()[10]
                            + marketPlace.ToString() + Path.GetExtension(path).ToLowerInvariant());

                System.IO.File.Delete(path);

                return returnFile;
            }
            catch(Exception)
            {
                System.IO.File.Delete(path);

                ViewData["Error"] = "The ASIN and/or Market Place is NOT in the database.";

                UploadUI();

                return View();
            }
        }

        private void UploadUI()
        {
            ViewBag.TimeStampFragrancex = _context.ServiceTimeStamp
                            .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                            .LastOrDefault()?.TimeStamp.ToString("MM/dd/yyyy hh:mm tt");

            ViewBag.typeAzFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToString("MM/dd/yyyy hh:mm tt");

            ViewBag.typeAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.amazonUS = _context.Amazon.Where(x => x.blackList != true
                                        && x.marketPlace == MarketPlace.US.ToString()).Count();

            ViewBag.amazonUK = _context.Amazon.Where(x => x.blackList != true
                                        && x.marketPlace == MarketPlace.UK.ToString()).Count();

            ViewBag.amazonJP = _context.Amazon.Where(x => x.blackList != true
                                        && x.marketPlace == MarketPlace.JP.ToString()).Count();

            ViewBag.amazonAU = _context.Amazon.Where(x => x.blackList != true
                                        && x.marketPlace == MarketPlace.AU.ToString()).Count();

            ViewBag.amazonFragrancex = _context.Amazon.Where(x => x.wholesaler == Wholesalers.Fragrancex.ToString()
                                         && x.blackList != true).Count();

            ViewBag.amazonAzImporter = _context.Amazon.Where(x => x.wholesaler == Wholesalers.AzImporter.ToString()
                                        && x.blackList != true).Count();

            ViewBag.amazonBlackListed = _context.Amazon.Where(x => x.blackList == true).Count();

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string file, MarketPlace marketPlace)
        {
            try
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

                var blackListed = _context.Amazon.Where(z => z.blackList == true).ToDictionary(x => x.Asin, y => y.blackList);

                var fragancex = _context.Wholesaler_Fragrancex.ToDictionary(x => x.Sku, x => x);

                var azImporter = _context.Wholesaler_AzImporter.ToDictionary(x => x.Sku, x => x);

                var shipping = _context.Shipping.ToDictionary(x => x.weightId, x => x.ItemPrice);

                var perfumeWorldWide = _context.PerfumeWorldWide.ToDictionary(x => x.sku, x => x);

                AmazonExcelUpdator amazonExcelUpdator = new AmazonExcelUpdator(path, fragancex, azImporter
                    , blackListed, shipping, perfumeWorldWide, marketPlace);

                amazonExcelUpdator.ExcelGenerator();

                var memory = new MemoryStream();

                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;

                FileStreamResult returnFile =
                    File(memory, Helper.GetContentType(path), "Amazon"
                    + "_Converted_" + DateTime.Today.GetDateTimeFormats()[10]
                    + Path.GetExtension(path).ToLowerInvariant());

                System.IO.File.Delete(path);

                return returnFile;
            }
            catch(Exception e)
            {
                ViewData["Error"] = e.Message.ToString();

                return View();
            }
        }

        [HttpPost]
        public IActionResult BlackList(string Asin, string modifer, MarketPlace marketPlace)
        {
            Amazon currAsin = new Amazon();

            var amazon = _context.Amazon.Where(x => x.Asin == Asin && x.marketPlace == marketPlace.ToString());

            currAsin.id = Convert.ToInt32(amazon.Select(x => x.id).FirstOrDefault());
            currAsin.Asin = Convert.ToString(amazon.Select(x => x.Asin).FirstOrDefault());
            currAsin.sku = Convert.ToString(amazon.Select(x => x.sku).FirstOrDefault());
            currAsin.wholesaler = Convert.ToString(amazon.Select(x => x.wholesaler).FirstOrDefault());
            currAsin.price = Convert.ToDouble(amazon.Select(x => x.price).FirstOrDefault());
            currAsin.marketPlace = marketPlace.ToString();

            try
            {
                if (currAsin != null)
                {
                    currAsin.blackList = Convert.ToBoolean(modifer);
                    _context.Amazon.Update(currAsin);
                    _context.SaveChanges();
                }
            }
            catch
            {
                ViewData["Error"] = "The ASIN and/or Market Place is NOT in the database.";
                return View(_context.Amazon.ToList().Where(x => x.blackList == true));
            }

            ViewData["Success"] = "ASIN added to the database successfully.";
            return View(_context.Amazon.ToList().Where(x => x.blackList == true));
        }

        private void SetDictionariesAsync()
        {
            var azImporter = _context.Wholesaler_AzImporter.ToHashSet();

            var fragrancex = _context.Wholesaler_Fragrancex.ToHashSet();

            shippingWeight = _context.Shipping.ToDictionary(x => x.weightId, y => y.ItemPrice);
            
            var tasks = new List<Task>();

            Task fragrancexPricesTask = new Task(() => fragrancexPrices = fragrancex.ToDictionary(x => x.Sku, y => y.WholePriceUSD));

            Task azImportPriceTask = new Task(() => azImportPrice = azImporter.ToDictionary(x => x.Sku, y => y.WholeSale));

            Task azImportQuantityTask = new Task(() => azImportQuantity = azImporter.ToDictionary(x => x.Sku, y => y.Quantity));

            Task azImporterWeightSkuTask = new Task(() => azImporterWeightSku = azImporter.ToDictionary(x => x.Sku, y => y.Weight));

            tasks.Add(fragrancexPricesTask);

            tasks.Add(azImportPriceTask);

            tasks.Add(azImportQuantityTask);

            tasks.Add(azImporterWeightSkuTask);

            Parallel.ForEach(tasks, task =>
            {
                task.RunSynchronously();
            });
        }

        private List<Amazon> amazonList { get; set; }

        private Dictionary<int, double> fragrancexPrices { get; set; }

        private Dictionary<string, double> azImportPrice { get; set; }

        private Dictionary<string, int> azImportQuantity { get; set; }

        private Dictionary<string, int> azImporterWeightSku { get; set; }

        private Dictionary<int, double> shippingWeight { get; set; }

        private Dictionary<string, string> amazonItems { get; set; }

        private List<Amazon> amazonList2 { get; set; }
    }
}
