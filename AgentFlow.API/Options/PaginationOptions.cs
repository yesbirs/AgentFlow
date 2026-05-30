namespace AgentFlow.API.Options
{
    public class PaginationOptions
    {
        public int DefaultPageSize { get; set; } = 10;
        public int MaxPageSize { get; set; } = 100;
    }
}