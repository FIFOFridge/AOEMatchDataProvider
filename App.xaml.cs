using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Models.User;
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

            DumpAppConfig();

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
                storage.Create("settings", new Models.Settings.AppSettings(), StorageEntryExpirePolicy.Never);
            }

            storage.Flush();
        }

        //todo: refactor name to: InitialCheckAndNavigate()
        internal static void InitialNavigation()
        {
            var storage = Resolve<IStorageService>();
            var eventAggregator = Resolve<IEventAggregator>();
            var regionManager = App.Resolve<IRegionManager>();
            var settings = storage.Get<Models.Settings.AppSettings>("settings");
            var userIdDetectionService = Resolve<IUserIdDetectionService>();

            //make sure string resources are valid
            if (!storage.Has("stringResources"))
            {
                if (!NavigationHelper.TryNavigateTo("MainRegion", "InitialResourcesValidation", null, out Exception exception))
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
                //todo: refactor: move this and initial configuration id setup into helper class
                var detectionResult = userIdDetectionService.DetectUserId();

                //navigate in any matching profile wasn't found or multiple profiles was found
                if (detectionResult.OperationResult != Models.UserIdDetectionService.DetectionOperationResult.UserSteamIdDetected)
                {
                    if (!NavigationHelper.TryNavigateTo("MainRegion", "InitialConfiguration", null, out Exception exception))
                    {
                        HandleCriticalError(exception);
                    }

                    return;
                }

                //assert userId isn't null or empty
                //todo: refactor: change lenght validation to regex
                if (detectionResult.UserId == null || detectionResult.UserId.Length < 1)
                    throw new InvalidOperationException("Invalid userId");

                //valid only for steam id
                settings.UserId = new UserId
                {
                    SteamId = detectionResult.UserId
                };

                InitialNavigation(); //recursion call
                return;
            }
            else
            {
                var navigationParameters = new NavigationParameters
                {
                    { "description", "Launching app..." },
                    { "timer", Resolve<IAppConfigurationService>().AppBootingUpdateTick }
                };

                NavigationHelper.NavigateTo("MainRegion", "AppStateInfo", navigationParameters);
            }
        }

        protected override Window CreateShell()
        {
            //shell = new MatchData(); //save reference
            shell = new Shell(App.Resolve<IEventAggregator>());

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
            containerRegistry.RegisterSingleton<IAoeDetectionService, AoeDetectionService>();
            containerRegistry.RegisterSingleton<IRequestService, RequestService>();

            containerRegistry.Register<IMatchProcessingService, MatchProcessingService>();
            containerRegistry.Register<IDataService, DataService>();
            containerRegistry.Register<IUserIdDetectionService, UserIdDetectionService>();
            #endregion

            #region Navigation
            //region: MainRegion
            containerRegistry.RegisterForNavigation<MatchFoundNotification>();
            containerRegistry.RegisterForNavigation<InitialConfiguration>();
            containerRegistry.RegisterForNavigation<InitialResourcesValidation>();
            containerRegistry.RegisterForNavigation<AppStateInfo>();
            containerRegistry.RegisterForNavigation<TeamsPanel>();
            containerRegistry.RegisterForNavigation<AppInactivity>();

            containerRegistry.RegisterForNavigation<BottomButtonsPanel>();
            containerRegistry.RegisterForNavigation<BottomShadowPanel>();

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
            //disposableResources.Add(RequestHelper.Instance);
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

        static void DumpAppConfig()
        {
            var logService = App.Resolve<ILogService>();
            var appConfigService = App.Resolve<IAppConfigurationService>();

            logService.Info("------------- App config dump -------------");
            logService.Info($"Version: {appConfigService.AppVersion}");
            //timeouts
            var timeouts = new Dictionary<string, object>();
            timeouts.Add("MatchUpdateTimeout", appConfigService.MatchUpdateTimeout);
            timeouts.Add("DefaultRequestTimeout", appConfigService.DefaultRequestTimeout);
            logService.Info($"Timeouts: ", timeouts);
            //ticks
            var ticks = new Dictionary<string, object>();
            ticks.Add("AppStateInfoUpdateTick", appConfigService.AppStateInfoUpdateTick);
            ticks.Add("TeamPanelUpdateTick", appConfigService.TeamPanelUpdateTick);
            ticks.Add("AppInactivitySamples", appConfigService.AppInactivitySamples);
            ticks.Add("AppBootingUpdateTick", appConfigService.AppBootingUpdateTick);
            logService.Info($"Ticks: ", ticks);
#if RELEASE
            logService.Info($"Is Release: true");
#else
            logService.Info($"Is Release: false");
#endif
            logService.Info("---------- End of App config dump ----------");
        }
    }
}
