/*using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.DAL;
using api.DTOs;
using api.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [ApiController]
    [Route("api/login")]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {

        private readonly IDbService _dbService;
        private Dictionary<string, Guid> tokens;

        private IConfiguration Configuration { get; set; }

        public LoginController(IDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            Configuration = configuration;
            tokens = new Dictionary<string, Guid>();
        }


        [HttpPost]
        public IActionResult Login(LoginRequestDto requestDto)
        {
            Claim[] claims;
            string name;

            if (requestDto.Login == "admin" && requestDto.Password == "ASDF1234")
            {
                name = "Administrator";
                claims = new[]
               {
                new Claim(ClaimTypes.NameIdentifier, "admin"),
                new Claim(ClaimTypes.Name, name),

                new Claim(ClaimTypes.Role, "employee"),
                new Claim(ClaimTypes.Role, "admin")
                };
            }
            else if (requestDto.Login == "emp" && requestDto.Password == "ASDF")
            {
                name = "Employee";
                claims = new[]
               {
                new Claim(ClaimTypes.NameIdentifier, "emp"),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "employee")
                };
            }
            else
            {
                var student = _dbService.FindStudentToLogin(requestDto.Login, requestDto.Password);
                name = student.FirstName + " " + student.LastName;
                claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "student")
                };
            }

            var token = GenerateToken(claims);
            var refreshToken = GenerateRefreshToken();
            tokens.Add(name, refreshToken);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }


        [HttpPost]
        public IActionResult Refresh(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var username = principal.Identity.Name;
            var savedRefreshToken = GetRefreshToken(username);
            if (savedRefreshToken.ToString() != refreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();
            SaveRefreshToken(username, newRefreshToken);

            return new ObjectResult(new
            {
                token = newJwtToken,
                refreshToken = newRefreshToken
            });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, 
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"])),
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private Guid GetRefreshToken(string username)
        {
            return tokens[username];
        }

        private JwtSecurityToken GenerateToken (IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );
            return token;
        }

        private Guid GenerateRefreshToken()
        {
            return Guid.NewGuid();
        }

        private void SaveRefreshToken(string name, Guid newToken)
        {
            tokens[name] = newToken;
        }

    }
}*/