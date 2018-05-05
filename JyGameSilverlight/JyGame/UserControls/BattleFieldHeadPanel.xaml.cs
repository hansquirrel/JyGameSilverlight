using JyGame.GameData;
using JyGame.UserControls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Linq;

namespace JyGame
{
	public partial class BattleFieldHeadPanel : UserControl
	{
		public BattleFieldHeadPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Clear()
        {
            this.HeadPanel.Children.Clear();
        }

        public void Reset(List<Spirit> spirits)
        {
            this.Clear();
            var sps = from sp in spirits orderby sp.Role.Attributes["shenfa"] descending select sp;
            foreach (var s in sps)
            {
                double width = this.ActualWidth;
                Image img = new Image() { Source = s.Role.Head, Width = width, Height = width, Opacity = 0.6 };
                this.HeadPanel.Children.Add(img);
                img.MouseLeftButtonUp += (ss,e)=>{
                    RuntimeData.Instance.gameEngine.uihost.rolePanel.Show(s.Role);
                };
                ToolTipService.SetToolTip(img, string.Format("点击查看{0}的状态", s.Role.Name));
            }
        }

        public void PopTop()
        {
            if (this.HeadPanel.Children.Count > 0)
            {
                Image topImage = this.HeadPanel.Children[0] as Image;
                topImage.Opacity = 1;
            }
        }

        public void RemoveTop()
        {
            if(this.HeadPanel.Children.Count>0)
                this.HeadPanel.Children.RemoveAt(0);
        }
	}
}