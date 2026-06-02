using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AgentFlow.API.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        bool IsAdmin { get; }
        ClaimsPrincipal? User { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public bool IsAdmin => User?.IsInRole("Admin") ?? false;
    }
}
