using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using infrastructure.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        var metadataConverter = new ValueConverter<Dictionary<string, string>?, string?>(
            value => value is null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => string.IsNullOrWhiteSpace(value)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(value!) ?? new Dictionary<string, string>());

        var metadataComparer = new ValueComparer<Dictionary<string, string>?>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left is not null && right is not null && left.Count == right.Count && !left.Except(right).Any()),
            value =>
            {
                if (value is null)
                {
                    return 0;
                }

                var hash = new HashCode();
                foreach (var (key, val) in value.OrderBy(pair => pair.Key))
                {
                    hash.Add(key);
                    hash.Add(val);
                }

                return hash.ToHashCode();
            },
            value => value is null
                ? null
                : value.ToDictionary(pair => pair.Key, pair => pair.Value));

        var component = modelBuilder.Entity<InfrastructureComponent>();

        var metadataProperty = component.Property(entity => entity.Metadata);

        metadataProperty.HasConversion(metadataConverter);
        metadataProperty.Metadata.SetValueComparer(metadataComparer);

        component.HasData(
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
