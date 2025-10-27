using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueManagement.Api.Models;

namespace QueueManagement.Api.Data.Configurations
{
    public class CounterConfiguration : IEntityTypeConfiguration<Counter>
    {
        public void Configure(EntityTypeBuilder<Counter> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(c => c.Tokens)
                   .WithOne(t => t.Counter)
                   .HasForeignKey(t => t.CounterId);
        }
    }
}