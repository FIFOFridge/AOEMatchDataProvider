using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IAoeDetectionService
    {
        bool IsRunning { get; }
    }
}
