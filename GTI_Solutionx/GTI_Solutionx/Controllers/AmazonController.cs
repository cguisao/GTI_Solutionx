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
            ViewBag.TimeStampFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.typeAzFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.typeAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.amazonItems = _context.Amazon.Where(x => x.blackList != true).Count();

            ViewBag.amazonFragrancex = _context.Amazon.Where(x => x.wholesaler == Wholesalers.Fragrancex.ToString()
                                         && x.blackList != true).Count();

            ViewBag.amazonAzImporter = _context.Amazon.Where(x => x.wholesaler == Wholesalers.AzImporter.ToString()
                                        && x.blackList != true).Count();

            ViewBag.amazonBlackListed = _context.Amazon.Where(x => x.blackList == true).Count();

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            Profile profile = new Profile();

            return View(_context.Amazon.ToList());
        }

        public IActionResult BlackList()
        {
            return View(_context.Amazon.ToList().Where(x => x.blackList == true));
        }

        public IActionResult Index()
        {
            ViewBag.TimeStampFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.typeAzFragrancex = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.typeAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.TimeStampPerfumeWorldWide = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.typePerfumeWorldWide = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.type;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            Profile profile = new Profile();

            return View(_context.Profile.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAmazonDB(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            var azImporter = _context.AzImporter.ToDictionary(x => x.Sku, x => x);

            var perfumeWorldWide = _context.PerfumeWorldWide.ToDictionary(x => x.sku, x => x);

            var fragrancex = _context.Fragrancex.ToDictionary(x => x.ItemID, x => x);

            var amazon = _context.Amazon.ToDictionary(x => x.Asin, x => x);

            var shipping = _context.Shipping.ToDictionary(x => x.weightId, x => x.ItemPrice);

            AmazonDBUploader amazonDBUploader = new AmazonDBUploader(path, azImporter, fragrancex
                , perfumeWorldWide, amazon, shipping);
            
            try
            {
                amazonDBUploader.ExcelGenerator();
            }
            catch (Exception e)
            {
                return null;
            }

            using (var tran = _context.Database.BeginTransaction())
            {
                await _context.BulkInsertOrUpdateAsync(amazonDBUploader.amazonPrintList);
                tran.Commit();
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
                + Path.GetExtension(path).ToLowerInvariant());

            System.IO.File.Delete(path);
            
            return returnFile;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAmazonList(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            var blackListed = _context.Amazon.Where(z => z.blackList == true).ToDictionary(x => x.Asin, y => y.blackList);

            var fragancex = _context.Fragrancex.ToDictionary(x => x.ItemID, x => x);

            var azImporter = _context.AzImporter.ToDictionary(x => x.Sku, x => x);

            var shipping = _context.Shipping.ToDictionary(x => x.weightId, x => x.ItemPrice);

            var perfumeWorldWide = _context.PerfumeWorldWide.ToDictionary(x => x.sku, x => x);

            AmazonExcelUpdator amazonExcelUpdator = new AmazonExcelUpdator(path, fragancex, azImporter
                , blackListed, shipping, perfumeWorldWide);

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

        [HttpPost]
        public IActionResult BlackList(string Asin, string modifer)
        {
            Amazon currAsin = new Amazon();

            var amazon = _context.Amazon.Where(x => x.Asin == Asin);

            currAsin.id = Convert.ToInt32(amazon.Select(x => x.id).FirstOrDefault());
            currAsin.Asin = Convert.ToString(amazon.Select(x => x.Asin).FirstOrDefault());
            currAsin.sku = Convert.ToString(amazon.Select(x => x.sku).FirstOrDefault());
            currAsin.wholesaler = Convert.ToString(amazon.Select(x => x.wholesaler).FirstOrDefault());
            currAsin.price = Convert.ToDouble(amazon.Select(x => x.price).FirstOrDefault());

            if (currAsin != null)
            {
                currAsin.blackList = Convert.ToBoolean(modifer);
                _context.Amazon.Update(currAsin);
                _context.SaveChanges();
            }
            
            return Redirect("BlackList");
        }

        private void SetDictionariesAsync()
        {
            var azImporter = _context.AzImporter.ToHashSet();

            var fragrancex = _context.Fragrancex.ToHashSet();

            shippingWeight = _context.Shipping.ToDictionary(x => x.weightId, y => y.ItemPrice);
            
            var tasks = new List<Task>();

            Task fragrancexPricesTask = new Task(() => fragrancexPrices = fragrancex.ToDictionary(x => x.ItemID, y => y.WholePriceUSD));

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
