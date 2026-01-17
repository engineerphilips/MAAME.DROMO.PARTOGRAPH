using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Blazored.LocalStorage;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AuthService : IAuthService
    {
        private readonly MonitoringDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorage;
        private const string SessionKey = "monitoring_session";

        public AuthService(
            MonitoringDbContext context,
            IConfiguration configuration,
            ILocalStorageService localStorage)
        {
            _context = context;
            _configuration = configuration;
            _localStorage = localStorage;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.MonitoringUsers
                    .Include(u => u.Region)
                    .Include(u => u.District)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Your account is inactive. Please contact administrator."
                    };
                }

                if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Your account is locked. Please try again later or contact administrator."
                    };
                }

                // Verify password
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
                    }
                    await _context.SaveChangesAsync();

                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                // Reset failed attempts on successful login
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LockedUntil = null;
                user.LastLogin = DateTime.UtcNow;
                user.LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                user.LoginCount++;
                await _context.SaveChangesAsync();

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
                await _context.SaveChangesAsync();

                var userDto = new MonitoringUserDto
                {
                    ID = user.ID,
                    FullName = user.FullName,
                    Email = user.Email,
                    AccessLevel = user.AccessLevel,
                    Role = user.Role,
                    RegionID = user.RegionID,
                    RegionName = user.Region?.Name,
                    DistrictID = user.DistrictID,
                    DistrictName = user.District?.Name
                };

                var session = new UserSession
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "480"))
                };

                await _localStorage.SetItemAsync(SessionKey, session);

                return new LoginResult
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred during login: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);
                if (session?.User != null)
                {
                    var user = await _context.MonitoringUsers.FindAsync(session.User.ID);
                    if (user != null)
                    {
                        user.RefreshToken = null;
                        await _context.SaveChangesAsync();
                    }
                }
                await _localStorage.RemoveItemAsync(SessionKey);
            }
            catch
            {
                // Ignore errors during logout
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);
                return session != null && session.ExpiresAt > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserSession?> GetCurrentSessionAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<UserSession>(SessionKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);
                if (session == null || string.IsNullOrEmpty(session.RefreshToken))
                    return false;

                var user = await _context.MonitoringUsers
                    .Include(u => u.Region)
                    .Include(u => u.District)
                    .FirstOrDefaultAsync(u =>
                        u.RefreshToken == session.RefreshToken &&
                        u.RefreshTokenExpiryTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                if (user == null)
                    return false;

                // Generate new tokens
                var newToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
                await _context.SaveChangesAsync();

                session.Token = newToken;
                session.RefreshToken = newRefreshToken;
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "480"));

                await _localStorage.SetItemAsync(SessionKey, session);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(MonitoringUser user)
        {
            var secret = _configuration["JwtSettings:Secret"] ?? "5f021d67-3ceb-44cd-8f55-5b10ca9039e1-monitoring";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "PartographMonitoringDashboard";
            var audience = _configuration["JwtSettings:Audience"] ?? "MonitoringUsers";
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "480");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AccessLevel", user.AccessLevel)
            };

            if (user.RegionID.HasValue)
                claims.Add(new Claim("RegionID", user.RegionID.Value.ToString()));

            if (user.DistrictID.HasValue)
                claims.Add(new Claim("DistrictID", user.DistrictID.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private bool VerifyPassword(string password, string? passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;

            // Use SHA256 for password verification (should match what's used in the SERVICE project)
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToBase64String(hashBytes);

            return computedHash == passwordHash;
        }
    }
}
