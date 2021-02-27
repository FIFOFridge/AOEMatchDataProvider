using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.User
{
    //player data related to ladder request
    public class UserLadderData
    {
        public string Country { get; set; }
        public int? PreviousRating { get; set; }
        public int? HighestRating { get; set; }
        public int? Streak { get; set; }
        public int? LowestStreak { get; set; }
        public int? HighestStreak { get; set; }
        public int? Games { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Drops { get; set; }
    }
}
