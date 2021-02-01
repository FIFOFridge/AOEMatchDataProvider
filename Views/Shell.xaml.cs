using AOEMatchDataProvider.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                //make sure hook is set
                if (value == true)
                {
                    if (!IsWindowProcHooked)
                        HookWindowProc();
                }

                ignoreInput = value;
            }
        }

        bool IsWindowProcHooked
        {
            get
            {
                return hwndSourceHook != null;
            }
        }

        HwndSource source;
        HwndSourceHook hwndSourceHook;

        public Shell()
        {
            InitializeComponent();

            Unloaded += Shell_Unloaded;

            ignoreInput = false;
        }

        Window IShell.Shell => this;

        void HookWindowProc()
        {
            if (!IsWindowProcHooked)
            {
                source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                hwndSourceHook = new HwndSourceHook(WndProc);
                source.AddHook(hwndSourceHook);
            }
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (IgnoreInput)
            {
                //
                if (msg == (int)WindowStyles.WM_NCHITTEST)
                    return (IntPtr)WindowStyles.HTTRANSPARENT;
                else
                    return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            if (IsWindowProcHooked)
            {
                source.RemoveHook(hwndSourceHook);
            }
        }

        enum WindowStyles
        {
            WM_NCHITTEST = 0x84,
            HTTRANSPARENT = -1
        }
    }
}
