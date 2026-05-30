using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class UpdateWorkflowDefinitionRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? StepsJson { get; set; }
        public bool IsActive { get; set; }
    }
}