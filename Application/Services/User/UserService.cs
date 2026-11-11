using Application.Common;
using Application.Exceptions;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task CraeteUser(CreateUserDto dto)
        {
            var users = await _userRepository.WhereAsync(u => u.Username == dto.Username.Trim());
            if (users.Any())
            {
                throw new BusinessException(ErrorCodeEnum.UserAlreadyExists, StatusCodeEnum.Conflict);
            }

            var roles = new List<Role>();
            foreach(var roleId in dto.RoleIds)
            {
                var role = await _roleRepository.GetById(roleId);
                if (role ==  null)
                {
                    throw new Exception("role not found");
                }
                roles.Add(role);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.Username.Trim(),
                Username = dto.Username.Trim(),
                Password = GeneratePassword(),
                Roles = roles
            };

            await _userRepository.AddAsync(user);
        }


        public async Task<PaginatedResultDto> GetAllUsers(DynamicPaginationDto dto)
        {
            var query = _userRepository.Query(dto.Groups);

            var result = await query.PagenateAsync(dto.PageNumber, dto.PageSize);

            return new PaginatedResultDto
            {
                Data = result.Data.Select(x => new
                {
                    x.Username,
                    x.FullName
                }),
                TotalCount = result.TotalCount,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize
            };
        }


        private string GeneratePassword(int length = 12)
        {
            const string chars =
                "ABCDEFGHJKLMNPQRSTUVWXYZ" +
                "abcdefghijkmnopqrstuvwxyz" +
                "23456789" +
                "!@#$%";

            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(result);
        }
    }
}
