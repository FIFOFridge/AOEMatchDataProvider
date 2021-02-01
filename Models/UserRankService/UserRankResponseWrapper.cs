using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.UserRankService
{
    //replaced by more general and abstract: RequestWrapper<T>
    //public class UserRankResponseWrapper
    //{
    //    public UserRank UserRank { get; }
    //    public UserGameProfileId PlayerProfileId { get; }

    //    public bool IsAPIError { get; }
    //    public bool IsResponseParsingError { get; }
    //    //bool IsCustomError { get; }
    //    public string RawResponse { get; }

    //    public Exception Exception { get; }

    //    public UserRankResponseWrapper(
    //        //UserId requestUserId,
    //        UserGameProfileId playerProfileId,
    //        string rawResponse,
    //        bool isAPIError,
    //        bool isResponseParsingError,
    //        //bool isCustomError,
    //        Exception exception,
    //        UserRank userRank
    //        )
    //    {
    //        PlayerProfileId = playerProfileId;
    //        IsAPIError = isAPIError;
    //        IsResponseParsingError = isResponseParsingError;
    //        //IsCustomError = isCustomError;
    //        RawResponse = rawResponse;
    //        UserRank = userRank;
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
