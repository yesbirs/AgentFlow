using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

public sealed class TaskResultConfiguration
    : IEntityTypeConfiguration<TaskResult>
{
    public void Configure(
        EntityTypeBuilder<TaskResult> builder)
    {
        builder.ToTable("TaskResults");

        builder.HasKey(result => result.Id);

        builder.Property(result => result.Id)
            .IsRequired();

        builder.Property(result => result.TaskId)
            .IsRequired();

        builder.Property(result => result.Content)
            .IsRequired();

        builder.Property(result => result.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(result => result.TaskId);

        builder.HasOne<AgentTask>()
            .WithMany()
            .HasForeignKey(result => result.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}