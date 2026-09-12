using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentFlow.Infrastructure.Persistence.Configurations;

public sealed class TaskDependencyConfiguration
    : IEntityTypeConfiguration<TaskDependency>
{
    /// <summary>
    /// Configures EF Core mappings for the TaskDependency entity.
    /// </summary>
    public void Configure(
        EntityTypeBuilder<TaskDependency> builder)
    {
        builder.ToTable("TaskDependencies");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.TaskId)
            .IsRequired();

        builder.Property(d => d.DependsOnTaskId)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.HasOne<AgentTask>()
            .WithMany()
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgentTask>()
            .WithMany()
            .HasForeignKey(d => d.DependsOnTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new
        {
            d.TaskId,
            d.DependsOnTaskId
        })
        .IsUnique();
    }
}