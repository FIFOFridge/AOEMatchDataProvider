using AOEMatchDataProvider.Converters;
using AOEMatchDataProvider.Events.Views.TeamsPanel;
using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Models.Match;
using AOEMatchDataProvider.Models.User;
using AOEMatchDataProvider.Resources.Other;
using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    public partial class PlayerPanel2 : UserControl, INotifyPropertyChanged
    {
        UserRankModeConverter userRankModeConverter;

        #region DependencyProperties
        public static readonly DependencyProperty UserMatchDataProperty =
        DependencyProperty.Register("UserMatchData", typeof(UserMatchData), typeof(PlayerPanel2));

        public UserMatchData UserMatchData
        {
            get { return GetValue(UserMatchDataProperty) as UserMatchData; }
            set
            {
                SetValue(UserMatchDataProperty, value);
                OnPropertyChanged();
                UpdateUserELO();
                UpdateBorderColour();
            }
        }

        public static readonly DependencyProperty MatchTypeProperty =
        DependencyProperty.Register("MatchType", typeof(MatchType), typeof(PlayerPanel2));

        public MatchType MatchType
        {
            get { return (MatchType)GetValue(MatchTypeProperty); }
            set
            {
                SetValue(MatchTypeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty PrimaryUserRankModeDisplayProperty =
        DependencyProperty.Register("PrimaryUserRankModeDisplay", typeof(UserRankMode?), typeof(PlayerPanel2));

        public UserRankMode? PrimaryUserRankModeDisplay
        {
            get { return (UserRankMode?)GetValue(PrimaryUserRankModeDisplayProperty); }
            set
            {
                SetValue(PrimaryUserRankModeDisplayProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty SecondaryUserRankModeDisplayProperty =
        DependencyProperty.Register("SecondaryUserRankModeDisplay", typeof(UserRankMode?), typeof(PlayerPanel2));

        public UserRankMode? SecondaryUserRankModeDisplay
        {
            get { return (UserRankMode?)GetValue(SecondaryUserRankModeDisplayProperty); }
            set
            {
                SetValue(SecondaryUserRankModeDisplayProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty ContentAlignProperty =
        DependencyProperty.Register("ContentAlign", typeof(PlayerPanelContentAlignMode), typeof(PlayerPanel2));

        public PlayerPanelContentAlignMode ContentAlign
        {
            get { return (PlayerPanelContentAlignMode)GetValue(ContentAlignProperty); }
            set
            {
                SetValue(ContentAlignProperty, value);
                OnPropertyChanged();
                UpdateBorderPlacment();
            }
        }
        #endregion

        #region Properties

        string borderColor;
        public string BorderColor
        {
            get
            {
                return borderColor;
            }

            protected set
            {
                borderColor = value;
                //CreateRightBorderGradient(value);
                OnPropertyChanged();
            }
        }

        int borderColorGradientAngle;
        public int BorderColorGradientAngle
        {
            get
            {
                return borderColorGradientAngle;
            }

            protected set
            {
                borderColorGradientAngle = value;
                OnPropertyChanged();
            }
        }
        #region Content Alignment Properties
        HorizontalAlignment contentHorizontalAlignment;
        public HorizontalAlignment ContentHorizontalAlignment
        {
            get
            {
                return contentHorizontalAlignment;
            }

            protected set
            {
                contentHorizontalAlignment = value;

                OnPropertyChanged();
            }
        }

        int borderColumnIndex;
        public int BorderColumnIndex
        {
            get
            {
                return borderColumnIndex;
            }

            protected set
            {
                borderColumnIndex = value;
                OnPropertyChanged();
            }
        }

        int horizontalBorderColumnIndex;
        public int HorizontalBorderColumnIndex
        {
            get
            {
                return horizontalBorderColumnIndex;
            }

            protected set
            {
                horizontalBorderColumnIndex = value;
                OnPropertyChanged();
            }
        }

        int baseElementColumnIndex;
        public int BaseElementColumnIndex
        {
            get
            {
                return baseElementColumnIndex;
            }

            protected set
            {
                baseElementColumnIndex = value;
                OnPropertyChanged();
            }
        }

        string leftColumnWidth;
        public string LeftColumnWidth
        {
            get
            {
                return leftColumnWidth;
            }

            protected set
            {
                leftColumnWidth = value;
                OnPropertyChanged();
            }
        }

        string rightColumnWidth;
        public string RightColumnWidth
        {
            get
            {
                return rightColumnWidth;
            }

            protected set
            {
                rightColumnWidth = value;
                OnPropertyChanged();
            }
        }

        #endregion

        string primaryELO;
        public string PrimaryELO
        {
            get
            {
                return primaryELO;
            }

            set
            {
                primaryELO = value;
                OnPropertyChanged();
            }
        }

        string secondaryELO;
        public string SecondaryELO
        {
            get
            {
                return secondaryELO;
            }

            set
            {
                secondaryELO = value;
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

        ILogService LogService { get; }

        public PlayerPanel2()
        {
            DataContext = this;
            InitializeComponent();

            LogService = App.Resolve<ILogService>();

            userRankModeConverter = new UserRankModeConverter();
        }

        public void UpdateUserELO()
        {
            //primary elo
            try
            {
                if (PrimaryUserRankModeDisplayProperty == DependencyProperty.UnsetValue)
                    PrimaryELO = "";

                PrimaryELO = (string)userRankModeConverter.Convert(UserMatchData.UserRankData, typeof(string), PrimaryUserRankModeDisplay, CultureInfo.CurrentCulture);
            }
            catch (Exception e)
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                    throw e;

                PrimaryELO = "-";

                LogService.Warning($"Exception occured during elo convertion: PrimaryUserRankModeDisplay: {PrimaryUserRankModeDisplay}, exception: {e.ToString()}");
            }

            //secondary elo
            try
            {
                if (SecondaryUserRankModeDisplayProperty == DependencyProperty.UnsetValue)
                    SecondaryELO = "";

                SecondaryELO = (string)userRankModeConverter.Convert(UserMatchData.UserRankData, typeof(string), SecondaryUserRankModeDisplay, CultureInfo.CurrentCulture);
            }
            catch (Exception e)
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                    throw e;

                SecondaryELO = "-";

                LogService.Warning($"Exception occured during elo convertion: SecondaryUserRankModeDisplay: {SecondaryUserRankModeDisplay}, exception: {e.ToString()}");
            }
        }

        void UpdateBorderColour()
        {
            if (UserMatchData.Color == null)
                throw new InvalidOperationException("Player color can not be null");

            //BorderColor = UserMatchData.Color;
            
            var key = UserMatchData.Color.ToLower();
            if(ColorsMaping.Colors.TryGetValue(key, out string mapedColor))
                BorderColor = mapedColor;
            else
                BorderColor = "Black";
        }

        //reposition contents placments binding based on ContentAlign dependency property
        void UpdateBorderPlacment()
        {
            if (ContentAlign == PlayerPanelContentAlignMode.Left)
            {
                BorderColumnIndex = 0;
                ContentHorizontalAlignment = HorizontalAlignment.Left;
                BorderColorGradientAngle = -90;
            }
            else //right
            {
                BorderColumnIndex = 2;
                ContentHorizontalAlignment = HorizontalAlignment.Right;
                BorderColorGradientAngle = 90;
            }
        }
    }

    public enum PlayerPanelContentAlignMode
    {
        Left,
        Right
    }
}
