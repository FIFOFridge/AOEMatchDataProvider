using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    public class AppConfigurationTestingService : IAppConfigurationService
    {
        public string AppVersion => "0.1.0_TESTING"; 

        public string AppVersionNormalized => "010_TESTING";

        //public string RootStorageDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        //public string StorageDirectory => Path.Combine(RootStorageDirectory, "TESTING_AOEMatchDataProvider", AppVersionNormalized);

        public string RootStorageDirectory => Directory.GetCurrentDirectory();

        public string StorageDirectory => Path.Combine(RootStorageDirectory, "tempTestingDir");

        //timeouts are same as in default service implementation
#if DEBUG
        public int AppStateInfoUpdateTick { get; } = 1000 * 60 * 4;
        public int TeamPanelUpdateTick { get; } = 1000 * 60 * 5;
#else //RELEASE
        public int AppStateInfoUpdateTick { get; } = 1000 * 60 * 4;
        public int TeamPanelUpdateTick { get; } = 1000 * 60 * 6;
#endif
        
        public int AppBootingUpdateTick => 5000;
        public int MatchUpdateTimeout { get; } = 1000 * 10;
        public int DefaultRequestTimeout { get; } = int.Parse((1000 * 7.5).ToString());

        public int AppInactivitySamples => 5;
    }
}
