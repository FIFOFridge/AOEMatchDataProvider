using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Request;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace AOEMatchDataProvider.ViewModels
{
    public class InitialResourcesValidationViewModel : BindableBase, INavigationAware
    {
        System.Windows.Visibility resourcesStateErrorVisibility;
        System.Windows.Visibility ResourcesStateErrorVisibility { get => resourcesStateErrorVisibility; set => SetProperty(ref resourcesStateErrorVisibility, value); }

        string resourcesStateDescription;
        public string ResourcesStateDescription { get => resourcesStateDescription; set => SetProperty(ref resourcesStateDescription, value); }

        string appState;
        public string AppState { get => appState; set => SetProperty(ref appState, value); }

        bool isDownlaodingAppResources;
        public bool IsDownlaodingAppResources { get => isDownlaodingAppResources; set => SetProperty(ref isDownlaodingAppResources, value); }

        bool canRetry;
        public bool CanRetry { get => canRetry; set => SetProperty(ref canRetry, value); }

        ILogService LogService { get; }
        IStorageService StorageService { get; }
        IEventAggregator EventAggregator { get; }
        IUserRankService UserRankService { get; }
        IApplicationCommands ApplicationCommands { get; }

        public DelegateCommand RetryUpdateCommand { get; set; }

        Timer retryTimer;

        public InitialResourcesValidationViewModel(IStorageService storageService, ILogService logService, IEventAggregator eventAggregator, IUserRankService userRankService, IApplicationCommands applicationCommands)
        {
            StorageService = storageService;
            LogService = logService;
            EventAggregator = eventAggregator;
            UserRankService = userRankService;
            ApplicationCommands = applicationCommands;

            EventAggregator.GetEvent<ViewDestroyed>().Subscribe(
                HandleViewDestroyed,
                ThreadOption.UIThread,
                false,
                (context) => context.DataContext == this //make sure we handle correct view destruction
                );

            RetryUpdateCommand = new DelegateCommand(ValidateResourcesState);

            ResourcesStateErrorVisibility = System.Windows.Visibility.Collapsed;
            ValidateResourcesState();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ApplicationCommands.SetMaxWindowOpacity.Execute(1);

            NavigationHelper.TryNavigateTo("QuickActionRegion", "BottomShadowPanel", null, out _);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        //todo: make app check for updates
        void CheckForAppUpdate()
        {
            throw new NotImplementedException();
        }

        void ValidateResourcesState()
        {
            if (!(StorageService.Has("stringResources")))
            {
                LogService.Info("Downloading app resources");
                ResourcesStateDescription = "Downloading app resources";

                Task.Run(UpdateResources)
                    .ContinueWith(HandleUpdateTaskCompleted);

                if (retryTimer != null)
                    retryTimer.Dispose();

                retryTimer = new Timer(1000 * 10);
                retryTimer.Elapsed += RetryTimer_Elapsed;
                retryTimer.AutoReset = false;
                retryTimer.Start();
            }
        }

        void RetryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //release timer resources
            retryTimer.Stop();
            retryTimer.Dispose();

            UpdateCanRetry(); //update cooldown
        }

        async Task UpdateResources()
        {
            if (IsDownlaodingAppResources)
                return;

            IsDownlaodingAppResources = true;

            try
            {
                RequestWrapper<AoeNetAPIStringResources> resourcesRequestWrapper = await UserRankService.GetStringResources();

                if (!resourcesRequestWrapper.IsSuccess)
                    throw new InvalidOperationException($"Unable to update game resources: {resourcesRequestWrapper.Exception.ToString()}");

                StorageService.Create("stringResources", resourcesRequestWrapper.Value, StorageEntryExpirePolicy.Expire, DateTime.UtcNow.AddDays(3));
                StorageService.Flush();
            }
            finally
            {
                IsDownlaodingAppResources = false;
            }
        }

        void HandleUpdateTaskCompleted(Task updateTask)
        {
            //Handle update error
            if(updateTask.IsFaulted || updateTask.IsCanceled)
            {
                ResourcesStateDescription = "Unable to download app resources, please try again for a while";

                string exceptionDump = string.Empty;
                string stackDump = string.Empty;

                //capter "first" expcetion
                if(updateTask.Exception != null)
                {
                    exceptionDump = updateTask.Exception.ToString();
                    stackDump = updateTask.Exception.StackTrace;

                    //override if inner aviable
                    if (updateTask.Exception.InnerExceptions != null)
                    {
                        exceptionDump = updateTask.Exception.InnerExceptions.First().ToString();
                        stackDump = updateTask.Exception.InnerExceptions.First().StackTrace;
                    }
                }

                var logProperties = new Dictionary<string, object>();
                logProperties.Add("stack", stackDump);
                logProperties.Add("exception dump", exceptionDump);
                logProperties.Add("task status", updateTask.Status);

                LogService.Error("Unable to complete update task successfully", logProperties);

                UpdateCanRetry();
                ResourcesStateErrorVisibility = System.Windows.Visibility.Visible;

                return;
            } 
            else
            {
                //make sure to check user ID is set
                App.Current.Dispatcher.Invoke(() =>
                {
                    App.InitialNavigation();
                });
            }
        }

        void UpdateCanRetry()
        {
            //timer is running
            if (retryTimer == null && !retryTimer.Enabled)
            {
                CanRetry = false;
                return;
            }

            //update is running
            if(isDownlaodingAppResources)
            {
                CanRetry = false;
                return;
            }

            //if not updating and cooldown passed then allow to retry update
            CanRetry = true;
        }

        void HandleViewDestroyed(UserControl userControl)
        {
            if (retryTimer != null) {
                //stop if needed
                if (retryTimer.Enabled) {
                    retryTimer.Stop();
                }

                retryTimer.Dispose();
            };
        }
    }
}
