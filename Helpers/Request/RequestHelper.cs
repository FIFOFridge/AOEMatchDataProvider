#define TEST

using AOEMatchDataProvider.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Request
{
    public sealed class RequestHelper : ICriticalDisposable
    {
        HttpClient HttpClient { get; }

        static RequestHelper instance;
        public static RequestHelper Instance
        {


            get
            {
                if (instance == null)
                    instance = new RequestHelper();

                return instance;
            }
        }

        private RequestHelper()
        {
            HttpClient = new HttpClient();
        }

        public static async Task<bool> ConnectionCheck(CancellationToken cancellationToken)
        {
            //todo: implement with static httpClient
            return true;
            //using (HttpClient client = new HttpClient())
            //{
            //    client.Timeout = TimeSpan.FromSeconds(10);

            //    try
            //    {
            //        HttpResponseMessage response = await client.GetAsync("https://aoe2.net/", cancellationToken);

            //        if (!(response.IsSuccessStatusCode))
            //            return false;

            //        return true;

            //    }
            //    catch (HttpRequestException hre)
            //    {
            //        return false;
            //    }
            //}
        }

        //https://stackoverflow.com/questions/46874693/re-using-httpclient-but-with-a-different-timeout-setting-per-request
        //public static async Task<ResponseWrapper> SubmitRequest(string query, CancellationToken cancellationToken, int timeout = 10)
        //{

        //    //ResponseWrapper responseWrapper = new ResponseWrapper();
        //    //HttpClient client = new HttpClient();

        //    ////using (HttpClient client = new HttpClient())
        //    ////{
        //    //    client.Timeout = TimeSpan.FromSeconds(timeout);

        //    //    try
        //    //    {
        //    //        HttpResponseMessage response = await client.GetAsync(query, cancellationToken);
        //    //        responseWrapper.ResponseContent = await response.Content.ReadAsStringAsync();

        //    //        responseWrapper.Response = response;
        //    //        responseWrapper.IsSuccess = response.IsSuccessStatusCode;
        //    //    }
        //    //    catch (HttpRequestException hre)
        //    //    {
        //    //        responseWrapper.Exception = hre;
        //    //    }
        //    //    finally
        //    //    {
        //    //        client.Dispose(); //dispose manually to avoid OperationCancelledException throwing after reuest is completed
        //    //    }
        //    ////}

        //    //return responseWrapper;
        //}

        public static async Task<RequestResponseWrapper> SubmitRequest(string query, CancellationToken cancellationToken, int timeout = 10)
        {
            RequestResponseWrapper responseWrapper = new RequestResponseWrapper();

            try
            {
                HttpResponseMessage response = await Instance.HttpClient.GetAsync(query, cancellationToken);
                responseWrapper.ResponseContent = await response.Content.ReadAsStringAsync();

                responseWrapper.Response = response;
                responseWrapper.IsSuccess = response.IsSuccessStatusCode;
            }
            catch (HttpRequestException hre)
            {
                responseWrapper.Exception = hre;
            }
            catch (OperationCanceledException oce)
            {
                responseWrapper.Exception = oce;
            }

            return responseWrapper;
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public class RequestResponseWrapper
        {
            public HttpResponseMessage Response { get; internal set; }
            public bool IsSuccess { get; internal set; }
            public Exception Exception { get; internal set; }
            public string ResponseContent { get; internal set; }

            // make sure it will be only constructed in controller
            internal RequestResponseWrapper()
            {

            }
        }
    }
}
