using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UserModuleApi.Filters
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (!context.User.HasClaim(c => (c.Type.ToLower() == ClaimTypes.Role.ToLower() ||
      c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
) && c.Value.ToLower() == requirement.Role.ToLower()))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
