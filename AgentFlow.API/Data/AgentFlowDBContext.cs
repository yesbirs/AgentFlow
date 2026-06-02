using AgentFlow.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.API.Data
{
    public class AgentFlowDBContext : IdentityDbContext
    {
        public AgentFlowDBContext(DbContextOptions<AgentFlowDBContext> options) : base(options)
        {
        }

        public DbSet<AgentTask> Tasks => Set<AgentTask>();

        // NEW: Projects table
        public DbSet<Project> Projects => Set<Project>();

        public DbSet<WorkFlowDefinition> WorkflowDefinitions => Set<WorkFlowDefinition>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AgentTask>().ToTable("Tasks");
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<WorkFlowDefinition>().ToTable("WorkflowDefinitions");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<FileAttachment>().ToTable("FileAttachments");

            modelBuilder.Entity<AgentTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}