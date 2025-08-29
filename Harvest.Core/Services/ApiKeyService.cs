using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Harvest.Core.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly AppDbContext _dbContext;
        private readonly byte[] _lookupHmacKey;
        const int HashIterations = 100000;
        const int OutputLength = 32; // 256 bits


        public ApiKeyService(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            
            // Get the server HMAC key from configuration
            var serverSecret = configuration["ApiKeys:ServerSecret"];
            if (string.IsNullOrEmpty(serverSecret))
            {
                throw new InvalidOperationException("ApiKeys:ServerSecret configuration is required");
            }
            
            _lookupHmacKey = Encoding.UTF8.GetBytes(serverSecret);
        }


        /// <summary>
        /// The returned APIKey is not saved. It will be stored or saved in the mobile app.
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns>A string API Key that is used in ValidateApiKeyAsync to lookup and validate the user</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> GenerateApiKeyAsync(int permissionId)
        {
            var permission = await _dbContext.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                throw new ArgumentException("Permission not found", nameof(permissionId)); 
            }

            // Generate a random 32-byte secret
            var secret = new byte[32];
            RandomNumberGenerator.Fill(secret);
            
            // Base64URL encode the secret for the API key
            var apiKey = Base64UrlEncoder.Encode(secret);

            // Generate a random 16-byte salt
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            // Create hash using PBKDF2
            var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, HashIterations, HashAlgorithmName.SHA256, OutputLength);

            // Create lookup HMAC
            byte[] lookupHmac;
            using (var hmac = new HMACSHA256(_lookupHmacKey))
            {
                lookupHmac = hmac.ComputeHash(secret);
            }

            // Store the salt, hash, and lookup in the permission
            permission.Salt = salt;
            permission.Hash = hash;
            permission.Lookup = lookupHmac;

            await _dbContext.SaveChangesAsync();

            return apiKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey">The value that was returned by GenerateApiKeyAsync</param>
        /// <returns>The Permission if found and validated</returns>
        public async Task<Permission> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return null;
            }

            try
            {
                // Decode the API key
                var secret = Base64UrlEncoder.DecodeBytes(apiKey);

                // Create lookup HMAC for this key
                byte[] lookupHmac;
                using (var hmac = new HMACSHA256(_lookupHmacKey))
                {
                    lookupHmac = hmac.ComputeHash(secret);
                }


                var permission = await _dbContext.Permissions
                    .Include(p => p.User)
                    .Include(p => p.Role)
                    .Include(p => p.Team)
                    .Where(p => p.Lookup != null && p.Lookup.SequenceEqual(lookupHmac))
                    .SingleOrDefaultAsync();

                if (permission == null)
                {
                    return null;
                }

                if (permission?.Salt == null || permission.Hash == null)
                {
                    return null;
                }

                // Verify the hash
                var computedHash = Rfc2898DeriveBytes.Pbkdf2(secret, permission.Salt, HashIterations, HashAlgorithmName.SHA256, OutputLength);
                
                if (computedHash.SequenceEqual(permission.Hash))
                {
                    return permission;
                }
            }
            catch
            {
                // Invalid key format or other error
                return null;
            }

            return null;
        }

        public async Task RevokeApiKeyAsync(int permissionId)
        {
            var permission = await _dbContext.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                throw new ArgumentException("Permission not found", nameof(permissionId));
            }

            // Clear the API key fields
            permission.Hash = null;
            permission.Salt = null;
            permission.Lookup = null;

            await _dbContext.SaveChangesAsync();
        }
    }
}