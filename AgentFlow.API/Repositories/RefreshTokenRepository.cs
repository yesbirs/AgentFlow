using AgentFlow.API.Data;
using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AgentFlowDBContext _dbContext;

        public RefreshTokenRepository(AgentFlowDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken> CreateRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshToken(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenByToken(string token, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
        }

        public async Task<RefreshToken> UpdateRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        public async Task DeleteRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            _dbContext.RefreshTokens.Remove(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetRefreshTokensByUserId(string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveRefreshTokensByUserId(string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}
