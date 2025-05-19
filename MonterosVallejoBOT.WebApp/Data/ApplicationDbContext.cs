using Microsoft.EntityFrameworkCore;
using MonterosVallejoBOT.WebApp.Models; 

namespace MonterosVallejoBOT.WebApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet para tu entidad. EF Core creará una tabla basada en esto.
    public DbSet<ChatResponse> ChatResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí puedes añadir configuraciones adicionales para tus entidades usando Fluent API
        // Por ejemplo, especificar el nombre de la tabla explícitamente:
        modelBuilder.Entity<ChatResponse>().ToTable("HistorialChat");

        // Ejemplo de configuración adicional si tuvieras más entidades o relaciones:
        // modelBuilder.Entity<ChatResponse>()
        //     .Property(p => p.Respuesta)
        //     .HasMaxLength(2000); // Si quisieras limitar la longitud
    }
}