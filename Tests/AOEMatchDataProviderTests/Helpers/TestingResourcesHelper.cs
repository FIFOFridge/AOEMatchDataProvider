using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Helpers
{
    public static class TestingResourcesHelper
    {
        static readonly string resourcesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResources");
        static readonly string apiResponsesDirectory = Path.Combine(resourcesDirectory, "ApiResponses");
        static readonly string queryCacheDataDirectory = Path.Combine(resourcesDirectory, "QueryCacheDataCache");

        public static string GetApiResoponses(string file)
        {
            return File.ReadAllText(Path.Combine(apiResponsesDirectory, file));
        }

        public static void InjectCachedQueriesData(IAppConfigurationService testingAppConfigurationService)
        {
            const string fileName = "cachedQueries.json";

            var from = Path.Combine(queryCacheDataDirectory, fileName);
            var to = Path.Combine(testingAppConfigurationService.StorageDirectory, fileName);

            File.Copy(from, to);
        }

        static internal void PreapareTestingDirectory(IAppConfigurationService testingAppConfigurationService)
        {
            if (!Directory.Exists(testingAppConfigurationService.StorageDirectory))
                Directory.CreateDirectory(testingAppConfigurationService.StorageDirectory);

            var testingStorageDirectory = new DirectoryInfo(testingAppConfigurationService.StorageDirectory);

            foreach (var file in testingStorageDirectory.GetFiles())
            {
                file.Delete();
            }
        }

        static internal AppSettings GetTestingSettings()
        {
            return new AppSettings()
            {
                UserId = new AOEMatchDataProvider.Models.User.UserId()
                {
                    SteamId = "testingId",
                    GameProfileId = null
                }
            };
        }
    }
}
