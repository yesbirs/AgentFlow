namespace AgentFlow.Application.Planning;

/// <summary>
/// Validates a <see cref="PlanningResult"/> for completeness and structural correctness.
/// </summary>
public sealed class PlanValidator
{
    /// <summary>
    /// Validates the provided plan, throwing <see cref="InvalidOperationException"/> when invalid.
    /// </summary>
    public void Validate(PlanningResult plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        ValidateTasks(plan);
        ValidateParents(plan);
        ValidateDependencies(plan);
        ValidateNoCycles(plan);
    }

    /// <summary>
    /// Ensures every task in the plan has a non-empty key, unique keys, a title and a description.
    /// Throws <see cref="InvalidOperationException"/> when a validation rule fails.
    /// </summary>
    private static void ValidateTasks(PlanningResult plan)
    {
        var keys = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var task in plan.Tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Key))
            {
                throw new InvalidOperationException(
                    "Task key cannot be empty.");
            }

            if (!keys.Add(task.Key))
            {
                throw new InvalidOperationException(
                    $"Duplicate task key '{task.Key}'.");
            }

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                throw new InvalidOperationException(
                    $"Task '{task.Key}' must have a title.");
            }

            if (string.IsNullOrWhiteSpace(task.Description))
            {
                throw new InvalidOperationException(
                    $"Task '{task.Key}' must have a description.");
            }
        }
    }

    /// <summary>
    /// Ensures parent keys (when provided) refer to existing tasks and that a task is not its own parent.
    /// </summary>
    private static void ValidateParents(PlanningResult plan)
    {
        var taskKeys = plan.Tasks
            .Select(t => t.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var task in plan.Tasks)
        {
            if (task.ParentKey is null)
            {
                continue;
            }

            if (!taskKeys.Contains(task.ParentKey))
            {
                throw new InvalidOperationException(
                    $"Parent task '{task.ParentKey}' " +
                    $"does not exist for task '{task.Key}'.");
            }

            if (string.Equals(
                    task.Key,
                    task.ParentKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Task '{task.Key}' cannot be its own parent.");
            }
        }
    }

    /// <summary>
    /// Validates that each declared dependency references existing tasks and that no task depends on itself.
    /// </summary>
    private static void ValidateDependencies(
        PlanningResult plan)
    {
        var taskKeys = plan.Tasks
            .Select(t => t.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var dependency in plan.Dependencies)
        {
            if (!taskKeys.Contains(dependency.TaskKey))
            {
                throw new InvalidOperationException(
                    $"Task '{dependency.TaskKey}' " +
                    "does not exist.");
            }

            if (!taskKeys.Contains(
                    dependency.DependsOnTaskKey))
            {
                throw new InvalidOperationException(
                    $"Dependency task " +
                    $"'{dependency.DependsOnTaskKey}' " +
                    "does not exist.");
            }

            if (string.Equals(
                    dependency.TaskKey,
                    dependency.DependsOnTaskKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Task '{dependency.TaskKey}' " +
                    "cannot depend on itself.");
            }
        }
    }

    /// <summary>
    /// Builds a dependency graph and ensures there are no cycles using depth-first search.
    /// </summary>
    private static void ValidateNoCycles(PlanningResult plan)
    {
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var task in plan.Tasks)
        {
            graph[task.Key] = new List<string>();
        }

        foreach (var dependency in plan.Dependencies)
        {
            graph[dependency.TaskKey]
                .Add(dependency.DependsOnTaskKey);
        }

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var task in graph.Keys)
        {
            if (HasCycle(task, graph, visiting, visited))
            {
                throw new InvalidOperationException(
                    "Task dependency graph contains a cycle.");
            }
        }
    }

    /// <summary>
    /// Performs a DFS from the specified task key to detect cycles. Returns true when a cycle is found.
    /// - Uses <c>visiting</c> to track the current recursion stack and <c>visited</c> for fully explored nodes.
    /// </summary>
    private static bool HasCycle(string taskKey, Dictionary<string, List<string>> graph, HashSet<string> visiting, HashSet<string> visited)
    {
        if (visiting.Contains(taskKey))
        {
            return true;
        }

        if (visited.Contains(taskKey))
        {
            return false;
        }

        visiting.Add(taskKey);

        foreach (var dependency in graph[taskKey])
        {
            if (HasCycle(
                    dependency,
                    graph,
                    visiting,
                    visited))
            {
                return true;
            }
        }

        visiting.Remove(taskKey);
        visited.Add(taskKey);

        return false;
    }
}
