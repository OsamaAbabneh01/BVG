using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace API.Authorization
{
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(PermissionEnum permission)
        {
            Policy = $"Permission:{permission.ToString()}";
        }
    }
}
