using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Other
{
    //implement in case of service has to be disposed even when unhandled exception will occur
    //sample use case: low level windows hooks (look: KeyHookService)
    public interface ICriticalDisposable : IDisposable {}
}
