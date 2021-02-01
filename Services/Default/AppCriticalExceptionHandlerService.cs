using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    public class AppCriticalExceptionHandlerService : IAppCriticalExceptionHandlerService
    {
        public AppCriticalExceptionHandlerService() { }

        public void HandleCriticalError(Exception exception)
        {
            App.HandleCriticalError(exception);
        }
    }
}
