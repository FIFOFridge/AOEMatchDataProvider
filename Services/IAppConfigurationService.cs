using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IAppConfigurationService
    {
        string AppVersion { get; }
        string AppVersionNormalized { get; }

        string RootStorageDirectory { get; }
        string StorageDirectory { get; }
        //string StorageTempDirectory { get; }

        int AppStateInfoUpdateTick { get; }
        int TeamPanelUpdateTick { get; }

        int AppBootingUpdateTick { get; }
        int MatchUpdateTimeout { get; }
        int DefaultRequestTimeout { get; }
    }
}
