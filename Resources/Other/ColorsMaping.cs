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
                { "blue",   "#FF3165EA" },
                { "red",    "#FFEA3131" },
                { "green",  "#FF41C13E" },
                { "yellow", "#FFEFE83F" },
                { "cyan",   "#FF3EAAC1" },
                { "pink",   "#FFF14BE1" },
                { "grey",   "#FF454545" },
                { "orange", "#FFFBA23B" }
            };

            Colors = colors;
        }
    }
}
