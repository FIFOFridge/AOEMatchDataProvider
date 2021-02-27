using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Match;
using AOEMatchDataProvider.Models.RequestService;
using AOEMatchDataProvider.Models.User;
using AOEMatchDataProvider.Models.UserRankService;
using Newtonsoft.Json;

namespace AOEMatchDataProvider.Services.Default
{
    public sealed class DataService : IDataService, IUserDataProcessingService, IDisposable
    {
        Dictionary<string, RequestState> requests;

        ILogService LogService { get; }
        IStorageService StorageService { get; }
        IQueryCacheService QueryCacheService { get; }
        IRequestService RequestService { get; }

        bool isCacheLoaded;

        public DataService(ILogService logService, IStorageService storageService, IQueryCacheService queryCacheService, IRequestService requestService)
        {
            requests = new Dictionary<string, RequestState>();
            //userRanksCache = new Dictionary<string, UserRankData>();
            LogService = logService;
            QueryCacheService = queryCacheService;
            StorageService = storageService;
            RequestService = requestService;

            isCacheLoaded = false;
        }

        void ValidateCacheState()
        {
            if (!isCacheLoaded)
            {
                QueryCacheService.Load();
                isCacheLoaded = true;
            }
        }

        #region IDataService implementation
        public async Task<RequestWrapper<Match>> GetUserMatch(UserId userId, int timeout)
        {
            ValidateCacheState();

            RequestWrapper<Match> requestWrapper = new RequestWrapper<Match>();
            RequestState requestState = null;

            Dictionary<string, object> logProperties = new Dictionary<string, object>
            {
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
                var requestResult = requestWrapper.RequestResponseWrapper = await RequestService.GetAsync(finallQuery, cts.Token);
                var responseRaw = requestResult.ResponseContent;

                if (!requestResult.IsSuccess)
                    throw new AggregateException("Request failed", requestResult.Exception);

                requestWrapper.Value = (this as IUserDataProcessingService).ProcessMatch(responseRaw, stringResources);
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

                string responseContent = "";
                string responseCode = "";

                if (requestWrapper != null) //to make sure this will not conflict with test cases
                {
                    if (requestWrapper.RequestResponseWrapper != null)
                    {
                        if (requestWrapper.RequestResponseWrapper.ResponseContent != null)
                        {
                            responseContent = requestWrapper.RequestResponseWrapper.ResponseContent;
                        }

                        if (requestWrapper.RequestResponseWrapper.Response != null) //to make sure this will not conflict with test cases
                        {
                            responseCode = requestWrapper.RequestResponseWrapper.Response.StatusCode.ToString();
                        }
                    }
                }

                logProperties.Add("stack", e.StackTrace);
                logProperties.Add("response-code", responseCode);
                logProperties.Add("response-raw", responseContent);
                LogService.Error($"Error while requesting user match: {e.ToString()}", logProperties);

                requestWrapper.Exception = e;
            }
            finally
            {
                if (requestState != null)
                    requestState.isRunning = false; //prevent cancelling operation by timeout handler
            }

            return requestWrapper;
        }

        public async Task<RequestWrapper<UserRank>> GetUserRankFromLadder(UserGameProfileId userGameProfileId, Ladders ladder, int timeout)
        {
            ValidateCacheState();

            RequestWrapper<UserRank> requestWrapper = new RequestWrapper<UserRank>();
            Dictionary<string, object> logProperties = new Dictionary<string, object>
            {
                { "timeout", timeout },
                { "game-profile-id", userGameProfileId.ProfileId },
                { "ladder", ladder.ToString() }
            };

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
                query["leaderboard_id"] = HttpUtility.UrlEncode(((int)ladder).ToString());
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
                requestWrapper.RequestResponseWrapper = await QueryCacheService.GetOrUpdate(
                    finallQuery,
                    cts.Token,
                    DateTime.UtcNow.AddHours(1.5)
                    );

                if (!requestWrapper.RequestResponseWrapper.IsSuccess)
                    throw new AggregateException("Request failed", requestWrapper.RequestResponseWrapper.Exception);

                requestWrapper.Value = (this as IUserDataProcessingService).ProcessUserRankFromLadder(requestWrapper.RequestResponseWrapper.ResponseContent, ladder, stringResources);
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

                string responseContent = "";
                string responseCode = "";

                if (requestWrapper != null) //to make sure this will not conflict with test cases
                {
                    if (requestWrapper.RequestResponseWrapper != null)
                    {
                        if (requestWrapper.RequestResponseWrapper.ResponseContent != null)
                        {
                            responseContent = requestWrapper.RequestResponseWrapper.ResponseContent;
                        }

                        if (requestWrapper.RequestResponseWrapper.Response != null) //to make sure this will not conflict with test cases
                        {
                            responseCode = requestWrapper.RequestResponseWrapper.Response.StatusCode.ToString();
                        }
                    }
                }

                logProperties.Add("stack", e.StackTrace);
                logProperties.Add("response-code", responseCode);
                logProperties.Add("response-raw", responseContent);
                LogService.Error($"Error while requesting ladder: {e.ToString()}", logProperties);

                requestWrapper.Exception = e;
            }
            finally
            {
                if (requestState != null)
                    requestState.isRunning = false; //prevent cancelling operation by timeout handler
            }

            return requestWrapper;
        }

        public async Task<RequestWrapper<UserLadderData>> GetUserDataFromLadder(UserGameProfileId userGameProfileId, Ladders ladder, int timeout)
        {
            ValidateCacheState();

            RequestWrapper<UserLadderData> requestWrapper = new RequestWrapper<UserLadderData>();
            Dictionary<string, object> logProperties = new Dictionary<string, object>
            {
                { "timeout", timeout },
                { "game-profile-id", userGameProfileId.ProfileId },
                { "ladder", ladder.ToString() }
            };

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
                query["leaderboard_id"] = HttpUtility.UrlEncode(((int)ladder).ToString());
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
                requestWrapper.RequestResponseWrapper = await QueryCacheService.GetOrUpdate(
                    finallQuery,
                    cts.Token,
                    DateTime.UtcNow.AddHours(1.5)
                    );

                if (!requestWrapper.RequestResponseWrapper.IsSuccess)
                    throw new AggregateException("Request failed", requestWrapper.RequestResponseWrapper.Exception);

                requestWrapper.Value = (this as IUserDataProcessingService).ProcessUserDataFromLadder(requestWrapper.RequestResponseWrapper.ResponseContent, stringResources);
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

                string responseContent = "";
                string responseCode = "";

                if (requestWrapper != null) //to make sure this will not conflict with test cases
                {
                    if (requestWrapper.RequestResponseWrapper != null)
                    {
                        if (requestWrapper.RequestResponseWrapper.ResponseContent != null)
                        {
                            responseContent = requestWrapper.RequestResponseWrapper.ResponseContent;
                        }

                        if (requestWrapper.RequestResponseWrapper.Response != null) //to make sure this will not conflict with test cases
                        {
                            responseCode = requestWrapper.RequestResponseWrapper.Response.StatusCode.ToString();
                        }
                    }
                }

                logProperties.Add("stack", e.StackTrace);
                logProperties.Add("response-code", responseCode);
                logProperties.Add("response-raw", responseContent);
                LogService.Error($"Error while requesting ladder: {e.ToString()}", logProperties);

                requestWrapper.Exception = e;
            }
            finally
            {
                if (requestState != null)
                    requestState.isRunning = false; //prevent cancelling operation by timeout handler
            }

            return requestWrapper;
        }

        Match IUserDataProcessingService.ProcessMatch(string jsonResponse, AoeNetAPIStringResources apiStringResources)
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
                userMatchData.UserRankData = new UserData();
                userMatchData.MatchType = (MatchType)matchResponse.last_match.leaderboard_id;

                //assign ratings if aviable
                if (player.rating != null)
                {
                    var ratingType = (Ladders)matchResponse.last_match.leaderboard_id;

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

        UserRank IUserDataProcessingService.ProcessUserRankFromLadder(string jsonResponse, Ladders ladder, AoeNetAPIStringResources apiStringResources)
        {
            var ratingResponse = JsonConvert.DeserializeObject<UserLadderDataResponse>(jsonResponse);

            //response to UserRank model
            var userRank = new UserRank
            {
                //we always want to use .First() becouse api don't allow for multiple requests between multiple leadboards
                Elo = ratingResponse.Leaderboard.First().Rating,
                Ladder = ratingResponse.Leaderboard.First().Rank
            };

            var userRankData = new UserData();

            userRankData.UserRatings.Add(ladder, userRank);

            return userRank;
        }

        UserLadderData IUserDataProcessingService.ProcessUserDataFromLadder(string jsonResponse, AoeNetAPIStringResources apiStringResources)
        {
            var leadbord = JsonConvert.DeserializeObject<UserLadderDataResponse>(jsonResponse);//.Leaderboard.First();
            var ladderData = leadbord.Leaderboard.First();

            return new UserLadderData()
            {
                Country = ladderData.Country,
                PreviousRating = ladderData.PreviousRating,
                HighestRating = ladderData.HighestRating,
                Streak = ladderData.Streak,
                LowestStreak = ladderData.LowestStreak,
                HighestStreak = ladderData.HighestStreak,
                Games = ladderData.Games,
                Wins = ladderData.Wins,
                Losses = ladderData.Losses,
                Drops = ladderData.Drops
            };
        }

        public async Task<RequestWrapper<AoeNetAPIStringResources>> GetStringResources()
        {
            const string request = @"https://aoe2.net/api/strings?game=aoe2de&language=en";
            RequestWrapper<AoeNetAPIStringResources> requestWrapper = new RequestWrapper<AoeNetAPIStringResources>();

            try
            {
                var response = await RequestService.GetAsync(request, CancellationToken.None);

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
