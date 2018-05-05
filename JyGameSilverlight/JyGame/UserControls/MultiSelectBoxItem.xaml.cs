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
	public partial class MultiSelectBoxItem : UserControl
	{
        JyGame.GameData.CommonSettings.IntCallBack callback = null;

        public MultiSelectBoxItem(string content, int tag, JyGame.GameData.CommonSettings.IntCallBack cb)
		{
			InitializeComponent();
            this.content.Foreground = new SolidColorBrush(Colors.Yellow);
            this.Tag = tag;
            this.content.Text = content;
            //this.BackRect.Visibility = Visibility.Collapsed;
            callback = cb;
		}

		private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
            this.content.Foreground = new SolidColorBrush(Colors.Red);
            //this.BackRect.Visibility = Visibility.Visible;
		}

		private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
            this.content.Foreground = new SolidColorBrush(Colors.Yellow);
            //this.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
            //this.BackRect.Visibility = Visibility.Collapsed;
		}

		private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            if(this.Tag!=null)
                callback((int)this.Tag);
		}
	}
}