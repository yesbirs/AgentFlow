using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class WorkFlowDefinition
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string StepsJson { get; set; } = "[]";

        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}