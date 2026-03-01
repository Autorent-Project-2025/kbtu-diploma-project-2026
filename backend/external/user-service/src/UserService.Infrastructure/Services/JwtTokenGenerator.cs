using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator, IDisposable
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenLifeTime;

        private readonly RSA _rsa;
        private readonly RsaSecurityKey _signingKey;

        public JwtTokenGenerator(IConfiguration config)
        {
            this._issuer = config["Jwt:Issuer"]
                ?? throw new Exception("Jwt:Issuer is missing");

            this._audience = config["Jwt:Audience"]
                ?? throw new Exception("Jwt:Audience is missing");

            this._tokenLifeTime = config.GetValue<int?>("Jwt:LifeTime")
                ?? throw new Exception("Jwt:LifeTime is missing");


            var privateKeyPem = (config["Jwt:Key"] ?? throw new Exception("Key missing"))
                .Replace("\\n", "\n").Trim();

            this._rsa = RSA.Create();
            try
            {
                this._rsa.ImportFromPem(privateKeyPem);
            }
            catch
            {
                this._rsa.Dispose();
                throw;
            }

            this._signingKey = new RsaSecurityKey(_rsa);
        }

        public string GenerateToken(User user)
        {
            var credentials = new SigningCredentials(
                _signingKey,
                SecurityAlgorithms.RsaSha256
            );

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("name", user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenLifeTime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void Dispose()
        {
            _rsa?.Dispose();
        }
    }
}
