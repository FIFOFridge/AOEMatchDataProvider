using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events;
using AOEMatchDataProvider.Extensions.ExceptionHandling;
using AOEMatchDataProvider.Extensions.ObjectExtension;
using AOEMatchDataProvider.Models.Settings;
using AOEMatchDataProvider.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AOEMatchDataProvider.ViewModels
{
    public class AppSettingsViewModel : BindableBase
    {
        #region Services
        ILogService LogService { get; }
        IStorageService StorageService { get; }
        IApplicationCommands ApplicationCommands { get; }
        IAppCriticalExceptionHandlerService IAppCriticalExceptionHandlerService { get; }
        IEventAggregator EventAggregator { get; }
        #endregion

        #region (Binding source) Shell views opacity sliders
        //double notificationOpacity;
        //public double NotificationOpacity { get => notificationOpacity; set => SetProperty(ref notificationOpacity, value); }
        //double teamsPanelOpacity;
        //public double TeamsPanelOpacity { get => teamsPanelOpacity; set => SetProperty(ref teamsPanelOpacity, value); }
        //double appStateInfoOpacity;
        //public double AppStateInfoOpacity { get => appStateInfoOpacity; set => SetProperty(ref appStateInfoOpacity, value); }
        public AppSettings AppSettingsToApply { get; }
        #endregion

        #region Commands
        public DelegateCommand SubmitSettingsChangeCommand { get; }
        public DelegateCommand<object> WindowCloseCommand { get; }
        #endregion

        public AppSettingsViewModel(
            ILogService logService, 
            IStorageService storageService, 
            IApplicationCommands applicationCommands,
            IEventAggregator eventAggregator
            )
        {
            LogService = logService;
            StorageService = storageService;
            ApplicationCommands = applicationCommands;
            EventAggregator = eventAggregator;

            //LoadValuesFromCurrentSettings();
            
            //at this point we require that setting entity already exist and their default values 
            //were already set while creating initial settings model by InitialConfigurationViewModel
            AppSettingsToApply = StorageService.Get<AppSettings>("settings").Copy(); //deep copy model

            SubmitSettingsChangeCommand = new DelegateCommand(SubmitSettingsChange);
            WindowCloseCommand = new DelegateCommand<object>(WindowClose);
        }

        void LoadValuesFromCurrentSettings()
        {
            //if(!StorageService.TryGet<AppSettings>("settings", out AppSettings appSettings))
            
            //try
            //{
            //    //at this point we require that setting entity already exist and their default values 
            //    //were already set while creating initial settings model by InitialConfigurationViewModel
            //    var appSettings = StorageService.Get<AppSettings>("settings");

            //    this.NotificationOpacity = appSettings.NotificationOpacity;
            //    this.TeamsPanelOpacity = appSettings.TeamsPanelOpacity;
            //    this.AppStateInfoOpacity = appSettings.AppStateInfoOpacity;
            //}
            //catch(Exception e)
            //{
            //    e.RethrowIfExceptionCantBeHandled();

            //    IAppCriticalExceptionHandlerService.HandleCriticalError(e);
            //}
        }

        void SubmitSettingsChange()
        {
            //TODO: perform allowed values range check
            StorageService.Update("settings", AppSettingsToApply);
            StorageService.Flush(); //save settings

            EventAggregator.GetEvent<AppSettingsChangedEvent>().Publish();
        }

        void WindowClose(object window)
        {
            (window as Window).Close();
        }
    }
}
