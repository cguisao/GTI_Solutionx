using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GTI_Solutionx.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using GTI_Solutionx.Data;
using GTI_Solutionx.Models.Dashboard;
using Microsoft.EntityFrameworkCore;
using GTI_Solutionx.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using System.Net;
using GTI_Solutionx.Code;

namespace GTI_Solutionx.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }
        public IActionResult Index()
        {
            ViewBag.TimeStampFragrancex = _context.ServiceTimeStamp
               .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
               .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.FragrancexItems = _context.Wholesaler_Fragrancex.Count();

            ViewBag.TimeStampAzImport = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.AzImporter.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.AzImportItems = _context.Wholesaler_AzImporter.Count();

            ViewBag.TimeStamPerfumeWorldWide = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.PerfumeWorldWide.ToString())
                .LastOrDefault()?.TimeStamp.ToShortDateString();

            ViewBag.PerfumeWorldWideItems = _context.PerfumeWorldWide.Count();

            ViewBag.AsinUploaded = _context.Amazon.Count(x => x.blackList != true);

            ViewBag.BlackListed = _context.Amazon.Count(x => x.blackList != false);
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DropzoneFileUpload(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            if (!file.FileName.Contains(".xlsx"))
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Wrong file type");
                return null;
            }

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        fileName + ".xlsx");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UserProcess(string email)
        {
            var RoleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult roleResult;
            //Adding User Role
            var roleCheck = await RoleManager.RoleExistsAsync(Users.user.ToString());
            if (!roleCheck)
            {
                //create the roles and seed them to the database
                roleResult = await RoleManager.CreateAsync(new IdentityRole(Users.user.ToString()));
            }
            
            ApplicationUser user = await UserManager.FindByEmailAsync(email);
            await UserManager.AddToRoleAsync(user, Users.user.ToString());
            await UserManager.RemoveFromRoleAsync(user, Users.Pending.ToString());
            return Redirect("Index");
        }

        [HttpPost]
        public IActionResult ContactUsMessage(string email
            , string subject, string message)
        {
            Helper helper = new Helper();

            helper.sendEmail("smtp.gmail.com", 587, "gtisolutions49@gmail.com", "lotero321"
                , "gtisolutions49@gmail.com", email, message, subject);
            return Redirect("Index"); ;
        }

        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
