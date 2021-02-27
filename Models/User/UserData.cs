using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.User
{
    public class UserData : BindableBase, ISerializableModel
    {
        Dictionary<Ladders, UserRank> userRatings;
        public Dictionary<Ladders, UserRank> UserRatings { get => userRatings; set => SetProperty(ref userRatings, value); }

        [JsonIgnore]
        public IEnumerable<Ladders> AviableRatings
        {
            get
            {
                if (UserRatings == null)
                    return new Ladders[0];

                return UserRatings.Keys.ToArray();
            }
        }

        public UserData()
        {
            UserRatings = new Dictionary<Ladders, UserRank>();
        }

        [JsonConstructor]
        internal UserData(Dictionary<Ladders, UserRank> UserRatings)
        {
            if (UserRatings == null)
                UserRatings = new Dictionary<Ladders, UserRank>();

            this.UserRatings = UserRatings;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<UserData>(serialized);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var userRating in UserRatings)
            {
                sb.Append("\n " + userRating.Key.ToString() + " -> elo: " + userRating.Value.Elo);
            }

            return sb.ToString();
        }
    }
}
