using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace JyGame
{
	public partial class MultiSelectBox : UserControl
	{
        public UIHost uiHost;
		public MultiSelectBox()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public JyGame.GameData.CommonSettings.IntCallBack callback = null;

        public void Show(string title,List<string> options, JyGame.GameData.CommonSettings.IntCallBack cb)
        {
            callback = cb;
            this.selectPanel.Children.Clear();
            this.title.Text = title;
            int index = 0;
            foreach (var o in options)
            {
                MultiSelectBoxItem item = new MultiSelectBoxItem(o, index++, Callback);
                this.selectPanel.Children.Add(item);
            }
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Callback(int rst)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            callback(rst);
        }


	}
}