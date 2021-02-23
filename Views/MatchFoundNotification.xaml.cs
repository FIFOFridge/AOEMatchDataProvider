using AOEMatchDataProvider.Events.Views;
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
    public partial class MatchFoundNotification : UserControl
    {
        IEventAggregator EventAggregator { get; }

        public MatchFoundNotification(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            this.Unloaded += AppStateInfo_Unloaded;
        }

        private void AppStateInfo_Unloaded(object sender, RoutedEventArgs e)
        {
            EventAggregator.GetEvent<ViewDestroyedEvent>().Publish(this);
        }
    }
}
