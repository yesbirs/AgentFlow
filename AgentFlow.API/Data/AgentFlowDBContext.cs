using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.API.Data
{
    public class AgentFlowDBContext : DbContext
    {
        public AgentFlowDBContext(DbContextOptions<AgentFlowDBContext> options) : base(options)
        {
        }

        public DbSet<AgentTask> Tasks => Set<AgentTask>();

        // NEW: Projects table
        public DbSet<Project> Projects => Set<Project>();

        public DbSet<WorkFlowDefinition> WorkflowDefinitions => Set<WorkFlowDefinition>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AgentTask>().ToTable("Tasks");
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<WorkFlowDefinition>().ToTable("WorkflowDefinitions");

            modelBuilder.Entity<AgentTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        // Define DbSets for your entities here, e.g.:
        // public DbSet<Project> Projects { get; set; }
    }
}