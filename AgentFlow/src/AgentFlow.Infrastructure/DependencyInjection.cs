using AgentFlow.API.BackgroundWorkers;
using AgentFlow.Application.Abstractions.LLM;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Application.Abstractions.Time;
using AgentFlow.Application.Execution;
using AgentFlow.Infrastructure.Execution;
using AgentFlow.Infrastructure.LLM;
using AgentFlow.Infrastructure.Persistence;
using AgentFlow.Infrastructure.Persistence.Repositories;
using AgentFlow.Infrastructure.Queue;
using AgentFlow.Infrastructure.Time;
using AgentFlow.Infrastructure.Workers;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services (persistence, queueing, workers) into the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));
        services.AddSingleton(options => new ServiceBusClient(configuration.GetConnectionString("ServiceBus")));
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAgentTaskRepository, AgentTaskRepository>();
        services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
        services.AddSingleton<IWorkQueue, AzureServiceBusWorkQueue>();
        services.AddScoped<ITaskExecutor, LlmTaskExecutor>();
        services.AddScoped<TaskWorker>();
        services.AddHostedService<AgentTaskBackgroundWorker>();
        services.AddHostedService<SchedulerBackgroundWorker>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddSingleton<IMessagePublisher, AzureServiceBusMessagePublisher>();
        services.AddHostedService<OutboxBackgroundWorker>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddHostedService<StaleTaskRecoveryBackgroundWorker>();
        services.AddScoped<ITaskResultRepository, TaskResultRepository>();

        return services;
    }
}