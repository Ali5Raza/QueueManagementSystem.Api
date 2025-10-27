using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QueueManagement.Api.Models;

namespace QueueManagement.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Token> Tokens { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<Queue> Queues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Seed default counters
            modelBuilder.Entity<Counter>().HasData(
                new Counter { Id = 1, Name = "Counter 1", Description = "General Services", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Counter { Id = 2, Name = "Counter 2", Description = "Special Services", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed default queue
            modelBuilder.Entity<Queue>().HasData(
                new Queue { Id = 1, Name = "General Queue", Description = "Main queue for all services", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Ensure default counters exist
            if (!Counters.Any())
            {
                var defaultCounters = new[]
                {
                    new Counter { Name = "Counter 1", Description = "General Services", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Counter { Name = "Counter 2", Description = "Special Services", IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                Counters.AddRange(defaultCounters);
                await base.SaveChangesAsync(cancellationToken);
            }

            // Ensure default queue exists
            if (!Queues.Any())
            {
                var defaultQueue = new Queue { Name = "General Queue", Description = "Main queue for all services", IsActive = true, CreatedAt = DateTime.UtcNow };
                Queues.Add(defaultQueue);
                await base.SaveChangesAsync(cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}