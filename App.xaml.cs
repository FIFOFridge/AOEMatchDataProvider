using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Mvvm;
using AOEMatchDataProvider.Other;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Services.Default;
using AOEMatchDataProvider.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Unity;

namespace AOEMatchDataProvider
{
    //TODO: replace with AppWrapper
    public partial class App : PrismApplication
    {
        ////view models that requires view injection to work, which is required in some specific cases
        //internal Dictionary<Type, Func<object, IViewInjector>> InjectableViews { get; private set; }

        internal static App Instance { get; private set; }
        //mutex to make sure only one instance of app is running
        internal static Mutex InstanceMutex { get; private set; }


        Window shell;
        static IUnityContainer appContainer;
        static IContainerRegistry appContainerRegistry;

        List<IDisposable> disposableResources;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Instance = this;

            ValidateAppSingleInstanceState();

            IKeyHookService keyHookService = Resolve<IKeyHookService>();
            IStorageService storageService = Resolve<IStorageService>();

            disposableResources = new List<IDisposable>();

            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            RegisterDisposableResources();

            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;

            //initialize request helper
            RequestHelper.InitializeRequestHelper(Resolve<IAppConfigurationService>());

            LoadSettings();
            InitialNavigation();
        }

        //Resolve type using default app container
        internal static T Resolve<T>()
        {
            return (T)appContainer.Resolve(typeof(T));
        }

        internal static void LoadSettings()
        {
            var storage = Resolve<IStorageService>();

            storage.Load();

            //create new instance of app settings if it don't exist
            if(!(storage.Has("settings")))
            {
                App.Resolve<ILogService>().Debug("Settings file not found, creating...");
                storage.Create("settings", new AppSettings(), StorageEntryExpirePolicy.Never);
            }

            storage.Flush();
        }

        internal static void InitialNavigation()
        {
            var storage = Resolve<IStorageService>();
            var eventAggregator = Resolve<IEventAggregator>();
            var regionManager = App.Resolve<IRegionManager>();
            var settings = storage.Get<AppSettings>("settings");

            //make sure string resources are valid
            if (!storage.Has("stringResources"))
            {
                if (!NavigationHelper.NavigateTo("MainRegion", "InitialResourcesValidation", null, out Exception exception))
                    HandleCriticalError(exception);

                return;
            }

            //if userId not set then navigate to user configuration
            if (settings.UserId == null || 
                (string.IsNullOrEmpty(settings.UserId.GameProfileId) &&
                 string.IsNullOrEmpty(settings.UserId.SteamId)
                )
            )
            {
                if (!NavigationHelper.NavigateTo("MainRegion", "InitialConfiguration", null, out Exception exception))
                    HandleCriticalError(exception);

                return;
            }

            //redirect to app state info
            else
            {

                var navigationParameters = new NavigationParameters
                {
                    { "description", "Waiting for match to start..." }
                };

               regionManager.RequestNavigate("MainRegion", "AppStateInfo", navigationParameters);
            }
        }

        //void SetupInjectableViewModels()
        //{
        //    InjectableViews = new Dictionary<Type, Func<object, IViewInjector>>();
        //    InjectableViews.Add(typeof(TeamsPanel), (view) =>
        //    {
        //        //create VM instance
        //        return new ViewModels.TeamsPanelViewModel(view as ITeamsPanelView);
        //    });
        //}

        protected override Window CreateShell()
        {
            //shell = new MatchData(); //save reference
            shell = new Shell();

            appContainerRegistry.RegisterInstance<IShell>(shell as IShell);

            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            appContainer = containerRegistry.GetContainer();
            appContainerRegistry = containerRegistry;

            #region Services
            containerRegistry.RegisterSingleton<IAppConfigurationService, AppConfigurationService>();

            containerRegistry.RegisterSingleton<ILogService, LogService>();

            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
            containerRegistry.RegisterSingleton<IKeyHookService, KeyHookService>();
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
            containerRegistry.RegisterSingleton<IQueryCacheService, QueryCacheService>();
            containerRegistry.RegisterSingleton<IAppCriticalExceptionHandlerService, AppCriticalExceptionHandlerService>();

            containerRegistry.Register<IUserRankService, UserRankService>();
            #endregion

            #region Navigation
            //region: MainRegion
            containerRegistry.RegisterForNavigation<MatchFoundNotification>();
            containerRegistry.RegisterForNavigation<InitialConfiguration>();
            containerRegistry.RegisterForNavigation<InitialResourcesValidation>();
            containerRegistry.RegisterForNavigation<AppStateInfo>();
            containerRegistry.RegisterForNavigation<TeamsPanel>();

            //region: QuickActions
            //todo: Implement quick actions & empty region
            #endregion
        }

        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleCriticalError(e.Exception);
            e.Handled = true;
        }

        void RegisterDisposableResources()
        {
            disposableResources.Add(App.Resolve<IKeyHookService>()); //register critical disposables
            //disposableResources.Add(App.Resolve<ILogService>());
            disposableResources.Add(RequestHelper.Instance);
        }

        internal static void HandleCriticalError(Exception exception)
        {
            var logger = App.Resolve<ILogService>();
            var logProperties = new Dictionary<string, object>();
            logProperties.Add("exception", exception.ToString());
            logProperties.Add("stack", exception.StackTrace);

            var appConfigurationService = App.Resolve<IAppConfigurationService>();
            var logsPath = System.IO.Path.Combine(appConfigurationService.StorageDirectory, "logs");

            logger.Critical($"Critical exception occured: {exception.ToString()} \n stack: {exception.StackTrace}", logProperties);

            Instance.DisposeCritical();

            MessageBox.Show($"Critical error occured, if it's not first time when error occured and your are using latest app version, please consider submiting bug. Please include current session log file.\n\n Error:\n {exception.Message}\n\n. Logs file location: {logsPath}");
            Environment.Exit(1);
        }

        #region Disposing app resources
        void DisposeMutex()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (InstanceMutex != null)
                    InstanceMutex.ReleaseMutex();
            });
        }

        public void DisposeAppResources(IEnumerable<IDisposable> disposableResources = null)
        {
            DisposeMutex();

            if (disposableResources == null)
                disposableResources = this.disposableResources; //all registred disposable resources

            foreach(var disposableResource in disposableResources)
            {
                if(disposableResource != null)
                {
                    try
                    {
                        disposableResource.Dispose();
                    }
                    catch (Exception e)
                    {
                        //supress

                        if (
                            e is StackOverflowException ||
                            e is ThreadAbortException ||
                            e is AccessViolationException
                            )
                        {
                            throw e;
                        }
                    }
                }
            }
        }

        void DisposeCritical()
        {
            DisposeMutex();

            DisposeAppResources(
                disposableResources.Where(
                    disposableResource => disposableResource is ICriticalDisposable
                    )
                );
        }
        #endregion

        public static void ValidateAppSingleInstanceState()
        {
            InstanceMutex = new Mutex(true, "fac07ff0-d1e1-4354-9c97-2f82d76c6346", out bool createdNew);

            if(!createdNew)
            {
                MessageBox.Show("Application is already running");
                Environment.Exit(2);
            }
        }

        public static string GetAppPathDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        //protected override void ConfigureViewModelLocator()
        //{
        //    base.ConfigureViewModelLocator();
        //    ViewModelLocationProvider.SetDefaultViewModelFactory((view, viewModelType) =>
        //    {
        //        //if view model implements IViewInjector then get it's mapping from InjectableViews and initialize it's VM
        //        if(viewModelType.GetInterfaces().Contains(typeof(IViewInjector)))
        //        {
        //            //VM is implementing IViewInjector but don't have set factory
        //            if (!(InjectableViews.ContainsKey(view.GetType())))
        //                throw new InvalidOperationException();

        //            //create VM using factory
        //            var vmInstance = InjectableViews[view.GetType()](view); 

        //            return vmInstance;
        //        } else //use default prism container to resolve
        //            return base.Container.Resolve(viewModelType);
        //    });
        //}
    }
}
