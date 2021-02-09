using AOEMatchDataProvider.Mvvm;
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
    public partial class Shell : Window, IShell
    {
        bool ignoreInput;
        public bool IgnoreInput
        {
            get
            {
                return ignoreInput;
            }

            set
            {
                if (value == true)
                    EnableIgnoreInput();
                else
                    DisableIgnoreInput();

                ignoreInput = value;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        Timer refreshMousePositionTimer;

        public Shell()
        {
            InitializeComponent();

            Unloaded += Shell_Unloaded;

            ignoreInput = false;
        }

        Window IShell.Shell => this;

        void EnableIgnoreInput()
        {
            if(refreshMousePositionTimer != null)
            {
                if(refreshMousePositionTimer.Enabled)
                {
                    refreshMousePositionTimer.Stop();
                }

                refreshMousePositionTimer.Dispose();
            }

            refreshMousePositionTimer = new Timer
            {
                AutoReset = true,
                Interval = 1000 / 10
            };
            refreshMousePositionTimer.Elapsed += RefreshMousePositionTimer_Elapsed;
            refreshMousePositionTimer.Start();
        }

        static int marginHorziontal = 100;
        static int marginVertical = 100;

        private void RefreshMousePositionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var point = GetMousePosition();
            bool isNearHorizontal = false;
            bool isNearVertical = false;

            App.Current.Dispatcher.Invoke(() =>
            {
                if (point.X < (this.Left + this.Width + marginHorziontal))
                    isNearHorizontal = true;
                else
                    isNearHorizontal = false;

                if (point.Y < (this.Top + this.Height + marginVertical))
                    isNearVertical = true;
                else
                    isNearVertical = false;

                if (isNearHorizontal && isNearVertical)
                    this.Opacity = 0;
                else
                    this.Opacity = 1;
            });
        }

        void DisableIgnoreInput()
        {
            if (refreshMousePositionTimer != null)
            {
                if (refreshMousePositionTimer.Enabled)
                {
                    refreshMousePositionTimer.Stop();
                }

                refreshMousePositionTimer.Dispose();
            }
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        enum WindowStyles
        {
            WM_NCHITTEST = 0x84,
            HTTRANSPARENT = -1
        }
    }
}
