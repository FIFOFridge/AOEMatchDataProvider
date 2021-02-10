using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views.Shell;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Request;
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
                LogService.Debug($"Window opacity has been changed to: {value}");
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
                LogService.Debug($"CanUpdateMatchData has been changed to: {value}");
            }
        }
        #region Services
        public IApplicationCommands ApplicationCommands { get; }
        public IAppConfigurationService AppConfigurationService { get; }
        public IAppCriticalExceptionHandlerService AppCriticalExceptionHandlerService { get; }
        public IKeyHookService KeyHookService { get; }
        public IUserRankService UserRankService { get; }
        public IStorageService StorageService { get; }
        public IEventAggregator EventAggregator { get; }
        public IRegionManager RegionManager { get; }
        public ILogService LogService { get; }
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
        
        public Models.Match CurrentMatch { get; set; }
        
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

        static readonly double minimumVisibleOpacity = 0.5;

        public ShellViewModel(
            IApplicationCommands applicationCommands,
            IAppConfigurationService appConfigurationService,
            IAppCriticalExceptionHandlerService appCriticalExceptionHandlerService,
            IKeyHookService keyHookService,
            IUserRankService userRankService,
            IStorageService storageService,
            IEventAggregator eventAggregator,
            IRegionManager regionManager,
            ILogService logService
            )
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
            #endregion Assign services
            #region Setup Application Commands
            UpdateMatchDataCommand = new DelegateCommand(UpdateMatchData);
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

        //todo: refactor whole UpdateMatchData by dividing it to separate methods (? and move logic to processing service?)
        async void UpdateMatchData()
        {
            if (!canUpdateMatchData) //make sure we can update
                return;

            canUpdateMatchData = false;

            NavigationParameters navigationParameter = new NavigationParameters();
            RequestWrapper<Models.Match> requestWrapper = null;

            try
            {
                var appSettings = StorageService.Get<AppSettings>("settings");

                requestWrapper = await UserRankService.GetUserMatch(appSettings.UserId, AppConfigurationService.MatchUpdateTimeout);

                //if request failed then display connection issues
                if (requestWrapper == null || requestWrapper.Exception != null)
                {
                    var logProperties = new Dictionary<string, object>();
                    logProperties.Add("exception", requestWrapper.Exception);
                    logProperties.Add("stack", requestWrapper.Exception.StackTrace);

                    LogService.Error($"Unable to update match data: {requestWrapper.Exception.ToString()}", logProperties);

                    navigationParameter.Add("description", "Unable to connect");

                    if (!NavigationHelper.NavigateTo("MainRegion", "MatchFoundNotification", navigationParameter, out Exception exception))
                        AppCriticalExceptionHandlerService.HandleCriticalError(exception);
                }

                //if current match has been already displayed then skip
                if (CurrentMatch != null && requestWrapper.Value.MatchId == CurrentMatch.MatchId)
                {
                    LogService.Debug("Skiping match update...");
                    return;
                }

                //update current match
                CurrentMatch = requestWrapper.Value;

                if (requestWrapper.IsSuccess)
                {
#if DEBUG //if DEBUG and successed then display even old match
                    navigationParameter.Add("UserMatchData", requestWrapper.Value.Users);
                    navigationParameter.Add("MatchType", requestWrapper.Value.MatchType);

                    if (requestWrapper.Value.IsInProgress)
                        LogService.Debug("Displaying match in progress");
                    else
                        LogService.Debug("Displaying last (already finished) match");

                    if (!NavigationHelper.NavigateTo("MainRegion", "MatchFoundNotification", navigationParameter, out Exception exception))
                        AppCriticalExceptionHandlerService.HandleCriticalError(exception);

#else //if RELEASE and successed then check is in progress
                    if(requestWrapper.Value.IsInProgress)
                    {
                        navigationParameter.Add("UserMatchData", requestWrapper.Value.Users);
                        navigationParameter.Add("MatchType", requestWrapper.Value.MatchType);

                        if (!NavigationHelper.NavigateTo("MainRegion", "MatchFoundNotification", navigationParameter, out Exception exception))
                            AppCriticalExceptionHandlerService.HandleCriticalError(exception);
                    }
                    else //if not in progress then display waiting screen
                    {
                        navigationParameter.Add("Message", "Waiting for match to start...");

                        RegionManager.RequestNavigate("MainRegion", "AppStateInfo", navigationParameter);
                    }
#endif
                }
                //DEBUG & RELEASE, handle failed request
                else //request not successed
                {
                    //log request fail
                    var requestError = RequestWrapperErrorDescriptor.GetRequestWrapperExceptionDescription(requestWrapper); //get exception description
                    LogService.Error($"Unable to complete match request: {requestError}");

                    //display error to user
                    navigationParameter.Add("description", $"Unable to get match data: {requestError}");
                    navigationParameter.Add("isError", true);

                    if (!NavigationHelper.NavigateTo("MainRegion", "AppStateInfo", navigationParameter, out Exception exception))
                        AppCriticalExceptionHandlerService.HandleCriticalError(exception);
                }
            }
            catch (Exception e) //we have to handle most of exceptions while executing 'async void'
            {
                //todo: make sure these can be rethrow from 'async void'
                //todo: if not forward them to App.HandleCriticalError(...)
                //if (
                //    e is StackOverflowException ||
                //    e is ThreadAbortException ||
                //    e is AccessViolationException
                //   )
                //    throw e; //rethrow if not related with method/or shouldn't be manually handled

                //log exception and terminate app, as it should be done default in case of unhandled exception
                //App.HandleCriticalError(e);

                Dictionary<string, object> logProperties = new Dictionary<string, object>();
                logProperties.Add("stack", e.StackTrace);

                AppCriticalExceptionHandlerService.HandleCriticalError(e);
                LogService.Error($"Unknow error occured while updating match data: {e.ToString()}", logProperties);
            }
            finally
            {
                canUpdateMatchData = true;
                //matchUpdateCancellationToken = CancellationToken.None; //timeout logic moved into UserRankServiec //clear cancellation token
            }

        }

        void ToggleWindowVisibility()
        {
            LogService.Info("Toggling window visibility");

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
            LogService.Info($"Seting windows opacity to: {opacity}");

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

            LogService.Debug($"CurrentMaxOpacity set to: {CurrentMaxOpacity}");

            //check if current window opacity need update
            //if window is hidden
            if(Opacity > minimumVisibleOpacity)
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
