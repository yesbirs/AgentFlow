namespace AgentFlow.API.Models
{
    public class CreateTaskRequest
    {
        // Required: ASP.NET will reject the request if this is missing or empty
        public string Name { get; set; }

        // Optional: can be null
        public string? Description { get; set; }

        // Enum-like string with a default
        public string Priority { get; set; } = "medium";

        // Nullable DateTime for scheduling
        public DateTime? ScheduledAt { get; set; }
    }
}