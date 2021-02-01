using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Command
{
    public interface IApplicationCommands
    {
        CompositeCommand UpdateMatchDataCommand { get; }

        #region Shell managment
        CompositeCommand ToggleWindowVisibility { get; }
        CompositeCommand SetTransparency { get; }
        CompositeCommand HideWindow { get; }
        CompositeCommand ShowWindow { get; }
        #endregion Shell managment
    }
}
