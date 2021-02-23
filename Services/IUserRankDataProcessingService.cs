using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Match;
using AOEMatchDataProvider.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    //separated part of IUserRankService for testing
    internal interface IUserRankDataProcessingService
    {
        Match ProcessMatch(string jsonResponse, AoeNetAPIStringResources apiStringResources);
        UserRank ProcessUserRank(string jsonResponse, UserRankMode rankMode, AoeNetAPIStringResources apiStringResources);
    }
}
