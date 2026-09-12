using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Configures EF Core mappings for the OutboxMessage entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .IsRequired();

        builder.Property(message => message.Type)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(message => message.PublishedAt)
            .IsRequired(false);

        builder.HasIndex(message => message.PublishedAt);
    }
}