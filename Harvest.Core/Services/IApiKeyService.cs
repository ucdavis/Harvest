using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Services
{
    public interface IApiKeyService
    {
        /// <summary>
        /// Generates a new API key for the specified permission, revoking any existing key
        /// </summary>
        /// <param name="permissionId">The permission ID to generate a key for</param>
        /// <returns>The generated API key (base64url encoded)</returns>
        Task<string> GenerateApiKeyAsync(int permissionId);

        /// <summary>
        /// Validates an API key against stored hashes
        /// </summary>
        /// <param name="apiKey">The API key to validate</param>
        /// <returns>The permission if valid, null otherwise</returns>
        Task<Permission> ValidateApiKeyAsync(string apiKey);

        /// <summary>
        /// Revokes an API key for the specified permission
        /// </summary>
        /// <param name="permissionId">The permission ID to revoke the key for</param>
        Task RevokeApiKeyAsync(int permissionId);
    }
}