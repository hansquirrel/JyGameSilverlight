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

namespace JyGame
{
	public partial class SuggestTip : UserControl
	{
		public SuggestTip()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show()
        {
            tips = new List<string>();
            foreach (var key in ResourceManager.ResourceMap.Keys)
            {
                if (key.StartsWith("小贴士"))
                {
                    tips.Add(ResourceManager.Get(key));
                }
            }
            this.Dispatcher.BeginInvoke(() => { NextTip(); });
        }

        List<string> tips = new List<string>();
		private void nextButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            NextTip();
		}

        private void NextTip()
        {
            string tipInfo = "";
            do
            {
                tipInfo = tips[Tools.GetRandomInt(0, tips.Count - 1)];
            } while (tipInfo.Equals(content.Text));
            content.Text = tipInfo;
        }
	}
}