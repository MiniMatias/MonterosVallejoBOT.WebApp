using Microsoft.EntityFrameworkCore;
using MonterosVallejoBOT.WebApp.Models; 

namespace MonterosVallejoBOT.WebApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatResponse> ChatResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    
        modelBuilder.Entity<ChatResponse>().ToTable("HistorialChat");
    }
}