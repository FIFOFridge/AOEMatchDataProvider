using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Request
{
    public static class RequestWrapperErrorDescriptor
    {
        public static string GetRequestWrapperExceptionDescription<T>(RequestWrapper<T> requestWrapper) where T : class
        {
            if (requestWrapper.RequestResponseWrapper != null)
            {
                if (requestWrapper.RequestResponseWrapper.Exception != null)
                {
                    var exception = requestWrapper.RequestResponseWrapper.Exception;

                    if (exception.GetType() == typeof(OperationCanceledException))
                    {
                        return "Operation timed out.";
                    }

                    if (exception.GetType() == typeof(HttpRequestException))
                    {
                        //todo: check it out
                        return "Http exception: " + exception.Message;
                    }
                }
            }

            if (requestWrapper.Exception != null)
            {
                if (requestWrapper.Exception.GetType() == typeof(JsonSerializationException))
                {
                    return "Data processing error.";
                }
            }

            return "Unknown error occured";
        }
    }
}
