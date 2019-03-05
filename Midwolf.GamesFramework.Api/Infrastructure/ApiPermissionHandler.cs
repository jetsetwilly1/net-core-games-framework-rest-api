using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Api.Infrastructure
{
    public class ApiPermissionsHandler : AuthorizationHandler<ApiMinimumRequirments>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiMinimumRequirments apiRequirements)
        {
            var succeed = false;

            foreach (var requirement in apiRequirements.Requirements)
            {
                if (requirement.GetType() == typeof(PlayerMinimumRequirement))
                {
                    if (PlayerCheck(context.User, requirement as PlayerMinimumRequirement))
                    {
                        context.Succeed(apiRequirements);
                        succeed = true;
                    }
                }
                else if (requirement.GetType() == typeof(UserMinimumRequirement))
                {
                    if (UserCheck(context.User, requirement as UserMinimumRequirement))
                    {
                        context.Succeed(apiRequirements);
                        succeed = true;
                    }
                }
            }

            if(!succeed) context.Fail();

            //TODO: Use the following if targeting a version of
            //.NET Framework older than 4.6:
            //      return Task.FromResult(0);
            return Task.CompletedTask;
        }

        private bool PlayerCheck(ClaimsPrincipal user, PlayerMinimumRequirement requirement)
        {
            var playerPermissions = new Dictionary<string, PlayerRolePermission> {
                { "apibasic", PlayerRolePermission.ApiBasic },
                { "register", PlayerRolePermission.Registered },
                { "administrate", PlayerRolePermission.Administer },
                { "moderate", PlayerRolePermission.Moderate },
                { "judge", PlayerRolePermission.Judge },
            };

            var minimumRequirement = requirement.Permission;

            var usersRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (usersRole != null)
            {
                if (playerPermissions.ContainsKey(usersRole))
                {
                    var permissionRole = playerPermissions[usersRole];

                    if ((int)permissionRole > (int)minimumRequirement)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool UserCheck(ClaimsPrincipal user, UserMinimumRequirement requirement)
        {
            var dict = new Dictionary<string, UserRolePermission> {
                { "public", UserRolePermission.Public },
                { "administrator", UserRolePermission.Administrator },
                { "superuser", UserRolePermission.SuperUser }
            };

            var minimumRequirement = requirement.Permission;

            var usersRole = user.FindFirst(ClaimTypes.Role)?.Value;

            if (usersRole != null)
            {
                if (dict.ContainsKey(usersRole))
                {
                    var permissionRole = dict[usersRole];

                    if ((int)permissionRole > (int)minimumRequirement)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
