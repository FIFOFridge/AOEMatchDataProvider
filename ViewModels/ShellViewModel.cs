using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views.Shell;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AOEMatchDataProvider.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        double opacity = 1.0;
        public double Opacity
        {
            get
            {
                return opacity;
            }

            set
            {
                SetProperty(ref opacity, value);
                LogService.Trace($"Window opacity has been changed to: {value}");
            }
        }

        bool canUpdateMatchData;
        public bool CanUpdateMatchData
        {
            get
            {
                return canUpdateMatchData;
            }

            set
            {
                SetProperty(ref canUpdateMatchData, value);
                LogService.Trace($"CanUpdateMatchData has been changed to: {value}");
            }
        }
        #region Services
        public IApplicationCommands ApplicationCommands { get; }
        public IAppConfigurationService AppConfigurationService { get; }
        public IAppCriticalExceptionHandlerService AppCriticalExceptionHandlerService { get; }
        public IKeyHookService KeyHookService { get; }
        public IDataService UserRankService { get; }
        public IStorageService StorageService { get; }
        public IEventAggregator EventAggregator { get; }
        public IRegionManager RegionManager { get; }
        public ILogService LogService { get; }
        public IAoeDetectionService AoeDetectionService { get; }
        public IMatchProcessingService MatchProcessingService { get; }
        #endregion Services

        //bool isRunning;
        //bool IsRunning
        //{
        //    get { return isRunning; }
        //    set { SetProperty(ref isRunning, value); }
        //}

        public DelegateCommand UpdateMatchDataCommand { get; private set; }
        public DelegateCommand ToggleWindowVisibilityCommand { get; private set; }
        public DelegateCommand ShowWindowCommand { get; private set; }
        public DelegateCommand HideWindowCommand { get; private set; }
        public DelegateCommand<object> SetTransparencyCommand { get; private set; }
        public DelegateCommand<object> SetCurrentShellMaxOpacityCommand { get; private set; }
        public DelegateCommand<object> SetIgnoreInputCommand { get; private set; }

        public DelegateCommand CloseAppCommand { get; private set; }
        public DelegateCommand ShowLicensesWindowCommand { get; private set; }

        public Match CurrentMatch { get; set; }

        bool ignoreInput;
        public bool IgnoreInput
        {
            get
            {
                return ignoreInput;
            }

            set
            {
                if (value == true)
                    EnableIgnoreInput();
                else
                    DisableIgnoreInput();

                ignoreInput = value;
            }
        }

        public double currentMaxOpacity;
        public double CurrentMaxOpacity { get; set; }

        public string Title { get => "AoE Match Data Provider - Beta - " + Assembly.GetExecutingAssembly().GetName().Version.ToString(); }

        static readonly double minimumVisibleOpacity = 0.5;

        public ShellViewModel(
            IApplicationCommands applicationCommands,
            IAppConfigurationService appConfigurationService,
            IAppCriticalExceptionHandlerService appCriticalExceptionHandlerService,
            IKeyHookService keyHookService,
            IDataService userRankService,
            IStorageService storageService,
            IEventAggregator eventAggregator,
            IRegionManager regionManager,
            ILogService logService,
            IAoeDetectionService aoeDetectionService,
            IMatchProcessingService matchProcessingService)
        {
            #region Assign services
            ApplicationCommands = applicationCommands;
            AppConfigurationService = appConfigurationService;
            AppCriticalExceptionHandlerService = appCriticalExceptionHandlerService;
            KeyHookService = keyHookService;
            UserRankService = userRankService;
            StorageService = storageService;
            EventAggregator = eventAggregator;
            RegionManager = regionManager;
            LogService = logService;
            AoeDetectionService = aoeDetectionService;
            MatchProcessingService = matchProcessingService;
            #endregion Assign services

            #region Setup Application Commands
            UpdateMatchDataCommand = new DelegateCommand(UpdateAppState);
            ToggleWindowVisibilityCommand = new DelegateCommand(ToggleWindowVisibility);
            ShowWindowCommand = new DelegateCommand(() => SetWindowOpacity(1));
            HideWindowCommand = new DelegateCommand(() => SetWindowOpacity(0));
            SetTransparencyCommand = new DelegateCommand<object>((p) => SetWindowOpacity(p));
            SetCurrentShellMaxOpacityCommand = new DelegateCommand<object>(SetWindowMaxOpacity);
            SetIgnoreInputCommand = new DelegateCommand<object>(SetIgnoreInput);
            //SetTransparencyCommand = new DelegateCommand<object>(SetWindowOpacity);

            ApplicationCommands = applicationCommands;

            ApplicationCommands.UpdateMatchDataCommand.RegisterCommand(UpdateMatchDataCommand);
            ApplicationCommands.ToggleWindowVisibility.RegisterCommand(ToggleWindowVisibilityCommand);
            ApplicationCommands.ShowWindow.RegisterCommand(ShowWindowCommand);
            ApplicationCommands.HideWindow.RegisterCommand(HideWindowCommand);
            ApplicationCommands.SetWindowOpacity.RegisterCommand(SetTransparencyCommand);
            ApplicationCommands.SetMaxWindowOpacity.RegisterCommand(SetCurrentShellMaxOpacityCommand);
            ApplicationCommands.SetIgnoreInput.RegisterCommand(SetIgnoreInputCommand);
            #endregion Setup Application Commands
            #region Window/Shell Commands
            CloseAppCommand = new DelegateCommand(CloseApp);
            ShowLicensesWindowCommand = new DelegateCommand(ShowLicensesWindow);
            #endregion

            EventAggregator.GetEvent<ShellResizedEvent>().Subscribe(HandleShellResizedEvent);

            CanUpdateMatchData = true;
        }

        async void UpdateAppState()
        {
            if (!CanUpdateMatchData) //make sure we can update
                return;

            CanUpdateMatchData = false;

            LogService.Debug("Updating app state...");

            try
            {
                NavigationParameters navigationParameter = new NavigationParameters();
#if !LIVEDEBUG

                if (!AoeDetectionService.IsRunning)
                {
                    LogService.Debug("AoE not running...");

                    navigationParameter.Add("description", "Waiting for game to start...");
                    navigationParameter.Add("timer", 5000);

                    if (!NavigationHelper.TryNavigateTo("MainRegion", "AppStateInfo", navigationParameter, out Exception exception))
                    {
                        AppCriticalExceptionHandlerService.HandleCriticalError(exception);
                    }

                    CanUpdateMatchData = true;
                    return;
                }
#endif

                LogService.Trace("Updadting current match state...");
                
                string lastMatchId = MatchProcessingService.LastPulledMatchId;
                int updateFailsInRow = MatchProcessingService.InRowProcessingFails;

                Tuple<string, string> navigationPair = null;

                var matchState = await MatchProcessingService.TryUpdateCurrentMatch();

                switch (matchState)
                {
                    case Models.MatchProcessingService.MatchUpdateStatus.ConnectionError:
                        navigationParameter.Add("description", "Unable to connect...");
                        navigationPair = Tuple.Create("MainRegion", "AppStateInfo");
                        break;

                    case Models.MatchProcessingService.MatchUpdateStatus.ProcessingError:
                        navigationParameter.Add("description", "Unable to process data, make sure you are using latest app version \n");
                        navigationPair = Tuple.Create("MainRegion", "AppStateInfo");
                        break;

#if !LIVEDEBUG //Dont check in DEBUG to speed up debug process
                    case Models.MatchProcessingService.MatchUpdateStatus.MatchEnded:
                        navigationParameter.Add("description", "Waiting for match to start...");
                        navigationPair = Tuple.Create("MainRegion", "AppStateInfo");
                        break;

                    case Models.MatchProcessingService.MatchUpdateStatus.UnsupportedMatchType:
                        navigationParameter.Add("description", "Unsupported match type...");
                        navigationPair = Tuple.Create("MainRegion", "AppStateInfo");
                        break;
#else // Display already finsihed match if LIVEDEBUG

                    case Models.MatchProcessingService.MatchUpdateStatus.MatchEnded:
                        navigationParameter.Add("UserMatchData", MatchProcessingService.CurrentMatch.Users);
                        navigationParameter.Add("MatchType", MatchProcessingService.CurrentMatch.MatchType);
                        navigationPair = Tuple.Create("MainRegion", "MatchFoundNotification");
                        break;
#endif

                    case Models.MatchProcessingService.MatchUpdateStatus.SupportedMatchType:
                        navigationParameter.Add("UserMatchData", MatchProcessingService.CurrentMatch.Users);
                        navigationParameter.Add("MatchType", MatchProcessingService.CurrentMatch.MatchType);
                        navigationPair = Tuple.Create("MainRegion", "MatchFoundNotification");
                        break;

                    case Models.MatchProcessingService.MatchUpdateStatus.UnknownError:
                        navigationParameter.Add("description", "Unknow error occured during processing match");
                        navigationPair = Tuple.Create("MainRegion", "AppStateInfo");
                        break;

                    default:
                        new InvalidOperationException($"Unrecognized match state: {matchState}");
                        break;
                }

                if(matchState != Models.MatchProcessingService.MatchUpdateStatus.SupportedMatchType) //update failed
                {
                    if((updateFailsInRow + 1) > AppConfigurationService.AppInactivitySamples) //check how many failed updates in a row we can get
                    {
                        NavigationHelper.NavigateTo("MainRegion", "AppInactivity", navigationParameter); //set application into inactivity mode
                        CanUpdateMatchData = true;
                        return;
                    }
                }

                if(matchState == Models.MatchProcessingService.MatchUpdateStatus.SupportedMatchType && //update successed and last match id is set
                    lastMatchId != null
                    )
                {
                    if(lastMatchId == MatchProcessingService.LastPulledMatchId) //last processed match is same as current
                    {
                        LogService.Trace("Skiping match update...");
                        CanUpdateMatchData = true;
                        return;
                    }
                }

                //handle previously set states
                NavigationHelper.NavigateTo(navigationPair.Item1, navigationPair.Item2, navigationParameter);

                CanUpdateMatchData = true;
            }
            catch (Exception e)
            {
                AppCriticalExceptionHandlerService.HandleCriticalError(e);
            }
        }

        void ToggleWindowVisibility()
        {
            if (Opacity <= 1 && Opacity > 0) //hide if visible
            {
                SetWindowOpacity(0);
            }
            else //show if hidden
            {
                SetWindowOpacity(1);
            }
        }

        void SetWindowOpacity(object opacity)
        {
            //todo: add safe checks, handle convertion exceptions
            var _opacity = double.Parse(opacity.ToString());

            //make sure _opacity will be smaller then max one
            if (_opacity > CurrentMaxOpacity)
                _opacity = CurrentMaxOpacity;

            Opacity = _opacity;
        }

        void SetWindowMaxOpacity(object opacity)
        {
            var _opacity = double.Parse(opacity.ToString());

            if (_opacity < minimumVisibleOpacity)
                throw new ArgumentOutOfRangeException($"Opacity defined as in visible range cannot be less then: {minimumVisibleOpacity}");

            CurrentMaxOpacity = _opacity;

            //check if current window opacity need update
            //if window is hidden
            if (Opacity > minimumVisibleOpacity)
            {
                //update current window opacity if window is visible
                if (Opacity != CurrentMaxOpacity)
                    SetWindowOpacity(CurrentMaxOpacity);
            }
        }

        void SetIgnoreInput(object ignoreInputState)
        {
            IgnoreInput = (bool)ignoreInputState;
        }

        void ShowLicensesWindow()
        {
            var window = new LicenseWindow();
            window.Show();
        }

        void CloseApp()
        {
            LogService.Info("Closing app by user");

            CanUpdateMatchData = false;

            //StorageService.Flush();

            //hide window as fast as possible, then dispose app resources and terminate process
            HideWindowCommand.Execute();

            App.Instance.DisposeAppResources();

            Environment.Exit(0);
        }

        #region Ignore Input Logic
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        Models.Views.Shell.Rectangle ShellRectangle { get; set; }

        void HandleShellResizedEvent(Models.Views.Shell.Rectangle rectangle)
        {
            ShellRectangle = rectangle;
        }

        static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        System.Timers.Timer refreshMousePositionTimer;

        void EnableIgnoreInput()
        {
            if (refreshMousePositionTimer != null)
            {
                if (refreshMousePositionTimer.Enabled)
                {
                    refreshMousePositionTimer.Stop();
                }

                refreshMousePositionTimer.Dispose();
            }

            refreshMousePositionTimer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 1000 / 10
            };
            refreshMousePositionTimer.Elapsed += RefreshMousePositionTimer_Elapsed;
            refreshMousePositionTimer.Start();
        }

        static readonly int marginHorziontal = 100;
        static readonly int marginVertical = 100;

        private void RefreshMousePositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!IgnoreInput)
                return;

            var point = GetMousePosition();
            bool isNearHorizontal = false;
            bool isNearVertical = false;

            App.Current.Dispatcher.Invoke(() =>
            {
                if (point.X < (ShellRectangle.Left + ShellRectangle.Width + marginHorziontal))
                    isNearHorizontal = true;
                else
                    isNearHorizontal = false;

                if (point.Y < (ShellRectangle.Top + ShellRectangle.Height + marginVertical))
                    isNearVertical = true;
                else
                    isNearVertical = false;

                if (isNearHorizontal && isNearVertical)
                {
                    //check if its no already set
                    //to avoid unnecessary calls cause it will be called each 100ms if IgnoreInput is set
                    //and SetWindowOpacity is based on objectType -> ToString() -> double.Parse(...)
                    //and this methods is already running by UI Dispatcher
                    if (Opacity != 0)
                        SetWindowOpacity(0); //hide if its near
                }
                else
                {
                    //check if its no already set
                    if (Opacity != CurrentMaxOpacity)
                        SetWindowOpacity(CurrentMaxOpacity); //show otherwise
                }
            });
        }

        void DisableIgnoreInput()
        {
            if (refreshMousePositionTimer != null)
            {
                if (refreshMousePositionTimer.Enabled)
                {
                    refreshMousePositionTimer.Stop();
                }

                refreshMousePositionTimer.Dispose();
            }
        }
        #endregion
    }
}
