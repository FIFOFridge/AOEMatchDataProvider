using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Validation
{
    //source: https://stackoverflow.com/a/21938058
    public class ConnectionValidatior
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsConnectedToInternet()
        {
            return InternetGetConnectedState(out int Desc, 0);
        }
    }
}
