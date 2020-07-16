using Midwolf.GamesFramework.Services.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    public interface IRandomDrawEventService
    {
        Task<bool> ExecuteDraw(int randomEventId);

        Task<bool> UpdateDrawExecutionJobByEndDate(int randomEventId);

        Task<bool> ClearDrawExecutionJobs(int randomEventId);
    }
}
