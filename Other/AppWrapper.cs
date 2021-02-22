using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Services.Default;
using AOEMatchDataProvider.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Unity;

namespace AOEMatchDataProvider.Other
{
    //common app wrapper, shared class for 'main' app and tests
    public abstract class AppWrapper : PrismApplication
    {
        static AppWrapper CurrentInstance;

        protected IUnityContainer AppContainer { get; private set; }
        protected IContainerRegistry AppContainerRegistry { get; private set; }

        readonly List<ContentControl> navigationViews;
        public IReadOnlyList<ContentControl> RegistredNavigationViews { get; protected set; }

        public AppWrapper()
        {
            if (CurrentInstance != null)
                throw new InvalidOperationException("App have to be disposed, before creating new one");

            navigationViews = new List<ContentControl>();
        }

        protected virtual T Resolve<T>()
        {
            return (T)AppContainer.Resolve(typeof(T));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            AppContainer = containerRegistry.GetContainer();

            #region Services
            containerRegistry.RegisterSingleton<IAppConfigurationService, AppConfigurationService>();
            containerRegistry.RegisterSingleton<ILogService, LogService>();
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
            containerRegistry.RegisterSingleton<IKeyHookService, KeyHookService>();
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
            containerRegistry.RegisterSingleton<IQueryCacheService, QueryCacheService>();

            containerRegistry.Register<IUserRankService, UserRankService>();
            #endregion
        }

        protected virtual void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InitialConfiguration>();
            containerRegistry.RegisterForNavigation<AppStateInfo>();
            containerRegistry.RegisterForNavigation<TeamsPanel>();
        }

        protected virtual void InitSettings()
        {
            var storage = Resolve<IStorageService>();

            storage.Load();

            //create new instance of app settings if it don't exist
            if (!(storage.Has("settings")))
            {
                App.Resolve<ILogService>().Debug("Settings file not found, creating...");
                storage.Create("settings", new Models.Settings.AppSettings(), StorageEntryExpirePolicy.Never);
            }

            storage.Flush();
        }

        protected override Window CreateShell()
        {
            throw new NotImplementedException(); //force implementation in child class
        }
    }
}
