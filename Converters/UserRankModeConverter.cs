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

            if (!(value.GetType() == typeof(UserData)))
                throw new InvalidCastException("Wrong target type");

            if (parameter == null) //ignore if null
                return "";

            if (!(parameter is Ladders || parameter is Ladders?))
                throw new InvalidCastException("Wrong parameter type");

            var ranks = value as UserData;//Dictionary<UserRankMode, UserRank>;
            var elo = string.Empty;

            if (!ranks.AviableRatings.Contains((Ladders)parameter))
            {
                return "-";
            }

            switch (parameter)
            {
                case Ladders.RandomMap:
                    elo = "Random Map: " + ranks.UserRatings[Ladders.RandomMap].Elo.ToString();
                    break;
                case Ladders.TeamRandomMap:
                    elo = "Team Random Map: " + ranks.UserRatings[Ladders.TeamRandomMap].Elo.ToString();
                    break;
                case Ladders.Deathmatch:
                    elo = "Deathmatch: " + ranks.UserRatings[Ladders.Deathmatch].Elo.ToString();
                    break;
                case Ladders.TeamdeathMatch:
                    elo = "Teamdeath Match:" + ranks.UserRatings[Ladders.TeamdeathMatch].Elo.ToString();
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
