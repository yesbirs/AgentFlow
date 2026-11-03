using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

public sealed class SessionResultConfiguration
    : IEntityTypeConfiguration<SessionResult>
{
    public void Configure(EntityTypeBuilder<SessionResult> builder)
    {
        builder.ToTable("SessionResults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne<Session>()
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SessionId);
    }
}