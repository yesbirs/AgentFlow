using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class TokenResponse
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
