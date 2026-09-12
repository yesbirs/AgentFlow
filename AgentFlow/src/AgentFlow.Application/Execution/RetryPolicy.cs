using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Execution;

/// <summary>
/// Simple retry policy used to determine whether a failed <see cref="AgentTask"/> should be retried.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>
    /// Maximum allowed attempts for a task.
    /// </summary>
    private readonly int _maxAttempts;

    /// <summary>
    /// Initializes a new instance of <see cref="RetryPolicy"/>.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of attempts before giving up.</param>
    public RetryPolicy(int maxAttempts)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts));
        }

        _maxAttempts = maxAttempts;
    }

    /// <summary>
    /// Determines whether the specified task is eligible for a retry.
    /// </summary>
    public bool ShouldRetry(AgentTask task)
    {
        return task.State == AgentFlow.Domain.Enums.TaskState.Failed
            && task.AttemptCount < _maxAttempts;
    }
}
