using AOEMatchDataProvider.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    public class LogTestingService : ILogService
    {
        TestContext testContext;

        public LogTestingService(TestContext testContext)
        {
            this.testContext = testContext;
        }

        public void Dispose() { }

        public void Debug(string message, Dictionary<string, object> logProperties = null)
        {
            // suppress
        }

        public void Info(string message, Dictionary<string, object> logProperties = null)
        {
            // supress
        }

        public void Critical(string message, Dictionary<string, object> logProperties = null)
        {
            Assert.Fail(message);
        }

        public void Error(string message, Dictionary<string, object> logProperties = null)
        {
            testContext.WriteLine("[ERROR] " + message);
        }

        public void Warning(string message, Dictionary<string, object> logProperties = null)
        {
            testContext.WriteLine("[WARNING] " + message);
        }

        public void Trace(string message, Dictionary<string, object> logProperties = null)
        {
            
        }
    }
}
