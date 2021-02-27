using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Events.Views.InitialConfiguration;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Validation;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Models.User;
using AOEMatchDataProvider.Services;
using AOEMatchDataProvider.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.ViewModels
{
    public class InitialConfigurationViewModel : BindableBase, INavigationAware
    {
        UserIdMode detectedUserIdMode;
        string userId;

        int mainRegionRowSpan;
        int MainRegionRowSpan
        {
            get
            {
                return mainRegionRowSpan;
            }

            set
            {
                SetProperty(ref mainRegionRowSpan, value);
            }
        }

        string configurationMessage;
        public string ConfigurationMessage
        {
            get
            {
                return configurationMessage;
            }

            set
            {
                SetProperty(ref configurationMessage, value);
            }
        }

        string detectedUserIdModeDescription;
        public string DetectedUserIdModeDescription
        {
            get
            {
                return detectedUserIdModeDescription;
            }

            set
            {
                SetProperty(ref detectedUserIdModeDescription, value);
            }
        }

        bool canContinue;
        public bool CanContinue
        {
            get
            {
                return canContinue;
            }

            set
            {
                SetProperty(ref canContinue, value);
            }
        }

        bool isDownlaodingAppResources;
        public bool IsDownlaodingAppResources 
        { 
            get
            {
                return isDownlaodingAppResources;
            }
            
            set
            {
                SetProperty(ref isDownlaodingAppResources, value);
            }
        }

        public DelegateCommand ContinueCommand { get; }

        IApplicationCommands ApplicationCommands { get; }
        IEventAggregator EventAggregator { get; }
        IRegionManager RegionManager { get; }
        IStorageService StorageService { get; }
        IDataService UserRankService { get; }
        ILogService LogService { get; }

        public InitialConfigurationViewModel(
            IEventAggregator eventAggregator,
            IRegionManager regionManager,
            IStorageService storageService,
            IDataService userRankService,
            ILogService logService,
            IApplicationCommands applicationCommands)
        {
            EventAggregator = eventAggregator;
            RegionManager = regionManager;
            StorageService = storageService;
            UserRankService = userRankService;
            LogService = logService;
            ApplicationCommands = applicationCommands;

            EventAggregator.GetEvent<UserIdChangedEvent>().Subscribe(UserIdChangedHandler);

            ConfigurationMessage = "To setup app ente:\n - Your steam ID 64 (for Steam game version)";
            //ConfigurationMessage = "To setup app enter one of the following:\n - Your steam ID 64 (for Steam game version)\n - Your game profile ID(for Windows Store game version)";

            ContinueCommand = new DelegateCommand(ContinueClicked);

            detectedUserIdMode = UserIdMode.Invalid;

            UpdateCanContinue();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            NavigationHelper.TryNavigateTo("QuickActionRegion", "BottomShadowPanel", null, out _);
            ApplicationCommands.SetMaxWindowOpacity.Execute(1);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        void ContinueClicked()
        {
            if (!CanContinue)
                throw new InvalidOperationException("what the hell?");

            var settings = StorageService.Get<Models.Settings.AppSettings>("settings");

            if(detectedUserIdMode == UserIdMode.SteamId64)
            {
                //if(settings.UserId == null)
                //{
                    settings.UserId = new UserId();
                //}

                settings.UserId.SteamId = userId;
            }
            //else if(detectedUserIdMode == UserIdMode.GameProfileId)
            //{
            //    //if (settings.UserId == null)
            //    //{
            //        settings.UserId = new Models.UserId();
            //    //}

            //    settings.UserId.GameProfileId = userId;
            //}

            var navigationParameters = new NavigationParameters
            {
                { "description", "Waiting for match to start..." }
            };

            StorageService.Flush(); //save changes

            //navigate to teams panel
            RegionManager.RequestNavigate("MainRegion", "AppStateInfo", navigationParameters);
        }

        void UpdateUserIdDescription()
        {
            switch (detectedUserIdMode)
            {
                case UserIdMode.Invalid:
                    DetectedUserIdModeDescription = "Wrong ID format.";
                    return;
                    //break;
                case UserIdMode.GameProfileId:
                    DetectedUserIdModeDescription = "Detected: Game profile ID.";
                    break;
                case UserIdMode.SteamId64:
                    DetectedUserIdModeDescription = "Detected: Steam ID.";
                    break;
                case UserIdMode.SteamProfileURL:
                    DetectedUserIdModeDescription = "Detected: Steam profile link.";
                    break;
                default:
                    throw new InvalidOperationException("Invalid UserIdMode value");
            }
        }

        void UpdateCanContinue()
        {
            if (detectedUserIdMode != UserIdMode.Invalid && StorageService.Has("stringResources"))
                CanContinue = true;
            else
                CanContinue = false;
            
        }

        public void UserIdChangedHandler(string entry)
        {
            if(!(UserIdValidator.TryParseUserId(entry, out UserIdMode userIdMode)))
            {
                this.detectedUserIdMode = UserIdMode.Invalid;
                return;
            }

            userId = entry;

            detectedUserIdMode = userIdMode;
            UpdateUserIdDescription();
            UpdateCanContinue();
        }
    }
}
