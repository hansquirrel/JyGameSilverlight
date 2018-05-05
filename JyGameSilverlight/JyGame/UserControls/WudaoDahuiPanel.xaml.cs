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
    enum WudaoDahuiPanelStatus
    {
        Idle,
        Working,
    }

    public partial class WudaoDahuiPanel : UserControl
    {
        public WudaoDahuiPanel()
        {
            InitializeComponent();
        }

        private WudaoDahuiPanelStatus Status = WudaoDahuiPanelStatus.Idle;
        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            RuntimeData.Instance.gameEngine.LoadMap(RuntimeData.Instance.CurrentBigMap);
        }

        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.Refresh();
        }


        private void Refresh()
        {
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            RolesPanel.Children.Clear();
            SayContentText.Text = RuntimeData.Instance.WudaoSay;
            Status = WudaoDahuiPanelStatus.Working;
            this.StatusInfoText.Text = "正在联机获取信息，请稍后...";

            //获取对手数据
            GameServerManager.Instance.GetWudaodahuiOpponents((opponents) =>
            {
                Status = WudaoDahuiPanelStatus.Idle;
                //如果读到数据了，但玩家已经取消了
                if (this.Visibility == System.Windows.Visibility.Collapsed)
                    return;

                //读取出错
                if (opponents == null)
                {
                    this.StatusInfoText.Text = "获取失败，请检查网络连接！";
                    return;
                }

                //没有合适的对手
                if (opponents.Count == 0)
                {
                    this.StatusInfoText.Text = "没有找到合适的对手！";
                    if(RuntimeData.Instance.Rank == 1)
                    {
                        this.StatusInfoText.Text = "恭喜你，你已经问鼎江湖第一，没有对手了。";
                    }
                    return;
                }

                this.StatusInfoText.Text = "请选择你的对手";

                //填充对手列表
                foreach(var oppent in from o in opponents orderby o.Rank select o)
                {
                    foreach (var r in oppent.Team)
                    {
                        r.Reset();
                    }
                    this.RolesPanel.Children.Add(new WudaoOpponentItem(oppent, (sender) =>
                    {
                        WudaoOpponent opp = (sender as WudaoOpponentItem).opp;
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        uiHost.arenaSelectRole.confirmBack = () =>
                        {
                            //先将选择的角色发到服务器存档
                            RuntimeData.Instance.WudaoSay = SayContentText.Text;
                            List<Role> roles = uiHost.arenaSelectRole.selectedMyFriends;
                            GameServerManager.Instance.SendWudaodahuiTeam(RuntimeData.Instance.UUID, roles);

                            //载入战斗
                            RuntimeData.Instance.gameEngine.uihost.battleFieldContainer.LoadWudao(opp);
                        };
                        uiHost.arenaSelectRole.cancelBack = () =>
                            {
                                this.Show();
                            };
                        uiHost.arenaSelectRole.load(3, new string[]{"主角"});
                    }));
                }
            });
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Status == WudaoDahuiPanelStatus.Working)
                return;
            else
                Refresh();
        }
    }
}
