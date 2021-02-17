using AOEMatchDataProvider.Models.RequestService;
using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    internal sealed class RequestTestingService : AOEMatchDataProvider.Services.Default.RequestService, IDisposable
    {
        Dictionary<
            Regex,
            Tuple<RequestTestingServiceMode, RequestResponseWrapper>
            > Rules
        { get; } = new Dictionary<Regex, Tuple<RequestTestingServiceMode, RequestResponseWrapper>>();

        internal RequestTestingService(IAppConfigurationService appConfigurationService) : base(appConfigurationService) { }

        public override async Task<RequestResponseWrapper> GetAsync(string query, CancellationToken cancellationToken)
        {
            RequestResponseWrapper responseWrapper = new RequestResponseWrapper();

            foreach (var rule in Rules)
            {
                var test = rule.Key;

                if (test.Match(query).Success)
                {
                    var testingServiceMode = rule.Value.Item1;

                    switch (testingServiceMode)
                    {
                        case RequestTestingServiceMode.AllowConnection:
                            return await base.GetAsync(query, cancellationToken);
                        
                        case RequestTestingServiceMode.ReturnPredefinedResult:
                            return rule.Value.Item2; //predefined custom response
                        
                        case RequestTestingServiceMode.ThrowHttpError:
                            responseWrapper.Exception = new HttpRequestException("Http exception");
                            return responseWrapper;
                        
                        case RequestTestingServiceMode.ThrowTimedoutError:
                            responseWrapper.Exception = new TaskCanceledException("Operation timedout");
                            return responseWrapper;
                        
                        default:
                            throw new InvalidOperationException("Wrong testingServiceMode parameter");
                    }
                }
            }

            throw new InvalidOperationException("Query wasnt matched with any testing rule");
        }

        public void AddTestingRule(
            Regex test,
            RequestTestingServiceMode testingServiceMode,
            RequestResponseWrapper requestResponseWrapper = null) => Rules.Add(test, Tuple.Create(testingServiceMode, requestResponseWrapper));

        public void RemoveTestingRule(Regex test) => Rules.Remove(test);

        public void ClearCustomRules() => Rules.Clear();

        internal enum RequestTestingServiceMode
        {
            AllowConnection,
            ReturnPredefinedResult,
            ThrowHttpError,
            ThrowTimedoutError
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
