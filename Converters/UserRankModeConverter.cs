using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.User;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AOEMatchDataProvider.Converters
{
    public class UserRankModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            //if (!(value.GetType() == typeof(Dictionary<UserRankMode, UserRank>)))
            //    throw new InvalidCastException("Wrong target type");

            if (!(value.GetType() == typeof(UserRankData)))
                throw new InvalidCastException("Wrong target type");

            if (parameter == null) //ignore if null
                return "";

            if (!(parameter is UserRankMode || parameter is UserRankMode?))
                throw new InvalidCastException("Wrong parameter type");

            var ranks = value as UserRankData;//Dictionary<UserRankMode, UserRank>;
            var elo = string.Empty;

            if (!ranks.AviableRatings.Contains((UserRankMode)parameter))
            {
                return "-";
            }

            switch (parameter)
            {
                case UserRankMode.RandomMap:
                    elo = "Random Map: " + ranks.UserRatings[UserRankMode.RandomMap].Elo.ToString();
                    break;
                case UserRankMode.TeamRandomMap:
                    elo = "Team Random Map: " + ranks.UserRatings[UserRankMode.TeamRandomMap].Elo.ToString();
                    break;
                case UserRankMode.Deathmatch:
                    elo = "Deathmatch: " + ranks.UserRatings[UserRankMode.Deathmatch].Elo.ToString();
                    break;
                case UserRankMode.TeamdeathMatch:
                    elo = "Teamdeath Match:" + ranks.UserRatings[UserRankMode.TeamdeathMatch].Elo.ToString();
                    break;
                default:
                    throw new InvalidOperationException("Incorrect target rank"); //unranked shouldn't be set as parameter
                    //break;
            }

            return elo;
        }

        //one way binding don't require convert back to be implemented
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
