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
	public partial class AttackInfoNew : UserControl
	{
		public AttackInfoNew()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        private Canvas ParentCanvas;
        public void Show(Color fontColor , int xpos, int ypos, string information, Canvas rootCanvas)
        {
            ParentCanvas = rootCanvas;
            info.Text = information;
            info2.Text = information;
            info.Foreground = new SolidColorBrush(fontColor);
            ParentCanvas.Children.Add(this);
            this.Animation.Completed += (s,e) =>
            {
                this.Animation.Stop();
                ParentCanvas.Children.Remove(this);
            };
            Canvas.SetZIndex(this, 99999);
            Canvas.SetLeft(this, xpos - this.Width / 2 + 50 );
            Canvas.SetTop(this, ypos - this.Height/2 + 20 );
            this.Animation.Begin();
        }
	}
}