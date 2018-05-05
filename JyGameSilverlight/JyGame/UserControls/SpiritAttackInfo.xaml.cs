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

namespace JyGame
{
	public partial class SpiritAttackInfo : UserControl
	{
		public SpiritAttackInfo()
		{
			// 为初始化变量所必需
			InitializeComponent();
            this.AttackInfoStory.Completed += new EventHandler(AttackInfoStory_Completed);
            this.CriticalHitStory.Completed += new EventHandler(CriticalHitStory_Completed);
		}

        void CriticalHitStory_Completed(object sender, EventArgs e)
        {
            Finished();
        }

        void AttackInfoStory_Completed(object sender, EventArgs e)
        {
            Finished();
        }

        void Finished()
        {
            _spirit.AttackInfoControls.Remove(this);
            _spirit.LayoutRoot.Children.Remove(this);
        }

        public void Go(Spirit spirit, AttackInfoInstance attackinfo)
        {
            _attackInfo = attackinfo;
            _spirit = spirit;

            this.AttackInfo.Text = attackinfo.Info;
            this.AttackInfo.Foreground = new SolidColorBrush(attackinfo.Color);

            spirit.LayoutRoot.Children.Add(this);
            spirit.AttackInfoControls.Add(this);
            Canvas.SetZIndex(this, CommonSettings.Z_SKILL);

            if (attackinfo.Type == AttackInfoType.CriticalHit)
            {
                this.CriticalHitStory.Begin();
            }
            else
            {
                this.AttackInfoStory.Begin();
            }
        }

        AttackInfoInstance _attackInfo;
        Spirit _spirit;
	}
}