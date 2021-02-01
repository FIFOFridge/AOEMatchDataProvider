using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Validation
{
    public static class UserIdValidator
    {
        public static bool TryParseUserId(string entry, out UserIdMode userIdMode)
        {
            //check is steam profile url
            Regex steamProfileUrl = new Regex(@"(https|http)://(www.)?steamcommunity.com/profiles/(\w+)", 
                RegexOptions.Compiled & RegexOptions.IgnoreCase);
            
            //check is steam64 id
            Regex steamId64 = new Regex(@"\d{17}");
            
            //check is game profile id
            Regex gameProfileId = new Regex(@"\d{3,}");

            if(steamProfileUrl.IsMatch(entry))
            {
                userIdMode = UserIdMode.SteamProfileURL;
                return true;
            }

            if(steamId64.IsMatch(entry))
            {
                userIdMode = UserIdMode.SteamId64;
                return true;
            }

            if (gameProfileId.IsMatch(entry))
            {
                userIdMode = UserIdMode.GameProfileId;
                return true;
            }

            userIdMode = UserIdMode.Invalid;
            return false;
        }
    }

    public enum UserIdMode
    {
        Invalid,
        SteamProfileURL,
        SteamId64,
        GameProfileId
    }
}
