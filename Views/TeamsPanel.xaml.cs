﻿using AOEMatchDataProvider.Controls.MatchData;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Extensions;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AOEMatchDataProvider.Views
{
    public partial class TeamsPanel : UserControl//, ITeamsPanelView
    {
        List<Models.UserMatchData> UserMatchData { get; }

        IEventAggregator EventAggregator { get; }
        ILogService LogService { get; }

        public TeamsPanel(IEventAggregator eventAggregator, ILogService logService)
        {
            InitializeComponent();
            //this.DataContext = new TeamsPanelViewModel(this); //TODO: Refector for event aggregator compatibility
            //eventAggregator.GetEvent<PlayerCollectionChanged>().Subscribe(HandleTeamsPanelStateChanged);
            //eventAggregator.GetEvent<PlayerDataChanged>().Subscribe(HandlePlayerDataChanged);

            EventAggregator = eventAggregator;
            LogService = logService;

            UserMatchData = new List<Models.UserMatchData>();

            EventAggregator.GetEvent<UserCollectionChanged>().Subscribe(HandleUserCollectionChanged);
            EventAggregator.GetEvent<UserRatingChangedEvent>().Subscribe(HandleUserRatingChanged);
        }

        void HandleUserCollectionChanged(IEnumerable<Models.UserMatchData> userMatchData)
        {
            var logProperties = new Dictionary<string, object>
            {
                { "userMatchData", userMatchData.ToString() }
            };
            LogService.Debug("Updating user collection", logProperties);

            Clear();
            foreach (var userStats in userMatchData)
            {
                Add(userStats);
            }
        }

        void HandleUserRatingChanged(Tuple<UserGameProfileId, UserRankData> userRatingData)
        {
            var id = userRatingData.Item1;
            var rank = userRatingData.Item2;

            var found = false;
            Models.UserMatchData targetData = null;

            team1.Dispatcher.Invoke(() =>
            {
                //search for data related with userGameProfileId in fisrt collection (team 1)
                foreach (var child in team1.Children)
                {
                    var control = child as PlayerPanel2;

                    if (control.UserMatchData.UserGameProfileId.ProfileId == id.ProfileId)
                    {
                        targetData = control.UserMatchData;
                        break;
                    }
                }

                //search for data related with userGameProfileId in second collection (team 2)
                foreach (var child in team2.Children)
                {
                    if (targetData != null)
                        break;

                    var control = child as PlayerPanel2;

                    if (control.UserMatchData.UserGameProfileId.ProfileId == id.ProfileId)
                    {
                        targetData = control.UserMatchData;
                        break;
                    }
                }

                //if trying to update not existing id then throw
                //if (!found)
                //    throw new ArgumentOutOfRangeException("Invalid user game profile id to update");

                if (found)
                    targetData.MergeUserRank(rank);
                else
                {
                    var logProperties = new Dictionary<string, object>();
                    logProperties.Add("Team 1: ", team1.Children);
                    logProperties.Add("Team 2: ", team2.Children);
                    logProperties.Add("User id to update: ", id);
                    LogService.Error("Unable to find game profileId to update");
                }
            });
        }

        public void Add(Models.UserMatchData userMatchData)
        {
            //get correct elo(s) types to display
            var formatingTargetTypes = EloFormattController.GetTargetEloTypes(userMatchData.MatchType);

            var playerPanel = new PlayerPanel2
            {
                //PlayerName = userStats.Name,
                //Civilization = userStats.Civilization,
                //PlayerColor = userStats.Color,
                //OneVsOneElo = null,
                //TeamGameElo = null,
                PrimaryUserRankModeDisplay = formatingTargetTypes.Item1,
                SecondaryUserRankModeDisplay = formatingTargetTypes.Item2,
                UserMatchData = userMatchData,
                Width = 275
            };

            if (userMatchData.Team == 1)
            {
                playerPanel.ContentAlign = PlayerPanelContentAlignMode.Left;
                team1.Children.Add(playerPanel);
            }
            else if(userMatchData.Team == 2)
            {
                playerPanel.ContentAlign = PlayerPanelContentAlignMode.Right;
                team2.Children.Add(playerPanel);
            }
        }

        public void Clear()
        {
            UserMatchData.Clear();
            team1.Children.Clear();
            team2.Children.Clear();
        }

        internal static class EloFormattController
        {
            //get primary and/or secondary types of elo to display for each user
            internal static Tuple<
                UserRankMode?, 
                UserRankMode?
                > GetTargetEloTypes(MatchType matchType)
            {
                UserRankMode? primary;
                UserRankMode? secondary;

                switch (matchType)
                {
                    case MatchType.RandomMap:
                        primary = UserRankMode.RandomMap;
                        secondary = null;
                        break;

                    case MatchType.TeamRandomMap:
                        primary = UserRankMode.RandomMap;
                        secondary = UserRankMode.TeamRandomMap;
                        break;

                    case MatchType.Deathmatch:
                        primary = UserRankMode.Deathmatch;
                        secondary = null;
                        break;

                    case MatchType.TeamdeathMatch:
                        primary = UserRankMode.Deathmatch;
                        secondary = UserRankMode.TeamdeathMatch;
                        break;

                    case MatchType.Unranked:
                        primary = UserRankMode.RandomMap;
                        secondary = null;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid match type");
                }

                return Tuple.Create(primary, secondary);
            }
        }
    }
}