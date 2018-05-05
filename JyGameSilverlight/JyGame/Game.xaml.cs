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
	public partial class Game : UserControl
	{
		public Game()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //GameProject.LoadGameProject();
            this.mainPage.Focus();
        }
	}
}