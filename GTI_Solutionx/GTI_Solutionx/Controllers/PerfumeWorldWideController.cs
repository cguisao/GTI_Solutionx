using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatabaseModifier;
using ExcelModifier;
using GTI_Solutionx.Code;
using GTI_Solutionx.Data;
using GTI_Solutionx.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GTI_Solutionx.Controllers
{
    public class PerfumeWorldWideController : Controller
    {
        public ApplicationDbContext _context;

        public PerfumeWorldWideController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            ViewBag.TimeStamp = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.Wholesalers;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();
            
            return View(_context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateExcel()
        {
            ViewBag.TimeStamp = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.Wholesalers;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            return View(_context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CompareExcel(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            fragrancex = _context.FragrancexTitle.ToDictionary(x => x.ItemID, y => y.Title);

            fragrancexUpc = _context.Wholesaler_Fragrancex.Where(z => z.Upc != null).ToDictionary(x => x.Sku, y => y.Upc);

            PerfumeWorldWideComparer perfumeWorldWideComparer = new PerfumeWorldWideComparer(fragrancex)
            {
                path = path,
                fragrancexUpc = fragrancexUpc
            };

            perfumeWorldWideComparer.ExcelGenerator();

            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;

            FileStreamResult returnFile =
                File(memory, Helper.GetContentType(path), Path.GetFileNameWithoutExtension(path)
                + "_Converted" + Path.GetExtension(path).ToLowerInvariant());

            System.IO.File.Delete(path);

            return returnFile;
        }

        [HttpPost]
        public IActionResult UpdateDatabase(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            PerfumeWorldWide = _context.PerfumeWorldWide.ToDictionary(x => x.sku, y => y);

            DBModifierPerfumeWorldWideExcel dBModifierPerfumeWorldWideExcel 
                = new DBModifierPerfumeWorldWideExcel(path, PerfumeWorldWide);

            dBModifierPerfumeWorldWideExcel.TableExecutor();

            ServiceTimeStamp service = new ServiceTimeStamp();

            service.TimeStamp = DateTime.Today;

            service.Wholesalers = Wholesalers.PerfumeWorldWide.ToString();

            service.type = "Excel";

            _context.ServiceTimeStamp.Add(service);

            _context.SaveChanges();

            System.IO.File.Delete(path);

            return RedirectToAction("UpdateExcel");
        }

        private Dictionary<int, string> fragrancex { get; set; }

        private Dictionary<int, long?> fragrancexUpc { get; set; }

        private Dictionary<string, PerfumeWorldWide> PerfumeWorldWide { get; set; }
    }
}