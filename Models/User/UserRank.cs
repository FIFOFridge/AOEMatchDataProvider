using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.User
{
    // Single ladder rank model
    public class UserRank : ISerializableModel
    {
        public int? Elo { get; set; }
        public int? Ladder { get; set; }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<UserRank>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
