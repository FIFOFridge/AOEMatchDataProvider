using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface ILogService : IDisposable
    {
        void Info(string message, Dictionary<string, object> logProperties = null);
        void Warning(string message, Dictionary<string, object> logProperties = null);
        void Error(string message, Dictionary<string, object> logProperties = null);
        void Debug(string message, Dictionary<string, object> logProperties = null);
        //void Exception(string message, Exception exception, Dictionary<string, object> logProperties = null); 
        void Critical(string message, Dictionary<string, object> logProperties = null);
    }
}
