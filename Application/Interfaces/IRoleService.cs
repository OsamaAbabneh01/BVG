using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IRoleService
    {
        Task CreateRole(CreateRoleDto dto);
    }
}
