using System.ComponentModel.DataAnnotations;

namespace AgentFlow.API.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        public bool IsActive => DateTime.UtcNow < ExpiresAt && !IsRevoked;
    }
}