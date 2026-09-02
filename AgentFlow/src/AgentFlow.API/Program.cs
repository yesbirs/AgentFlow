using AgentFlow.API.Endpoints;
using AgentFlow.API.ExceptionHandling;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Planning;
using AgentFlow.Application.Scheduling;
using AgentFlow.Application.Sessions.CreateSession;
using AgentFlow.Application.Sessions.CreateTask;
using AgentFlow.Application.Sessions.GetSessionById;
using AgentFlow.Application.Sessions.GetSessions;
using AgentFlow.Application.Sessions.PlanSession;
using AgentFlow.Infrastructure;
using AgentFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//add all dependecies from Infrastructure project
builder.Services.AddInfrastructure(builder.Configuration);

// the below line is not necessary as it will be added in the AddInfrastructure method in the Infrastructure project

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddScoped<CreateSessionHandler>();
builder.Services.AddScoped<GetSessionByIdHandler>();
builder.Services.AddScoped<GetSessionsHandler>();
builder.Services.AddScoped<CreateTaskHandler>();
builder.Services.AddScoped<IPlanner, FakePlanner>();
builder.Services.AddScoped<PlanValidator>();
builder.Services.AddScoped<PlanApplicationService>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<PlanSessionHandler>();
//Exception handling middleware can be added here
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IScheduler, Scheduler>();
builder.Services.AddScoped<TaskDispatcher>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen();

var app = builder.Build();
//app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
//app.MapPost("api/sessions", async (CreateSessionCommand command, CreateSessionHandler handler, CancellationToken cancellationToken) =>
//{
//    var session = await handler.Handle(command, cancellationToken);
//    return Results.Created($"api/sessions/{session.Id}", session);
//});

app.MapSessionEndpoints();
app.MapControllers();

app.Run();