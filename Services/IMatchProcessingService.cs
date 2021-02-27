using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Match;
using AOEMatchDataProvider.Models.MatchProcessingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IMatchProcessingService : IDisposable
    {
        ILogService LogService { get; }
        IStorageService StorageService { get; }
        IQueryCacheService QueryCacheService { get; }
        IDataService UserRankProcessingService { get; }

        string LastPulledMatchId { get; }
        bool IsLastPulledMatchInProgress { get; }
        int InRowProcessingFails { get; }

        Match CurrentMatch { get; }

        Task<MatchUpdateStatus> TryUpdateCurrentMatch();
    }
}
