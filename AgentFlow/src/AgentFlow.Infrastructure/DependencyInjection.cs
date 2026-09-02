using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Application.Execution;
using AgentFlow.Infrastructure.Execution;
using AgentFlow.Infrastructure.Persistence;
using AgentFlow.Infrastructure.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAgentTaskRepository, AgentTaskRepository>();
        services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
        services.AddSingleton<IWorkQueue, InMemoryWorkQueue>();
        services.AddScoped<ITaskExecutor, FakeTaskExecutor>();
        services.AddScoped<TaskWorker>();

        return services;
    }
}