using AOEMatchDataProvider.Models.RequestService;
using AOEMatchDataProvider.Services;
using AOEMatchDataProviderTests.Helpers;
using AOEMatchDataProviderTests.Services.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services
{
    
    [TestClass]
    [TestCategory("Service")]
    [TestCategory("IQueryCacheService")]
    public class QueryCacheServiceTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            ServiceResolver.SetupServicesForeIQueryCacheService(TestContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceResolver.DisposeServices();
        }

        //TODO: Setup mock server for request cache testing: https://github.com/WireMock-Net/WireMock.Net

        const string productRequest = "https://reqres.in/api/products/3";
        //const int liveAPITimeout = 1000 * 10; replaced by testing request service that handle and return predefined response for specified requests
        const int timeout = 5000;

        [TestMethod]
        public async Task SampleRequestCache()
        {
            TestingResourcesHelper.PreapareTestingDirectory(ServiceResolver.GetService<IAppConfigurationService>());
            var requestTestingService = ServiceResolver.GetService<IRequestService>() as RequestTestingService;

            //return match in progress
            requestTestingService.AddTestingRule(
                new System.Text.RegularExpressions.Regex("https://reqres.in/api/products/3"),
                RequestTestingService.RequestTestingServiceMode.ReturnPredefinedResult,
                new RequestResponseWrapper()
                {
                    ResponseContent = TestingResourcesHelper.GetApiResoponses("Product3.json"),
                    IsSuccess = true
                });

            var queryCacheService = ServiceResolver.GetService<IQueryCacheService>() as AOEMatchDataProvider.Services.Default.QueryCacheService; //cast to specified service to get access for internal props
            queryCacheService.Load();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);

            var requestResponseWrapper = await queryCacheService.GetOrUpdate(productRequest, cancellationTokenSource.Token, DateTime.UtcNow.AddYears(1000));

            if (!requestResponseWrapper.IsSuccess)
                Assert.Fail("Request failed");

            Assert.IsTrue(queryCacheService.Queries.CachedQueries.ContainsKey(productRequest), "Cache should exists");

            var requestResponseWrapperFromCache = await queryCacheService.GetOrUpdate(productRequest, cancellationTokenSource.Token, DateTime.UtcNow.AddYears(1000));

            Assert.AreEqual(requestResponseWrapper, requestResponseWrapperFromCache);
        }

        [TestMethod]
        public async Task CacheLoad()
        {
            TestingResourcesHelper.PreapareTestingDirectory(ServiceResolver.GetService<IAppConfigurationService>());
            TestingResourcesHelper.InjectCachedQueriesData(ServiceResolver.GetService<IAppConfigurationService>());

            var storageService = ServiceResolver.GetService<IStorageService>();
            storageService.Load();

            var queryCacheService = ServiceResolver.GetService<IQueryCacheService>() as 
                                        AOEMatchDataProvider.Services.Default.QueryCacheService; //cast to specified service to get access for internal fields/methods
            queryCacheService.Load();

            Assert.IsTrue(queryCacheService.Queries.CachedQueries.ContainsKey(productRequest), "Cache should exists");

            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);

            //var requestResponseWrapper = await queryCacheService.GetOrUpdate(productRequest, cancellationTokenSource.Token, DateTime.UtcNow.AddDays(1));

            //if (!requestResponseWrapper.IsSuccess)
            //    Assert.Fail("Request failed");

            //Assert.IsTrue(queryCacheService.Queries.CachedQueries.ContainsKey(productRequest), "Cache exists");
        }

        #region Sample API Response https://reqres.in/api/products/3
        //{
        //   "data":{
        //      "id":3,
        //      "name":"true red",
        //      "year":2002,
        //      "color":"#BF1932",
        //      "pantone_value":"19-1664"
        //   },
        //   "support":{
        //      "url":"https://reqres.in/#support-heading",
        //      "text":"To keep ReqRes free, contributions towards server costs are appreciated!"
        //   }
        //}
        #endregion

}

    #region API Model
    public class Data
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("pantone_value")]
        public string PantoneValue { get; set; }
    }

    public class Support
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Product
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("support")]
        public Support Support { get; set; }
    }
    #endregion

}
