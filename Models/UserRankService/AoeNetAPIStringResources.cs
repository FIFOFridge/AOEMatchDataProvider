using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    //api values to strings maping
    //model generated with: https://json2csharp.com/
    public class AoeNetAPIStringResources : ISerializableModel
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("age")]
        public List<Age> Age { get; set; }

        [JsonProperty("civ")]
        public List<Civ> Civ { get; set; }

        [JsonProperty("game_type")]
        public List<GameType> GameType { get; set; }

        [JsonProperty("leaderboard")]
        public List<Leaderboard> Leaderboard { get; set; }

        [JsonProperty("map_size")]
        public List<MapSize> MapSize { get; set; }

        [JsonProperty("map_type")]
        public List<MapType> MapType { get; set; }

        [JsonProperty("rating_type")]
        public List<RatingType> RatingType { get; set; }

        [JsonProperty("resources")]
        public List<Resource> Resources { get; set; }

        [JsonProperty("speed")]
        public List<Speed> Speed { get; set; }

        [JsonProperty("victory")]
        public List<Victory> Victory { get; set; }

        [JsonProperty("visibility")]
        public List<Visibility> Visibility { get; set; }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<AoeNetAPIStringResources>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Age
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Civ
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class GameType
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Leaderboard
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class MapSize
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class MapType
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class RatingType
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Resource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Speed
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Victory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }

    public class Visibility
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("string")]
        public string String { get; set; }
    }
}
