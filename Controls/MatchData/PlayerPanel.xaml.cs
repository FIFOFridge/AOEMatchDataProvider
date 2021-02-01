using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace AOEMatchDataProvider.Controls.MatchData
{
    /// <summary>
    /// Logika interakcji dla klasy PlayerPanel.xaml
    /// </summary>
    public partial class PlayerPanel : UserControl, INotifyPropertyChanged
    {
        public PlayerPanel()
        {
            DataContext = this;

            //RightBorderColor = ""
            InitializeComponent();
        }

        #region Dependency properties
        #region TeamGameElo
        public static readonly DependencyProperty TeamGameEloProperty = 
            DependencyProperty.Register("TeamGameElo", typeof(string), typeof(PlayerPanel));

        public string TeamGameElo
        {
            get { return GetValue(TeamGameEloProperty) as string; }
            set { SetValue(TeamGameEloProperty, value); }
        }
        #endregion

        #region OneVsOneElo
        public static readonly DependencyProperty OneVsOneEloProperty =
            DependencyProperty.Register("OneVsOneElo", typeof(string), typeof(PlayerPanel));

        public string OneVsOneElo
        {
            get { return GetValue(OneVsOneEloProperty) as string; }
            set { SetValue(TeamGameEloProperty, value); }
        }
        #endregion

        #region PlayerName
        public static readonly DependencyProperty PlayerNameProperty =
            DependencyProperty.Register("PlayerName", typeof(string), typeof(PlayerPanel));

        public string PlayerName
        {
            get { return GetValue(PlayerNameProperty) as string; }
            set { SetValue(PlayerNameProperty, value); }
        }
        #endregion

        #region Civilization
        public static readonly DependencyProperty CivilizationProperty =
            DependencyProperty.Register("Civilization", typeof(string), typeof(PlayerPanel));

        public string Civilization
        {
            get { return GetValue(CivilizationProperty) as string; }
            set { SetValue(CivilizationProperty, value); }
        }
        #endregion

        #region PlayerColor
        public static readonly DependencyProperty PlayerColorProperty =
            DependencyProperty.Register("PlayerColor", typeof(string), typeof(PlayerPanel));

        public string PlayerColor
        {
            get { return GetValue(PlayerColorProperty) as string; }
            set
            {
                SetValue(PlayerColorProperty, value);
                //RightBorderColor = "Red";//Colors.Red;//(Color)ColorConverter.ConvertFromString(value);

                //update gradient which which depend on player color
                //rightBorderBackground = CreateRightBorderGradient(value);
                //OnPropertyChanged("RightBorderBackground"); //update binding value
            }
        }
        #endregion

        #region UserGameProfileId
        public static readonly DependencyProperty UserGameProfileIdProperty =
            DependencyProperty.Register("UserGameProfileId", typeof(string), typeof(PlayerPanel));

        public string UserGameProfileId
        {
            get { return GetValue(UserGameProfileIdProperty) as string; }
            set
            {
                SetValue(UserGameProfileIdProperty, value);
            }
        }
        #endregion
        #endregion

        #region Properties
        //LinearGradientBrush rightBorderBackground;
        //public LinearGradientBrush RightBorderBackground
        //{
        //    get
        //    {
        //        return rightBorderBackground;
        //    }
        //}

        string rightBorderColor;
        public string RightBorderColor
        {
            get
            {
                return rightBorderColor;
            }

            set
            {
                rightBorderColor = value;
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

        LinearGradientBrush CreateRightBorderGradient(string color)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.EndPoint = new Point(0.5, 1);
            gradientBrush.StartPoint = new Point(0.5, 0);

            var playerColor = (Color)ColorConverter.ConvertFromString(color);

            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.90)); //transparent
            gradientBrush.GradientStops.Add(new GradientStop(playerColor, 0.0));

            return gradientBrush;
        }
    }
}
