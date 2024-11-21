using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SentSms> SentSms { get; set; }
        public DbSet<SentEmail> SentEmail { get; set; }

        public DbSet<UserCategory> UserCategories { get; set; }
        public DbSet<UserSubcategory> UserSubcategories { get; set; }
        public DbSet<UserSubcategoryEvent> UserSubcategoryEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventUser> EventUsers { get; set; }

        public DbSet<UserRussiaSubject> UserRussiaSubjects { get; set; }
        public DbSet<UserCountry> UserCountries { get; set; }
        public DbSet<UserCompetence> UserCompetences { get; set; }

        #region Service
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Quota> Quotas { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelOption> HotelOptions { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<UserSubcategoryEventPackage> UserSubcategoryEventPackages { get; set; }
        public DbSet<PackageService> PackageServices { get; set; }
        public DbSet<UserPackageService> UserPackageServices { get; set; }
        public DbSet<BadgeService> BadgeServices { get; set; }
        public DbSet<BadgeServiceApplicationRole> BadgeServiceApplicationRoles { get; set; }
        public DbSet<BadgeColor> BadgeColors { get; set; }
        public DbSet<BadgeServiceCheckup> BadgeServiceCheckups { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventUser>()
                .HasKey(c => new { c.UserId, c.UserSubcategoryEventId });

            builder.Entity<UserSubcategoryEventPackage>()
                .HasKey(c => new { c.PackageId, c.UserSubcategoryEventId });

            builder.Entity<BadgeServiceApplicationRole>()
                .HasKey(c => new { c.BadgeServiceId, c.RoleId });

            builder.Entity<ApplicationUser>()
                .HasMany(c => c.EventUsers)
                .WithOne(c => c.User);

            builder.Entity<EventUser>()
                .HasOne(c => c.Creator)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BadgeServiceCheckup>()
               .HasOne(c => c.Creator)
               .WithMany()
               .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Service>()
               .HasOne(c => c.BadgeService)
               .WithMany()
               .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<EventUser>()
                .HasOne(c => c.UserSubcategoryEvent)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicationUser>()
                .HasMany(c => c.PackageServices)
                .WithOne(c => c.User);

            builder.Entity<UserPackageService>()
                .HasOne(c => c.Creator)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicationRole>()
                .HasMany(e => e.Users)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Roles)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Service>()
               .HasOne(c => c.HotelOption)
               .WithMany(c => c.Services)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSubcategory>()
            .Property(e => e.IncludeProperties)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}