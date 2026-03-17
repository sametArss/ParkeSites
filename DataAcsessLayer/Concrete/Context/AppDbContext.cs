using EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;

namespace DataAcsessLayer.Concrete.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔗 Project - ProjectImage ilişkisi
            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectImages)
                .WithOne(i => i.Project)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔐 Admin unique username
            modelBuilder.Entity<User>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
    }
}