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

        static NavigationHelper()
        {
            regionManager = App.Resolve<IRegionManager>();
        }

        public static bool TryNavigateTo(string region, string view, NavigationParameters navigationParameters, out Exception exception)
        {
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
            exception = e;
            return false;
        }

        public static void NavigateTo(string region, string view, NavigationParameters navigationParameters)
        {
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
            if(e != null)
                throw e;
        }
    }
}
