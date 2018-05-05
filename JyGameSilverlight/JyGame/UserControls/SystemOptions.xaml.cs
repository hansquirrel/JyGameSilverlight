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
	public partial class SystemOptions : UserControl
	{
        public UIHost uiHost;

		public SystemOptions()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

		private void save_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.saveLoadPanel.Show(true);
            this.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void load_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.saveLoadPanel.Show(false);
            this.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void exit_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            if (MessageBox.Show("确认返回么？未保存的资料将会丢失。", "返回主菜单", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                uiHost.mainMenu.Load();
                this.Visibility = System.Windows.Visibility.Collapsed;
                
            }
		}

		private void save_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			// 在此处添加事件处理程序实现。
		}

		private void evn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.envsetPanel.Show();
		}
	}
}