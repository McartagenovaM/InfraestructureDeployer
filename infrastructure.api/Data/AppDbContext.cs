using infrastructure.api.Models;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<InfrastructureComponent> Components => Set<InfrastructureComponent>();

        // No HasData with Guid.NewGuid or DateTime.UtcNow here,
        // seed will be done in Program.cs after building the app.
    }
}
