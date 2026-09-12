namespace AgentFlow.Application.Abstractions.Persistence;

/// <summary>
/// Represents a unit-of-work for batching repository operations and managing transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists tracked changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the currently active transaction.
    /// </summary>
    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the currently active transaction.
    /// </summary>
    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);
}
