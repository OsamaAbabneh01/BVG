using Microsoft.AspNetCore.Authorization;

namespace API.Authorization
{
    public class PermissionAuthrizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissionClaim = context.User.FindFirst("permissions")?.Value;

            if (string.IsNullOrWhiteSpace(permissionClaim))
            {
                return Task.CompletedTask;
            }

            var permissions = permissionClaim.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var hasPermission = permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase);
        
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
