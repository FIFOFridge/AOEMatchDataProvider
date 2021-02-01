using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Resources.Other
{
    public static class ColorsMaping
    {
        public static IReadOnlyDictionary<string, string> Colors { get; }

        static Dictionary<string, string> colors;

        static ColorsMaping()
        {
            colors = new Dictionary<string, string>
            {
                { "Blue",   "#FF3545d4" },
                { "Red",    "#FFd43943" },
                { "Green",  "#FF58d439" },
                { "Yellow", "#FFe6d929" },
                { "Cyan",   "#FF35decd" },
                { "Grey",   "#FF5c5c5c" },
                { "Orange", "#FFdeae35" }
            };

            Colors = colors;
        }
    }
}
