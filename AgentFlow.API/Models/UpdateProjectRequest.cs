namespace AgentFlow.API.Models
{
    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }
}