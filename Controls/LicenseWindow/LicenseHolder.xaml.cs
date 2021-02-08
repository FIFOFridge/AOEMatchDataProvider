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

namespace AOEMatchDataProvider.Controls.LicenseWindow
{
    /// <summary>
    /// Logika interakcji dla klasy LicenseHolder.xaml
    /// </summary>
    public partial class LicenseHolder : UserControl, INotifyPropertyChanged
    {
        #region DependencyProperties
        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(LicenseHolder));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty UrlProperty =
        DependencyProperty.Register("Url", typeof(string), typeof(LicenseHolder));

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set
            {
                SetValue(UrlProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty LicenseTypeProperty =
        DependencyProperty.Register("LicenseType", typeof(string), typeof(LicenseHolder));

        public string LicenseType
        {
            get { return (string)GetValue(LicenseTypeProperty); }
            set
            {
                SetValue(LicenseTypeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty LicenseContentProperty =
        DependencyProperty.Register("LicenseContent", typeof(string), typeof(LicenseHolder));

        public string LicenseContent
        {
            get { return (string)GetValue(LicenseContentProperty); }
            set
            {
                SetValue(LicenseContentProperty, value);
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


        public LicenseHolder()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
