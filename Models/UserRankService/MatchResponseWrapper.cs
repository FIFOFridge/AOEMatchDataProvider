using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.UserRankService
{
    //replaced by more general and abstract: RequestWrapper<T>
    //public class MatchResponseWrapper
    //{
    //    public Match Match { get; }
    //    public UserId RequestUserId { get; }

    //    public bool IsAPIError { get; }
    //    public bool IsResponseParsingError { get; }
    //    //bool IsCustomError { get; }
    //    public string RawResponse { get; }

    //    public Exception Exception { get; }

    //    public MatchResponseWrapper(
    //        UserId requestUserId, 
    //        string rawResponse,
    //        bool isAPIError,
    //        bool isResponseParsingError,
    //        //bool isCustomError,
    //        Exception exception,
    //        Match match
    //        )
    //    {
    //        RequestUserId = requestUserId;
    //        IsAPIError = isAPIError;
    //        IsResponseParsingError = isResponseParsingError;
    //        //IsCustomError = isCustomError;
    //        RawResponse = rawResponse;
    //        Match = match;
    //    }

    //    public bool IsSuccessfull
    //    {
    //        get
    //        {
    //            return !IsAPIError && !IsResponseParsingError; //&& !IsCustomError;
    //        }
    //    }
    //}
}
