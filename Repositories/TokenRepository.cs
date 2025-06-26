using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AssetWeb.Data;
using AssetWeb.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AssetWeb.Repositories
{

    public class TokenRepository : ITokenRepository
    {
        private readonly IConfiguration configuration;
        private readonly AssetWebAuthDbContext dbContext;
        private readonly IProfileRepository profileRepository;

        public TokenRepository(IConfiguration configuration, AssetWebAuthDbContext dbContext,IProfileRepository profileRepository)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.profileRepository = profileRepository;
        }

        public string GetJwtToken(User user, List<string> roles)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtAudience,
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string, RefreshToken) GetRefreshToken(User user)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = HashedToken(rawToken);

            var refreshToken = new RefreshToken
            {
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            return (rawToken, refreshToken);
        }

        public string HashedToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<User?> GetCurrentUserAsync(string id)
        {
            var user = await dbContext.Users.Include("RefreshTokens").FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (refreshToken == null || refreshToken.IsRevoked == true)
            {
                return null;
            }
            var userId = refreshToken.UserId;
            var user = await profileRepository.GetUserByIdAsync(userId);
            return user;
        }

        public async Task<bool> CheckUserExists(User user)
        {
            var findUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (findUser == null)
            {
                return false;
            }
            return true;
        }

        public async Task<Guid?> RevokeExistingRefreshToken(string id)
        {
            var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == id && x.IsRevoked == false);
            if (refreshToken == null)
            {
                return null;
            }
            refreshToken.IsRevoked = true;
            return refreshToken.Id;
        }
    }
}