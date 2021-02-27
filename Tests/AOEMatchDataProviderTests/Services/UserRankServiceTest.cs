using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Match;
using AOEMatchDataProvider.Models.User;
using AOEMatchDataProvider.Models.UserRankService;
using AOEMatchDataProvider.Services;
using AOEMatchDataProviderTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services
{
    [TestClass]
    [TestCategory("Service")]
    [TestCategory("IUserRankService")]
    public sealed class UserRankServiceTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            ServiceResolver.SetupServicesForIUserRankDataProcessingService(TestContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceResolver.DisposeServices();
        }

        //Check match details in RandomMap configuration (1vs1, leaderboard_id = 3 / UserRankMode.RandomMap)
        //WITH user ELO provided in match 
        //(user elo from match is valid only for specified match category "leadboard_id")
        [TestMethod]
        public void UsersDataProcessingFromMatchData()
        {
            var responseRaw = TestingResourcesHelper.GetApiResoponses("MatchDetailsValid.json");
            var processingService = ServiceResolver.GetService<IUserDataProcessingService>();

            var stringResources = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(TestingResourcesHelper.GetApiResoponses("ApiStringResources.json"));

            Match match = processingService.ProcessMatch(responseRaw, stringResources);

            Assert.AreEqual(match.Users.Count, 2, "Users in match count is incorrect");

            Assert.AreEqual(
                match.Users.First() //matchDetailsValid.json --> players[0]
                    .UserRankData.UserRatings[Ladders.RandomMap].Elo,
                2255,
                "Player[0] ELO from match is incorrect");

            Assert.AreEqual(
                match.Users.Last() //matchDetailsValid.json --> players[1]
                    .UserRankData.UserRatings[Ladders.RandomMap].Elo,
                2292,
                "Player[1] ELO from match is incorrect");

            //make sure other fields also was processed as expected

            //test both players profile IDs
            Assert.IsNotNull(match.Users.First().UserGameProfileId, "Player[0] profile id wasn't processed");
            Assert.IsNotNull(match.Users.Last().UserGameProfileId, "Player[1] profile id wasn't processed");

            //test both players names
            Assert.IsNotNull(match.Users.First().Name, "Player[0] profile name wasn't processed");
            Assert.IsNotNull(match.Users.Last().Name, "Player[1] profile name wasn't processed");

            //validate color property (have to be set)
            Assert.IsNotNull(match.Users.First().Color, "Player[0] color is wasn't processed");
            Assert.IsNotNull(match.Users.Last().Color, "Player[1] color is wasn't processed");

            //validate user match types and match type
            Assert.AreEqual(match.MatchType, MatchType.RandomMap);
            Assert.AreEqual(match.Users.First().MatchType, MatchType.RandomMap);
            Assert.AreEqual(match.Users.Last().MatchType, MatchType.RandomMap);
        }

        //Check match details in RandomMap configuration (1vs1, leaderboard_id = 3 / UserRankMode.RandomMap)
        //WITHOUT user ELO provided in match 
        //(user elo from match is valid only for specified match category "leadboard_id")
        [TestMethod]
        public void IncompleteUsersDataProcessingFromMatchData()
        {
            var responseRaw = TestingResourcesHelper.GetApiResoponses("MatchDetailsValid.json");
            var processingService = ServiceResolver.GetService<IUserDataProcessingService>();

            var stringResources = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(TestingResourcesHelper.GetApiResoponses("ApiStringResources.json"));

            Match match = processingService.ProcessMatch(responseRaw, stringResources);

            Assert.AreEqual(match.Users.Count, 2, "Users in match count is incorrect");

            //make sure other fields was processed as expected

            //test both players profile IDs
            Assert.IsNotNull(match.Users.First().UserGameProfileId, "Player[0] profile id wasn't processed");
            Assert.IsNotNull(match.Users.Last().UserGameProfileId, "Player[1] profile id wasn't processed");

            //test both players names
            Assert.IsNotNull(match.Users.First().Name, "Player[0] name wasn't processed");
            Assert.IsNotNull(match.Users.Last().Name, "Player[1] name wasn't processed");

            //validate color property (have to be set)
            Assert.IsNotNull(match.Users.First().Color, "Player[0] color is wasn't processed");
            Assert.IsNotNull(match.Users.Last().Color, "Player[1] color is wasn't processed");

            //validate user match types and match type
            Assert.AreEqual(match.MatchType, MatchType.RandomMap);
            Assert.AreEqual(match.Users.First().MatchType, MatchType.RandomMap);
            Assert.AreEqual(match.Users.Last().MatchType, MatchType.RandomMap);
        }

        [TestMethod]
        public void UserDataByLadderRequest()
        {
            var processingService = ServiceResolver.GetService<IUserDataProcessingService>();

            var stringResources = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(TestingResourcesHelper.GetApiResoponses("ApiStringResources.json"));

            var ladderRandomMap = processingService.ProcessUserRankFromLadder(
                    TestingResourcesHelper.GetApiResoponses("LeadboardRandomMapValid.json"),
                    Ladders.RandomMap,
                    stringResources
                );

            var ladderTeamRandomMap = processingService.ProcessUserRankFromLadder(
                    TestingResourcesHelper.GetApiResoponses("LeadbordTeamRandomMapValid.json"),
                    Ladders.TeamRandomMap,
                    stringResources
                );

            Assert.AreEqual(ladderRandomMap.Elo, 2279, "Rating should be same as declarated with response");
            Assert.AreEqual(ladderTeamRandomMap.Elo, 2432, "Rating should be same as declarated with response");
        }

        //Check match state processing (1vs1, leaderboard_id = 3 / UserRankMode.RandomMap)
        [TestMethod]
        public void MatchState()
        {
            var processingService = ServiceResolver.GetService<IUserDataProcessingService>();

            var stringResources = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(TestingResourcesHelper.GetApiResoponses("ApiStringResources.json"));

            Match matchFinished = processingService.ProcessMatch(TestingResourcesHelper.GetApiResoponses("MatchFinished.json"), stringResources);
            Match matchInProgress = processingService.ProcessMatch(TestingResourcesHelper.GetApiResoponses("MatchInProgress.json"), stringResources);

            //todo: rewrite assertion fail description messages to better describe testing properties roles
            //check is match finished
            Assert.IsNotNull(matchFinished.Finished, "Match state wasn't processed correctly");
            Assert.IsNull(matchInProgress.Finished, "Match state wasn't processed correctly"); //have to be null

            //same as above but using property
            Assert.IsTrue(matchInProgress.IsInProgress, "Match state wasn't processed correctly");
            Assert.IsFalse(matchFinished.IsInProgress, "Match state wasn't processed correctly");
        }
    }
}
