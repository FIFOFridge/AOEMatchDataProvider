using AOEMatchDataProvider.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    public class AppCriticalExceptionHandlerTestingService : IAppCriticalExceptionHandlerService
    {
        public void HandleCriticalError(Exception exception)
        {
            Assert.Fail($"Test failed by critical exception: {exception.ToString()}", exception);
        }
    }
}
