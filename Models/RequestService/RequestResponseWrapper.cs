using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.RequestService
{
    public class RequestResponseWrapper : ISerializableModel
    {
        [JsonIgnore]
        public HttpResponseMessage Response { get; internal set; }
        public bool IsSuccess { get; internal set; }
        public Exception Exception { get; internal set; }
        public string ResponseContent { get; internal set; }

        // make sure it will be only constructed in RequestHelper
        internal RequestResponseWrapper()
        {

        }

        [JsonConstructor]
        internal RequestResponseWrapper(/*HttpResponseMessage Response, */bool IsSuccess, Exception Exception, string ResponseContent)
        {
            //this.Response = Response;
            this.IsSuccess = IsSuccess;
            this.Exception = Exception;
            this.ResponseContent = ResponseContent;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<RequestResponseWrapper>(serialized);
        }
    }
}
