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
        public string AppVersion => "0.1.3_PREVIEW_DEBUG";
#else
        public string AppVersion => "0.1.3";
#endif

#if DEBUG
        public string AppVersionNormalized => "013_P_D";
#else
        public string AppVersionNormalized => "013";
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

#if LIVEDEBUG
        public int AppBootingUpdateTick { get; } = 1000;
        public int AppStateInfoUpdateTick { get; } = 1000 * 60 * 2;
        public int TeamPanelUpdateTick { get; } = 1000 * 60 * 3;
        public int AppInactivitySamples { get; } = 5;
#else
        public int AppBootingUpdateTick { get; } = 1000 * 5; //initial boot is required to avoid double hotkey registration at startup
        public int AppStateInfoUpdateTick { get; } = 1000 * 60 * 3;
        public int TeamPanelUpdateTick { get; } = 1000 * 60 * 5;
        public int AppInactivitySamples { get; } = 4;
#endif

#if LIVEDEBUG
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
