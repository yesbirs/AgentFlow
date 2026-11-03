using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core database context for the application. Also implements <see cref="IUnitOfWork"/> for transaction management.
    /// </summary>
    public sealed class AppDbContext : DbContext, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Sessions table.
        /// </summary>
        public DbSet<Session> Sessions => Set<Session>();

        /// <summary>
        /// Tasks table.
        /// </summary>
        public DbSet<AgentTask> Tasks => Set<AgentTask>();

        /// <summary>
        /// Task dependencies table.
        /// </summary>
        public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();

        /// <summary>
        /// Outbox messages table.
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        /// <summary>
        /// Task results table.
        /// </summary>
        public DbSet<TaskResult> TaskResults => Set<TaskResult>();

        public DbSet<SessionResult> SessionResults => Set<SessionResult>();

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await Database.BeginTransactionAsync(
                cancellationToken);
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException(
                    "No active database transaction.");
            }

            await Database.CurrentTransaction.CommitAsync(
                cancellationToken);
        }

        /// <summary>
        /// Rolls back the current database transaction if present.
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Database.CurrentTransaction is null)
            {
                return;
            }

            await Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }
}