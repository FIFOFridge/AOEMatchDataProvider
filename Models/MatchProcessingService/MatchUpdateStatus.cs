using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.MatchProcessingService
{
    public enum MatchUpdateStatus
    {
        ConnectionError = 0,
        ProcessingError,
        UnknownError,
        MatchEnded,
        UnsupportedMatchType,
        SupportedMatchType
    }
}
