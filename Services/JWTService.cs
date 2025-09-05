using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
 

namespace EchoPlayAPI.Services
{
    public class JWTService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JWTService(string secret, string issuer, string audience, int expiryMinutes = 60)
        {
            _secret = secret ?? throw new ArgumentNullException(nameof(secret));
            _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            _audience = audience ?? throw new ArgumentNullException(nameof(audience));
            _expiryMinutes = expiryMinutes;
        }

        public string GenerateToken(string userEmail)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}