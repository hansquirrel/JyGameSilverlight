using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.BattleNet;
using System.Windows.Threading;
using JyGame.GameData;

namespace JyGame
{
	public partial class OnlineGameInviteItem : UserControl
	{
		public OnlineGameInviteItem()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        DispatcherTimer timer = new DispatcherTimer();
        private int currentSecond = -1;
        StackPanel _father = null;
        string channel = "";
        BattleNetUser me = null;
        BattleNetUser user = null;
        OnlineGame _gameHost = null;
        public void Init(BattleNetUser me, BattleNetUser user, string channel, int second, StackPanel father, OnlineGame gameHost)
        {
            this.me = me;
            this.user = user;
            this.channel = channel;
            _gameHost = gameHost;
            string content = user.Name + "邀请您进行游戏，是否接受？";
            infoText.Text = content;
            timer.Interval = TimeSpan.FromSeconds(1);
            currentSecond = second;
            RefreshTime();
            _father = father;
            timer.Tick += (s, e) =>
            {
                currentSecond--;
                if (currentSecond == 0)
                {
                    SayNo();
                    if (_father != null)
                    {
                        _father.Children.Remove(this);
                    }
                }
                RefreshTime();
            };
            timer.Start();
            _father.Children.Add(this);
        }

        private void RefreshTime()
        {
            timeText.Text = currentSecond.ToString();
        }

        private void SayYes()
        {
            BattleNetManager.Instance.JoinChannel(new string[] { "ALL", me.Channel, channel }, (isJoinned, timeout) =>
            {
                //初始化战场，由于异步IO，必须在发送消息之前处理
                OLBattleGlobalSetting.Instance.init(channel);

                BattleNetManager.Instance.Chat(user.Channel, "YES#" + channel);
                //开始战斗
                _gameHost.GameBegin(channel, user, 2);
            });
        }

        private void SayNo()
        {
            BattleNetManager.Instance.Chat(user.Channel, "NO#" + channel);
        }

        public void Close()
        {
            this.timer.Stop();
            if (_father != null)
            {
                _father.Children.Remove(this);
            }
        }

        private void okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SayYes();
            Close();
        }

        private void cancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SayNo();
            Close();
        }
	}
}