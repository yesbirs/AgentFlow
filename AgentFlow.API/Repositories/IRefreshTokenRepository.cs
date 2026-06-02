using AgentFlow.API.Models;

namespace AgentFlow.API.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task<RefreshToken?> GetRefreshToken(Guid id, CancellationToken cancellationToken);

        Task<RefreshToken?> GetRefreshTokenByToken(string token, CancellationToken cancellationToken);

        Task<RefreshToken> UpdateRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task DeleteRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task<List<RefreshToken>> GetRefreshTokensByUserId(string userId, CancellationToken cancellationToken);

        Task<List<RefreshToken>> GetActiveRefreshTokensByUserId(string userId, CancellationToken cancellationToken);
    }
}
