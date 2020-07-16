using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midwolf.Api.Infrastructure
{
    public interface IApiRequirement
    {
    }

    public enum PlayerRolePermission
    {
        ApiBasic = 1,
        Registered = 2,
        Administer = 3,
        Moderate = 4,
        Judge = 5
    }

    public enum UserRolePermission
    {
        Public = 1,
        Administrator = 2,
        SuperUser = 3
    }

    public class ApiMinimumRequirments : IAuthorizationRequirement
    {
        public ICollection<IApiRequirement> Requirements { get; set; }

        public ApiMinimumRequirments(ICollection<IApiRequirement> requirements)
        {
            Requirements = requirements;
        }
    }

    public class PlayerMinimumRequirement : IApiRequirement
    {
        public PlayerRolePermission Permission { get; set; }

        public PlayerMinimumRequirement(PlayerRolePermission permission)
        {
            Permission = permission;
        }
    }

    public class UserMinimumRequirement : IApiRequirement
    {
        public UserRolePermission Permission { get; set; }

        public UserMinimumRequirement(UserRolePermission permission)
        {
            Permission = permission;
        }
    }
}
