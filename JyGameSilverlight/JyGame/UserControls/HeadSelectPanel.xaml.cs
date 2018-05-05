using JyGame.GameData;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JyGame
{
	public partial class HeadSelectPanel : UserControl
	{
		public HeadSelectPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show()
        {
            this.HeadPanel.Children.Clear();

            int k = 0;
            StackPanel sp = null;
            foreach (var s in Heads)
            {
                if (k % 4 == 0)
                {
                    k = 0;
                    sp = new StackPanel() { Orientation = Orientation.Horizontal };
                    HeadPanel.Children.Add(sp);
                }
                string key = s;
                Image img = new Image() { Source = ResourceManager.GetImage(s), Width = 80, Height = 80 };
                img.MouseLeftButtonUp += (ss, ee) =>
                {
                    if (this.Callback != null)
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        this.Dispatcher.BeginInvoke(() => { Callback(key); });
                    }
                };
                sp.Children.Add(img);
                k++;
            }
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public List<string> Heads = new List<string>();

        public CommonSettings.StringCallBack Callback = null;
	}
}