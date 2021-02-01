using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Windows
{
    public class WindowManagmentHelper
    {
        bool FocusWindowByProcessId(string processId, ILogService logService = null)
        {
            Process process = Process.GetProcessesByName(processId).First();

            if (process == null)
                return false;

            if (process.MainWindowHandle == null)
                return false;

            return SetForegroundWindow(process.MainWindowHandle);
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
