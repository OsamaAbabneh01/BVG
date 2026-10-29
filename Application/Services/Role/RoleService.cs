using Application.Exceptions;
using Domain.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

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
            var roles = await _roleRepository.WhereAsync(r => r.Name == dto.Name.Trim());
            if (roles.Any())
            {
                throw new BusinessException(ErrorCodeEnum.RoleAlreadyExists, StatusCodeEnum.Conflict);
            }
            var role = roles.FirstOrDefault();

            await _roleRepository.AddAsync(new Role
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim()
            });
        }
    }
}
