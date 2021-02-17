using AOEMatchDataProvider.Models.RequestService;
using AOEMatchDataProvider.Services;
using AOEMatchDataProviderTests.Helpers;
using AOEMatchDataProviderTests.Services.Test;
using AOEMatchDataProvider.Models.MatchProcessingService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AOEMatchDataProvider.Models;

namespace AOEMatchDataProviderTests.Services
{
    [TestClass]
    [TestCategory("Service")]
    [TestCategory("IMatchProcessingService")]
    public class MatchProcessingServiceTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            ServiceResolver.SetupServicesForIMatchProcessingService(TestContext);

            var storageService = ServiceResolver.GetService<IStorageService>();

            //setup app resources
            if (!storageService.Has("stringResources"))
            {
                var stringResources = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(TestingResourcesHelper.GetApiResoponses("ApiStringResources.json"));
                storageService.Create("stringResources", stringResources, StorageEntryExpirePolicy.AfterSession);
            }

            if (!storageService.Has("settings"))
            {
                storageService.Create("settings", TestingResourcesHelper.GetTestingSettings(), StorageEntryExpirePolicy.AfterSession);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceResolver.DisposeServices();
        }

        [TestMethod]
        public async Task TestMatchProcessingValidMatch()
        {
            var requestTestingService = ServiceResolver.GetService<IRequestService>() as RequestTestingService;
            var matchService = ServiceResolver.GetService<IMatchProcessingService>();

            //return match in progress response
            requestTestingService.AddTestingRule(
                new System.Text.RegularExpressions.Regex("https://aoe2.net/api/player/lastmatch?"),
                RequestTestingService.RequestTestingServiceMode.ReturnPredefinedResult,
                new RequestResponseWrapper()
                {
                    ResponseContent = TestingResourcesHelper.GetApiResoponses("MatchInProgress.json"),
                    IsSuccess = true
                });

            var matchState = await matchService.TryUpdateCurrentMatch();

            Assert.IsTrue(matchState == MatchUpdateStatus.SupportedMatchType);

            requestTestingService.ClearCustomRules();
        }

        [TestMethod]
        public async Task TestMatchProcessingConnectionError()
        {
            var requestTestingService = ServiceResolver.GetService<IRequestService>() as RequestTestingService;
            var matchService = ServiceResolver.GetService<IMatchProcessingService>();

            //return match in progress
            requestTestingService.AddTestingRule(
                new System.Text.RegularExpressions.Regex("https://aoe2.net/api/player/lastmatch?"),
                RequestTestingService.RequestTestingServiceMode.ThrowHttpError);

            var matchState = await matchService.TryUpdateCurrentMatch();

            Assert.IsTrue(matchState == MatchUpdateStatus.ConnectionError);

            requestTestingService.ClearCustomRules();

            //return match in progress
            requestTestingService.AddTestingRule(
                new System.Text.RegularExpressions.Regex("https://aoe2.net/api/player/lastmatch?"),
                RequestTestingService.RequestTestingServiceMode.ThrowTimedoutError);

            var timedOutMatchState = await matchService.TryUpdateCurrentMatch();

            Assert.IsTrue(timedOutMatchState == MatchUpdateStatus.ConnectionError);

            requestTestingService.ClearCustomRules();
        }

        [TestMethod]
        public async Task TestMatchProcessingMatchEnded()
        {
            var requestTestingService = ServiceResolver.GetService<IRequestService>() as RequestTestingService;
            var matchService = ServiceResolver.GetService<IMatchProcessingService>();

            //return match in progress
            requestTestingService.AddTestingRule(
                new System.Text.RegularExpressions.Regex("https://aoe2.net/api/player/lastmatch?"),
                RequestTestingService.RequestTestingServiceMode.ReturnPredefinedResult,
                new RequestResponseWrapper()
                {
                    ResponseContent = TestingResourcesHelper.GetApiResoponses("MatchFinished.json"),
                    IsSuccess = true
                });

            var matchState = await matchService.TryUpdateCurrentMatch();

            Assert.IsTrue(matchState == MatchUpdateStatus.MatchEnded);

            requestTestingService.ClearCustomRules();
        }

        //[TestMethod]
        //public async Task TestMatchProcessingUnsupported()
        //{
        //    var requestTestingService = ServiceResolver.GetService<IRequestService>() as RequestTestingService;
        //    var matchService = ServiceResolver.GetService<IMatchProcessingService>();

        //    //return match in progress
        //    requestTestingService.AddTestingRule(
        //        new System.Text.RegularExpressions.Regex("https://aoe2.net/api/player/lastmatch?"),
        //        RequestTestingService.RequestTestingServiceMode.ReturnPredefinedResult,
        //        new RequestResponseWrapper()
        //        {
        //            ResponseContent = TestingResourcesHelper.GetApiResoponses("MatchUnsupported.json"),
        //            IsSuccess = true
        //        });

        //    var matchState = await matchService.TryUpdateCurrentMatch();

        //    Assert.IsTrue(matchState == MatchUpdateStatus.UnsupportedMatchType);

        //    requestTestingService.ClearCustomRules();
        //}
    }
}
