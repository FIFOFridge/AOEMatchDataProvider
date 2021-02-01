using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace AOEMatchDataProvider.Command
{
    public class ApplicationCommands : IApplicationCommands
    {
        public CompositeCommand UpdateMatchDataCommand { get; } = new CompositeCommand();

        #region Shell managment
        public CompositeCommand ToggleWindowVisibility { get; } = new CompositeCommand();
        public CompositeCommand HideWindow { get; } = new CompositeCommand();
        public CompositeCommand ShowWindow { get; } = new CompositeCommand();
        public CompositeCommand SetTransparency { get; } = new CompositeCommand();
        #endregion Shell managment

        public ApplicationCommands() { }
    }
}
