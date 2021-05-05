using AOEMatchDataProvider.Helpers.Validation;
using AOEMatchDataProvider.Models.UserIdDetectionService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public class UserIdDetectionService : IUserIdDetectionService
    {
        public UserIdDetectionService()
        {

        }

        public DetectionResult DetectUserId()
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string aoeFolderPath = Path.Combine(userFolderPath, "Games", "Age of Empires 2 DE");

            if(!Directory.Exists(aoeFolderPath))
            {
                return new DetectionResult(DetectionOperationResult.UnableToDetectUserId, null, UserIdMode.Invalid);
            }

            string detectedId = null;
            int detectedIds = 0;
            UserIdMode detectedMode = UserIdMode.Invalid;

            foreach (var dir in Directory.GetDirectories(aoeFolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                var dirToTest = new DirectoryInfo(dir).Name;

                UserIdMode tempMode = UserIdMode.Invalid;

                if (UserIdValidator.TryParseUserId(dirToTest, out tempMode))
                {
                    //todo: add windows store support
                    if (tempMode == UserIdMode.SteamId64)
                    {
                        detectedMode = tempMode;
                        detectedIds++;
                        detectedId = dirToTest;
                    }
                }

            }

            //make sure detect id is in correct format
            //todo: add windows store support
            if(detectedMode != UserIdMode.SteamId64)
            {
                return new DetectionResult(DetectionOperationResult.UnableToDetectUserId, null, UserIdMode.Invalid);
            }

            //todo: add array support for multiple ids
            if (detectedIds > 1)
                return new DetectionResult(DetectionOperationResult.MultipleUserIdsDetected, null, UserIdMode.Invalid);

            return new DetectionResult(DetectionOperationResult.UserSteamIdDetected, detectedId, detectedMode);
        }
    }
}
