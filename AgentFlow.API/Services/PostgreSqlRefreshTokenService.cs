using AgentFlow.API.Models;
using AgentFlow.API.Options;
using AgentFlow.API.Repositories;
using System.Security.Cryptography;

namespace AgentFlow.API.Services
{
    public class PostgreSqlRefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtOptions _jwtOptions;

        public PostgreSqlRefreshTokenService(
            IRefreshTokenRepository refreshTokenRepository,
            Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<RefreshToken> GenerateRefreshToken(string userId, CancellationToken cancellationToken)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiresInDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            return await _refreshTokenRepository.CreateRefreshToken(refreshToken, cancellationToken);
        }

        public async Task<RefreshToken?> GetValidRefreshToken(string token, CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetRefreshTokenByToken(token, cancellationToken);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null;
            }

            return refreshToken;
        }

        public async Task RevokeRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            refreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateRefreshToken(refreshToken, cancellationToken);
        }

        public async Task RevokeAllUserTokens(string userId, CancellationToken cancellationToken)
        {
            var userTokens = await _refreshTokenRepository.GetActiveRefreshTokensByUserId(userId, cancellationToken);

            foreach (var token in userTokens)
            {
                await RevokeRefreshToken(token, cancellationToken);
            }
        }

        public async Task<bool> IsTokenValid(string token, CancellationToken cancellationToken)
        {
            var refreshToken = await GetValidRefreshToken(token, cancellationToken);
            return refreshToken != null;
        }
    }
}
