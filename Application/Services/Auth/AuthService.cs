using Application.Exceptions;
using Domain.DTOs;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameWithPermissions(dto.Username);
            if (user == null)
            {
                throw new BusinessException(ErrorCodeEnum.InvalidCredential, StatusCodeEnum.UnAuthorized);
            }

            if (user.Password != dto.Password)
            {
                throw new BusinessException(ErrorCodeEnum.InvalidCredential, StatusCodeEnum.UnAuthorized);
            }

            var permissions = user.Roles
                .SelectMany(x => x.Permissions)
                .Select(x => x.Name)
                .ToHashSet();

            return new LoginResponseDto
            {
                AccessToken = GenerateToken(user.Id, user.Username, permissions)
            };
        }

        private string GenerateToken(Guid userId, string username, HashSet<string> permissions)
        {
            var key = _configuration["Jwt:Key"];

            if (key == null)
                throw new BusinessException(ErrorCodeEnum.InvalidConfiguration, StatusCodeEnum.InternalServerError);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("permissions", string.Join(",", permissions))
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
