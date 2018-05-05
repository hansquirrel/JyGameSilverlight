using JyGame.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JyGame.UserControls
{
    public partial class WudaoOpponentItem : UserControl
    {
        public WudaoOpponentItem()
        {
            InitializeComponent();
        }

        public WudaoOpponentItem(WudaoOpponent opponent, CommonSettings.ObjectCallBack callback)
        {
            InitializeComponent();
            opp = opponent;
            this.callback = callback;
            SayContent.Text = opponent.Say;
            HeadImage.Source = opponent.Team[0].Head;
            InfoText.Text = string.Format("{0}({1})  周目:{2}\n游戏时间:{3}\n战斗力评估:{4}",
               opponent.Team[0].Name,
               opponent.Menpai == "" ? "无门派" : opponent.Menpai,
               opponent.Round == 0 ? "/" : opponent.Round.ToString(),
               CommonSettings.DateTimeToGameTime(opponent.GameTime),
               opponent.Power
            );
            if (opponent.Rank == 1)
            {
                RankText.Text = string.Format("武林霸主");
                RankText.Foreground = new SolidColorBrush(Colors.Orange);
            }else if(opponent.Rank < 10)
            {
                RankText.Text = string.Format("江湖排名：{0}", opponent.Rank);
                RankText.Foreground = new SolidColorBrush(Colors.Green);
            }else if(opponent.Rank < 50)
            {
                RankText.Text = string.Format("江湖排名：{0}", opponent.Rank);
                RankText.Foreground = new SolidColorBrush(Colors.Yellow);
            }else if(opponent.Rank < 100)
            {
                RankText.Text = string.Format("江湖排名：{0}", opponent.Rank);
                RankText.Foreground = new SolidColorBrush(Colors.Purple);
            }else if(opponent.Rank< 500)
            {
                RankText.Text = string.Format("江湖排名：{0}", opponent.Rank);
                RankText.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                RankText.Text = string.Format("江湖排名：{0}", opponent.Rank);
                RankText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            foreach(var r in opponent.Team)
            {
                Role role = r;
                Image img = new Image() { Source = r.Head, Width = 40, Height = 40 };
                ToolTipService.SetToolTip(img, "点击查看详情");
                img.MouseLeftButtonUp += (s, e) =>
                {
                    RuntimeData.Instance.gameEngine.uihost.rolePanel.Show(role);
                };
            
                TeamPanel.Children.Add(img);
            }
        }

        public WudaoOpponent opp = null;
        private CommonSettings.ObjectCallBack callback = null;

        private void BattleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            callback(this);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            HeadImage.MouseEnter += HeadImage_MouseEnter;
            HeadImage.MouseLeave += HeadImage_MouseLeave;
            SayContent.Visibility = System.Windows.Visibility.Collapsed;
        }

        void HeadImage_MouseLeave(object sender, MouseEventArgs e)
        {

            SayContent.Visibility = System.Windows.Visibility.Collapsed;
        }

        void HeadImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (opp.Say != string.Empty)
            {
                SayContent.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
