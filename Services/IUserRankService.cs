﻿using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.UserRankService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IUserRankService
    {
        Task<RequestWrapper<Match>> GetUserMatch(UserId userId, int timeout);
        Task<RequestWrapper<UserRank>> GetUserRank(UserGameProfileId playerProfileId, UserRankMode rankMode, int timeout); //CancellationToken cancellationToken);

        Task<RequestWrapper<AoeNetAPIStringResources>> GetStringResources();
    }
}