using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
                LogService.Info($"Window opacity has been changed to: {value}");
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
                LogService.Info($"CanUpdateMatchData has been changed to: {value}");
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

        public DelegateCommand CloseAppCommand { get; private set; }

        public Models.Match CurrentMatch { get; set; }

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
            //SetTransparencyCommand = new DelegateCommand<object>(SetWindowOpacity);

            ApplicationCommands = applicationCommands;

            //register UpdateMatchDataCommand app command
            ApplicationCommands.UpdateMatchDataCommand.RegisterCommand(UpdateMatchDataCommand);
            ApplicationCommands.ToggleWindowVisibility.RegisterCommand(ToggleWindowVisibilityCommand);
            ApplicationCommands.ShowWindow.RegisterCommand(ShowWindowCommand);
            ApplicationCommands.HideWindow.RegisterCommand(HideWindowCommand);
            ApplicationCommands.SetTransparency.RegisterCommand(SetTransparencyCommand);
            #endregion Setup Application Commands
            #region Window/Shell Commands
            CloseAppCommand = new DelegateCommand(CloseApp);
            #endregion

            //setup app toggle key
            //keyHookService.Add(System.Windows.Forms.Keys.Home, ToggleWindowVisibilityCommand.Execute);

            CanUpdateMatchData = true;
        }

        async void UpdateMatchData()
        {
            if (!canUpdateMatchData) //make sure we can update
                return;

            canUpdateMatchData = false;

            NavigationParameters navigationParameter = new NavigationParameters();
            RequestWrapper<Models.Match> requestWrapper = null;

            try
            {
                //userId is missing so redirect it to configuration screen
                if (!(StorageService.TryGet("settings", out object appSettings)))
                {
                    RegionManager.RequestNavigate("MainRegion", "InitialConfiguration");
                }

                requestWrapper = await UserRankService.GetUserMatch(((AppSettings)appSettings).UserId, AppConfigurationService.MatchUpdateTimeout);

                //if request failed then display API connection issues
                if (requestWrapper == null || requestWrapper.Exception != null)
                {
                    var logProperties = new Dictionary<string, object>();
                    logProperties.Add("exception", requestWrapper.Exception);
                    logProperties.Add("stack", requestWrapper.Exception.StackTrace);

                    LogService.Error($"Unable to update match data: {requestWrapper.Exception.ToString()}", logProperties);

                    navigationParameter.Add("description", "Can not connect to API");

                    if (!NavigationHelper.NavigateTo("MainRegion", "MatchFoundNotification", navigationParameter, out Exception exception))
                        AppCriticalExceptionHandlerService.HandleCriticalError(exception);
                }

                //if current match is already displayed then skip
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
                    navigationParameter.Add("IsError", true);

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
                Opacity = 0;
            }
            else //show if hidden
            {
                Opacity = 1;
            }
        }

        void SetWindowOpacity(object opacity)
        {
            LogService.Info($"Seting windows opacity to: {opacity}");

            //todo: add safe checks, handle convertion exceptions
            var _opacity = double.Parse(opacity.ToString());

            Opacity = _opacity;
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
    }
}
