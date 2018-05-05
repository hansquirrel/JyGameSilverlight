using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Threading;

namespace JyGame.UserControls
{
	public partial class DialogIndicator : UserControl
	{
        int picCurrent = 0;
        const int SWITCHTIME = 500;
        private DispatcherTimer Timer;
        List<ImageSource> Images = new List<ImageSource>();

		public DialogIndicator()
		{
			// 为初始化变量所必需
			InitializeComponent();

            /*Dispatcher.BeginInvoke(() =>
            {
                this.Storyboard1.Completed += (s, e) =>
                {
                    this.Storyboard1.Begin();
                };
                this.Storyboard1.Begin();
            });*/
            Timer = new DispatcherTimer();
            Images.Clear();
		}

        public void load_image()
        {
            if (Images.Count == 0)
            {
                Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.对话0"));
                Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.对话1"));
                Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.对话2"));
                Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.对话3"));
                Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.对话4"));
            }
        }

        public void start_tick()
        {
            load_image();
            Timer.Interval = TimeSpan.FromMilliseconds(SWITCHTIME);
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Start();
        }

        public void stop_tick()
        {
            Timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (picCurrent >= Images.Count)
            {
                picCurrent = 0;
            }

            image.Source = Images[picCurrent];
            picCurrent++;
        }
	}
}