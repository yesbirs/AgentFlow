using AgentFlow.API.Models;
using AgentFlow.API.Options;
using AgentFlow.API.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            ILogger<AuthController> logger,
            Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IRefreshTokenService refreshTokenService)
        {
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Registering user: {Email}", request.Email);
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User login attempt: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            var token = await GenerateJwtToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, cancellationToken);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(new TokenResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtOptions.ExpiresInMinutes * 60
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token refresh attempt");

            var refreshToken = await _refreshTokenService.GetValidRefreshToken(request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token" });
            }

            // Revoke the old refresh token
            await _refreshTokenService.RevokeRefreshToken(refreshToken, cancellationToken);

            // Generate new tokens
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);

            if (user == null)
            {
                return Unauthorized(new { Message = "User not found" });
            }

            var newJwtToken = await GenerateJwtToken(user);
            var newRefreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, cancellationToken);

            return Ok(new TokenResponse
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = _jwtOptions.ExpiresInMinutes * 60
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User logout attempt");

            var refreshToken = await _refreshTokenService.GetValidRefreshToken(request.RefreshToken, cancellationToken);

            if (refreshToken != null)
            {
                await _refreshTokenService.RevokeRefreshToken(refreshToken, cancellationToken);
            }

            return Ok(new { Message = "Logged out successfully" });
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class RegisterRequest
        {
            [Required]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            [Required]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }
    }
}