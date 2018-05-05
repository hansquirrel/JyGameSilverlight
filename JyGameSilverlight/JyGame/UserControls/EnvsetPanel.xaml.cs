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
	public partial class EnvsetPanel : UserControl
	{
        public UIHost uiHost = null;

		public EnvsetPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SkillAnimationSpeedCombo.Items.Add("快速");
            SkillAnimationSpeedCombo.Items.Add("正常");
            SkillAnimationSpeedCombo.Items.Add("慢速");
        }

        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            audioCheckBox.IsChecked = !AudioManager.IsAudioMuted;
            musicCheckBox.IsChecked = !AudioManager.IsMusicMuted;
            hightQualityAudio.IsChecked = Configer.Instance.HighQualityAudio;
            SkillAnimationSpeedCombo.SelectedIndex = (int)Configer.Instance.AnimationSpeed;
            autoSaveCheckBox.IsChecked = Configer.Instance.AutoSave;
            fullScreenCheckBox.IsChecked = Application.Current.Host.Content.IsFullScreen;
            autoBattleCheckBox.IsChecked = Configer.Instance.AutoBattle;
            danmuCheckBox.IsChecked = Configer.Instance.Danmu;
            jiqiCheckBox.IsChecked = Configer.Instance.JiqiAnimation;
        }

        private void SkillAnimationSpeedCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Configer.Instance.AnimationSpeed = (SkillAnimationSpeed)(sender as ComboBox).SelectedIndex;
        }

		private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            this.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void audioCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isMute = (sender as CheckBox).IsChecked;
            if (isMute != null)
            {
                Configer.Instance.Audio = (bool)isMute;
            }
		}

		private void musicCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isMute = (sender as CheckBox).IsChecked;
            if (isMute != null)
            {
                Configer.Instance.Music = (bool)isMute;
            }
		}

		private void hightQualityAudio_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if(isSet!=null)
            {
                Configer.Instance.HighQualityAudio = (bool)isSet;
            }
            AudioManager.Replay();
		}

		private void autoSaveCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if(isSet!=null)
            {
                Configer.Instance.AutoSave = (bool)isSet;
            }
		}

		private void fullScreenCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if (isSet != null)
            {
                Application.Current.Host.Content.IsFullScreen = (bool)isSet;
            }

		}

		private void autoBattleCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if (isSet != null)
            {
                Configer.Instance.AutoBattle = (bool)isSet;
            }
		}


		private void danmuCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if (isSet != null)
            {
                Configer.Instance.Danmu = (bool)isSet;
                if(Configer.Instance.Danmu)
                {
                    RuntimeData.Instance.gameEngine.uihost.DanmuCanvas.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    RuntimeData.Instance.gameEngine.uihost.DanmuCanvas.Visibility = System.Windows.Visibility.Collapsed ;
                }
            }
		}

		private void jiqiCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            bool? isSet = (sender as CheckBox).IsChecked;
            if (isSet != null)
            {
                Configer.Instance.JiqiAnimation = (bool)isSet;
            }
		}

		
	}
}