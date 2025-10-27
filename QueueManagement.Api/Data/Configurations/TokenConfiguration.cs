using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueueManagement.Api.Models;

namespace QueueManagement.Api.Data.Configurations
{
    public class TokenConfiguration : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.TokenNumber).IsRequired().HasMaxLength(20);
            builder.Property(t => t.EncryptedCnic).IsRequired().HasMaxLength(200);
            builder.Property(t => t.LastFourCnic).IsRequired().HasMaxLength(4);
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(t => t.Counter)
                   .WithMany(c => c.Tokens)
                   .HasForeignKey(t => t.CounterId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Queue)
                   .WithMany(q => q.Tokens)
                   .HasForeignKey(t => t.QueueId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}