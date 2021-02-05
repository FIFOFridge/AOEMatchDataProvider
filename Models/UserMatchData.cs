using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    public class UserMatchData
    {
        //[JsonIgnore]
        //public Dictionary<UserRankMode, UserRank> Rating { get; set; }
        public UserRankData UserRankData { get; internal set; }

        public string Name { get; set; }
        public int Team { get; set; }

        public string Civilization { get; set; }
        public string Color { get; set; }

        public UserGameProfileId UserGameProfileId { get; internal set; }
        public MatchType MatchType { get; internal set; }

        //public MatchType MatchType { get; internal set; }

        public UserMatchData()
        {
            //Rating = new Dictionary<UserRankMode, UserRank>();
        }

        public override string ToString()
        {
            return $"User match data (UserGameProfileId: {UserGameProfileId.ToString()}), ratings: {UserRankData.ToString()}";
        }
    }

    //TODO: implement deathmatches
    //aviable rating modes, excluding "Unranked"


    //enum Team
    //{
    //    First,
    //    Secound
    //}
}
