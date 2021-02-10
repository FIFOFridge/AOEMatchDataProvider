using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Controls.MatchData;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Mvvm;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Views;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace AOEMatchDataProvider.ViewModels
{
    //todo: refactor TeamsPanel & TeamsPanelViewModel to Match & MatchViewModel
    public class TeamsPanelViewModel : BindableBase, INavigationAware
    {
        System.Timers.Timer updateTimer;

        List<Models.UserMatchData> userMatchData;
        public List<Models.UserMatchData> UserMatchData
        {
            get
            {
                return userMatchData;
            }

            set
            {
                SetProperty(ref userMatchData, value);
            }
        }

        MatchType matchType;
        public MatchType MatchType
        {
            get
            {
                return matchType;
            }

            set
            {
                SetProperty(ref matchType, value);
                //OnPropertyChanged();

                //https://www.tutorialspoint.com/chash-Put-spaces-between-words-starting-with-capital-letters
                MatchTypeFormatted = "match type: " + string.Concat(
                    matchType.ToString()
                    .Select(
                        x => char.IsUpper(x) ? " " + x : x.ToString()
                        ))
                    .TrimStart(' ');
            }
        }

        string matchTypeFormatted;
        string MatchTypeFormatted
        {
            get
            {
                return matchTypeFormatted;
            }

            set
            {
                SetProperty(ref matchTypeFormatted, value);
            }
        }

        string serverName;
        public string ServerName
        {
            get
            {
                return serverName;
            }

            set
            {
                var valueWithPrefix = "server: " + value;
                SetProperty(ref serverName, valueWithPrefix);
            }
        }

        IApplicationCommands ApplicationCommands { get; }
        IAppConfigurationService AppConfigurationService { get; }
        IEventAggregator EventAggregator { get; }
        IUserRankService UserRankService { get; }
        IKeyHookService KeyHookService { get; }
        ILogService LogService { get; }

        public TeamsPanelViewModel(
            IApplicationCommands applicationCommands,
            IAppConfigurationService appConfigurationService,
            IEventAggregator eventAggregator,
            IUserRankService userRankService,
            IKeyHookService keyHookService,
            ILogService logService
            )
        {
            ApplicationCommands = applicationCommands;
            AppConfigurationService = appConfigurationService;
            EventAggregator = eventAggregator;
            UserRankService = userRankService;
            KeyHookService = keyHookService;
            LogService = logService;

            //ApplicationCommands.ShowWindow.Execute(null);

            updateTimer = new System.Timers.Timer(AppConfigurationService.TeamPanelUpdateTick)
            {
                AutoReset = true
            };
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Start();

            //assign home/end keys with window visibility toggle
            KeyHookService.Add(this, System.Windows.Forms.Keys.Home, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));
            KeyHookService.Add(this, System.Windows.Forms.Keys.End, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));

            EventAggregator.GetEvent<ViewDestroyed>()
                .Subscribe(
                    HandleUnload,
                    ThreadOption.UIThread,
                    false,
                    view => view.DataContext == this //if view has this context then handle operation
                );
        }

        //Request more detailed user rank data if needed, only in case of team games, when team game elo is not not unreliable
        //so we are going to request 1v1 elo for specified game type
        async Task UpdateUsersData()
        {
            var logProperties = new Dictionary<string, object>
            {
                { "threadId", Thread.CurrentThread }
            };

            LogService.Debug("Starting user data update tasks", logProperties);

            List<Task> updateTasks = new List<Task>();

            UserRankMode userRankModeToUpadte = UserRankMode.RandomMap;

            switch (matchType)
            {
                //if game is 1v1 then all data is processed from match request, so we don't have to update anything
                case MatchType.RandomMap:
                    LogService.Debug("Skiping update", logProperties);
                    return;
                case MatchType.Deathmatch:
                    LogService.Debug("skiping update", logProperties);
                    return;

                //update 1v1 ratings for team matches
                case MatchType.TeamdeathMatch:
                    userRankModeToUpadte = UserRankMode.Deathmatch;
                    break;
                case MatchType.TeamRandomMap:
                    userRankModeToUpadte = UserRankMode.RandomMap;
                    break;

                //update 1v1 
                case MatchType.Unranked:
                    userRankModeToUpadte = UserRankMode.RandomMap;
                    break;

                //if match is custom game (any unraked, includes all "quick match" types) then use Random map rating (most reliable type across rating types)
                default:
                    //userRankModeToUpadte = UserRankMode.RandomMap; 
                    break;
            }

            int userELOUpdateTimeout = AppConfigurationService.DefaultRequestTimeout;

            foreach (var user in UserMatchData)
            {
                //update primary/secondary ELO
                LogService.Info($"Starting user ELO update: for user id: {user.UserGameProfileId} user rank mode to update: {userRankModeToUpadte}, operation timeout: {userELOUpdateTimeout}");

                var userDataUpdateTask = UserRankService.GetUserRank(user.UserGameProfileId, userRankModeToUpadte, userELOUpdateTimeout);

                updateTasks.Add(
                    userDataUpdateTask.ContinueWith(
                            t => HandleUserDataUpdated(userDataUpdateTask, userRankModeToUpadte, user.UserGameProfileId)
                        )
                    );
            }

            await Task.WhenAll(updateTasks);

            LogService.Debug("All updates has been completed", logProperties);
        }

        void HandleUserDataUpdated(Task<Helpers.Request.RequestWrapper<UserRank>> updateTask, UserRankMode userRankMode, UserGameProfileId userGameProfileId)
        {
            Dictionary<string, object> logProperties = null;

            //unahndled exception inside task scope
            if (updateTask.IsFaulted || updateTask.IsCanceled)
            {
                if (updateTask.Exception != null)
                {
                    logProperties = new Dictionary<string, object>
                    {
                        { "exception", updateTask.Exception.ToString() },
                        { "stack", updateTask.Exception.StackTrace }
                    };
                }

                LogService.Error("HandleUserDataUpdated: Internal task processing exception", logProperties ?? null);
                return;
            }

            //handled exception
            if (!updateTask.Result.IsSuccess)
            {
                if (updateTask.Exception != null)
                {
                    logProperties = new Dictionary<string, object>
                    {
                        { "exception", updateTask.Exception.ToString() },
                        { "stack", updateTask.Exception.StackTrace }
                    };
                }

                LogService.Warning("HandleUserDataUpdated: Unable to complete task successfully", logProperties);
                return;
            }

            if (updateTask.Exception != null)
            {
                LogService.Warning("Error while updating user rating: ");
                return;
            }

            UserRankData userRankData = new UserRankData();
            userRankData.UserRatings.Add(userRankMode, updateTask.Result.Value);

            //publish rating change event
            EventAggregator.GetEvent<UserRatingChangedEvent>().Publish(Tuple.Create(userGameProfileId, userRankData));
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ApplicationCommands.UpdateMatchDataCommand.CanExecute(null))
            {
                ApplicationCommands.UpdateMatchDataCommand.Execute(null);
            }
        }

        //have to include: player list + match type 
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!navigationContext.Parameters.ContainsKey("UserMatchData"))
                throw new ArgumentNullException();

            if (!navigationContext.Parameters.ContainsKey("MatchType"))
                throw new ArgumentNullException();

            ApplicationCommands.SetMaxWindowOpacity.Execute(0.9);

            UserMatchData = (List<Models.UserMatchData>)navigationContext.Parameters["UserMatchData"];
            MatchType = (MatchType)navigationContext.Parameters["MatchType"];

            NavigationHelper.NavigateTo("QuickActionRegion", "BottomButtonsPanel", null, out _);

            EventAggregator.GetEvent<UserCollectionChanged>().Publish(userMatchData);
            Task.Run(UpdateUsersData);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        void HandleUnload(UserControl view)
        {
            KeyHookService.Remove(this, System.Windows.Forms.Keys.Home);
            KeyHookService.Remove(this, System.Windows.Forms.Keys.End);

            if (updateTimer != null) //cleanup timer
            {
                updateTimer.Stop();
                updateTimer.Dispose();
            }
        }
    }
}
