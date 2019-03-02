using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class FlowEntity
    {
        public int Id { get; set; }

        public int? SuccessEvent { get; set; }

        public int? FailEvent { get; set; }

        public bool IsStart { get; set; }
    }
}
