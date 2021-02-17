using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Windows
{
    public static class ExternalRedirectionHelper
    {
        public static void RedirectTo(string url)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}
