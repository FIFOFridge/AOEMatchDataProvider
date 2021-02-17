using AOEMatchDataProvider.Models.RequestService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public class RequestService : IRequestService, IDisposable
    {
        protected IAppConfigurationService AppConfigurationService { get; }
        protected HttpClient HttpClient { get; }
        
        private bool disposedValue;

        public RequestService(IAppConfigurationService appConfigurationService)
        {
            AppConfigurationService = appConfigurationService;
            HttpClient = new HttpClient();
        }

        public virtual async Task<RequestResponseWrapper> GetAsync(string query, CancellationToken cancellationToken)
        {
            RequestResponseWrapper responseWrapper = new RequestResponseWrapper();

            //set default timeout for request
            if (cancellationToken == CancellationToken.None)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(AppConfigurationService.DefaultRequestTimeout);
            }

            try 
            {
                HttpResponseMessage response = await HttpClient.GetAsync(query, cancellationToken);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    HttpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
