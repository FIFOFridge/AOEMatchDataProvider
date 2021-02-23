using AOEMatchDataProvider.Models.User;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.Settings
{
    public class AppSettings : BindableBase, ISerializableModel
    {
        UserId userId;
        public UserId UserId { get => userId; set => SetProperty(ref userId, value); }

        //public bool closeOnAoeExit;

        #region Shell opacity for specific views
        double notificationOpacity;
        public double NotificationOpacity { get => notificationOpacity; set => SetProperty(ref notificationOpacity, value); }
        double teamsPanelOpacity;
        public double TeamsPanelOpacity { get => teamsPanelOpacity; set => SetProperty(ref teamsPanelOpacity, value); }
        double appStateInfoOpacity;
        public double AppStateInfoOpacity { get => appStateInfoOpacity; set => SetProperty(ref appStateInfoOpacity, value); }
        #endregion

        #region Hotkeys
        //TODO: implement in VMs
        
        //toggle
        System.Windows.Forms.Keys toggleKey;
        System.Windows.Forms.Keys ToggleKey { get => toggleKey; set => SetProperty(ref toggleKey, value); }

        //notification(s)
        System.Windows.Forms.Keys notificationShow;
        System.Windows.Forms.Keys NotificationShow { get => notificationShow; set => SetProperty(ref notificationShow, value); }

        System.Windows.Forms.Keys notificationSkip;
        System.Windows.Forms.Keys NotificationSkip { get => notificationSkip; set => SetProperty(ref notificationSkip, value); }
        #endregion

        public AppSettings()
        {
            NotificationOpacity = 0.55;
            TeamsPanelOpacity = 0.9;
            AppStateInfoOpacity = 0.85;

            this.ToggleKey = System.Windows.Forms.Keys.End;
            this.NotificationShow = System.Windows.Forms.Keys.Home;
            this.NotificationSkip = System.Windows.Forms.Keys.End;
        }

        [JsonConstructor]
        internal AppSettings(
            double AppStateInfoOpacity, 
            double TeamsPanelOpacity, 
            double NotificationOpacity, 
            System.Windows.Forms.Keys ToggleKey, 
            System.Windows.Forms.Keys NotificationShow, 
            System.Windows.Forms.Keys NotificationSkip
            )
        {
            this.AppStateInfoOpacity = AppStateInfoOpacity;
            this.TeamsPanelOpacity = TeamsPanelOpacity;
            this.NotificationOpacity = NotificationOpacity;
            
            this.ToggleKey = ToggleKey;
            this.NotificationShow = NotificationShow;
            this.NotificationSkip = NotificationSkip;
        }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<AppSettings>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
