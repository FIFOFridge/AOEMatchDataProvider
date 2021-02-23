using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events;
using AOEMatchDataProvider.Extensions.ExceptionHandling;
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

            AppSettingsToApply = StorageService.Get<AppSettings>("settings");

            SubmitSettingsChangeCommand = new DelegateCommand(SubmitSettingsChange);
            WindowCloseCommand = new DelegateCommand<object>(WindowClose);
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
