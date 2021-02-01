using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.UserRankService;
using Newtonsoft.Json;
using static AOEMatchDataProvider.Helpers.Request.RequestHelper;

namespace AOEMatchDataProvider.Services.Default
{
    public sealed class UserRankService : IUserRankService, IUserRankDataProcessingService, IDisposable
    {
        Dictionary<string, RequestState> requests;

        ILogService LogService { get; }
        IStorageService StorageService { get; }
        IQueryCacheService QueryCacheService { get; }

        bool isCacheLoaded;

        public UserRankService(ILogService logService, IStorageService storageService, IQueryCacheService queryCacheService)
        {
            requests = new Dictionary<string, RequestState>();
            //userRanksCache = new Dictionary<string, UserRankData>();
            LogService = logService;
            QueryCacheService = queryCacheService;
            StorageService = storageService;

            isCacheLoaded = false;

            LogService.Info("created instance of: UserRankService");
        }

        void ValidateCacheState()
        {
            if(!isCacheLoaded)
            {
                QueryCacheService.Load();
                isCacheLoaded = true;
            }
        }

        #region IUserRankService implementation
        public async Task<RequestWrapper<Match>> GetUserMatch(UserId userId, int timeout)
        {
            ValidateCacheState();

            RequestWrapper<Match> requestWrapper = new RequestWrapper<Match>();
            RequestState requestState = null;

            Dictionary<string, object> logProperties = new Dictionary<string, object>
            {
                { "userId", userId },
                { "timeout", timeout }
            };

            try
            {
                if (!StorageService.Has("stringResources"))
                    throw new InvalidOperationException("App resources unaviable while sending request");

                if (userId.GameProfileId == null && userId.SteamId == null)
                    throw new InvalidOperationException("Missing user id");

                var stringResources = StorageService.Get<AoeNetAPIStringResources>("stringResources");

                //preapare query
                var query = HttpUtility.ParseQueryString(string.Empty);

                if (userId.SteamId != null) //use steam 64 id
                    query["steam_id"] = HttpUtility.UrlEncode(userId.SteamId.ToString());
                else //use game profile id
                    query["profile_id"] = HttpUtility.UrlEncode(userId.GameProfileId.ToString());

                query["game"] = HttpUtility.UrlEncode("aoe2de"); //only aviable for DE at this moment, https://aoe2.net/#api

                var finallQuery = "https://aoe2.net/api/player/lastmatch?" + query.ToString();
                logProperties.Add("query", finallQuery);

                //setup timeout for current request
                var cts = new CancellationTokenSource();
                requestState = AddTimeoutHandler(cts, timeout);

                requestState.isRunning = true;
                //sumbit query
                requestWrapper.RequestUrl = finallQuery;
                var requestResult = requestWrapper.RequestResponseWrapper = await RequestHelper.SubmitRequest(finallQuery, cts.Token);

                var responseRaw = requestResult.ResponseContent;

                requestWrapper.Value = (this as IUserRankDataProcessingService).ProcessMatch(responseRaw, stringResources);
            }
            catch(Exception e)
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                {
                    throw e;
                }

                logProperties.Add("stack", e.StackTrace);
                LogService.Error("Error while requesting user match", logProperties);

                requestWrapper.Exception = e;
            }
            finally
            {
                if (requestState != null)
                    requestState.isRunning = false; //prevent cancelling operation by timeout handler
            }

            return requestWrapper;
        }

        public async Task<RequestWrapper<UserRank>> GetUserRank(UserGameProfileId userGameProfileId, UserRankMode rankMode, int timeout)
        {
            ValidateCacheState();

            RequestWrapper<UserRank> requestWrapper = new RequestWrapper<UserRank>();
            Dictionary<string, object> logProperties = new Dictionary<string, object>();
            logProperties.Add("userGameProfileId", userGameProfileId);
            logProperties.Add("rankMode", rankMode.ToString());
            logProperties.Add("timeout", timeout);

            //queryCacheService.Load();

            RequestResponseWrapper requestResult = null;
            RequestState requestState = null;

            try
            {
                if (!StorageService.Has("stringResources"))
                    throw new InvalidOperationException("App resources unaviable while sending request");

                if (string.IsNullOrEmpty(userGameProfileId.ProfileId))
                    throw new InvalidOperationException("Missing user id");

                var stringResources = StorageService.Get<AoeNetAPIStringResources>("stringResources");

                //preapare query
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["profile_id"] = HttpUtility.UrlEncode(userGameProfileId.ProfileId);
                query["leaderboard_id"] = HttpUtility.UrlEncode(((int)rankMode).ToString());
                query["game"] = HttpUtility.UrlEncode("aoe2de"); //only aviable for DE at this moment, https://aoe2.net/#api

                var finallQuery = "https://aoe2.net/api/leaderboard?" + query.ToString();
                logProperties.Add("query", finallQuery);

                //create cts
                var cts = new CancellationTokenSource();
                //create timeout handler for current query
                requestState = AddTimeoutHandler(cts, timeout);

                requestState.isRunning = true;
                requestWrapper.RequestUrl = finallQuery;
                //sumbit query via query cache service
                requestWrapper.RequestResponseWrapper = requestResult = await QueryCacheService.GetOrUpdate(
                    finallQuery,
                    cts.Token,
                    DateTime.UtcNow.AddHours(1.5)
                    );

                requestWrapper.Value = (this as IUserRankDataProcessingService).ProcessUserRank(requestResult.ResponseContent, rankMode, stringResources);
            }
            catch (Exception e) // request timedout
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                {
                    throw e;
                }

                //logProperties.Add("stack", e.StackTrace);
                //LogService.Error($"Error while requesting user rating: {e.Message}", logProperties);

                requestWrapper.Exception = e;
            }
            finally
            {
                if (requestState != null)
                    requestState.isRunning = false; //prevent cancelling operation by timeout handler
            }

            return requestWrapper;
        }

        Match IUserRankDataProcessingService.ProcessMatch(string jsonResponse, AoeNetAPIStringResources apiStringResources)
        {
            var matchResponse = JsonConvert.DeserializeObject<MatchResponse>(
                jsonResponse
            );

            var match = new Match
            {
                //todo: handle opened as separate state independent then start
                Opened = matchResponse.last_match.started,

                Finished = matchResponse.last_match.finished,
                MatchId = matchResponse.last_match.match_id,

                Users = new List<UserMatchData>(),
                MatchType = (MatchType)matchResponse.last_match.leaderboard_id
            };

            //create user match data models from response model
            foreach (var player in matchResponse.last_match.players)
            {
                //assign basic data
                UserMatchData userMatchData = new UserMatchData
                {
                    Name = player.name,


                    Team = player.team,
                    Color = ((UserColors)player.color).ToString()
                };

                //for compatibility with: MatchMinimalResponse set, cause API is returning null civs sometimes
                string civilization = "";

                try
                {
                    civilization = apiStringResources.Civ //find matching civilization from API resources
                                                .First(id => id.Id == player.civ)
                                                .String;
                }
                catch (Exception e)
                {
                    if (
                        e is StackOverflowException ||
                        e is ThreadAbortException ||
                        e is AccessViolationException
                        )
                    {
                        throw e;
                    }

                    civilization = "Unknown uivilization";
                }
                finally
                {
                    userMatchData.Civilization = civilization;
                }

                //assign game profile id
                var userGameProfileId = new UserGameProfileId
                {
                    ProfileId = player.profile_id.ToString()
                };
                userMatchData.UserGameProfileId = userGameProfileId;
                userMatchData.UserRankData = new UserRankData();
                userMatchData.MatchType = (MatchType)matchResponse.last_match.leaderboard_id;

                //assign ratings if aviable
                if (player.rating != null)
                {
                    var ratingType = (UserRankMode)matchResponse.last_match.leaderboard_id;

                    var userRank = new UserRank
                    {
                        Elo = player.rating.Value,
                        Ladder = null //we have to send separate request to check ladder
                    };

                    userMatchData.UserRankData.UserRatings.Add(ratingType, userRank);
                }

                match.Users.Add(userMatchData);
            }

            return match;
        }

        UserRank IUserRankDataProcessingService.ProcessUserRank(string jsonResponse, UserRankMode rankMode, AoeNetAPIStringResources apiStringResources)
        {
            var ratingResponse = JsonConvert.DeserializeObject<UserRatingResponse>(jsonResponse);

            //response to UserRank model
            var userRank = new UserRank
            {
                //we always want to use .First() becouse api don't allow for multiple requests between multiple leadboards
                Elo = ratingResponse.Leaderboard.First().Rating,
                Ladder = ratingResponse.Leaderboard.First().Rank
            };

            var userRankData = new UserRankData();

            userRankData.UserRatings.Add(rankMode, userRank);

            return userRank;
        }

        public async Task<RequestWrapper<AoeNetAPIStringResources>> GetStringResources()
        {
            const string request = @"https://aoe2.net/api/strings?game=aoe2de&language=en";
            RequestWrapper<AoeNetAPIStringResources> requestWrapper = new RequestWrapper<AoeNetAPIStringResources>();

            try
            {
                var response = await RequestHelper.SubmitRequest(request, CancellationToken.None, 10);
                
                requestWrapper.RequestResponseWrapper = response;

                if (!response.IsSuccess)
                {
                    requestWrapper.Exception = response.Exception;
                }

                requestWrapper.Value = JsonConvert.DeserializeObject<AoeNetAPIStringResources>(response.ResponseContent);
            }
            catch (Exception e) // request timedout
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                {
                    throw e;
                }

                requestWrapper.Exception = e;
            } 

            return requestWrapper;
        }
        #endregion IUserRankService implementation

        //#region Request/Data processing
        //UserRankData ProcessUserRating(UserRatingResponse userRatingResponse)
        //{
        //    var userRankData = new UserRankData();

        //    //the api don't allow for collecting more then one user per request (when specifing userId)
        //    //so most propably that's some request error
        //    if (userRatingResponse.Leaderboard.Count > 1)
        //        throw new InvalidOperationException("Invalid user data");

        //    var ratingType = (UserRankMode)userRatingResponse.LeaderboardId;
        //    var user = userRatingResponse.Leaderboard.First();

        //    var userRank = new UserRank
        //    {
        //        Elo = user.Rating,
        //        Ladder = user.Rank
        //    };

        //    userRankData.UserRatings.Add(ratingType, userRank);

        //    return userRankData;
        //}
        //#endregion

        RequestState AddTimeoutHandler(CancellationTokenSource cancellationTokenSource, int timeout)
        {
            RequestState requestState = new RequestState
            {
                token = GetToken(),
                cancellationTokenSource = cancellationTokenSource,
                isRunning = false
            };

            Timer timer = new Timer((state) =>
            {
                var rs = state as RequestState;

                if (rs.isRunning) //if still runing then timeout reached
                {
                    rs.cancellationTokenSource.Cancel();
                }

                rs.timer.Dispose(); //dispose timer

            }, requestState, timeout, Timeout.Infinite);

            requestState.timer = timer;

            requests.Add(requestState.token, requestState);

            return requestState;
        }

        #region IDisposable Support
        internal bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var kvp in requests) //dispose timers
            {
                kvp.Value.timer.Dispose();

                //cancell all running tasks while disposing
                if (kvp.Value.isRunning)
                {
                    kvp.Value.cancellationTokenSource.Cancel();
                }
            }

            requests = null;
            disposed = true;
        }
        #endregion

        internal class RequestState
        {
            internal string token;
            internal bool isRunning;
            internal CancellationTokenSource cancellationTokenSource;
            internal Timer timer;
        }

        private Random random = new Random();
        public string GetToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var token = new string(Enumerable.Repeat(chars, 20)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            if (requests.ContainsKey(token))
                return GetToken();

            return token;
        }
    }
}
