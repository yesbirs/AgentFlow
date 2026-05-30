namespace AgentFlow.API.Models
{
    public class ProjectQueryParameters
    {
        public string? Status { get; set; }
        public string? SortBy { get; set; } = "createdAt";
        public string? SortOrder { get; set; } = "desc";
        public string? OwnerEmail { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}