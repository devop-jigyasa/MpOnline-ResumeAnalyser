using Microsoft.EntityFrameworkCore;

namespace MpOnline.ResumeAnalyser.API.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resume> Resumes => Set<Resume>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Ensure email uniqueness
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
