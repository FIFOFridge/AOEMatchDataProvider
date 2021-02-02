using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AOEMatchDataProvider.Controls.Common
{
    /// <summary>
    /// Logika interakcji dla klasy KeyHint.xaml
    /// </summary>
    public partial class KeyHint : UserControl
    {
        #region DependencyProperties
        public new static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register("Foreground", typeof(string), typeof(KeyHint));

        public new string Foreground
        {
            get { return (string)GetValue(ForegroundProperty); }
            set
            {
                SetValue(ForegroundProperty, value);
                OnPropertyChanged();
            }
        }

        public new static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register("Background", typeof(string), typeof(KeyHint));

        public new string Background
        {
            get { return (string)GetValue(BackgroundProperty); }
            set
            {
                SetValue(BackgroundProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(KeyHint));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged();
            }
        }

        public new static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register("FontSize", typeof(string), typeof(KeyHint));

        public new string FontSize
        {
            get { return (string)GetValue(FontSizeProperty); }
            set
            {
                SetValue(FontSizeProperty, value);
                OnPropertyChanged();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public KeyHint()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
