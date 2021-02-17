using AOEMatchDataProvider.Models.RequestService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static AOEMatchDataProvider.Helpers.Request.RequestHelper;

namespace AOEMatchDataProvider.Models.QueryCacheHelper
{
    public class QueriesCache : ISerializableModel
    {
        [JsonProperty]
        internal Dictionary<
            string, //request string
            QueryCacheEntry //request response
            > CachedQueries { get; set; }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<QueriesCache>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal class QueryCacheEntry
        {
            [JsonProperty]
            internal RequestResponseWrapper ResponseValue { get; set; }
            [JsonProperty]
            internal bool RetryOnException { get; set; }
            [JsonProperty]
            internal int MaxRetryCount { get; set; }
            [JsonProperty]
            internal int RetryCount { get; set; }
            [JsonProperty]
            internal DateTime Expires { get; set; }
            [JsonProperty]
            internal string Request { get; set; }

            [JsonIgnore]
            internal bool IsSuccessed
            {
                get
                {
                    return ResponseValue != null && ResponseValue.IsSuccess;
                }
            }

            [JsonConstructor]
            internal QueryCacheEntry(RequestResponseWrapper responseWrapper, DateTime expier, string request, bool retryOnException = true, int maxRetryCount = 2)
            {
                ResponseValue = responseWrapper;
                RetryOnException = retryOnException;
                MaxRetryCount = maxRetryCount;
                Expires = expier;
                Request = request;
                RetryCount = 0;
            }
        }
    }
}
