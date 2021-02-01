using AOEMatchDataProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Extensions
{
    public static class UserExtensions
    {
        public static UserRankData GetUserRankData(this IEnumerable<UserMatchData> userMatchDatas, UserGameProfileId userGameProfileId)
        {
            var userMatchData = userMatchDatas.First
                (umd => umd.UserGameProfileId.ProfileId == userGameProfileId.ProfileId);

            if (userMatchData == null)
                return null;

            //if (!(userMatchData.UserRankData.UserRatings.ContainsKey(userRankMode)))
            //    return null;

            return userMatchData.UserRankData;
        }

        public static UserRank GetUserRank(this IEnumerable<UserMatchData> userMatchDatas, UserGameProfileId userGameProfileId, UserRankMode userRankMode)
        {
            var userMatchData = userMatchDatas.First
                (umd => umd.UserGameProfileId.ProfileId == userGameProfileId.ProfileId);

            if (userMatchData == null)
                return null;

            if (!(userMatchData.UserRankData.UserRatings.ContainsKey(userRankMode)))
                return null;

            return userMatchData.UserRankData.UserRatings[userRankMode];
        }

        public static void MergeUserRank(this UserMatchData userMatchData, UserRankData userRankData)
        {
            //iterate over all rank types
            foreach(var rank in Enum.GetValues(typeof(UserRankMode)))
            {
                var rankType = (UserRankMode)rank;

                //check what ranting type we can expect to be update
                if(userRankData.AviableRatings.Contains(rankType))
                {
                    //update if already exist
                    if (userMatchData.UserRankData.UserRatings.ContainsKey(rankType))
                        userMatchData.UserRankData.UserRatings[rankType] = userRankData.UserRatings[rankType];
                    //add new if not exist
                    else
                        userMatchData.UserRankData.UserRatings.Add(rankType, userRankData.UserRatings[rankType]);
                }
            }
        }
    }
}
