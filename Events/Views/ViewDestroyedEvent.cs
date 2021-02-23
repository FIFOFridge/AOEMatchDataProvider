using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AOEMatchDataProvider.Events.Views
{
    public class ViewDestroyedEvent : PubSubEvent<UserControl> { }
}
