namespace AgentFlow.API.Models
{
    public class WorkflowDefinitionQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}
