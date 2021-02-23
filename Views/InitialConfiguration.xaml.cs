using AOEMatchDataProvider.Events.Views.InitialConfiguration;
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
    public partial class InitialConfiguration : UserControl
    {
        IEventAggregator EventAggregator { get; }

        public InitialConfiguration(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            InitializeComponent();
        }

        private void UserId_TextChanged(object sender, TextChangedEventArgs e)
        {
            EventAggregator.GetEvent<UserIdChangedEvent>().Publish(UserId.Text);
        }
    }
}
