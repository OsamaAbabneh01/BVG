using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task CreateRole(CreateRoleDto dto)
        {
            var roles = await _roleRepository.Where(r => r.Name == dto.Name.Trim());
            if (roles.Any())
            {
                throw new Exception("role already exists");
            }
            var role = roles.FirstOrDefault();

            await _roleRepository.AddAsync(new Role
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
            });
        }
    }
}
