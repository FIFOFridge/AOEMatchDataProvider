using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Mvvm;
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
        IShell Shell { get; }

        public MatchFoundNotificationViewModel(
            IKeyHookService keyHookService, 
            IApplicationCommands applicationCommands,
            IEventAggregator eventAggregator,
            IAppCriticalExceptionHandlerService appCriticalExceptionHandlerService,
            IShell shell
            )
        {
            KeyHookService = keyHookService;
            ApplicationCommands = applicationCommands;
            EventAggregator = eventAggregator;
            AppCriticalExceptionHandlerService = appCriticalExceptionHandlerService;
            Shell = shell;

            KeyHookService.Add(this, System.Windows.Forms.Keys.Home, DisplayMatch);
            KeyHookService.Add(this, System.Windows.Forms.Keys.End, HideWindowAndLoadMatchInBackground);

            EventAggregator.GetEvent<ViewDestroyed>()
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
            Shell.IgnoreInput = false;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //validate parameters
            if (!navigationContext.Parameters.ContainsKey("UserMatchData"))
                throw new ArgumentNullException();

            if (!navigationContext.Parameters.ContainsKey("MatchType"))
                throw new ArgumentNullException();

            //ignore mouse input
            Shell.IgnoreInput = true;
            ApplicationCommands.SetTransparency.Execute(0.6);
            parameters = navigationContext.Parameters;
        }

        void DisplayMatch()
        {
            if(!NavigationHelper.NavigateTo("MainRegion", "TeamsPanel", parameters, out Exception exception))
                AppCriticalExceptionHandlerService.HandleCriticalError(exception);
        }

        void HideWindowAndLoadMatchInBackground()
        {
            //hide window and then navigate to match
            ApplicationCommands.ToggleWindowVisibility.Execute(null);

            if (!NavigationHelper.NavigateTo("MainRegion", "TeamsPanel", parameters, out Exception exception))
                AppCriticalExceptionHandlerService.HandleCriticalError(exception);
        }

        void HandleUnload(UserControl view)
        {
            KeyHookService.Remove(this, System.Windows.Forms.Keys.Home);
            KeyHookService.Remove(this, System.Windows.Forms.Keys.End);
        }
    }
}
