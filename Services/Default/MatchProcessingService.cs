using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.MatchProcessingService;
using AOEMatchDataProvider.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public class MatchProcessingService : IMatchProcessingService, IDisposable
    {
        private bool disposedValue;

        public IAppConfigurationService AppConfigurationService { get; }
        public ILogService LogService { get; }
        public IStorageService StorageService { get; }
        public IQueryCacheService QueryCacheService { get; }
        public IUserRankService UserRankProcessingService { get; }

        public string LastPulledMatchId { get; protected set; }
        public bool IsLastPulledMatchInProgress { get; protected set; }
        public int InRowProcessingFails { get; protected set; }

        public Match CurrentMatch { get; protected set; }

        public MatchProcessingService(ILogService logService, IStorageService storageService, IQueryCacheService queryCacheService, IUserRankService userRankProcessingService, IAppConfigurationService appConfigurationService)
        {
            #region Assign services
            AppConfigurationService = appConfigurationService;
            LogService = logService;
            StorageService = storageService;
            QueryCacheService = queryCacheService;
            UserRankProcessingService = userRankProcessingService;
            #endregion
        }

        public virtual async Task<MatchUpdateStatus> TryUpdateCurrentMatch()
        {
            CurrentMatch = null;

            LogService.Debug("Updating current match status...");

            var user = StorageService.Get<AppSettings>("settings");
            var response = await UserRankProcessingService.GetUserMatch(user.UserId, AppConfigurationService.MatchUpdateTimeout);

            //handle update failed cases
            if(!response.IsSuccess)
            {
                if(response.Exception is AggregateException)
                {
                    if (
                        response.RequestResponseWrapper.Exception is HttpRequestException ||
                        response.RequestResponseWrapper.Exception is OperationCanceledException
                        )
                    {
                        InRowProcessingFails++;
                        return MatchUpdateStatus.ConnectionError;
                    }

                }

                if (response.Exception is SerializationException)
                {
                    InRowProcessingFails++;
                    return MatchUpdateStatus.ProcessingError;
                }

                InRowProcessingFails++;
                return MatchUpdateStatus.UnknownError;
            }

            //update match data
            CurrentMatch = response.Value;
            LastPulledMatchId = response.Value.MatchId;
            IsLastPulledMatchInProgress = response.Value.IsInProgress;

            if (!response.Value.IsInProgress)
            {
                InRowProcessingFails++;
                return MatchUpdateStatus.MatchEnded;
            }

            //validate match type
            switch(response.Value.MatchType)
            {
                case MatchType.RandomMap:
                case MatchType.TeamRandomMap:
                case MatchType.Deathmatch:
                case MatchType.TeamdeathMatch:
                    InRowProcessingFails = 0;
                    return MatchUpdateStatus.SupportedMatchType;

                default:
                    InRowProcessingFails++;
                    return MatchUpdateStatus.UnsupportedMatchType;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
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
