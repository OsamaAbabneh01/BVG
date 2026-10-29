using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public interface IRoleService
    {
        Task CreateRole(CreateRoleDto dto);
    }
}
