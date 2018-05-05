using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.GameData;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Net;
using System.Windows.Browser;
using System.Windows.Threading;


namespace JyGame
{
	public partial class OLBattleStatusPanel : UserControl
	{
		public OLBattleStatusPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();

            textTimer = new DispatcherTimer();
            textTimer.Interval = TimeSpan.FromMilliseconds(CommonSettings.OL_LOAD_SHOWWORD_TIME);
            textTimer.Tick += new EventHandler(textShow);

            this.Visibility = Visibility.Collapsed;
		}
        
        DispatcherTimer textTimer;
        String dian = ".";
        String text = "耐心加载中";

        private void textShow(object sender, EventArgs args)
        {
            if (dian.Length >= 6)
                dian = ".";
            else
                dian += ".";

            this.textTip.Text = text + dian;
        }

        public void startShow(string str)
        {
            text = str;
            this.Visibility = Visibility.Visible;
            suggestTip.Show();
            textTimer.Start();
        }

        public void stopShow()
        {
            textTimer.Stop();
            this.Visibility = Visibility.Collapsed;
        }
	}
}