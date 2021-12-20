
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TableIT.Core
{
    public static class ServiceUtils
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new();

        public static string GenerateAccessToken(string tableId, TimeSpan? lifetime = null) 
            => GenerateAccessToken(null, tableId, lifetime);

        public static string GenerateAccessToken(string? audience, string? tableId, TimeSpan? lifetime = null)
        {
            IEnumerable<Claim>? claims = null;
            if (tableId != null)
            {
                claims = new[]
                {
                    new Claim("tableid", tableId),
                };
            }

            return GenerateAccessTokenInternal(audience, claims, lifetime ?? TimeSpan.FromHours(1));
        }

        private static string GenerateAccessTokenInternal(string? audience, IEnumerable<Claim>? claims, TimeSpan lifetime)
        {
            var expire = DateTime.UtcNow.Add(lifetime);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: audience,
                subject: claims == null ? null : new ClaimsIdentity(claims),
                expires: expire);
            return JwtTokenHandler.WriteToken(token);
        }
    }
}
