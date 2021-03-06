﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrgxPublicApiSDK;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ExcelModifier;
using DatabaseModifier;
using System.Collections.Concurrent;
using GTI_Solutionx.Data;
using GTI_Solutionx.Models.Dashboard;
using GTI_Solutionx.Code;
using Microsoft.AspNetCore.Authorization;
using EFCore.BulkExtensions;
using FrgxPublicApiSDK.Models;

namespace GTI_Solutionx.Controllers
{
    public class ShopifyController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ApplicationDbContext _context;
    
        public ShopifyController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "Admin, user")]
        public IActionResult Index()
        {
            ViewBag.TimeStamp = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault().TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault().type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault().Wholesalers;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();
            
            var profile = new Profile();

            return View(_context.UsersList.Distinct().ToList());
        }

        [Authorize(Roles = "Admin, user")]
        public IActionResult Upload()
        {
            UploadUI();

            return View(_context.Profile.ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Update()
        {
            return View(_context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UpdateExcel()
        {
            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();

            var test = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString());

            return View(_context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin, user")]
        public IActionResult Delete()
        {
            return View(_context.UsersList.Distinct().ToList());
        }

        [Authorize(Roles = "Admin, user")]
        [HttpPost]
        public async Task<IActionResult> ProductExport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file.FileName);
            
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ServiceTimeStamp service = new ServiceTimeStamp();

            if (_context.ServiceTimeStamp.Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault<ServiceTimeStamp>() == null)
            {
                Helper.tablePreparer(path);

                service.TimeStamp = DateTime.Today;
                service.Wholesalers = Wholesalers.Fragrancex.ToString();
                _context.ServiceTimeStamp.Add(service);
                _context.SaveChanges();

            }
            else if(_context.ServiceTimeStamp.Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault<ServiceTimeStamp>().TimeStamp != DateTime.Today)
            {
                Helper.tablePreparer(path);
                service.Wholesalers = Wholesalers.Fragrancex.ToString();
                service.TimeStamp = DateTime.Today;
                _context.ServiceTimeStamp.Add(service);
                _context.SaveChanges();
            }

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
        public IActionResult Upload(string file, string User, Profile profile2)
        {
            try
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

                Profile profile = _context.Profile.AsNoTracking().Where<Profile>(x => x.ProfileUser == User).FirstOrDefault();

                var shopifyProfile = _context.ShopifyUser.ToDictionary(x => x.sku, x => x);
                var shopifyUsers = _context.UsersList.Where(x => x.userID == profile.ProfileUser)
                    .ToDictionary(x => x.sku, y => y.userID);

                ConcurrentDictionary<string, string> shopifyUsersConcurrent =
                    new ConcurrentDictionary<string, string>(shopifyUsers);

                ShopifyExcelCreator shopifyModifier =
                    new ShopifyExcelCreator(profile, shopifyProfile, shopifyUsersConcurrent, path);

                try
                {
                    shopifyModifier.ExcelGenerator();
                }
                catch (Exception e)
                {
                    System.IO.File.Delete(path);

                    throw e;
                }

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
                    try
                    {
                        DBModifierShopifyUserList user =
                            new DBModifierShopifyUserList(shopifyModifier.shopifyUserTemp, profile);
                        user.TableExecutor();
                        // Execute raw query

                        var command = sourceConnection.CreateCommand();
                        command.CommandText = "exec MergeUsersList;";
                        command.Connection = sourceConnection;
                        command.ExecuteNonQuery();
                        sourceConnection.Close();

                    }
                    catch (Exception e)
                    {
                        sourceConnection.Close();
                        System.IO.File.Delete(path);
                        throw e;
                    }
                }
            }
            catch(Exception e)
            {
                ViewData["Error"] = e.Message.ToString();

                UploadUI();

                return View(_context.Profile.ToList());
            }

            UploadUI();

            return View(_context.Profile.ToList());
        }

        [Authorize(Roles = "Admin, user")]
        [HttpPost]
        public async Task<IActionResult> Index(string file, string shipping
            , string fee, string profit, string promoting, string markdown, int items, int min
            , int max, string User)
        {
            try
            {
                var path = Path.Combine(
                       Directory.GetCurrentDirectory(), @"wwwroot\Excel_Source\Shopify_Upload.xlsx");

                Match shippingMatch = Regex.Match(shipping, @"[\d]+");

                Match amazonFee = Regex.Match(fee, @"[\d]+[/.]?[\d]+");

                Match promotingFee = Regex.Match(promoting, @"[\d]+[/.]?[\d]+");

                Match profitMatch = Regex.Match(profit, @"[\d]+");

                Profile oldProfile = _context.Profile.AsNoTracking()
                                        .Where<Profile>(x => x.ProfileUser == User)
                                            .FirstOrDefault();

                Profile profile = new Profile
                {
                    shipping = Double.Parse(shippingMatch.Value),

                    fee = Double.Parse(shippingMatch.Value),

                    profit = Double.Parse(profitMatch.Value),

                    promoting = Double.Parse(promotingFee.Value),

                    ProfileUser = User,

                    items = items,

                    min = min,

                    max = max,

                    html = oldProfile.html,

                    LongstartTitle = oldProfile.LongstartTitle,

                    MidtartTitle = oldProfile.MidtartTitle,

                    ShortstartTitle = oldProfile.ShortstartTitle,

                    endTtile = oldProfile.endTtile,

                    sizeDivider = oldProfile.sizeDivider

                };

                if (markdown != null)
                {
                    Match markdownMatch = Regex.Match(markdown, @"[\d]+");
                    profile.markdown = Double.Parse(markdownMatch.Value);
                }

                var shopifyProfile = _context.ShopifyUser.ToDictionary(x => x.sku, x => x);
                var fragrancex = _context.Wholesaler_Fragrancex.ToDictionary(x => x.Sku, y => y);
                var upc = _context.UPC.ToDictionary(x => x.ItemID, y => y);
                var shopifyUsers = _context.UsersList.Where(x => x.userID == profile.ProfileUser)
                    .ToDictionary(x => x.sku, y => y.userID);

                ConcurrentDictionary<string, string> shopifyUsersConcurrent =
                    new ConcurrentDictionary<string, string>(shopifyUsers);

                ShopifyExcelCreator shopifyModifier =
                    new ShopifyExcelCreator(profile, shopifyProfile, fragrancex, shopifyUsersConcurrent, upc, path);

                try
                {
                    shopifyModifier.saveExcel();
                }
                catch (Exception e)
                {
                    System.IO.File.Delete(path);
                    throw e;
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;

                FileStreamResult returnFile =
                    File(memory, Helper.GetContentType(path), profile.ProfileUser
                    + "_Converted_" + DateTime.Today.GetDateTimeFormats()[10]
                    + Path.GetExtension(path).ToLowerInvariant());

                _context.Profile.Update(profile);

                _context.SaveChanges();

                return returnFile;
            }
            catch(Exception e)
            {
                ViewData["Error"] = e.Message.ToString();

                return View();
            }
        }

        [HttpPost]
        public IActionResult Update(string file)
        {   
            ServiceTimeStamp service = new ServiceTimeStamp();

            var timeStampDB = _context.ServiceTimeStamp
                                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                                    .LastOrDefault()?.TimeStamp;
            
            var timeStampToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                                , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).Date.AddHours(5);
            
            if (timeStampDB == null || DateTime.Compare((DateTime)timeStampDB, timeStampToday) < 0 )
            {
                try
                {
                    FragancexSQLPreparer(service);
                }
                catch (Exception e)
                {
                    ViewData["Error"] = e.Message.ToString();
                    return View(_context.ServiceTimeStamp
                            .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
                }

            }
            else
            {
                ViewData["Info"] = "Database already updated today after " + timeStampToday;
                return View(_context.ServiceTimeStamp
                    .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                        .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
            }

            ViewData["Success"] = "Database updated successfully at " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                    , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            return View(_context.ServiceTimeStamp
                    .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                        .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin, user")]
        [HttpPost]
        public async Task<IActionResult> UpdateExcel(string file)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file + ".xlsx");

            try
            {
                var upc = _context.UPC.ToDictionary(x => x.ItemID, y => y.Upc);

                DBModifierFragrancexExcel dBModifierFragrancexExcel = new DBModifierFragrancexExcel(path, upc);

                try
                {
                    dBModifierFragrancexExcel.TableExecutor();
                }
                catch (Exception e)
                {
                    throw e;
                }

                // Update the FragrancexList db

                var fragranceTitle = _context.FragrancexTitle.ToDictionary(x => x.ItemID, y => y.Title);

                // delete everything from the db, then update
                try
                {
                    using (var tran = _context.Database.BeginTransaction())
                    {
                        await _context.BulkDeleteAsync(_context.Wholesaler_Fragrancex.ToList());
                        tran.Commit();
                    }

                    using (var tran = _context.Database.BeginTransaction())
                    {
                        await _context.BulkInsertOrUpdateAsync(dBModifierFragrancexExcel.fragancexList);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                DBModifierFragrancexExcelList dBModifierFragrancexExcelList = new DBModifierFragrancexExcelList(path, fragranceTitle);

                // insert to the db and update fragranceTitle

                dBModifierFragrancexExcelList.TableExecutor();

                try
                {
                    using (var tran = _context.Database.BeginTransaction())
                    {
                        await _context.BulkInsertOrUpdateAsync(dBModifierFragrancexExcelList.fragrance);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                ServiceTimeStamp service = new ServiceTimeStamp();

                service.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                    , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                service.Wholesalers = Wholesalers.Fragrancex.ToString();

                service.type = "Excel";

                _context.ServiceTimeStamp.Add(service);

                _context.SaveChanges();
            }
            catch(Exception e)
            {
                System.IO.File.Delete(path);

                ViewData["Error"] = e.Message.ToString();
                return View(_context.ServiceTimeStamp
                            .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                                .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
            }

            System.IO.File.Delete(path);

            ViewData["Success"] = "Database updated successfully at " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                   , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            return View(_context.ServiceTimeStamp
                        .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                            .OrderByDescending(x => x.TimeStamp).Take(5).ToList());
        }

        [Authorize(Roles = "Admin, user")]
        [HttpPost]
        public IActionResult Delete(string user)
        {
            try
            {
                var users = _context.UsersList.Where(x => x.userID == user).ToList();

                _context.UsersList.RemoveRange(users);
                
                _context.SaveChanges();

                ViewData["Success"] = "User " + user + " deleted successfully!!";

                return View(_context.UsersList.Distinct().ToList());
            }
            catch(Exception e)
            {
                ViewData["Error"] = e.Message.ToString();

                return View(_context.UsersList.Distinct().ToList());
            }
        }

        private void FragancexSQLPreparer(ServiceTimeStamp service)
        {
            var upc = _context.UPC.ToDictionary(x => x.ItemID, y => y.Upc);
            
            try
            {
                var listingApiClient = new FrgxListingApiClient("346c055aaefd", "a5574c546cbbc9c10509e3c277dd7c7039b24324");

                // For testing purposes

                //List<Product> allProducts = new List<Product>();

                //var product = listingApiClient.GetProductById("412492");

                //allProducts.Add(product);

                var allProducts = listingApiClient.GetAllProducts();

                DBModifierFragrancexAPI dBModifierFragrancexAPI = new DBModifierFragrancexAPI("", upc)
                {
                    allProducts = allProducts
                };

                dBModifierFragrancexAPI.TableExecutor();

                // delete everything from the db, then update

                using (var tran = _context.Database.BeginTransaction())
                {
                    _context.BulkDelete(_context.Wholesaler_Fragrancex.ToList());
                    tran.Commit();
                }

                using (var tran = _context.Database.BeginTransaction())
                {
                    _context.BulkInsertOrUpdate(dBModifierFragrancexAPI.fragrancex);
                    tran.Commit();
                }

                service.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow
                    , TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                service.Wholesalers = Wholesalers.Fragrancex.ToString();

                service.type = "API";

                _context.ServiceTimeStamp.Add(service);

                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void UploadUI()
        {
            ViewBag.TimeStamp = _context.ServiceTimeStamp
               .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
               .LastOrDefault().TimeStamp.ToString("dd/MM/yyyy HH:mm tt");

            ViewBag.type = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault().type;

            ViewBag.Wholesalers = _context.ServiceTimeStamp
                .Where(x => x.Wholesalers == Wholesalers.Fragrancex.ToString())
                .LastOrDefault().Wholesalers;

            Guid guid = Guid.NewGuid();

            ViewBag.ExcelGuid = guid.ToString();
        }
    }
}