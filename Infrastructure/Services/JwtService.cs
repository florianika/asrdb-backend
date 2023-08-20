﻿using Application.Exceptions;
using Application.Ports;
using Domain.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class JwtService : IAuthTokenService
    {
        private readonly IOptions<JwtSettings> _settings;
        public JwtService(IOptions<JwtSettings> settings)
        {
            _settings = settings;
        }

        public Task<string> GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _settings.Value.AccessTokenSettings.SecretKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


            var claimsIdentity = new ClaimsIdentity();

            // Access Token must only carry the user Id
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, user.AccountRole.ToString()));
            // Add scope claim, which contains an array of scopes
            var scope = user.Claims.SingleOrDefault(c => c.Type == "scope");
            if (scope != null) claimsIdentity.AddClaim(new System.Security.Claims.Claim("scope", string.Join(" ", scope.Value)));

            var jwtHandler = new JwtSecurityTokenHandler();

            var jwt = jwtHandler.CreateJwtSecurityToken(
                issuer: _settings.Value.AccessTokenSettings.Issuer,
                audience: _settings.Value.AccessTokenSettings.Audience,
                subject: claimsIdentity,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddSeconds(_settings.Value.AccessTokenSettings.LifeTimeInSeconds),
                issuedAt: DateTime.Now,
                signingCredentials: signingCredentials);

            var serializedJwt = jwtHandler.WriteToken(jwt);

            return Task.FromResult(serializedJwt);
        }
        public Task<string> GenerateIdToken(User user)
        {

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _settings.Value.AccessTokenSettings.SecretKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Name, user.Name));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Email, user.Email));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.GivenName, user.Name));
            claimsIdentity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Surname, user.LastName));
            
            // Add custom claims if any
            foreach (var c in user.Claims ?? System.Linq.Enumerable.Empty<Domain.Claim.Claim>())
            {
                claimsIdentity.AddClaim(new System.Security.Claims.Claim(c.Type, c.Value));
            }
            // Add AccountRole claim
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, user.AccountRole.ToString()));
            // Add AccountStatus claim
            claimsIdentity.AddClaim(new Claim("AccountStatus", user.AccountStatus.ToString()));


            var jwtHandler = new JwtSecurityTokenHandler();

            var jwt = jwtHandler.CreateJwtSecurityToken(
                issuer: _settings.Value.AccessTokenSettings.Issuer,
                audience: _settings.Value.AccessTokenSettings.Audience,
                subject: claimsIdentity,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddSeconds(_settings.Value.AccessTokenSettings.LifeTimeInSeconds),
                issuedAt: DateTime.Now,
                signingCredentials: signingCredentials);

            var serializedJwt = jwtHandler.WriteToken(jwt);

            return Task.FromResult(serializedJwt);
        }
        public Task<string> GenerateRefreshToken()
        {
            var size = _settings.Value.RefreshTokenSettings.Length;
            var buffer = new byte[size];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return Task.FromResult(Convert.ToBase64String(buffer));
        }
        public Task<int> GetRefreshTokenLifetimeInMinutes()
        {
            return Task.FromResult(_settings.Value.RefreshTokenSettings.LifeTimeInMinutes);
        }
        public Task<Guid> GetUserIdFromToken(string token)
        {
            try
            {

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // we may be trying to validate an expired token so it makes no sense checking for it's lifetime.
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _settings.Value.AccessTokenSettings.Issuer,
                    ValidAudience = _settings.Value.AccessTokenSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_settings.Value.AccessTokenSettings.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(0)
                };

                var jwtHandler = new JwtSecurityTokenHandler();
                var claims = jwtHandler.ValidateToken(token, tokenValidationParameters, out _);
                var userId = Guid.Parse(claims.FindFirst(ClaimTypes.NameIdentifier).Value);

                return Task.FromResult(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(ex.Message, ex);
            }
        }
        public Task<bool> IsTokenValid(string token, bool validateLifeTime)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(false);
            }
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = validateLifeTime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Value.AccessTokenSettings.Issuer,
                ValidAudience = _settings.Value.AccessTokenSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_settings.Value.AccessTokenSettings.SecretKey)),
                ClockSkew = TimeSpan.FromMinutes(0)
            };

            var jwtHandler = new JwtSecurityTokenHandler();
            try
            {
                jwtHandler.ValidateToken(token, tokenValidationParameters, out _);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

    }
}
