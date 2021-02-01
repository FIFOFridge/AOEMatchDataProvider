using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.UserRankService
{
    //todo: fix naming convetion using JSONProperty attribute for each getter/setter
    //model generated with: https://json2csharp.com/
    public class MatchResponse
    {
        public int profile_id { get; set; }
        public string steam_id { get; set; }
        public string name { get; set; }
        public string clan { get; set; }
        public string country { get; set; }
        public LastMatch last_match { get; set; }

        public class LastMatch
        {
            public string match_id { get; set; }
            public object lobby_id { get; set; }
            public string match_uuid { get; set; }
            public string version { get; set; }
            public string name { get; set; }
            public int num_players { get; set; }
            public int num_slots { get; set; }
            public object average_rating { get; set; }
            public bool cheats { get; set; }
            public bool full_tech_tree { get; set; }
            public int ending_age { get; set; }
            public object expansion { get; set; }
            public int game_type { get; set; }
            public object has_custom_content { get; set; }
            public bool has_password { get; set; }
            public bool lock_speed { get; set; }
            public bool lock_teams { get; set; }
            public int map_size { get; set; }
            public int map_type { get; set; }
            public int pop { get; set; }
            public bool ranked { get; set; }
            public int leaderboard_id { get; set; }
            public int rating_type { get; set; }
            public int resources { get; set; }
            public object rms { get; set; }
            public object scenario { get; set; }
            public string server { get; set; }
            public bool shared_exploration { get; set; }
            public int speed { get; set; }
            public int starting_age { get; set; }
            public bool team_together { get; set; }
            public bool team_positions { get; set; }
            public int treaty_length { get; set; }
            public bool turbo { get; set; }
            public int victory { get; set; }
            public int victory_time { get; set; }
            public int visibility { get; set; }
            public string opened { get; set; }
            public string started { get; set; }
            public string finished { get; set; }
            public List<Player> players { get; set; }
        }

        public class Player
        {
            public int profile_id { get; set; }
            public string steam_id { get; set; }
            public string name { get; set; }
            public string clan { get; set; }
            public string country { get; set; }
            public int slot { get; set; }
            public int slot_type { get; set; }
            public int? rating { get; set; }
            public object rating_change { get; set; }
            public int? games { get; set; }
            public int? wins { get; set; }
            public int? streak { get; set; }
            public int? drops { get; set; }
            public int? color { get; set; }
            public int team { get; set; }
            public int? civ { get; set; }
            public bool? won { get; set; }
        }
    }
}
