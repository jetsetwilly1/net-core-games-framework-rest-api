using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Services.Models.Db
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string PublicToken { get; set; }
        public string Roles { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<GameEntity> Games { get; set; }
    }
}
