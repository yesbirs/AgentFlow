namespace AgentFlow.API.Options
{
    public class BackGroundServiceOptions
    {
        public int HeartbeatIntervalSeconds { get; set; } = 30;
        public int StuckTaskCheckIntervalMinutes { get; set; } = 5;
        public int StuckTaskBatchSize { get; set; } = 100;
        // Limits to avoid runaway configuration values
        public int MinStuckTaskCheckIntervalMinutes { get; set; } = 1;
        public int MaxStuckTaskBatchSize { get; set; } = 1000;
    }
}