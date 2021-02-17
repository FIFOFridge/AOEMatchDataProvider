using AOEMatchDataProvider.Models.RequestService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IRequestService
    {
        Task<RequestResponseWrapper> GetAsync(string query, CancellationToken cancellationToken);
    }
}
