using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; } = null;
        public string Status { get; set; } = "active";
        public string OwnerEmail { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property for related tasks
        public List<AgentTask> Tasks { get; set; } = new List<AgentTask>();
    }
}