using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="AgentTask"/> entities using the application's <see cref="AppDbContext"/>.
    /// Provides methods to add tasks, retrieve a task by id, list pending tasks for a session, and attempt an atomic claim of a task.
    /// </summary>
    public class AgentTaskRepository : IAgentTaskRepository
    {
        /// <summary>
        /// EF Core database context used to access the Tasks DbSet.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="AgentTaskRepository"/> with the provided database context.
        /// </summary>
        /// <param name="context">The application's <see cref="AppDbContext"/>.</param>
        public AgentTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new <see cref="AgentTask"/> entity to the context. This method only adds the entity to the change tracker
        /// and does not call <c>SaveChanges</c>.
        /// </summary>
        /// <param name="task">The task entity to add.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public async Task AddAsync(AgentTask task, CancellationToken cancellationToken = default)
        {
            await _context.Tasks.AddAsync(task, cancellationToken);
        }

        /// <summary>
        /// Retrieves a task by its identifier.
        /// </summary>
        /// <param name="taskId">The unique identifier of the task.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The <see cref="AgentTask"/> if found; otherwise <c>null</c>.</returns>
        public async Task<AgentTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
        {
            return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        }

        /// <summary>
        /// Gets all tasks in <see cref="AgentFlow.Domain.Enums.TaskState.Pending"/> state for the specified session.
        /// </summary>
        /// <param name="sessionId">The session identifier used to filter tasks.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A read-only list of pending <see cref="AgentTask"/> entities for the session.</returns>
        public async Task<IReadOnlyList<AgentTask>> GetPendingTasksAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.Where(task => task.SessionId == sessionId && task.State == TaskState.Pending).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Attempts to atomically claim a pending task by updating its state to <see cref="TaskState.Queued"/> and
        /// setting the <see cref="AgentTask.UpdatedAt"/> timestamp. Uses EF Core's ExecuteUpdateAsync to perform
        /// the update in the database and returns whether the claim affected exactly one row.
        /// </summary>
        /// <param name="taskId">The identifier of the task to claim.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns><c>true</c> if the task was claimed (one row affected); otherwise <c>false</c>.</returns>
        public async Task<bool> TryClaimAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.Tasks
                .Where(task =>
                    task.Id == taskId &&
                    task.State == TaskState.Pending)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            task => task.State,
                            TaskState.Queued)
                        .SetProperty(
                            task => task.UpdatedAt,
                            DateTime.UtcNow),
                    cancellationToken);

            return affectedRows == 1;
        }

        public async Task<IReadOnlyList<AgentTask>> GetStaleRunningTasksAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .Where(task =>
                    task.State == TaskState.Running &&
                    task.UpdatedAt < cutoff)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets tasks associated with a specific session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IReadOnlyList<AgentTask>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(task => task.SessionId == sessionId)
                .OrderBy(task => task.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}