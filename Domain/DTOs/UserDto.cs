using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs
{
    public class UserDto
    {
    }

    public class CreateUserDto
    {
        [Required]
        [MaxLength(200)]
        [MinLength(5)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public List<Guid> RoleIds { get; set; } = new List<Guid>();
    }
}
