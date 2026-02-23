using ConfigurationManager.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationManager.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<ConfigurationVersion> ConfigurationVersions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Configuration>()
                .HasOne(c => c.User)
                .WithMany(u => u.Configurations)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Configuration>()
                .HasIndex(c => new { c.Name, c.UserId })
                .IsUnique();

            modelBuilder.Entity<ConfigurationVersion>()
                .HasOne(v => v.Configuration)
                .WithMany(c => c.Versions)
                .HasForeignKey(v => v.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConfigurationVersion>()
                .HasIndex(v => new { v.ConfigurationId, v.VersionNumber })
                .IsUnique();

            modelBuilder.Entity<Configuration>()
                .HasIndex(c => new { c.UserId, c.IsActive })
                .IsUnique()
                .HasFilter("IsActive = 1");

            base.OnModelCreating(modelBuilder);
        }
    }
}