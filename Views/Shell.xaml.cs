using AOEMatchDataProvider.Events.Views;
using AOEMatchDataProvider.Events.Views.Shell;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AOEMatchDataProvider.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        IEventAggregator EventAggregator { get;}

        public Shell(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            
            EventAggregator = eventAggregator;
            Unloaded += Shell_Unloaded;

            this.SizeChanged += Shell_SizeChanged;
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            EventAggregator.GetEvent<ShellResizedEvent>().Publish(
                new Models.Views.Shell.Rectangle(
                    (int)this.Top,
                    (int)this.Left,
                    (int)this.Width,
                    (int)this.Height)
                );
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            //EventAggregator.GetEvent<ViewDestroyed>().Publish(this);
        }
    }
}
