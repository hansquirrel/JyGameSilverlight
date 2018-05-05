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
	public partial class MapPointer : UserControl
	{
		public MapPointer()
		{
			// 为初始化变量所必需
			InitializeComponent();

            Dispatcher.BeginInvoke(() =>
            {
                this.Storyboard1.Completed += (s, e) =>
                {
                    this.Storyboard1.Begin();
                };
                this.Storyboard1.Begin();
            });
		}
	}
}