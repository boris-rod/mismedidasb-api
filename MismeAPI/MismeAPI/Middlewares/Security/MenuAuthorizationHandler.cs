using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Middlewares.Security
{
    public class MenuAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Menu>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       OperationAuthorizationRequirement requirement,
                                                       Menu resource)
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
                if (requirement.Name == Operations.Read.Name)
                {
                    if (resource.Active)
                    {
                        if (!resource.GroupId.HasValue || (resource.GroupId.HasValue && resource.GroupId.Value == groupId))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else
                    {
                        if (role == RoleEnum.GROUP_ADMIN.ToString() && resource.GroupId.Value == groupId)
                        {
                            context.Succeed(requirement);
                        }
                    }
                }

                if (requirement.Name == Operations.Update.Name || requirement.Name == Operations.Delete.Name)
                {
                    if (resource.GroupId.HasValue)
                    {
                        if (role == RoleEnum.GROUP_ADMIN.ToString() && resource.GroupId.Value == groupId)
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
