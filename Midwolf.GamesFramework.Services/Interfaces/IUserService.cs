using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IUserService
    {
        Task<NewUser> CreateUserAsync(NewUser userDto);

        Task<UserTokens> RefreshUserTokens(RefreshUserTokens refreshDto);

        Task<IEnumerable<ApiUser>> GetAll();
    }
}
