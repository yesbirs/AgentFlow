using AgentFlow.API.Models;

namespace AgentFlow.API.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshToken(string userId, CancellationToken cancellationToken);

        Task<RefreshToken?> GetValidRefreshToken(string token, CancellationToken cancellationToken);

        Task RevokeRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task RevokeAllUserTokens(string userId, CancellationToken cancellationToken);

        Task<bool> IsTokenValid(string token, CancellationToken cancellationToken);
    }
}
