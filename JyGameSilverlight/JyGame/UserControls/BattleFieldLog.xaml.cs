using JyGame.GameData;
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
	public partial class BattleFieldLog : UserControl
	{
		public BattleFieldLog()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

		private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Clear();
		}

		private void HideShowButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            IsHide = !IsHide;
		}

        public void Clear()
        {
            this.LogPanel.Children.Clear();
        }

        public void Add(string log)
        {
            TextBlock tb = new TextBlock() { 
                Text = log ,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.White),
                IsHitTestVisible = false
            };
            this.LogPanel.Children.Insert(0, tb);
        }

        private bool IsHide {
            set
            {
                if (value)
                {
                    this.LogView.Visibility = Visibility.Visible;
                    HideShowButton.Content = "隐藏";
                }
                else
                {
                    this.LogView.Visibility = Visibility.Collapsed;
                    HideShowButton.Content = "显示";
                }
            }
            get { return this.LogView.Visibility == Visibility.Visible; } }

        private void AutoBattleCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox.IsChecked != null)
                Configer.Instance.AutoBattle = (bool)(checkBox.IsChecked);
        }

        private void AutoBattleCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked != null)
                Configer.Instance.AutoBattle = (bool)(checkBox.IsChecked);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AutoBattleCheckBox.IsChecked = Configer.Instance.AutoBattle;
        }
	}
}