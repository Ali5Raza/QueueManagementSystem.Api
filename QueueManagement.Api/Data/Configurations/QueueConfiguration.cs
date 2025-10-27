using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueManagement.Api.Models;

namespace QueueManagement.Api.Data.Configurations
{
    public class QueueConfiguration : IEntityTypeConfiguration<Queue>
    {
        public void Configure(EntityTypeBuilder<Queue> builder)
        {
            builder.HasKey(q => q.Id);
            builder.Property(q => q.Name).IsRequired().HasMaxLength(100);
            builder.Property(q => q.Description).HasMaxLength(500);
            builder.Property(q => q.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(q => q.Tokens)
                   .WithOne(t => t.Queue)
                   .HasForeignKey(t => t.QueueId);
        }
    }
}