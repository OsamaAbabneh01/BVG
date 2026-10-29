using API.Authorization;
using Application.Services;
using Domain.DTOs;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("/api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto body)
        {
            await _userService.CraeteUser(body);
            return Ok();
        }

        [HttpPost("pagination")]
        //[Authorize(Policy = "Permission:GetAll")]
        //[Permission(PermissionEnum.GetAll)]
        public async Task<IActionResult> GetAllUsers([FromBody] DynamicPaginationDto dto)
        {
            var result = await _userService.GetAllUsers(dto);
            return Ok(result);
        }
    }
}
