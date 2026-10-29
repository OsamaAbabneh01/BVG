using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto dto);
    }
}
