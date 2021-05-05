using AOEMatchDataProvider.Helpers.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.UserIdDetectionService
{
    public class DetectionResult
    {
        internal DetectionResult(
            DetectionOperationResult operationResult, 
            string userId,
            UserIdMode userIdMode
            )
        {
            OperationResult = operationResult;
            UserId = userId;
            UserIdMode = userIdMode;
        }

        public DetectionOperationResult OperationResult { get; }
        public string UserId { get; }
        public UserIdMode UserIdMode { get; }
    }
}
