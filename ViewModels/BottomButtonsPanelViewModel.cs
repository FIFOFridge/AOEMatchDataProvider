using AOEMatchDataProvider.Helpers.Navigation;
using AOEMatchDataProvider.Helpers.Windows;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.ViewModels
{
    public class BottomButtonsPanelViewModel : BindableBase
    {
        public DelegateCommand GithubClickCommand { get; }
        public DelegateCommand SettingsClickCommand { get; }
        public DelegateCommand CoffeClickCommand { get; }

        public BottomButtonsPanelViewModel()
        {
            GithubClickCommand = new DelegateCommand(() => ExternalRedirectionHelper.RedirectTo("https://github.com/FIFOFridge/AOEMatchDataProvider"));
            //todo: create separated settings window
            //SettingsClickCommand = new DelegateCommand(() => NavigationHelper.NavigateTo("MainRegion", "InitialConfigurationViewModel", null));
            CoffeClickCommand = new DelegateCommand(() => ExternalRedirectionHelper.RedirectTo("coffe-redirection")); //patch before release
        }
    }
}
