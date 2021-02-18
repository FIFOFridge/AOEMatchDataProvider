using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public class AoeDetectionService : IAoeDetectionService
    {
        ILogService LogService { get; }

        public AoeDetectionService(ILogService logService)
        {
            LogService = logService;
        }

        public bool IsRunning
        {
            get
            {
                foreach (Process pList in Process.GetProcesses())
                {
                    if (pList.ProcessName.ToLower().Contains("aoe2de"))
                        return true;
                }


                return false;
            }
        }
    }
}
