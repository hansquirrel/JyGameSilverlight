using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.UserControls;
using JyGame.GameData;
using Effects.Shader;

namespace JyGame
{
	public partial class RoleDetailPanel : UserControl
	{
        public const int HPPanelOriginalWidth = 98;
        public const int HPPanelOriginalHeight = 11;
        public const int MPPanelOriginalWidth = 95;
        public const int MPPanelOriginalHeight = 11;

        public Role currentShowRole = null;

		public RoleDetailPanel()
		{
			InitializeComponent();
		}

        public void Show(Role role)
        {
            //TODO..
            //this.Head.Source = role.Head;
            //this.RoleInfo.Text = string.Format("\n {0}\n生命: {1}/{2}\n内力：{3}/{4}",role.Name,role.Hp,role.MaxHp,role.Mp,role.MaxMp);
            currentShowRole = role;

            if (role.Attributes["hp"] <= 0) role.Attributes["hp"] = 0;
            if (role.Attributes["mp"] <= 0) role.Attributes["mp"] = 0;

            //血槽长度变化
            double remainHPPct = (double)role.Attributes["hp"] / (double)role.Attributes["maxhp"];
            int HPPanelWidth = (int)((double)HPPanelOriginalWidth * remainHPPct);
            HP.Width = HPPanelWidth;

            double remainMPPct = (double)role.Attributes["mp"] / (double)role.Attributes["maxmp"];
            int MPPanelWidth = (int)((double)MPPanelOriginalWidth * remainMPPct);
            MP.Width = MPPanelWidth;

            head.Source = role.Head;

            roleName.Text = role.Name;
            /*TextStroke textStroke = new TextStroke();
            textStroke.SetValue(TextStroke.FontcolorProperty, Color.FromArgb(100, 200, 0, 0));
            textStroke.SetValue(TextStroke.BordercolorProperty, Color.FromArgb(200, 0, 0, 200));
            //textStroke.SetValue(TextStroke.DdxUvDdyUvProperty, Color.FromArgb(200, 0, 0, 200));
            roleName.Effect = textStroke;*/

            this.hpText.Text = string.Format("{0}/{1}", role.Attributes["hp"], role.Attributes["maxhp"]);
            this.mpText.Text = string.Format("{0}/{1}", role.Attributes["mp"], role.Attributes["maxmp"]);

            this.DrawBalls(role);
            this.Visibility = System.Windows.Visibility.Visible;

            this.FillBuffPanel();
        }

        private void FillBuffPanel()
        {
            this.buffPanel.Children.Clear();
            foreach (var buffInstance in this.currentShowRole.Buffs)
            {
                TextBlock tb = new TextBlock()
                {
                    FontSize = 10
                };
                tb.Foreground = buffInstance.IsDebuff ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Yellow);

                tb.Text = buffInstance.ToString();
                ToolTipService.SetToolTip(tb, ResourceManager.Get("buff." + buffInstance.buff.Name) + "\n" + buffInstance.Info());
                buffPanel.Children.Add(tb);
            }
        }

        public void DrawBalls(Role r)
        {
            ball1.Visibility = System.Windows.Visibility.Collapsed;
            ball2.Visibility = System.Windows.Visibility.Collapsed;
            ball3.Visibility = System.Windows.Visibility.Collapsed;

            if (r.Balls >= 1)
            {
                ball1.Visibility = System.Windows.Visibility.Visible;
            }
            if (r.Balls >= 3)
            {
                ball2.Visibility = System.Windows.Visibility.Visible;
            }
            if (r.Balls >= 5)
            {
                ball3.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }


	}
}