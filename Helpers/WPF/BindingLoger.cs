using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.WPF
{
    //moved to LogService
    //internal sealed class BindingLoger : TraceListener, IDisposable
    //{
    //    bool disposed;
    //    bool flushedNewLine;

    //    ILogService LogService { get; }

    //    internal BindingLoger(ILogService logService)
    //    {
    //        LogService = logService;
    //        disposed = false;
    //    }

    //    public override void Write(string message)
    //    {
    //        LogService.Warning($"BindingLoger: ${message}", null);
    //    }

    //    public override void WriteLine(string message)
    //    {
    //        LogService.Warning($"BindingLoger: ${message}", null);
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (!disposed)
    //        {
    //            if (disposing)
    //            {
                    
    //            }

    //            base.Dispose(disposing);
    //        }

    //        disposed = true;
    //    }
    //}
}
