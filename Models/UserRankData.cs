﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    public class UserRankData : ISerializableModel
    {
        public Dictionary<UserRankMode, UserRank> UserRatings { get; }

        [JsonIgnore]
        public IEnumerable<UserRankMode> AviableRatings
        {
            get
            {
                if (UserRatings == null)
                    return new UserRankMode[0];

                return UserRatings.Keys.ToArray();
            }
        }


        public UserRankData()
        {
            UserRatings = new Dictionary<UserRankMode, UserRank>();
        }

        [JsonConstructor]
        internal UserRankData(Dictionary<UserRankMode, UserRank> UserRatings)
        {
            if (UserRatings == null)
                UserRatings = new Dictionary<UserRankMode, UserRank>();

            this.UserRatings = UserRatings;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<UserRankData>(serialized);
        }
    }
}
