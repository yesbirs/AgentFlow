using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentFlow.API.Models
{
    public class AgentTask
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = "medium";
        public DateTime? ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "pending";
        public Guid? ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }
}