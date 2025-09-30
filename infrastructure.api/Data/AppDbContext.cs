using infrastructure.api.Models;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<InfrastructureComponent> Components => Set<InfrastructureComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InfrastructureComponent>().HasData(
            new InfrastructureComponent
            {
                Id = Guid.NewGuid(),
                Name = "Core API",
                Type = "appservice",
                Environment = "dev",
                Status = "provisioned",
                CreatedUtc = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["resourceGroup"] = "rg-dev-apps"
                }
            },
            new InfrastructureComponent
            {
                Id = Guid.NewGuid(),
                Name = "Database",
                Type = "sql",
                Environment = "qa",
                Status = "provisioned",
                CreatedUtc = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["tier"] = "basic"
                }
            },
            new InfrastructureComponent
            {
                Id = Guid.NewGuid(),
                Name = "Virtual Network",
                Type = "vnet",
                Environment = "prod",
                Status = "provisioned",
                CreatedUtc = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["addressSpace"] = "10.0.0.0/16"
                }
            }
        );
    }
}
