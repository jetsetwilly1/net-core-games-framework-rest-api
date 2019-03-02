using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IFlowService : IErrorService<Error>
    {
        Task<ICollection<Flow>> AddFlowAsync(int gameId, ICollection<Flow> eventDto);

        Task<bool> DeleteFlowAsync(int gameId);

        Task<ICollection<Flow>> GetFlowAsync(int gameId);
    }
}
