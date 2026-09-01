namespace AgentFlow.Application.Planning;

public sealed class PlanValidator
{
    public void Validate(PlanningResult plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        ValidateTasks(plan);
        ValidateParents(plan);
        ValidateDependencies(plan);
        ValidateNoCycles(plan);
    }

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