using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models
{
    public sealed class AppSettings
    {
        public string Secret { get; set; }
    }

    public sealed class ConnectionsStrings
    {
        public string Mysql { get; set; }
    }
}
