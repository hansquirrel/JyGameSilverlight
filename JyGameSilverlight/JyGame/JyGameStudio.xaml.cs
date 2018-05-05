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
	public partial class JyGameStudio : UserControl
	{
		public JyGameStudio()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        MainPage GameWindow;
        
        private void RestartGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.GameViewBox.Child = null;
            this.GameWindow = new MainPage();
            this.GameViewBox.Child = this.GameWindow;
        }

        public void GameState(NextGameState state)
        {
            this.CurrentGameState.Text = state.ToString();
        }

	}
}