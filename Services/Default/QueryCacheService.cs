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

        public QueryCacheService(IStorageService storageService)
        {
            queryThrottler = new SemaphoreSlim(4);

            StorageService = storageService;
            //LoadLocalyCachedQueries();
        }

        //TODO: make async version
        public void Load()
        {
            //create new instance if not exists in storage
            if (!(StorageService.Has("cachedQueries")))
            {
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
                Queries = StorageService.Get<QueriesCache>("cachedQueries");
            }
        }

        public async Task<RequestResponseWrapper> GetOrUpdate(string request, CancellationToken cancellationToken, DateTime expire, int retryCount = 2, int timeout = 10)
        {
            ValidateEntry(request); //remove if expiered

            if (Queries.CachedQueries.ContainsKey(request)) //submit request if not cached
            {
                var entry = Queries.CachedQueries[request];

                //update if needed
                if(ShouldBeUpdated(request))
                {
                    entry.ResponseValue = await ExecuteQuery(request, cancellationToken, timeout);
                    entry.RetryCount++;
                }

                StorageService.Flush();
                return entry.ResponseValue;
            }

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
