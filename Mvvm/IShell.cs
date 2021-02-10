using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AOEMatchDataProvider.Mvvm
{
    public interface IShell
    {
        Window Shell { get; }

        //bool IgnoreInput { get; set; }
    }
}
