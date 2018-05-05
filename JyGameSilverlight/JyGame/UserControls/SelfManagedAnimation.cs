using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public class SelfManagedAnimation
    {
        public SelfManagedAnimation(
            Canvas parentCanvas,
            int repeatTime,
            AnimationGroup group, 
            int switchTime,
            double x,
            double y,
            double width=0,
            double height=0,
            JyGame.GameData.CommonSettings.VoidCallBack callBack=null,
            int Z_INDEX=CommonSettings.Z_SKILL,
            double opacity = 1,
            bool removeLast = true,
            bool anchor = true) //x、y为中心点
        {
            this.removeLast = removeLast;

            _image = new Image() { Height = height, Width = width, Opacity = opacity };
            //_image.CacheMode = new BitmapCache();
            
            
            _image.IsHitTestVisible = false;

            parentCanvas.Children.Add(_image);
            Canvas.SetLeft(_image, x);
            Canvas.SetTop(_image, y);
            Canvas.SetZIndex(_image, Z_INDEX);
            callback = callBack;
            basex = x;
            basey = y;
            this.anchor = anchor;
            ParentCanvas = parentCanvas;
            Animation = group;
            RepeatTime = repeatTime;
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(switchTime);
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Start();
        }
        private bool anchor = true;
        private double basex = 0;
        private double basey = 0;
        private Image _image = null;
        JyGame.GameData.CommonSettings.VoidCallBack callback = null;

        private DispatcherTimer Timer { get; set; }
        private Canvas ParentCanvas { get; set; }
        private int RepeatTime = 1;
        private BattleField ParentBattleField { get; set; }
        private AnimationGroup Animation { get; set; }
        private bool removeLast = true;

        /// <summary>
        /// 图片切换时间
        /// 单位：毫秒
        /// </summary>
        private int SwitchTime { get; set; }
        private int NowPic = 0;
        private int repeat = 0;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (NowPic >= Animation.Images.Count)
            {
                repeat++;
                NowPic = 0;
                if (repeat >= RepeatTime)
                {
                    Timer.Stop();
                    if (removeLast)
                        ParentCanvas.Children.Remove(_image);
                    else
                        NowPic = Animation.Images.Count - 1;
                    if(callback != null) callback();
                }
            }
            AnimationImage img = Animation.Images[NowPic];
            if (anchor)
            {
                _image.Width = img.W;
                _image.Height = img.H;
                Canvas.SetLeft(_image, basex - img.AnchorX);
                Canvas.SetTop(_image, basey - img.AnchorY);
            }
            _image.Source = img.Image;
            NowPic++;
        }

        public void remove()
        {
            Timer.Stop();
            ParentCanvas.Children.Remove(_image);
        }
    }
}
