using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Middlewares.Security
{
    public class GroupAuthorizationCrudHandler : AuthorizationHandler<OperationAuthorizationRequirement, Group>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       OperationAuthorizationRequirement requirement,
                                                       Group resource)
        {
            var userId = context.User.GetUserIdFromToken();
            var role = context.User.GetUserRoleFromToken();

            if (role == RoleEnum.ADMIN.ToString())
            {
                context.Succeed(requirement);
            }
            else
            {
                if (role == RoleEnum.GROUP_ADMIN.ToString())
                {
                    if (resource.Users.Any(u => u.Id == userId) && (requirement.Name == Operations.Read.Name || requirement.Name == Operations.Update.Name || requirement.Name == Operations.Delete.Name))
                    {
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    if (resource.Users.Any(u => u.Id == userId) && (requirement.Name == Operations.Read.Name))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
