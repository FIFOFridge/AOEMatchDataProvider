using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.ViewModels
{
    public class AppInactivityViewModel : BindableBase, INavigationAware
    {
        ILogService LogService { get; }
        IApplicationCommands ApplicationCommands { get; }
        IKeyHookService KeyHookService { get; }

        public DelegateCommand RequestAppStateUpdateCommand { get; set; }
        
        bool isRequestAppStateUpdateEnabled;
        public bool IsRequestAppStateUpdateEnabled
        {
            get
            {
                return isRequestAppStateUpdateEnabled;
            }

            set
            {
                SetProperty(ref isRequestAppStateUpdateEnabled, value);
            }
        }

        string tokenKeyHandlerHome;
        string tokenKeyHandlerEnd;

        public AppInactivityViewModel(ILogService logService, IApplicationCommands applicationCommands, IKeyHookService keyHookService)
        {
            LogService = logService;
            ApplicationCommands = applicationCommands;
            KeyHookService = keyHookService;

            RequestAppStateUpdateCommand = new DelegateCommand(RequestAppStateUpdate);

            IsRequestAppStateUpdateEnabled = true;

            tokenKeyHandlerEnd = KeyHookService.Add(System.Windows.Forms.Keys.Home, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));
            tokenKeyHandlerHome = KeyHookService.Add(System.Windows.Forms.Keys.End, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsRequestAppStateUpdateEnabled = true;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) 
        {
            KeyHookService.Remove(tokenKeyHandlerHome);
            KeyHookService.Remove(tokenKeyHandlerEnd);
        }

        void RequestAppStateUpdate()
        {
            if (!IsRequestAppStateUpdateEnabled)
                return;

            IsRequestAppStateUpdateEnabled = false;

            LogService.Debug("Requesting app status update by user");
            ApplicationCommands.UpdateMatchDataCommand.Execute(null);
        }
    }
}
