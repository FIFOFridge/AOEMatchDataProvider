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
        public bool IsRunning
        {
            get
            {
                return Process.GetProcessesByName("aoe2de.exe").Length > 0;
            }
        }
    }
}
