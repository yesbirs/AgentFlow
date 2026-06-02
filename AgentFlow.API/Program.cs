using AgentFlow.Api.Options;
using AgentFlow.Api.Services;
using AgentFlow.API.BackGroundServices;
using AgentFlow.API.Data;
using AgentFlow.API.MiddleWares;
using AgentFlow.API.Options;
using AgentFlow.API.Repositories;
using AgentFlow.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rate limiting - name must match controller attributes
builder.Services.AddRateLimiter(options =>
{
    options.AddTokenBucketLimiter("fixed", limiterOptions =>  // CHANGED: "fixed" not "GlobalLimiter"
    {
        limiterOptions.TokenLimit = 100;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 50;
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
        limiterOptions.TokensPerPeriod = 10;
        limiterOptions.AutoReplenishment = true;
    });
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AgentFlowDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
    if (jwtOptions == null)
    {
        throw new InvalidOperationException("Jwt configuration section is missing.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
    };
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc();

builder.Services.AddScoped<ITaskService, PostgreSqlTaskService>();
builder.Services.AddScoped<IProjectService, PostgreSqlProjectService>();
builder.Services.AddScoped<IWorkflowDefinitionService, PostgreSqlWorkflowDefinitionService>();
builder.Services.AddScoped<IRefreshTokenService, PostgreSqlRefreshTokenService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IFileService, LocalFileService>();

builder.Services.AddHostedService<HeartbeatBackgroundService>();
builder.Services.AddHostedService<StuckTaskCheckerService>();

builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
builder.Services.Configure<BackGroundServiceOptions>(builder.Configuration.GetSection("BackgroundServices"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = false);
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

builder.Services.AddDbContext<AgentFlowDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseCors("AllowReactDev");
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();