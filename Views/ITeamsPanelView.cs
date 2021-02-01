using AOEMatchDataProvider.Controls.MatchData;
using AOEMatchDataProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AOEMatchDataProvider.Views
{
    //TODO: Refector for event aggregator compatibility
    public interface ITeamsPanelView
    {
        //    WrapPanel Team1 { get; }
        //    WrapPanel Team2 { get; }

        void Add(Models.UserMatchData userStats);
        void Update(Models.UserMatchData userStats);
        void Clear();
    }
}
