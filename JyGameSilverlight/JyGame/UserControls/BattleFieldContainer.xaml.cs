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
using JyGame.GameData;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml;
using JyGame.BattleNet;

namespace JyGame.UserControls
{
    public partial class BattleFieldContainer : UserControl
    {
        #region 状态记录

        DispatcherTimer OLGameReadyTimer;

        public UIHost uiHost
        {
            set
            {
                field.uiHost = value;
                _uiHost = value;
            }
            get
            {
                return _uiHost;
            }
        }
        private UIHost _uiHost;

        #endregion

        #region 初始化、载入相关

        public BattleFieldContainer()
        {
            InitializeComponent();
            field.battleFieldContainer = this;

            //战场初始化
            LayoutRoot.Height = CommonSettings.SCREEN_HEIGHT;
            LayoutRoot.Width = CommonSettings.SCREEN_WIDTH;

            this.InitScrollTimer();

            if(Configer.Instance.Debug)
            {
                debugInfo.Visibility = System.Windows.Visibility.Visible;
                DispatcherTimer t = new DispatcherTimer();
                t.Interval = TimeSpan.FromMilliseconds(500);
                t.Tick += (s, e) =>
                {
                    debugInfo.Text = string.Format("调试信息\n战场元素个数 {0}", this.field.RootCanvas.Children.Count); 
                };
                t.Start();
            }
            else
            {
                debugInfo.Visibility = System.Windows.Visibility.Collapsed;
            }

            #region fade相关设置
            this.FadeIn.Completed += new EventHandler(FadeIn_Completed);
            this.FadeOut.Completed += new EventHandler(FadeOut_Completed);
            #endregion

            #region 联机游戏相关
            OLGameReadyTimer = new DispatcherTimer();
            OLGameReadyTimer.Interval = TimeSpan.FromMilliseconds(100);
            OLGameReadyTimer.Tick += new EventHandler(checkOLGameReady);
            #endregion

            //初始化回调
            this.skillHotKeysPanel.Callback = (skillbox) =>
                {
                    if (field.Status == BattleStatus.Moving) return;
                    field.currentSkill = skillbox as SkillBox;
                    uiHost.skillPanel.Visibility = System.Windows.Visibility.Collapsed;
                    uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;
                    uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                    field.BlockUnselective();
                    field.Status = BattleStatus.SelectAttack;
                };
        }

        public bool BattleFieldScrollable
        {
            get
            {
                return _battleFieldScrollable;
            }
            set 
            {
                _battleFieldScrollable = value;
            }
        }
        private bool _battleFieldScrollable = false;

        public void Load(string battleKey, CommonSettings.IntCallBack callback)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            //战斗开始前先选择我方角色
            UIHost uihost = RuntimeData.Instance.gameEngine.uihost;
            uihost.mapUI.resetHead();
            uihost.scence.HideHeads();
            uihost.arenaSelectRole.confirmBack = () =>
            {
                //初始化战场
                uihost.Dispatcher.BeginInvoke(() =>
                {
                    this.Visibility = System.Windows.Visibility.Visible;
                    field.Load(battleKey, callback);
                    this.BattleFieldScrollable = true;
                    //uiHost.Chat(storyScenario);

                    this.IsEnabled = false;
                    this.FadeOut.Stop();
                    this.FadeIn.Begin();
                });
            };
            uihost.arenaSelectRole.cancel.Visibility = Visibility.Collapsed;

            int maxFriendsNo = 0;
            Battle battle = BattleManager.GetBattle(battleKey);
            foreach (BattleRole role in battle.battleRoles)
            {
                if (role.team == 1 && role.roleKey==null)
                {
                    maxFriendsNo += 1;
                }
            }

            uihost.arenaSelectRole.load(maxFriendsNo, battle.GetBattleMusts());
        }


        public void LoadTower(Battle battle, List<int> friends)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            this.Visibility = System.Windows.Visibility.Visible;
            field.LoadTower(battle, friends);
            this.BattleFieldScrollable = true;
            //uiHost.Chat("test_TOWER");

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }

        public void LoadHuashan(Battle battle, List<int> friends)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            this.Visibility = System.Windows.Visibility.Visible;
            field.LoadHuashan(battle, friends);
            this.BattleFieldScrollable = true;
            //uiHost.Chat("test_HUASHAN");

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }


        #region 联机战斗

        public void LoadOLBattle(Battle battle, List<int> friends, int myTeamIndex, string channel)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            //首先，将自己的选人结果(XML)送到服务器，并等待确认返回对方选人结果
            //List<Role> battleEnemies = new List<Role>();
            OLBattleGlobalSetting.Instance.battleFriends.Clear();
            //battleEnemies.Clear();
            for (int i = 0; i < friends.Count; i++)
            {
                OLBattleGlobalSetting.Instance.battleFriends.Add(RuntimeData.Instance.Team[friends[i]]);
            }
            OLBattleGlobalSetting.Instance.myTeamIndex = myTeamIndex;
            OLBattleGlobalSetting.Instance.channel = channel;
            OLBattleGlobalSetting.Instance.battle = battle;
            XElement rolesNode = new XElement("roles");
            foreach (var role in OLBattleGlobalSetting.Instance.battleFriends)
            {
                rolesNode.Add(role.GenerateRoleXml());
            }

            string cmd = "SELECT_ROLE_RESULT";
            BattleNetManager.Instance.Chat(channel, cmd + "$" + rolesNode.ToString());
            if(!OLBattleGlobalSetting.Instance.enemyOK)
                uiHost.onlineGameLoadingPanel.startShow("请等待对手选人");
            OLGameReadyTimer.Start();
        }

        public void loadOLBattleEnemyConfirm(string roleData)
        {
            //TextBlock tb = new TextBlock() { Text = string.Format("[{0}]{1}:{2}", ee.Channel, ee.User.Name, ee.Message) };
            XElement enemyRoles = XElement.Parse(roleData);
            foreach (var roleNode in Tools.GetXmlElements(enemyRoles, "role"))
            {
                OLBattleGlobalSetting.Instance.battleEnemies.Add(Role.Parse(roleNode));
            }

            OLBattleGlobalSetting.Instance.enemyOK = true;
        }

        private void checkOLGameReady(object sender, EventArgs e)
        {
            if (OLBattleGlobalSetting.Instance.enemyOK)
            {
                OLGameReadyTimer.Stop();
                uiHost.onlineGameLoadingPanel.stopShow();
                loadOLBattleStart();
            }
        }

        public void loadOLBattleStart()
        {
            //分配双方battleID，team 1的一方从0开始编号，另一方从1000开始编号。
            List<int> friendBattleID = new List<int>();
            List<int> enemyBattleID = new List<int>();
            int friendStartIndex = OLBattleGlobalSetting.Instance.myTeamIndex == 1 ? 0 : 1000;
            int enemyStartIndex = OLBattleGlobalSetting.Instance.myTeamIndex == 1 ? 1000 : 0;
            for (int i = 0; i < OLBattleGlobalSetting.Instance.battleFriends.Count; i++)
                friendBattleID.Add(friendStartIndex++);
            for (int i = 0; i < OLBattleGlobalSetting.Instance.battleEnemies.Count; i++)
                enemyBattleID.Add(enemyStartIndex++);

            //加载战场
            this.Visibility = System.Windows.Visibility.Visible;
            field.LoadOLBattle(OLBattleGlobalSetting.Instance.battle, OLBattleGlobalSetting.Instance.myTeamIndex, OLBattleGlobalSetting.Instance.battleFriends, OLBattleGlobalSetting.Instance.battleEnemies, friendBattleID, enemyBattleID);
            this.BattleFieldScrollable = true;

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }

        #endregion 

        public void LoadArena(Battle battle, List<int> friends, List<String> enemies)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            this.Visibility = System.Windows.Visibility.Visible;
            field.LoadArena(battle, friends, enemies);
            this.BattleFieldScrollable = true;
            //uiHost.Chat("test_ARENA");

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }

        public void LoadTrail(Battle battle, int friend, CommonSettings.IntCallBack callback)
        {
            CurrentScrollDirection = ScrollDirection.NONE;
            List<int> friends = new List<int>();
            friends.Add(friend);

            this.Visibility = System.Windows.Visibility.Visible;
            field.Load(battle, callback, friends);
            this.BattleFieldScrollable = true;
            //uiHost.Chat("test_ARENA");

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }

        /// <summary>
        /// 武道大会
        /// </summary>
        /// <param name="opp"></param>
        public void LoadWudao(WudaoOpponent opp)
        {
            WudaoOpponent me = new WudaoOpponent();
            me.Team = uiHost.arenaSelectRole.selectedMyFriends;

            if(me.IsCheat)
            {
                MessageBox.Show("检测到你在作弊，禁止参加武道大会");
                RuntimeData.Instance.IsCheated = true;
                return;
            }
            if(opp.IsCheat)
            {
                MessageBox.Show("检测到对方作弊，直接判定胜利。");
                GameServerManager.Instance.BeatWudaoOpponent(opp);
                MessageBox.Show("恭喜你，你的江湖排名升级到第" + opp.Rank + "名");
                RuntimeData.Instance.gameEngine.CallScence(
                                    uiHost.battleFieldContainer.field,
                                    new NextGameState() { Type = "wudaodahui", Value = "" });
                return;
            }

            CurrentScrollDirection = ScrollDirection.NONE;
            this.Visibility = System.Windows.Visibility.Visible;
            Battle battle = BattleManager.GetBattle("武道大会_比武");
            field.LoadWudaodahui(battle, opp, (ret) => 
            {
                uiHost.mapUI.resetTeam();
                List<Role> roles = uiHost.arenaSelectRole.selectedMyFriends;
                //赢了交换名次
                if (ret == 1)
                {
                    GameServerManager.Instance.BeatWudaoOpponent(opp);
                    MessageBox.Show("恭喜你，你的江湖排名升级到第" + opp.Rank + "名");
                }

                //RuntimeData.Instance.gameEngine.LoadMap(RuntimeData.Instance.CurrentBigMap);
                RuntimeData.Instance.gameEngine.CallScence(
                                    uiHost.battleFieldContainer.field,
                                    new NextGameState() { Type = "wudaodahui", Value = "" });
            });
            this.BattleFieldScrollable = true;

            this.IsEnabled = false;
            this.FadeOut.Stop();
            this.FadeIn.Begin();
        }


        void FadeIn_Completed(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }
        #endregion

        #region 场景切换
        public void Hide()
        {
            this.IsEnabled = false;
            this.FadeIn.Stop();
            this.FadeOut.Begin();
        }

        void FadeOut_Completed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.IsEnabled = true;
        }

        #endregion

        //将镜头对准现在的精灵
        public void cameraMoveToCurrentRole()
        {
            Spirit currentRole = field.currentSpirit;
            cameraMoveTo(currentRole.CanvasLeft, currentRole.CanvasTop, () => { field.Status = BattleStatus.NextPersonAct; });
        }

        public void initCamera(CommonSettings.VoidCallBack callback)
        {
            Spirit role = field.Spirits[0];
            cameraMoveTo(role.CanvasLeft, role.CanvasTop, callback);
        }

        public void cameraScrollTo(double x,double y,CommonSettings.VoidCallBack callback)
        {
            double left = Canvas.GetLeft(field);
            double top = Canvas.GetTop(field);
            double toMoveX = x - (double)CommonSettings.SCREEN_WIDTH * 0.5f;
            double toMoveY = y - (double)CommonSettings.SCREEN_HEIGHT * 0.5f;
            

            double animationTime = 300;
            if (Math.Abs(left - LimitLeft(-toMoveX) + Math.Abs(top - LimitTop(-toMoveY))) < 150)
                animationTime = 150;

            Storyboard storyboard = new Storyboard();
            bool IsAnimation = false;
            if ((left + toMoveX) != 0)
            {
                DoubleAnimation animationX = new DoubleAnimation()
                {
                    From = left,
                    To = LimitLeft(-toMoveX),
                    Duration = new Duration(TimeSpan.FromMilliseconds(animationTime))
                };
                Storyboard.SetTarget(animationX, field);
                Storyboard.SetTargetProperty(animationX, new PropertyPath("(Canvas.Left)"));
                storyboard.Children.Add(animationX);
                IsAnimation = true;
            }
            if ((top + toMoveY) != 0)
            {
                DoubleAnimation animationY = new DoubleAnimation()
                {
                    From = top,
                    To = LimitTop(-toMoveY),
                    Duration = new Duration(TimeSpan.FromMilliseconds(animationTime))
                };

                Storyboard.SetTarget(animationY, field);
                Storyboard.SetTargetProperty(animationY, new PropertyPath("(Canvas.Top)"));
                storyboard.Children.Add(animationY);
                IsAnimation = true;
            }
            if (IsAnimation)
            {
                storyboard.Begin();
                storyboard.Completed += (ss, e) =>
                {
                    callback();
                };
            }
            else
            {
                callback();
            }
        }

        public void cameraMoveTo(double moveToX, double moveToY, CommonSettings.VoidCallBack callback)
        {
            cameraScrollTo(moveToX, moveToY, () =>
            {
                if(callback != null)
                    callback();
            });
        }


        #region debug
        private void debug_win_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.field.Status = BattleStatus.WIN;
        }

        private void debug_lose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.field.Status = BattleStatus.LOSE;
        }
        #endregion

        #region 联机战斗聊天

        private DateTime lastChatTime = DateTime.Now.AddMinutes(-10);
        private void SendBattleChat()
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
            string sendMsg = "BATTLE_MESSAGE" + "$" + msg;
            BattleNetManager.Instance.Chat(OLBattleGlobalSetting.Instance.channel, sendMsg);
            ChatTextBox.Text = "";

            string info = string.Format("[{0}][{1}]:{2}", DateTime.Now.ToString(), "我", msg);
            TextBlock tb = new TextBlock() { Text = info, Foreground = new SolidColorBrush(Colors.Yellow), FontSize = 12 };
            ChatListBox.Items.Add(tb);
            ChatListBox.UpdateLayout();
            ChatListBox.ScrollIntoView(tb);
        }

        private void SendChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendBattleChat();
        }

        private void ClearChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ChatListBox.Items.Clear();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBattleChat();
            }
        }
        #endregion


        #region 卷屏

        private double LimitLeft(double left)
        {
            if(left > 0) return 0;
            if (left < CommonSettings.SCREEN_WIDTH - field.backgroundSize.Width)
                left = CommonSettings.SCREEN_WIDTH - field.backgroundSize.Width;
            return left;
        }

        private double LimitTop(double top)
        {
            if (top > 0) return 0;
            if (top < CommonSettings.SCREEN_HEIGHT - field.backgroundSize.Height)
                top = CommonSettings.SCREEN_HEIGHT - field.backgroundSize.Height;
            return top;
        }

        public int fieldMarginLeft
        {
            get { return -(int)Canvas.GetLeft(field); }
        }
        public int fieldMarginTop
        {
            get { return -(int)Canvas.GetTop(field); }
        }

        DispatcherTimer scrollTimer;

        private void InitScrollTimer()
        {
            scrollTimer = new DispatcherTimer();
            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            scrollTimer.Tick += scrollTimer_Tick;

            scrollStoryBoards = new Dictionary<ScrollDirection, Storyboard>();
        }

        private ScrollDirection CurrentScrollDirection = ScrollDirection.NONE;

        private Dictionary<ScrollDirection, Storyboard> scrollStoryBoards = null;
        void scrollTimer_Tick(object sender, EventArgs e)
        {
            switch(CurrentScrollDirection)
            {
                case ScrollDirection.NONE:

                    break;
                case ScrollDirection.TOP:
                    {
                        double top = Canvas.GetTop(field);
                        Canvas.SetTop(field, LimitTop(top + 10));                        
                        break;
                    }
                case ScrollDirection.LEFT:
                    {
                        double left = Canvas.GetLeft(field);
                        Canvas.SetLeft(field, LimitLeft(left + 10));
                        break;
                    }
                case ScrollDirection.RIGHT:
                    {
                        double left = Canvas.GetLeft(field);
                        Canvas.SetLeft(field, LimitLeft(left - 10));
                        break;
                    }
                case ScrollDirection.BUTTOM:
                    {
                        double top = Canvas.GetTop(field);
                        Canvas.SetTop(field, LimitTop(top - 10));
                        break;
                    }
                default:
                    break;
            }
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (BattleFieldScrollable == false)
                return;
            
            if (mouseX < 25)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.LEFT;
            }
            if (mouseY < 25)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.TOP;
            }
            if (mouseX > 800 - 25)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.RIGHT;
            }
            if (mouseY > 600 - 25)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.BUTTOM;
            }
        }

        double mouseX = 0;
        double mouseY = 0;
        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseX = e.GetPosition(sender as FrameworkElement).X;
            mouseY = e.GetPosition(sender as FrameworkElement).Y;
            if (BattleFieldScrollable == false)
                return;
            if (mouseX < 2)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.LEFT;
            }
            else if (mouseY < 2)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.TOP;
            }
            else if (mouseX > 800 - 2)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.RIGHT;
            }
            else if (mouseY > 600 - 2)
            {
                scrollTimer.Start();
                CurrentScrollDirection = ScrollDirection.BUTTOM;
            }
            else
            {
                scrollTimer.Stop();
            }
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (BattleFieldScrollable == false)
                return;
            scrollTimer.Stop();
        }
        #endregion
    }

    enum ScrollDirection
    {
        NONE,
        TOP,
        LEFT,
        RIGHT,
        BUTTOM
    }
}
