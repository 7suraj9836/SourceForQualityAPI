using Microsoft.AspNetCore.Authorization;

namespace UserModuleApi.Filters
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; }
        public RoleRequirement(string role)
        {
            Role = role;
        }
    }
}
