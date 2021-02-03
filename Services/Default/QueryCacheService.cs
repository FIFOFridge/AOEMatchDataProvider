using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models.QueryCacheHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AOEMatchDataProvider.Helpers.Request.RequestHelper;
using static AOEMatchDataProvider.Models.QueryCacheHelper.QueriesCache;

namespace AOEMatchDataProvider.Services.Default
{
    internal sealed class QueryCacheService : IQueryCacheService
    {
        internal QueriesCache Queries { get; set; }
        SemaphoreSlim queryThrottler;

        IStorageService StorageService { get; }
        ILogService LogService { get; }

        public QueryCacheService(IStorageService storageService, ILogService logService)
        {
            queryThrottler = new SemaphoreSlim(4);

            StorageService = storageService;
            LogService = logService;
            //LoadLocalyCachedQueries();
        }

        //TODO: make async version
        public void Load()
        {
            //create new instance if not exists in storage
            if (!(StorageService.Has("cachedQueries")))
            {
                LogService.Info("Creating cachedQueries entry");

                Queries = new QueriesCache
                {
                    CachedQueries = new Dictionary<string, QueryCacheEntry>()
                };

                StorageService.Create("cachedQueries", Queries, StorageEntryExpirePolicy.Never);
                StorageService.Flush();
            }
            //load if found
            else
            {
                LogService.Info("Loading cachedQueries entry");

                Queries = StorageService.Get<QueriesCache>("cachedQueries");

                //todo: inspect why queries are null
                //inspection context:
                //- app launched without any storage entries loaded from local drive (clean start)
                //- CachedQueries initialized by (this) service
                //- CachedQueries flushed to disk
                //- app closed without any updates to CachedQueries
                //- app launched
                //- CachedQueries are loaded with "null" value
                if(Queries.CachedQueries == null)
                {
                    LogService.Info("Recreating Queries.CachedQueries");

                    //recreate and flush to make sure CachedQueries are not null
                    Queries = new QueriesCache
                    {
                        CachedQueries = new Dictionary<string, QueryCacheEntry>()
                    };

                    StorageService.Flush();
                }
            }
        }

        public async Task<RequestResponseWrapper> GetOrUpdate(string request, CancellationToken cancellationToken, DateTime expire, int retryCount = 2, int timeout = 10)
        {
            ValidateEntry(request); //remove if expiered

            if (Queries.CachedQueries.ContainsKey(request)) //return cache if contains key
            {
                LogService.Debug($"Returning cached request: {request}");

                var entry = Queries.CachedQueries[request];

                //update if needed
                if(ShouldBeUpdated(request))
                {
                    LogService.Debug($"Updating cached request: {request}");

                    entry.ResponseValue = await ExecuteQuery(request, cancellationToken, timeout);
                    entry.RetryCount++;
                }

                StorageService.Flush();
                return entry.ResponseValue;
            }

            LogService.Debug($"Submiting cachable request: {request}");

            //submit initial query
            var wrapper = await ExecuteQuery(request, cancellationToken, timeout);

            //cache request
            QueryCacheEntry queryCacheEntry = new QueryCacheEntry(wrapper, expire, request, retryCount > 0, retryCount);
            Queries.CachedQueries.Add(request, queryCacheEntry);

            StorageService.Flush();
            return wrapper;
        }

        async Task<RequestResponseWrapper> ExecuteQuery(string request, CancellationToken cancellationToken, int timeout)
        {
            RequestResponseWrapper responseWrapper = null;

            await queryThrottler.WaitAsync(); //limit queries concurrency

            try
            {
                responseWrapper = await RequestHelper.SubmitRequest(request, cancellationToken, timeout);
            }
            finally
            {
                queryThrottler.Release();
            }

            return responseWrapper;
        }

        void ValidateEntry(string key)
        {
            if(Queries.CachedQueries.ContainsKey(key))
            {
                var cacheEntry = Queries.CachedQueries[key];

                //remove if expiered
                if (DateTime.UtcNow > cacheEntry.Expires)
                {
                    Queries.CachedQueries.Remove(key);
                }
            }
        }

        public bool ShouldBeUpdated(string key)
        {
            if (!(Queries.CachedQueries.ContainsKey(key)))
                throw new InvalidOperationException($"Unable to find key (query): {key} in collection");

            var entry = Queries.CachedQueries[key];

            //if not successed and max retry count not reached yet we can try to resend request
            if (!entry.IsSuccessed &&
                (entry.RetryCount < entry.MaxRetryCount)
                )
            {
                return true;
            }
            else
                return false;
        }
    }
}
