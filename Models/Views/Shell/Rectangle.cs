using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.Views.Shell
{
    public class Rectangle
    {
        public Int32 Top { get; }
        public Int32 Left { get; }
        public Int32 Width { get; }
        public Int32 Height { get; }

        public Rectangle(int top, int left, int width, int height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }
    }
}
