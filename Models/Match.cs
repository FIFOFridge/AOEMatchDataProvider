using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    public class Match
    {
        public string MatchId { get; set; }

        public string Opened { get; set; }
        public string Finished { get; set; }

        //[JsonIgnore]
        //public bool IsInProgress => 
        //    string.IsNullOrEmpty(Finished) &&
        //    !string.IsNullOrEmpty(Opened)
        //    ;

        [JsonIgnore]
        public bool IsInProgress => string.IsNullOrEmpty(Finished);

        public MatchType MatchType { get; set; }
        public List<UserMatchData> Users { get; set; }
    }
}
