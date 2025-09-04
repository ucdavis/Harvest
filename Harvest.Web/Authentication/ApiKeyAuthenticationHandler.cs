using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly AppDbContext _dbContext;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyService apiKeyService,
            AppDbContext dbContext)
            : base(options, logger, encoder, clock)
        {
            _apiKeyService = apiKeyService;
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation("ApiKeyAuthenticationHandler.HandleAuthenticateAsync called for scheme: {Scheme}", Scheme.Name);
            
            var apiKey = GetApiKeyFromHeader();

            if (string.IsNullOrEmpty(apiKey))
            {
                Logger.LogInformation("No API key found in Authorization header");
                return AuthenticateResult.NoResult();
            }

            Logger.LogInformation("API key found in header, validating...");

            try
            {
                var permission = await _apiKeyService.ValidateApiKeyAsync(apiKey);
                if (permission == null)
                {
                    Logger.LogWarning("Invalid API key provided");
                    return AuthenticateResult.Fail("Invalid API key");
                }

                Logger.LogInformation("API key validated successfully for PermissionId: {PermissionId}", permission.Id);

                // Load full permission with related data
                permission = await _dbContext.Permissions
                    .Include(p => p.User)
                    .Include(p => p.Team)
                    .Where(p => p.Id == permission.Id)
                    .SingleOrDefaultAsync();

                if (permission?.User == null)
                {
                    Logger.LogError("Permission not found or user not associated for PermissionId: {PermissionId}", permission?.Id);
                    return AuthenticateResult.Fail("Permission not found or user not associated");
                }

                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, permission.User.Kerberos ?? permission.User.Iam),
                    new Claim(UserService.IamIdClaimType, permission.User.Iam),
                    new Claim(ClaimTypes.Email, permission.User.Email ?? ""),
                    new Claim(ClaimTypes.GivenName, permission.User.FirstName ?? ""),
                    new Claim(ClaimTypes.Surname, permission.User.LastName ?? ""),
                    new Claim("PermissionId", permission.Id.ToString()),
                };

                // Add team information if available
                if (permission.Team != null)
                {
                    claims.Add(new Claim("TeamId", permission.Team.Id.ToString()));
                    claims.Add(new Claim("TeamSlug", permission.Team.Slug ?? ""));
                }

                // Fix: Pass the Scheme.Name as the authenticationType parameter
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                Logger.LogInformation("Authentication successful for user: {UserIam}, AuthenticationType: {AuthType}", 
                    permission.User.Iam, identity.AuthenticationType);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error authenticating API key");
                return AuthenticateResult.Fail("Authentication error");
            }
        }

        private string GetApiKeyFromHeader()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return null;
            }

            var authValue = authHeader.ToString();

            // Support both "Bearer {key}" and just "{key}" formats
            if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authValue.Substring(7); // Remove "Bearer " prefix
            }

            return authValue;
        }
    }
}