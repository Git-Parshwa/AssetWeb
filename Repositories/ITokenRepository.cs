using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;

namespace AssetWeb.Repositories
{
    public interface ITokenRepository
    {
        string GetJwtToken(User user, List<string> roles);
        (string, RefreshToken) GetRefreshToken(User user);
        string HashedToken(string refreshToken);
        Task<User?> GetCurrentUserAsync(string id);
        Task<User?> GetUserFromTokenAsync(string token);
        Task<bool> CheckUserExists(User user);
        Task<Guid?> RevokeExistingRefreshToken(string id);
    }
}