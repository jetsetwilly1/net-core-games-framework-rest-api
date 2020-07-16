using System;
using System.Collections.Generic;
using System.Text;

namespace Midwolf.GamesFramework.Services.Models.Interfaces
{
    public interface IGame
    {
        int Id { get; }
        string Title { get; set; }
        double Created { get; }
        double LastUpdated { get; }
        int EntriesCount { get; }
        int PlayersCount { get; }
        int UserId { get; set; }
    }
}
