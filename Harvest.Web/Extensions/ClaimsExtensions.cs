﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Web.Extensions
{
    public static class ClaimsExtensions
    {
        public const string IamIdClaimType = "ucdPersonIAMID";
        public static string GetNameClaim(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || !claimsPrincipal.Identity.IsAuthenticated) return string.Empty;

            var nameClaim = claimsPrincipal.FindFirst("name");

            return nameClaim != null ? nameClaim.Value : claimsPrincipal.Identity.Name;
        }

        public static User GetUserInfo(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || !claimsPrincipal.Identity.IsAuthenticated) return null;
            var iam = claimsPrincipal.Claims.SingleOrDefault(a => a.Type == IamIdClaimType);
            //Need this when I was testing my own login. was blank if I just used the first value
            var first = claimsPrincipal.Claims.FirstOrDefault(a => a.Type == ClaimTypes.GivenName && !string.IsNullOrWhiteSpace(a.Value));
            return new User
            {
                FirstName = first?.Value,
                LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname),
                Email = claimsPrincipal.FindFirstValue(ClaimTypes.Email),
                Kerberos = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), //???
                Iam = iam?.Value
            };
        }
    }
}
