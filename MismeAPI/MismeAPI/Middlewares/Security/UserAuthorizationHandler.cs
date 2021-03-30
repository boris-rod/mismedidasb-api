using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Middlewares.Security
{
    public class UserAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, User>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       OperationAuthorizationRequirement requirement,
                                                       User resource)
        {
            var userId = context.User.GetUserIdFromToken();
            var role = context.User.GetUserRoleFromToken();
            var groupId = context.User.GetUserGroupFromToken();

            if (role == RoleEnum.ADMIN.ToString())
            {
                context.Succeed(requirement);
            }
            else
            {
                if (requirement.Name == Operations.ManagePlans.Name)
                {
                    // user and group admin are in the same group
                    if (resource.GroupId.HasValue && resource.GroupId.Value == groupId)
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
