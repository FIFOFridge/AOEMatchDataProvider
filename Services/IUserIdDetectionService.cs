using AOEMatchDataProvider.Models.UserIdDetectionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IUserIdDetectionService
    {
        DetectionResult DetectUserId();
    }
}
