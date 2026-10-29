using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public interface IUserService
    {
        Task CraeteUser(CreateUserDto dto);

        Task<PaginatedResultDto> GetAllUsers(DynamicPaginationDto dto);
    }
}
