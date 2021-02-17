using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Services.Default;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Helpers.Navigation
{
    public static class NavigationHelper
    {
        static IRegionManager regionManager;
        static ILogService LogService { get; }

        static NavigationHelper()
        {
            regionManager = App.Resolve<IRegionManager>();
            LogService = App.Resolve<ILogService>();
        }

        public static bool TryNavigateTo(string region, string view, NavigationParameters navigationParameters, out Exception exception)
        {
            LogService.Debug($">> Safe navigating to: {view} in region: {region}");
            Exception e = null;

            App.Current.Dispatcher.Invoke(() =>
            {
                regionManager.RequestNavigate(region, view, result =>
                {
                     e = result.Error;
                },
                navigationParameters);
            });

            if (e == null) //success
            {
                exception = null;
                return true;
            }

            //request navigation failed
            LogService.Warning($"Exception occured while safe navigation to: {view} in region: {region}, exception: {e}");
            exception = e;
            return false;
        }

        public static void NavigateTo(string region, string view, NavigationParameters navigationParameters)
        {
            LogService.Debug($">> Navigating to: {view} in region: {region}");
            Exception e = null;

            App.Current.Dispatcher.Invoke(() =>
            {
                regionManager.RequestNavigate(region, view, result =>
                {
                    e = result.Error;
                },
                navigationParameters);
            });

            //rethrow navigation exception
            if (e != null)
            {
                LogService.Error($"Exception occured while navigation to: {view} in region: {region}, exception: {e}");
                throw e;
            }
        }
    }
}
