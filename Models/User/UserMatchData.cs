using AOEMatchDataProvider.Models.Match;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.User
{
    //match data related to match request
    public class UserMatchData
    {
        public UserData UserRankData { get; internal set; }

        public string Name { get; set; }
        public int Team { get; set; }

        public string Civilization { get; set; }
        public string Color { get; set; }

        public UserGameProfileId UserGameProfileId { get; internal set; }
        public MatchType MatchType { get; internal set; }

        public UserMatchData() { }

        public override string ToString()
        {
            return $"User match data (UserGameProfileId: {UserGameProfileId.ToString()}), ratings: {UserRankData.ToString()}";
        }
    }
}
