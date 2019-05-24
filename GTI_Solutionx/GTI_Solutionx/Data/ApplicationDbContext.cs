using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GTI_Solutionx.Models;
using GTI_Solutionx.Models.Dashboard;

namespace GTI_Solutionx.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        } 

        public DbSet<Profile> Profile { set; get; }
        public DbSet<ServiceTimeStamp> ServiceTimeStamp { get; set; }
        public DbSet<Wholesaler_Fragrancex> Wholesaler_Fragrancex { get; set; }
        public DbSet<UPC> UPC { get; set; }
        public DbSet<Wholesaler_AzImporter> Wholesaler_AzImporter { get; set; }
        public DbSet<PerfumeWorldWide> PerfumeWorldWide { get; set; }
        public DbSet<Shipping> Shipping { get; set; }
        public DbSet<Amazon> Amazon { get; set; }
        public DbSet<FragrancexTitles> FragrancexTitle { get; set; }
        public DbSet<ShopifyUser> ShopifyUser { get; set; }
        public DbSet<UsersList> UsersList { get; set; }
        public DbSet<UsersListTemp> UsersListTemp { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<>
            //builder.Entity<AspNetUserRoles>
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
