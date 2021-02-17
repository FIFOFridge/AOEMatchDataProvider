using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AOEMatchDataProvider.Models.RequestService;

namespace AOEMatchDataProvider.Services
{
    public interface IQueryCacheService
    {
        Task<RequestResponseWrapper> GetOrUpdate(string request, CancellationToken cancellationToken, DateTime expire, int retryCount = 2, int timeout = 10);
        
        //TODO: make async version
        void Load();
    }
}
