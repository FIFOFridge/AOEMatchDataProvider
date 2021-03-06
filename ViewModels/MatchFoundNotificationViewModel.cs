﻿using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AOEMatchDataProvider.ViewModels
{
    public class MatchFoundNotificationViewModel : BindableBase, INavigationAware
    {
        NavigationParameters parameters;

        IKeyHookService KeyHookService { get; }
        IApplicationCommands ApplicationCommands { get; }
        IAppCriticalExceptionHandlerService AppCriticalExceptionHandlerService { get; }
        IEventAggregator EventAggregator { get; }
        IStorageService StorageService { get; }

        string keyHandlerTokenHome;
        string keyHandlerTokenEnd;

        public MatchFoundNotificationViewModel(
            IKeyHookService keyHookService,
            IApplicationCommands applicationCommands,
            IEventAggregator eventAggregator,
            IAppCriticalExceptionHandlerService appCriticalExceptionHandlerService,
            IStorageService storageService)
        {
            KeyHookService = keyHookService;
            ApplicationCommands = applicationCommands;
            EventAggregator = eventAggregator;
            AppCriticalExceptionHandlerService = appCriticalExceptionHandlerService;
            StorageService = storageService;

            keyHandlerTokenHome = KeyHookService.Add(System.Windows.Forms.Keys.Home, DisplayMatch);
            keyHandlerTokenEnd = KeyHookService.Add(System.Windows.Forms.Keys.End, HideWindowAndLoadMatchInBackground);

            EventAggregator.GetEvent<ViewDestroyedEvent>()
            .Subscribe(
                HandleUnload,
                ThreadOption.UIThread,
                false,
                view => view.DataContext == this //if view has this context then and only then handle operation
            );
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //re enable mouse input
            //Shell.IgnoreInput = false;
            ApplicationCommands.SetIgnoreInput.Execute(false);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //validate parameters
            if (!navigationContext.Parameters.ContainsKey("UserMatchData"))
                throw new ArgumentNullException();

            if (!navigationContext.Parameters.ContainsKey("MatchType"))
                throw new ArgumentNullException();

            NavigationHelper.TryNavigateTo("QuickActionRegion", "BottomShadowPanel", null, out _);

            UpdateMaxOpacity();
            EventAggregator.GetEvent<AppSettingsChangedEvent>().Subscribe(UpdateMaxOpacity);

            //ignore mouse input
            //Shell.IgnoreInput = true;
            ApplicationCommands.SetIgnoreInput.Execute(true);
            parameters = navigationContext.Parameters;
        }

        void DisplayMatch()
        {
            ApplicationCommands.ShowWindow.Execute(null);

            if(!NavigationHelper.TryNavigateTo("MainRegion", "TeamsPanel", parameters, out Exception exception))
                AppCriticalExceptionHandlerService.HandleCriticalError(exception);
        }

        void HideWindowAndLoadMatchInBackground()
        {
            //hide window and then navigate to match
            ApplicationCommands.HideWindow.Execute(null);

            if (!NavigationHelper.TryNavigateTo("MainRegion", "TeamsPanel", parameters, out Exception exception))
                AppCriticalExceptionHandlerService.HandleCriticalError(exception);
        }

        void UpdateMaxOpacity()
        {
            var settings = StorageService.Get<AppSettings>("settings");

            ApplicationCommands.SetMaxWindowOpacity.Execute(settings.NotificationOpacity);
        }

        void HandleUnload(UserControl view)
        {
            KeyHookService.Remove(keyHandlerTokenHome);
            KeyHookService.Remove(keyHandlerTokenEnd);
        }
    }
}
