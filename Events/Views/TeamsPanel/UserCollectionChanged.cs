using AOEMatchDataProvider.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Events.Views.TeamsPanel
{
    public class UserCollectionChanged : PubSubEvent<IEnumerable<UserMatchData>> { }
}
