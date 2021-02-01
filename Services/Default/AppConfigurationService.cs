using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    internal class AppConfigurationService : IAppConfigurationService
    {
#if DEBUG
        public string AppVersion => "0.1.0_PREVIEW_DEBUG";
#else
        public string AppVersion => "0.1.0";
#endif

#if DEBUG
        public string AppVersionNormalized => "010_P_D";
#else
        public string AppVersionNormalized => "010";
#endif
        public string RootStorageDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public string StorageDirectory => Path.Combine(RootStorageDirectory, "AOEMatchDataProvider", AppVersionNormalized);

        public AppConfigurationService()
        {
            if(!Directory.Exists(StorageDirectory))
            {
                Directory.CreateDirectory(StorageDirectory);
            }
        }

        public int AppStateInfoUpdateTick { get; } = 1000 * 60 * 4;
        public int TeamPanelUpdateTick { get; } = 1000 * 60 * 5;


#if DEBUG
        //TODO: Create some workaround based on debugger status in: AOEMatchDataProvider.Helpers.Request.RequestHelper
        //https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debugger.isattached?view=net-5.0

        public int MatchUpdateTimeout { get; } = 1000 * 10 * 10; //prevent reaching timeout during live debug, cause timer will be working anyway 
        public int DefaultRequestTimeout { get; } = 1000 * 7 * 10;
#else //RELEASE
        public int MatchUpdateTimeout { get; } = 1000 * 10;
        public int DefaultRequestTimeout { get; } = 1000 * 7;//int.Parse((1000 * 7.5).ToString());
#endif
    }
}
