using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using DatabaseModifier;
using GTI_Solutionx.Data;
using Microsoft.AspNetCore.Authorization;

namespace GTI_Solutionx.Controllers
{
    public class UpcController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ApplicationDbContext _context;

        public UpcController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Upcs()
        {
            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            return View(_context.UPC.ToList());
        }

        public IActionResult UpcViewer(int? page, string Search_Data)
        {
            var upcs = _context.UPC.ToList();

            var pageNumber = page ?? 1;

            return View();
        }

        [HttpPost]
        public IActionResult UPCImporter(string file)
        {

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            // Update the DB with the new UPCs

            DBModifierUPC databaseUPC = new DBModifierUPC(path);

            databaseUPC.TableExecutor();

            System.IO.File.Delete(path);

            return RedirectToAction("Upcs");
        }
    }
}