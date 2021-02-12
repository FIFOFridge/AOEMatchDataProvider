using AOEMatchDataProvider.Command;
using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Services;
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
    public class AppStateInfoViewModel : BindableBase, INavigationAware
    {
        Timer updateTimer;

        string description;
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                SetProperty(ref description, value);
            }
        }

        IApplicationCommands ApplicationCommands { get; }
        IEventAggregator EventAggregator { get; }
        IAppConfigurationService AppConfigurationService { get; }
        IKeyHookService KeyHookService { get; }

        string tokenKeyHandlerHome;
        string tokenKeyHandlerEnd;

        public AppStateInfoViewModel(
            IApplicationCommands applicationCommands,
            IEventAggregator eventAggregator,
            IAppConfigurationService appConfigurationService,
            IKeyHookService keyHookService
            )
        {
            KeyHookService = keyHookService;
            ApplicationCommands = applicationCommands;
            EventAggregator = eventAggregator;
            AppConfigurationService = appConfigurationService;

            tokenKeyHandlerHome = KeyHookService.Add(System.Windows.Forms.Keys.Home, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));
            tokenKeyHandlerEnd = KeyHookService.Add(System.Windows.Forms.Keys.End, () => ApplicationCommands.ToggleWindowVisibility.Execute(null));

            updateTimer = new Timer(AppConfigurationService.AppStateInfoUpdateTick)
            {
                AutoReset = true
            };
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Start();

            //handle "unload" event from view to cleanup VM resources
            EventAggregator.GetEvent<ViewDestroyed>()
                .Subscribe(
                    HandleUnload,
                    ThreadOption.UIThread,
                    false,
                    view => view.DataContext == this //if view has this context then handle operation
                );

////LIVEDEBUG with DEBUG flag combination will update app state asap and use even old match (usefull for debuging without AOE2DE itslef)
////remove LIVEDEBUG compilation flag to prevent instant app state update call
//#if LIVEDEBUG
//            UpdateTimer_Elapsed(null, null); //instant call
//#endif
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!(navigationContext.Parameters.ContainsKey("description")))
                throw new InvalidOperationException($"Navigation event is missing 'description' parameter: {navigationContext}");

            NavigationHelper.TryNavigateTo("QuickActionRegion", "BottomButtonsPanel", null, out _);
            
            ApplicationCommands.SetMaxWindowOpacity.Execute(0.85);
            var description = navigationContext.Parameters["description"] as string;
            Description = description;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            //prevent view cache, we have to make sure navigation event parameters
            //will be passed and set each time when navigating to view
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) 
        {
            KeyHookService.Remove(tokenKeyHandlerHome);
            KeyHookService.Remove(tokenKeyHandlerEnd);
        }

        //run update match data, to determinate match status and update application state
        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ApplicationCommands.UpdateMatchDataCommand.CanExecute(null))
            {
                ApplicationCommands.UpdateMatchDataCommand.Execute(null);
            }
        }
        
        void HandleUnload(UserControl view)
        {
            KeyHookService.Remove(tokenKeyHandlerHome);
            KeyHookService.Remove(tokenKeyHandlerEnd);

            if (updateTimer != null) //cleanup timer
            {
                updateTimer.Stop();
                updateTimer.Dispose();
            }
        }
    }
}
