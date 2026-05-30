namespace AgentFlow.API.Models
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }
}