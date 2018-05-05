using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public partial class SelectBox : UserControl
    {
        public SelectBox()
        {
            InitializeComponent();
        }

        public JyGame.GameData.CommonSettings.VoidCallBack CallBack;
        //public JyGame.GameData.CommonSettings.StringCallBack StringCallBack;
        public string currentSelection = "yes";

        public void ShowSelection()
        {
            this.Visibility = Visibility.Visible;
            this.Focus();
        }

        private void yes_Click(object sender, RoutedEventArgs e)
        {
            this.currentSelection = "yes";
            this.Visibility = Visibility.Collapsed;
            CallBack();
        }

        private void no_Click(object sender, RoutedEventArgs e)
        {
            this.currentSelection = "no";
            this.Visibility = Visibility.Collapsed;
            CallBack();
        }
    }
}
