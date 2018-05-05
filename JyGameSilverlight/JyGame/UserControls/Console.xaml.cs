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
	public partial class Console : UserControl
	{
		public Console()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            string cmd = commandText.Text;
            string[] tmp = cmd.Split(new char[] { ' ' });
            if (tmp.Length == 0 || tmp.Length > 2)
            {
                MessageBox.Show("invalid command");
                return;
            }
            else
            {
                NextGameState next = new NextGameState();
                next.Type = tmp[0];
                if(tmp.Length==2)
                    next.Value = tmp[1];

                RuntimeData.Instance.gameEngine.CallScence( RuntimeData.Instance.gameEngine.uihost.mapUI, next);
            }
		}
	}
}