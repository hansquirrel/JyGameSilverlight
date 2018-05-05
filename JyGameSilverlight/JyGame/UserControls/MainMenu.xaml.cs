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
using System.Windows.Browser;

namespace JyGame
{
	public partial class MainMenu : UserControl
	{
        public UIHost uiHost;

		public MainMenu()
		{
			// 为初始化变量所必需
			InitializeComponent();

            fadeOut.Completed += (s, e) =>
            {
                fadeOut.Stop();
                this.Visibility = System.Windows.Visibility.Collapsed;
                this.IsHitTestVisible = true;
            };
		}

        public void Load()
        {
            OLBattleGlobalSetting.Instance.OLGame = false;
            this.Visibility = System.Windows.Visibility.Visible;
            uiHost.RightClickCallback = OnMouseRightClick;
            uiHost.saveLoadPanel.LoadCallback = OnLoadGame;
            //AudioManager.PlayMusic(ResourceManager.Get("音乐.jyonlineBGM"));

            string[] welcomeMusicList = new string[]{
                "音乐.武侠回忆",
            };
            string music = welcomeMusicList[Tools.GetRandomInt(0, welcomeMusicList.Length - 1)];
            AudioManager.PlayMusic(ResourceManager.Get(music));

            //suggestTip.Show();
        }

        private void OnMouseRightClick()
        {
            uiHost.saveLoadPanel.Hide();
        }

        private void OnLoadGame()
        {
            this.fadeOut.Begin();
            uiHost.reset();
        }

		private void newgame_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            RuntimeData.Instance.Init();
            RuntimeData.Instance.gameEngine.RollRole();
            this.fadeOut.Begin();
            this.IsHitTestVisible = false;
		}

		private void loadgame_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.saveLoadPanel.Show(false);
		}

		private void aboutus_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            NextGameState next = new NextGameState();
            next.Type = "story";
            next.Value = "aboutus_main";
            this.Visibility = System.Windows.Visibility.Collapsed;
            RuntimeData.Instance.gameEngine.CallScence(RuntimeData.Instance.gameEngine.uihost.mapUI, next);
		}

		private void onlineGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            this.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.reset();
            uiHost.onlineGamePanel.Load();
		}

		private void audioButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.audioPanel.Show();
		}

		private void donateButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            //HtmlPage.Window.Navigate(new Uri(CommonSettings.BBSUrl, UriKind.RelativeOrAbsolute), "_blank");
            uiHost.mapUI.showDonate();
		}

		private void setEnvButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.envsetPanel.Show();
		}

		private void TanHaoImage_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
            TanHaoStory.AutoReverse = true;
            TanHaoStory.RepeatBehavior = RepeatBehavior.Forever;
            TanHaoStory.Begin();
		}

		private void mods_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            MessageBox.Show("即将开放，尽请期待。");
		}
	}
}