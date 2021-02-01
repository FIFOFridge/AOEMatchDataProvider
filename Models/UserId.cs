using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    public class UserId
    {
        public string SteamId { get; set; } //steam id aviable only for steam version users
        public string GameProfileId { get; set; } //profile id aviable for all users
    }
}
