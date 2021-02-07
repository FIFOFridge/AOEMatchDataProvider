#define TEST

using AOEMatchDataProvider.Other;
using AOEMatchDataProvider.Services;
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

        static IAppConfigurationService AppConfigurationService { get; set; }

        static bool IsInitialized => AppConfigurationService != null;

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

        public static void InitializeRequestHelper(IAppConfigurationService appConfigurationService/*, TimeSpan maxTimeout*/)
        {
            if (AppConfigurationService != null)
                throw new InvalidOperationException("Request helper is already initialized");

            //HttpClient.Timeout = maxTimeout;
            AppConfigurationService = appConfigurationService;
        }

        public static async Task<RequestResponseWrapper> GetAsync(string query, CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("RequestHelper have to be initialized");

            RequestResponseWrapper responseWrapper = new RequestResponseWrapper();

            //set default timeout for request
            if(cancellationToken == CancellationToken.None)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(AppConfigurationService.DefaultRequestTimeout);
            }

            try
            {
                HttpResponseMessage response = await Instance.HttpClient.GetAsync(query, cancellationToken);
                responseWrapper.ResponseContent = await response.Content.ReadAsStringAsync();

                responseWrapper.Response = response;
                responseWrapper.IsSuccess = response.IsSuccessStatusCode;
            }
            //handle time out/cancellation exception (OperationCanceledException) and request exception
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

            // make sure it will be only constructed in RequestHelper
            internal RequestResponseWrapper()
            {

            }
        }
    }
}
