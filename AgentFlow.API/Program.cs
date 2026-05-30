using AgentFlow.Api.Services;
using AgentFlow.API.BackGroundServices;
using AgentFlow.API.Data;
using AgentFlow.API.MiddleWares;
using AgentFlow.API.Options;
using AgentFlow.API.Repositories;
using AgentFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITaskService, PostgreSqlTaskService>();
builder.Services.AddScoped<IProjectService, PostgreSqlProjectService>();
builder.Services.AddScoped<IWorkflowDefinitionService, PostgreSqlWorkflowDefinitionService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddHostedService<HeartbeatBackgroundService>();
builder.Services.AddHostedService<StuckTaskCheckerService>();
// Register options so they can be injected via IOptions<T>
builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
builder.Services.Configure<BackGroundServiceOptions>(builder.Configuration.GetSection("BackgroundServices"));

builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
builder.Services.Configure<BackGroundServiceOptions>(builder.Configuration.GetSection("BackgroundServices"));

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = false);
builder.Services.AddDbContext<AgentFlowDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseGlobalExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();