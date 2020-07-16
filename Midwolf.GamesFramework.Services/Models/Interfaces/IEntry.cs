using System;
using System.Collections.Generic;
using System.Text;

namespace Midwolf.GamesFramework.Services.Models.Interfaces
{
    public interface IEntry
    {
        int Id { get; }
        
        int? PlayerId { get; set; }
        
        double CreatedAt { get; }

        int State { get; set; }
    }
}
