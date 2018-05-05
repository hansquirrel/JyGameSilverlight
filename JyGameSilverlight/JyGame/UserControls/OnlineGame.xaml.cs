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
using System.Collections.Generic;
using JyGame.GameData;

namespace JyGame
{
    public enum OnlineStatus
    {
        /// <summary>
        /// 没登陆
        /// </summary>
        NotLogin, 

        /// <summary>
        /// 正在登陆
        /// </summary>
        Logining,

        /// <summary>
        /// 在大厅
        /// </summary>
        InHost,  
 
        /// <summary>
        /// 在游戏中
        /// </summary>
        InGame,
    }

	public partial class OnlineGame : UserControl
	{
        public UIHost uiHost = null;
		public OnlineGame()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Load()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.Status = OnlineStatus.NotLogin;
            OnlineUsersListBox.Items.Clear();
        }


        private void RootCanvas_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!Configer.Instance.Debug)
            {
                AccountText.Text = "";
                PasswordText.Password = "";
            }


            //掉线
            BattleNetManager.Instance.OnDroppedNotify += (s, ee) =>
            {
                MessageBox.Show("与服务器断开连接。");
                Status = OnlineStatus.NotLogin;
            };

            //聊天
            BattleNetManager.Instance.OnNewChatNotify += (s, ee) =>
            {
                //MessageBox.Show("chat notify:" + ee.Message);

                if (ee.Channel == "ALL")
                {
                    string msg = string.Format("[{0}][{1}]:{2}", DateTime.Now.ToString(), ee.User.Name, ee.Message);
                    TextBlock tb = new TextBlock() { Text = msg, Foreground = new SolidColorBrush(Colors.White), FontSize = 12 };
                    ChatListBox.Items.Add(tb);
                    ChatListBox.UpdateLayout();
                    ChatListBox.ScrollIntoView(tb);
                }
                else if (ee.Channel == Me.Channel)
                {
                    string msg = ee.Message;
                    string cmd = msg.Split(new char[] { '#' })[0];
                    string uuid = "";
                    if (msg.Split(new char[] { '#' }).Length > 1)
                        uuid = msg.Split(new char[] { '#' })[1];


                    if (cmd == "CREATE_GAME")
                    {
                        //string targetChannel = ee.User.Channel;
                        //if (MessageBox.Show(
                        //    string.Format("玩家{0}邀请你进行游戏，是否接收?", ee.User.Name),
                        //    "游戏邀请",
                        //    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        //{
                        //    BattleNetManager.Instance.JoinChannel(new string[] { "ALL", Me.Channel, uuid }, (isJoinned, timeout) =>
                        //    {
                        //        //初始化战场，由于异步IO，必须在发送消息之前处理
                        //        OLBattleGlobalSetting.Instance.init(uuid);

                        //        BattleNetManager.Instance.Chat(targetChannel, "YES#" + uuid);
                        //        //开始战斗
                        //        GameBegin(uuid, ee.User, 2);
                        //    });
                           
                        //}
                        //else
                        //{
                        //    BattleNetManager.Instance.Chat(targetChannel, "NO#" + uuid);
                        //}
                        BeInvitedToGame(ee.User, uuid);
                    }
                    else if (cmd == "YES")
                    {
                        if (Status == OnlineStatus.InHost)
                        {
                            BattleNetManager.Instance.JoinChannel(new string[] { "ALL", Me.Channel, uuid }, (isJoinned, timeout) =>
                            {
                                //开始战斗
                                GameBegin(uuid, ee.User, 1);
                            });
                        }
                    }
                    else if (cmd == "NO")
                    {
                        if (Status == OnlineStatus.InHost)
                        {
                            MessageBox.Show("对方拒绝了你的请求");
                        }
                        //MatchButton.IsEnabled = true;
                        //MatchButton.Content = "发起对战";
                        return;
                    }
                }
            };

            //战斗
            BattleNetManager.Instance.OnNewChatNotify += (s, ee) =>
            {
                //MessageBox.Show("battle notify:" + ee.Message);
                if (ee.Channel != OLBattleGlobalSetting.Instance.channel)
                    return;

                string msg = ee.Message;
                string cmd = msg.Split(new char[] { '$' })[0];
                string data = "";
                if(msg.Split(new char[] {'$'}).Length > 1)
                    data = msg.Split(new char[] { '$' })[1];

                switch (cmd)
                {
                    case "SELECT_ROLE_RESULT":
                        //MessageBox.Show("你的对手已经选人完毕！");
                        uiHost.battleFieldContainer.loadOLBattleEnemyConfirm(data);
                        break;

                    case "LOADING_FINISH":
                        OLBattleGlobalSetting.Instance.enemyLoadFinish = true;
                        break;

                    case "OL_BATTLE_DATA":
                        //MessageBox.Show(data);
                        uiHost.battleFieldContainer.field.OLDataDisplay(data);
                        break;

                    case "BATTLE_MESSAGE":
                        string battleMsg = string.Format("[{0}][{1}]:{2}", DateTime.Now.ToString(), ee.User.Name, data);
                        TextBlock tb = new TextBlock() { Text = battleMsg, Foreground = new SolidColorBrush(Colors.White), FontSize = 12 };
                        uiHost.battleFieldContainer.ChatListBox.Items.Add(tb);
                        uiHost.battleFieldContainer.ChatListBox.UpdateLayout();
                        uiHost.battleFieldContainer.ChatListBox.ScrollIntoView(tb);
                        break;
                }
            };

            RefreshTimer.Interval = TimeSpan.FromSeconds(5);
            RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            RefreshTimer.Stop();
        }

        private void BeInvitedToGame(BattleNetUser user, string channel)
        {
            AudioManager.PlayEffect(ResourceManager.Get("音效.加点"));
            OnlineGameInviteItem item = new OnlineGameInviteItem();
            item.Init(Me, user, channel, 10, this.inviteStackPanel, this);
        }

        /// <summary>
        /// 刷新在线用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RefreshTimer_Tick(object sender, EventArgs e)
        {
            BattleNetManager.Instance.GetOnlineUsers((users, timeout) =>
            {
                this.RefreshOnlineUsers(users);
            });
        }

        private void RefreshOnlineUsers(List<BattleNetUser> users)
        {
            OnlineUsersListBox.Items.Clear();

            foreach (var u in users)
            {
                OnlineUsersListBox.Items.Add(new TextBlock() { Text = u.ToString(), Tag = u, Foreground = new SolidColorBrush((u.Name == Account) ? Colors.Yellow : Colors.White), FontSize = 12 });
            }

            //List<TextBlock> tobeRemoved = new List<TextBlock>();
            //Dictionary<BattleNetUser, bool> existUser = new Dictionary<BattleNetUser, bool>();

            //foreach (TextBlock luser in OnlineUsersListBox.Items)
            //{
            //    bool find = false;
            //    foreach (var u in users)
            //    {
            //        existUser[u] = true;
            //        if (u.ToString() == luser.Text)
            //        {
            //            find = true;
            //            break;
            //        }
            //    }
            //    if (!find)
            //    {
            //        tobeRemoved.Add(luser);
            //    }
            //}

            ////删除本地列表中有，服务器没有的
            //foreach (var t in tobeRemoved)
            //{
            //    OnlineUsersListBox.Items.Remove(t);
            //}

            ////添加本地没有，服务器有的
            //foreach (var u in users)
            //{
            //    if (existUser.ContainsKey(u))
            //        continue;
                
            //    OnlineUsersListBox.Items.Add(new TextBlock() { Text = u.ToString(), Tag = u, Foreground = new SolidColorBrush((u.Name == Account)?Colors.Yellow : Colors.White), FontSize = 12 });
            //}
        }

        DispatcherTimer RefreshTimer = new DispatcherTimer();

        private List<SaveInfo> CurrentSaves
        {
            set
            {
                _currentSaves = value;
                saveSelectComboBox.Items.Clear();
                for (int i = 0; i < _currentSaves.Count; ++i)
                {
                    SaveInfo sinfo = _currentSaves[i];
                    saveSelectComboBox.Items.Add(new TextBlock() { Text = sinfo.Name, Tag = sinfo });
                }
                if(saveSelectComboBox.Items.Count>0)
                    saveSelectComboBox.SelectedItem = saveSelectComboBox.Items[0];
            }
            get
            {
                return _currentSaves;
            }
        }

        private List<SaveInfo> _currentSaves;
        public OnlineStatus Status
        {
            set
            {
                _status = value;
                switch (_status)
                {
                    case OnlineStatus.NotLogin:
                        BattleNetManager.Instance.Logout();
                        //AudioManager.PlayMusic(ResourceManager.Get("音乐.经典"));
                        uiHost.reset();
                        this.Visibility = System.Windows.Visibility.Visible;
                        LoginCanvas.Visibility = System.Windows.Visibility.Visible;
                        LoginCanvas.IsHitTestVisible = true;
                        HostCanvas.Visibility = System.Windows.Visibility.Collapsed;
                        HostCanvas.IsHitTestVisible = false;
                        AccountText.IsEnabled = true;
                        PasswordText.IsEnabled = true;
                        LoginButton.IsEnabled = true;
                        CancelButton.IsEnabled = true;
                        RefreshTimer.Stop();
                        break;
                    case OnlineStatus.Logining:
                        //AudioManager.PlayMusic(ResourceManager.Get("音乐.经典"));
                        uiHost.reset();
                        this.Visibility = System.Windows.Visibility.Visible;
                        LoginCanvas.Visibility = System.Windows.Visibility.Visible;
                        LoginCanvas.IsHitTestVisible = true;
                        HostCanvas.Visibility = System.Windows.Visibility.Collapsed;
                        HostCanvas.IsHitTestVisible = false;
                        AccountText.IsEnabled = false;
                        PasswordText.IsEnabled = false;
                        LoginButton.IsEnabled = false;
                        CancelButton.IsEnabled = false;
                        RefreshTimer.Stop();
                        break;
                    case OnlineStatus.InHost:
                        //AudioManager.PlayMusic(ResourceManager.Get("音乐.经典"));
                        uiHost.reset();
                        this.Visibility = System.Windows.Visibility.Visible;
                        MatchButton.IsEnabled = true;
                        MatchButton.Content = "发起对战";
                        LoginCanvas.Visibility = System.Windows.Visibility.Collapsed;
                        LoginCanvas.IsHitTestVisible = false;
                        HostCanvas.Visibility = System.Windows.Visibility.Visible;
                        HostCanvas.IsHitTestVisible = true;
                        WelcomeText.Text = "欢迎你，" + Account;

                        //BattleNetManager.Instance.GetSaves((saves, timeout) =>
                        //{
                        //    CurrentSaves = saves;
                        //    BattleNetManager.Instance.JoinChannel(new string[] { "ALL", "PRIVATE_" + Account }, (isJoined, timeouted) =>
                        //    {
                        //        RefreshTimer_Tick(null, null);
                        //        RefreshTimer.Start();
                        //    });
                        //});
                        BattleNetManager.Instance.JoinChannel(new string[] { "ALL", Me.Channel }, (isJoined, timeouted) =>
                        {
                            RefreshTimer_Tick(null, null);
                            RefreshTimer.Start();
                        });
                        //本地存档
                        CurrentSaves = SaveLoadManager.Instance.GetList();
                        if (CurrentSaves == null || CurrentSaves.Count == 0)
                        {
                            MessageBox.Show("错误，你必须至少有一个存档才能参与联机游戏!");
                            this.Dispatcher.BeginInvoke(() => { Status = OnlineStatus.NotLogin; });
                        }
                        
                        break;
                    case OnlineStatus.InGame:
                        RefreshTimer.Stop();
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
            get
            {
                return _status;
            }
        }
        private OnlineStatus _status;

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private string Account;
        private string Password;
        private BattleNetUser Me;

		private void LoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            string account = AccountText.Text;
            string password = PasswordText.Password;

            if (account == "")
            {
                MessageBox.Show("请输入账号");
                return;
            }
            if (password == "")
            {
                MessageBox.Show("请输入密码");
                return;
            }
            Account = account;
            Password = password;
            Me = new BattleNetUser() { Name = Account, Score = 1000 };//先这样处理，之后改成从服务器获取
            Status = OnlineStatus.Logining;
            BattleNetManager.Instance.Login(account, password, (isLogin, timeout) =>
            {
                if (!isLogin)
                {
                    MessageBox.Show("登陆失败，账号密码错误");
                    Status = OnlineStatus.NotLogin;
                    return;
                }
                if (timeout)
                {
                    MessageBox.Show("登陆失败，与服务器连接超时！");
                    Status = OnlineStatus.NotLogin;
                    return;
                }
                Status = OnlineStatus.InHost;
            });
		}

		private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            uiHost.mainMenu.Load();
            this.Visibility = System.Windows.Visibility.Collapsed;
		}

        private void saveSelectComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TextBlock selectedItem = saveSelectComboBox.SelectedItem as TextBlock;
            if (selectedItem == null) return;
            SaveInfo save = selectedItem.Tag as SaveInfo;
            
            string sinfo = string.Format("{0} ", save.ToString());
            SaveDetailInfo.Text = sinfo;
        }

        private DateTime lastMatchTime = DateTime.MinValue;
		private void MatchButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            TextBlock tb = OnlineUsersListBox.SelectedItem as TextBlock;
            if (tb == null)
            {
                MessageBox.Show("请选择你要对战的对手!");
                return;
            }
            if (tb.Text == Account)
            {
                MessageBox.Show("不能和自己对战");
                return;
            }
            if ((DateTime.Now - lastMatchTime).TotalSeconds < 15)
            {
                MessageBox.Show("您太着急了，15秒内只能发起一起对战，请再等待" + (15 - (DateTime.Now - lastMatchTime).Seconds).ToString() + "秒");
                return;
            }
            lastMatchTime = DateTime.Now;

            string targetUserName = tb.Text;
            string targetChannel = (tb.Tag as BattleNetUser).Channel;

            //由于异步IO，必须在对方确认之前做游戏初始化工作

            string cmd = "CREATE_GAME";
            string channelUuid = System.Guid.NewGuid().ToString();

            OLBattleGlobalSetting.Instance.init(channelUuid);
            
            BattleNetManager.Instance.Chat(targetChannel, cmd + "#" + channelUuid);
            MessageBox.Show("邀请发送成功");
            //MatchButton.IsEnabled = false;
            //MatchButton.Content = "等待对方确认";
		}

		private void SendChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            SendChat();
		}

		private void ClearChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            ChatListBox.Items.Clear();
		}

		private void StartSaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			
		}

		private void DeleteSaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
		}

		private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
            if (e.Key == Key.Enter)
            {
                SendChat();
            }
		}

        public void Win(CommonSettings.VoidCallBack callback = null)
        {
            string uuid = OLBattleGlobalSetting.Instance.channel;
            BattleNetManager.Instance.CommitBattleResult(uuid, Me.Name, _opponent.Name, "", 
                (ret, isTimeout) => {
                    if(callback != null)
                        callback();
                }
                );
        }

        public void Lose(CommonSettings.VoidCallBack callback = null)
        {
            if(callback != null)
                callback();
        }

        private DateTime lastChatTime = DateTime.Now.AddMinutes(-10);
        private void SendChat()
        {
            if ((DateTime.Now - lastChatTime).TotalSeconds <= 5)
            {
                MessageBox.Show("你发言太快了，请坐下来休息一会吧！");
                return;
            }
            
            string msg = ChatTextBox.Text;
            if (msg.Length > 100)
            {
                MessageBox.Show("你发言字数太多，最多100字");
                return;
            }
            lastChatTime = DateTime.Now;
            BattleNetManager.Instance.Chat("ALL", msg);
            ChatTextBox.Text = "";

            string info = string.Format("[{0}][{1}]:{2}", DateTime.Now.ToString(), Me.Name, msg);
            TextBlock tb = new TextBlock() { Text = info, Foreground = new SolidColorBrush(Colors.Yellow), FontSize = 12 };
            ChatListBox.Items.Add(tb);
            ChatListBox.UpdateLayout();
            ChatListBox.ScrollIntoView(tb);
        }

        private void LogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BattleNetManager.Instance.Logout();
        }

        BattleNetUser _opponent = null;
        public void GameBegin(string channelUuid, BattleNetUser opponent, int team)
        {
            //MessageBox.Show(string.Format("现在开始进入战斗...房间号:{0}，对手:{1}，队伍号:{2}", channelUuid, opponent.Name, team));

            foreach (var c in inviteStackPanel.Children)
            {
                if (c is OnlineGameInviteItem)
                {
                    OnlineGameInviteItem item = c as OnlineGameInviteItem;
                    item.Close();
                }
            }
            _opponent = opponent;

            TextBlock selectedItem = saveSelectComboBox.SelectedItem as TextBlock;
            if (selectedItem == null)
            {
                MessageBox.Show("错误，请先选择进行战斗的存档..");
                return;
            }

            this.Status = OnlineStatus.InGame;
            SaveInfo save = selectedItem.Tag as SaveInfo;

            RuntimeData.Instance.Load(save.Name);
            OLBattleGlobalSetting.Instance.OLGame = true;
            OLBattleGlobalSetting.Instance.myTeamIndex = team;
            OLBattleGlobalSetting.Instance.channel = channelUuid;

            //OL战斗模式，固定发放物品
            RuntimeData.Instance.Items.Clear();
            RuntimeData.Instance.Items.Add(ItemManager.GetItem("大还丹").Clone());
            RuntimeData.Instance.Items.Add(ItemManager.GetItem("大还丹").Clone());
            RuntimeData.Instance.Items.Add(ItemManager.GetItem("九转熊蛇丸").Clone());
            RuntimeData.Instance.Items.Add(ItemManager.GetItem("九转熊蛇丸").Clone());

            RuntimeData.Instance.gameEngine.OLBattle(true, team, channelUuid);

        }

        private void EnvButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uiHost.envsetPanel.Visibility == System.Windows.Visibility.Collapsed)
                uiHost.envsetPanel.Show();
            else
                uiHost.envsetPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
	}
}