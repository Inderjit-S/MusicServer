﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MusicServer.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace MusicServer.Data
{
    public class JwtHandler(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        // Generates the token
        public async Task<JwtSecurityToken> GetTokenAsync(ApplicationUser user) =>
                new(
                    issuer: configuration["JwtSettings:Issuer"],
                    audience: configuration["JwtSettings:Audience"],
                    claims: await GetClaimsAsync(user),
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["JwtSettings:ExpirationTimeInMinutes"])),
                    signingCredentials: GetSigningCredentials());

        // Takes encryption and encoding
        private SigningCredentials GetSigningCredentials()
        {
            byte[] key = Encoding.UTF8.GetBytes(configuration["JwtSettings:SecurityKey"]!);
            SymmetricSecurityKey secret = new(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            List<Claim> claims = [new Claim(ClaimTypes.Name, user.UserName!)];
            claims.AddRange(from role in await userManager.GetRolesAsync(user) select new Claim(ClaimTypes.Role, role));
            return claims;
        }
    }
}