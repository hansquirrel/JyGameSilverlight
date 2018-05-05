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

namespace JyGame
{
	public partial class GameOver : UserControl
	{
		public GameOver()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.HeadImage.Source = RuntimeData.Instance.Team[0].Head;
            this.nameLabel.Text = RuntimeData.Instance.Team[0].Name;
            AudioManager.PlayMusic(ResourceManager.Get("音乐.游戏失败"));
        }

		private void returnButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
			// 在此处添加事件处理程序实现。
            this.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.mainMenu.Load();
		}
	}
}