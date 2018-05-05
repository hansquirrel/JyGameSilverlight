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

namespace JyGame
{
	public partial class MapEvent : UserControl
	{
        int picCurrent = 0;
        const int SWITCHTIME = 300;
        private DispatcherTimer Timer;
        List<ImageSource> Images = new List<ImageSource>();

		public MapEvent()
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
            Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.事件1"));
            Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.事件2"));
            Images.Add(JyGame.GameData.ResourceManager.GetImage("UI.事件3"));
            picCurrent = Tools.GetRandomInt(0, 2);
		}

        public void switch2event()
        {
            start_tick();
            
        }

        public void ShowEventTag()
        {
            this.TanHaoStory.AutoReverse = true;
            this.TanHaoStory.RepeatBehavior = RepeatBehavior.Forever;
            this.TanHaoStory.Begin();
            this.tanHaoImage.Visibility = System.Windows.Visibility.Visible;
        }

        public void switch2noevent()
        {
            stop_tick();
            this.image.Source = Tools.GetImage("/Resource/ui/event_null.png");
        }

        public void switch2enter()
        {
            //stop_tick();
            //this.image.Source = Tools.GetImage("/Resource/ui/event_active.png");
        }

        public void switch2selfimage(ImageSource source)
        {
            stop_tick();
            this.image.Source = source;
        }

        public void start_tick()
        {
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