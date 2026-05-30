namespace AgentFlow.API.Models
{
    public class UpdateTaskRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Priority { get; set; } = "medium";
        public DateTime? ScheduledAt { get; set; }

        // allow changing status during update
        public string Status { get; set; } = "pending";
    }
}