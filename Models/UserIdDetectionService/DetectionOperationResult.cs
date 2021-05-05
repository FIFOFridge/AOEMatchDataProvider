using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.UserIdDetectionService
{
    public enum DetectionOperationResult
    {
        UnableToDetectUserId = 1,
        UserSteamIdDetected,
        //UserWindowsStoreIdDetected,
        MultipleUserIdsDetected = 4
    }
}
