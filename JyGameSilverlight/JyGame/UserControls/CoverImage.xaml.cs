using System;
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
	public partial class CoverImage : UserControl
	{
        public JyGame.GameData.CommonSettings.VoidCallBack Callback;

		public CoverImage()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
            this.Visibility = System.Windows.Visibility.Collapsed;
            Callback();
		}
	}
}