namespace AgentFlow.API.Models
{
    public class TaskQueryParameters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? ProjectId { get; set; }
        public string? SortBy { get; set; } = "createdAt";
        public string? SortOrder { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}