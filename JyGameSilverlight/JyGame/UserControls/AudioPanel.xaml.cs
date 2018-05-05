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
	public partial class AudioPanel : UserControl
	{
		public AudioPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        private UIHost uiHost { get { return RuntimeData.Instance.gameEngine.uihost; } }

        private bool isInited = false;
        public void Show()
        {
            if (!isInited)
            {
                audioList.Items.Clear();
                foreach (var a in ResourceManager.ResourceMap)
                {
                    if (a.Key.Contains("音乐."))
                    {
                        TextBlock tb = new TextBlock() { Text = a.Key.Replace("音乐.", ""), Tag = a.Value };
                        audioList.Items.Add(tb);
                        audioList.SelectionChanged += (s, ee) =>
                        {
                            string uri = (audioList.SelectedItem as TextBlock).Tag as string;
                            AudioManager.PlayMusic(uri);
                        };
                    }
                }
                isInited = true;
            }
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Hide();
        }

	}
}