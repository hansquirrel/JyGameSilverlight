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
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using JyGame.Interface;
using JyGame.Logic;
using System.Xml.Linq;
using System.Xml;
using JyGame.BattleNet;
using System.Globalization;

namespace JyGame.UserControls
{
    public enum BattleStatus
    {
        Starting = 0,
        WaitingPlayer,
        SelectMove,
        Moving,
        SelectAction,
        SelectItem,
        UsingItem,
        SelectItemTarget,
        SelectSkill,
        SelectAttack,
        Attacking,
        NextPerson,
        NextPersonAct,
        WaitingForActivePerson, //集气
        NextActivePersonAction, //下一个激活的角色行动
        AI,
        OLNextPerson, //联机模式下，下一个行动的是对方角色
        WIN,
        LOSE
    }

    /// <summary>
    /// 角色行动
    /// </summary>
    public enum RoleActionType
    {
        Attack,
        Items,
        Rest,
        Cancel,
        RoleStatus,
    }


    public partial class BattleField : UserControl, IScence
    {
        #region 初始化与界面组件
        #region 状态

        /// <summary>
        /// 是否强制玩家使用AI行动
        /// </summary>
        public bool ForceAI = false;

        public UIHost uiHost = null;
        public string currentScenario;
        public BattleFieldContainer battleFieldContainer;
        public CommonSettings.IntCallBack battleCallback = null;
        public Size backgroundSize = new Size(0, 0);
        public int actualXBlockNo;
        public int actualYBlcokNo;
        private bool isEnd = false;

        public List<Spirit> Spirits = new List<Spirit>();
        public List<String> EnemyTeam = new List<String>();
        public double TotalExp = 0;
        public List<String> FriendTeam = new List<String>();

        private Dictionary<BattleStatus, StatusLogicFun> statusMap = new Dictionary<BattleStatus, StatusLogicFun>();

        //public bool OLGame = false;
        //public OLBattleData battleData = new OLBattleData();
        public bool myTurn = false;

        private delegate void StatusLogicFun();

        public BattleStatus Status
        {
            get 
            {
                lock (this)
                {
                    return _status;
                }
            }
            set
            {
                lock (this)
                {

                    if (value != BattleStatus.Starting && value!= BattleStatus.WaitingForActivePerson &&
                        !isBattleFieldActive && value != BattleStatus.WaitingPlayer)
                        return;
                    _status = value;
                    if (_status == BattleStatus.Starting)
                    {
                        if (OLBattleGlobalSetting.Instance.OLGame)
                            uiHost.battleFieldContainer.ChatCanvas.Visibility = Visibility.Visible;
                        else
                            uiHost.battleFieldContainer.ChatCanvas.Visibility = Visibility.Collapsed;
                        isBattleFieldActive = true;
                    }
                    else if (_status == BattleStatus.LOSE || _status == BattleStatus.WIN || _status == BattleStatus.WaitingPlayer)
                        isBattleFieldActive = false;

                    if (statusMap.ContainsKey(value))
                    {
                        this.Dispatcher.BeginInvoke(() => { statusMap[value](); });
                    }
                }
            }
        }
        private bool isBattleFieldActive = false;
        private BattleStatus _status;

        void InitStatusMap()
        {
            statusMap.Add(BattleStatus.Starting, new StatusLogicFun(OnNewRound));
            statusMap.Add(BattleStatus.WaitingPlayer, new StatusLogicFun(OnWaitingPlayer));
            statusMap.Add(BattleStatus.SelectMove, new StatusLogicFun(OnCurrentRoleMove));
            statusMap.Add(BattleStatus.SelectAction, new StatusLogicFun(OnSelectRoleAction));
            statusMap.Add(BattleStatus.SelectItem, new StatusLogicFun(OnSelectItem));
            statusMap.Add(BattleStatus.SelectItemTarget, new StatusLogicFun(OnSelectItemTarget));
            statusMap.Add(BattleStatus.SelectSkill, new StatusLogicFun(OnSelectSkill));
            statusMap.Add(BattleStatus.SelectAttack, new StatusLogicFun(OnRoleAttack));
            statusMap.Add(BattleStatus.NextPerson, new StatusLogicFun(OnNextRole));
            statusMap.Add(BattleStatus.NextPersonAct, new StatusLogicFun(OnNextRoleStartAct));
            statusMap.Add(BattleStatus.OLNextPerson, new StatusLogicFun(OnOLNextPerson));
            statusMap.Add(BattleStatus.AI, new StatusLogicFun(OnAIAction));
            statusMap.Add(BattleStatus.WIN, new StatusLogicFun(OnWin));
            statusMap.Add(BattleStatus.LOSE, new StatusLogicFun(OnLose));
            statusMap.Add(BattleStatus.WaitingForActivePerson, new StatusLogicFun(OnWatingForNextPerson));
            statusMap.Add(BattleStatus.NextActivePersonAction, new StatusLogicFun(OnNextActivePerson));
        }

        #endregion

        #region mapdata
        public BattleBlock[,] blockMap = new BattleBlock[CommonSettings.MAXWIDTH, CommonSettings.MAXHEIGHT];
        public bool[,] mapCoverLayer = new bool[CommonSettings.MAXWIDTH, CommonSettings.MAXHEIGHT];

        //game logic
        private int _gameTurn = 1;
        int GameTurn
        {
            get { return _gameTurn; }
            set 
            {
                _gameTurn = value; 
                battleFieldContainer.RoundText.Text = string.Format("回合数目 {0}/{1}", GameTurn, MaxGameTurn);
            }
        }
        public int MaxGameTurn { get { return currentBattle.maxRound; } }
        public Spirit currentSpirit = null;

        private int rollbackCurrentX;
        private int rollbackCurrentY;
        private bool rollbackCurrentFace;

        Item currentItem = null;
        public SkillBox currentSkill = null;
        Dictionary<int, bool> ActionedSpirit = new Dictionary<int, bool>(); //当前turn已经移动的spirit

        private bool IsBattle = true;
        #endregion

        #region 初始化战场
        public BattleField()
        {
            InitializeComponent();
            InitStatusMap();
            ai = new BattleAI(this);

            //初始化sp
            SpTimer.Interval = TimeSpan.FromMilliseconds(CommonSettings.SP_COMPUTE_DURATION);
            SpTimer.Tick += SpTimer_Tick;
        }

        private BattleAI ai;
        private Battle currentBattle
        {
            get { return _currentBattle; }
            set{
                _currentBattle = value;
                if (Configer.Instance.Debug)
                {
                    App.DebugPanel.LoadBattle(value);
                }
            }
        }
        private Battle _currentBattle = null;
        #region implements of interface

        public void Load(string battleName)
        {
            ForceAI = false;
            this.Load(battleName, null);
        }

        public void Load(Battle battle, CommonSettings.IntCallBack callback, List<int> friendsIndex = null)
        {
            ForceAI = false;
            currentBattle = battle;
            battleCallback = callback;
            currentScenario = battle.Key;
            isEnd = false;
            this.Visibility = Visibility.Visible;

            LoadMapTemplate(battle.templateKey);
            AudioManager.PlayMusic(battle.Music);
            LoadMapBlocks();

            if (friendsIndex == null)
            {
                LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles,
                    uiHost.arenaSelectRole.selectedFriends, battle.randomRoleAnimation);
            }
            else
            {
                LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles,
                    friendsIndex, battle.randomRoleAnimation);
            }
            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;

            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void LoadWudaodahui(Battle battle, WudaoOpponent opponent, CommonSettings.IntCallBack callback)
        {
            ForceAI = true;
            if (Configer.Instance.Debug)
            {
                App.DebugPanel.LoadBattle(battle);
            }
            currentBattle = battle;
            battleCallback = callback;
            currentScenario = battle.Key;
            isEnd = false;
            this.Visibility = Visibility.Visible;

            LoadMapTemplate(battle.templateKey);
            AudioManager.PlayMusic(battle.Music);
            LoadMapBlocks();

            LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles,
                uiHost.arenaSelectRole.selectedFriends, battle.randomRoleAnimation);

            int[] oppx = new int[3] { 13, 13, 14};
            int[] oppy = new int[3] { 5, 6, 6};
            int index = 0;
            foreach(var r in opponent.Team){
                if (index >= 3) break;
                AddSpirit(new Spirit(r, this, System.Windows.Visibility.Visible, r.Animation)
                {
                    X = oppx[index],
                    Y = oppy[index],
                    FaceRight = false,
                    Team = 2
                });
                index++;
            }

            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;

            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void Load(string battleKey, CommonSettings.IntCallBack callback, List<int> friendsIndex = null)
        {
            ForceAI = false;
            Load(BattleManager.GetBattle(battleKey), callback, friendsIndex);
        }

        private void LoadDialogs(Battle battle, CommonSettings.IntCallBack callback)
        {
            if (battle.Actions != null && battle.Actions.Count > 0)
            {
                List<Dialog> dialogs = new List<Dialog>();
                foreach (var a in battle.Actions)
                {
                    string[] paras = a.Value.Split(new char[] { '#' });
                    Dialog dialog = new Dialog();
                    dialog.role = paras[0];
                    dialog.type = "DIALOG";
                    dialog.info = paras[1];
                    dialogs.Add(dialog);
                }
                uiHost.dialogPanel.ShowDialogs(dialogs, callback, true);
            }
            else
            {
                callback(0);
            }
        }

        
        public void LoadArena(Battle battle, List<int> friends, List<String> enemies)
        {
            ForceAI = false;
            currentBattle = battle;
            currentScenario = "test_ARENA";
            isEnd = false;
            this.Visibility = Visibility.Visible;

            //装载战场资料
            LoadMapTemplate(battle.templateKey);
            LoadMapBlocks();
            AudioManager.PlayMusic(battle.Music);
            LoadArenaSpirits(battle.battleRoles, friends, enemies);

            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;
            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void LoadTower(Battle battle, List<int> friends)
        {
            ForceAI = false;
            currentBattle = battle;
            currentScenario = "test_TOWER";
            isEnd = false;
            this.Visibility = Visibility.Visible;

            //装载战场资料
            LoadMapTemplate(battle.templateKey);
            LoadMapBlocks();
            AudioManager.PlayMusic(battle.Music);
            LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles, uiHost.towerSelectRole.selectedFriends, battle.randomRoleAnimation);

            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;

            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void LoadHuashan(Battle battle, List<int> friends)
        {
            ForceAI = false;
            currentBattle = battle;
            currentScenario = "test_HUASHAN";
            isEnd = false;
            this.Visibility = Visibility.Visible;

            //装载战场资料
            LoadMapTemplate(battle.templateKey);
            LoadMapBlocks();
            AudioManager.PlayMusic(battle.Music);
            LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles, uiHost.towerSelectRole.selectedFriends, battle.randomRoleAnimation);

            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;
            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void LoadOLBattle(Battle battle, int myTeamIndex, List<Role> battleFriends, List<Role> battleEnemies, List<int> friendBattleID, List<int> enemyBattleID)
        {
            ForceAI = false;
            currentBattle = battle;
            currentScenario = "test_OLBATTLE";
            isEnd = false;
            this.Visibility = Visibility.Visible;

            //装载战场资料
            LoadMapTemplate(battle.templateKey);
            LoadMapBlocks();
            AudioManager.PlayMusic(battle.Music);
            //LoadSpirits(battle.battleRoles, battle.randomRoleLevel, battle.randomRoleName, battle.randomRoles, uiHost.towerSelectRole.selectedFriends, battle.randomRoleAnimation);
            //加载精灵的代码必须要用新的函数
            LoadOLBattleSpirits(battle.battleRoles, myTeamIndex, battleFriends, battleEnemies, friendBattleID, enemyBattleID);

            //设置回调
            uiHost.skillPanel.Callback = OnSelectSkill;
            uiHost.roleActionPanel.Callback = OnSelectRoleAction;
            uiHost.itemSelectPanel.Callback = OnUseItem;
            uiHost.RightClickCallback = OnMouseRightClick;
            LoadResource(() =>
            {
                //初始化战场状态
                InitBattleValues();
                //初始化对话
                LoadDialogs(battle, (ret) => { Begin(); });
            });
        }

        public void Hide()
        {
            foreach (Spirit sp in Spirits)
            {
                sp.Remove();
                //Spirits.Remove(sp);
                this.RootCanvas.Children.Remove(sp);
            }
            Spirits.Clear();

            this.battleFieldContainer.Hide();
        }

        private void OnMouseRightClick()
        {
            switch (Status)
            {
                case BattleStatus.SelectItem:
                    this.OnUseItem(null);
                    break;
                case BattleStatus.SelectSkill:
                    this.OnSelectSkill(null);
                    break;
                case BattleStatus.SelectAction:
                    this.OnSelectRoleAction(RoleActionType.Cancel);
                    break;
                case BattleStatus.SelectAttack:
                    BlockUnselective();
                    Status = BattleStatus.SelectSkill;
                    break;
                case BattleStatus.SelectItemTarget:
                    BlockUnselective();
                    currentItem = null;
                    Status = BattleStatus.SelectItem;
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void InitBattleValues()
        {
            GameTurn = 0;
            currentSpirit = null;
            currentSkill = null;
            ActionedSpirit.Clear();
            this.battleFieldContainer.logPanel.Clear();
        }

        private void Log(string log)
        {
            this.battleFieldContainer.logPanel.Add(log);
        }

        private void LoadResource(CommonSettings.VoidCallBack callback)
        {
            List<string> images = new List<string>();

            //images.Add(Tools.GetImageUrl(imgBackground));
            string bgUrl = this._currentBattle.Template.BackgroundUrl;
            images.Add(bgUrl);
            foreach (var s in Spirits)
            {
                images.AddRange(s.GetAnimationImages());
            }
            List<string> rst = new List<string>();
            foreach (var s in images.Distinct())
            {
                rst.Add(s);
            }
            string[] aoyis = new string[] { "jiqi1", "jiqi2" };
            foreach (var aoyi in aoyis)
            {
                AnimationGroup group = AnimationManager.GetAnimation(aoyi, "skill");
                foreach (var img in group.Images)
                {
                    rst.Add(img.Url);
                }
            }
            uiHost.loadingPanel.Show(rst, () =>
            {
                callback();
            });
        }

        private void Begin()
        {
            battleFieldContainer.initCamera(() =>
            {
                if (OLBattleGlobalSetting.Instance.OLGame)
                    Status = BattleStatus.WaitingPlayer;
                else
                {
                    //Status = BattleStatus.Starting;
                    _battleTimestamp = 0;
                    Status = BattleStatus.WaitingForActivePerson;
                }
            });
        }

        private void NextPerson()
        {
            Status = BattleStatus.NextPerson;
        }

        /// <summary>
        /// 初始化地图模板
        /// </summary>
        /// <param name="id"></param>
        private void LoadMapTemplate(string key)
        {
            MapTemplate mapTemplate = BattleManager.GetMapTemplate(key);
            mapCoverLayer = mapTemplate.mapCoverLayer;

            //this.RootCanvas.Background = new ImageBrush() { ImageSource = mapTemplate.Background, Stretch = Stretch.None };
            this.backgroundSize.Width = mapTemplate.backgroundWidth;
            this.backgroundSize.Height = mapTemplate.backgroundHeight;
            this.actualXBlockNo = mapTemplate.actualXBlockNo;
            this.actualYBlcokNo = mapTemplate.actualYBlockNo;
            //clear if exist
            if (imgBackground != null)
            {
                RootCanvas.Children.Remove(imgBackground);
            }
            imgBackground = new Image();
            imgBackground.Source = mapTemplate.Background;
            this.RootCanvas.Children.Add(imgBackground);
        }
        Image imgBackground =null;

        private void LoadSpirits(List<BattleRole> roles, int randomLevel, string randomName, List<RandomRole> randomRoles, List<int> friends, string randomAnimations="")
        {
            if (Spirits.Count > 0)
            {
                foreach (var sp in Spirits)
                {
                    sp.Remove();
                    //Spirits.Remove(sp);
                    this.RootCanvas.Children.Remove(sp);
                }
                Spirits.Clear();
            }
            TotalExp = 0;
            EnemyTeam.Clear();
            FriendTeam.Clear();
            battleIDIndex = 0;

            //add
            int friendIndex = 0;
            foreach (BattleRole role in roles)
            {
                Role r = null;
                if (role.team == 1)//我方角色
                {
                    //r = RoleManager.GetRole(role.roleKey).Clone();
                    if (role.roleKey == null) //非固定角色，从选择列表中添加
                    {
                        if (friendIndex < friends.Count)
                        {
                            r = RuntimeData.Instance.Team[friends[friendIndex]];
                            FriendTeam.Add(r.Key);
                            friendIndex++;
                        }
                        else
                        {
                            r = null;
                        }
                    }
                    else //固定角色，直接添加
                    {
                        if (RoleManager.GetRole(role.roleKey) != null)
                        {
                            r = RoleManager.GetRole(role.roleKey).Clone();
                            FriendTeam.Add(r.Key);
                        }
                        else
                        {
                            r = null;
                        }
                    }
                }
                else //敌方角色
                {
                    double zhoumuFactor = 1 + (RuntimeData.Instance.Round - 1) * 0.1;
                    r = RoleManager.GetRole(role.roleKey).Clone();
                    //炼狱难度：生命、内力
                    if (RuntimeData.Instance.GameMode == "crazy")
                    {
                        r.Attributes["maxhp"] = (int)(r.Attributes["maxhp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                        r.Attributes["hp"] = (int)(r.Attributes["hp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                        r.Attributes["maxmp"] = (int)(r.Attributes["maxmp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                        r.Attributes["mp"] = (int)(r.Attributes["mp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                    }
                    else if (RuntimeData.Instance.GameMode == "hard")
                    {
                        r.Attributes["maxhp"] = (int)(r.Attributes["maxhp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                        r.Attributes["hp"] = (int)(r.Attributes["hp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                        r.Attributes["maxmp"] = (int)(r.Attributes["maxmp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                        r.Attributes["mp"] = (int)(r.Attributes["mp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                    }

                    #region NPC成长
                    string[] tobeAddAttr = new string[] {
                        "maxhp", "maxmp", "hp", "mp", "gengu", "bili", "dingli", "shenfa", "fuyuan" , 
                        "quanzhang","jianfa","daofa", "qimen",
                    };
                    int costDay = (RuntimeData.Instance.Date - System.DateTime.ParseExact("0001-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)).Days;
                    double addRate = 1 + costDay / 180.0 * 0.01; //每半年成长2%
                    if (addRate > 2) addRate = 2; //最多成长100%
                    foreach(var addAttr in tobeAddAttr)
                    {
                        r.Attributes[addAttr] = (int)(r.Attributes[addAttr] * addRate);
                    }
                    #endregion

                    r.addRandomTalentAndWeapons();
                    r.addSkillLevel();
                    EnemyTeam.Add(role.roleKey);
                    TotalExp += r.LevelupExp * addRate / 15.0;
                }
                if (r != null)
                {
                    //重置技能CD，hp等
                    if (r.Attributes["hp"] <= 0) r.Attributes["hp"] = 1;
                    if (r.Attributes["mp"] <= 0) r.Attributes["mp"] = 0;
                    foreach (var s in r.Skills)
                    {
                        s.CurrentCd = 0;
                    }

                    AddSpirit(new Spirit(r, this, Visibility.Visible, r.Animation)
                    {
                        X = role.x,
                        Y = role.y,
                        Team = role.team,
                        FaceRight = role.faceRight,
                    });
                }
            }

            //增加一些随机敌人
            if (randomRoles != null)
            {
                if (currentBattle.isRandomBoss)
                    AddRandomEnemiesBoss(randomLevel, randomRoles);
                else
                    AddRandomEnemies(randomLevel, randomName, randomRoles, randomAnimations);
            }
        }

        private void LoadOLBattleSpirits(List<BattleRole> roles, int myTeamIndex, List<Role> battleFriends, List<Role>battleEnemies, List<int> friendBattleID, List<int> enemyBattleID)
        {
            if (Spirits.Count > 0)
            {
                foreach (var sp in Spirits)
                {
                    sp.Remove();
                    //Spirits.Remove(sp);
                    this.RootCanvas.Children.Remove(sp);
                }
                Spirits.Clear();
            }
            TotalExp = 0;
            EnemyTeam.Clear();
            FriendTeam.Clear();
            //battleIDIndex = 0;

            //add
            int friendIndex = 0;
            int enemyIndex = 0;
            int friendBattleIDIndex = 0;
            int enemyBattleIDIndex = 0;
            int currentID = 0;
            for (int i = 0; i < roles.Count;i++ )
            {
                BattleRole role = roles[i];
                Role r = null;
                if (role.team == myTeamIndex)//我方角色
                {
                    if (friendIndex < battleFriends.Count)
                    {
                        r = battleFriends[friendIndex++];
                        currentID = friendBattleID[friendBattleIDIndex];
                        friendBattleIDIndex++;
                    }
                }
                else //敌方角色
                {
                    if (enemyIndex < battleEnemies.Count)
                    {
                        r = battleEnemies[enemyIndex++];
                        currentID = enemyBattleID[enemyBattleIDIndex];
                        enemyBattleIDIndex++;
                    }
                }
                if (r != null)
                {
                    //重置技能CD，hp等
                    if (r.Attributes["hp"] <= 0) r.Attributes["hp"] = 1;
                    if (r.Attributes["mp"] <= 0) r.Attributes["mp"] = 0;
                    foreach (var s in r.Skills)
                    {
                        s.CurrentCd = 0;
                    }

                    AddSpirit(new Spirit(r, this, Visibility.Visible, r.Animation)
                    {
                        X = role.x,
                        Y = role.y,
                        Team = (role.team == myTeamIndex) ? 1 : 2,
                        FaceRight = role.faceRight,
                    }, currentID);
                }
            }
        }


        private void AddRandomEnemiesBoss(int level, List<RandomRole> randomRoles)
        {
            List<Role> role2select = new List<Role>();
            role2select.Clear();
            foreach (var role in RoleManager.GetRoles())
            {
                if (role.Attributes["arena"] != 0)
                {
                    if(level < 5 && level*5 <role.Level && (level+1)*5 >= role.Level)
                        role2select.Add(role);
                    if (level >= 5 && role.Level > level * 5)
                        role2select.Add(role);
                }
            }
            foreach(RandomRole randomRole in randomRoles)
            {
                Role enemy = role2select[Tools.GetRandomInt(0, role2select.Count) % role2select.Count].Clone();

                //加入随机天赋
                enemy.addRandomTalentAndWeapons();

                //在战场上注册
                EnemyTeam.Add(enemy.Key);
                foreach (var s in enemy.Skills)
                {
                    s.CurrentCd = 0;
                }

                AddSpirit(new Spirit(enemy, this, Visibility.Visible, enemy.Animation)
                {
                    X = randomRole.x,
                    Y = randomRole.y,
                    Team = 2,
                    FaceRight = randomRole.faceRight
                });
            }
        }

        //增加随机敌人
        /****************
         * 
         *  n: 增加的敌人数目
         *  level: 敌人级别，分为0、1、2、3四个级别，HP/MP为等级*70
         *  0级：等级为1~5，使用初级武功（难度0~4的武功），属性点均为30左右
         *  1级：等级为6~10，使用中级武功（难度5~6的武功），属性点均为50左右
         *  2级：等级为10~15，使用高级武功（难度7~9的武功），属性点均为70左右
         *  3级：等级为15~20，使用顶级武功（难度10~100的武功），属性点均为90左右
         * 
         * *************/
        private void AddRandomEnemies(int level, string randomName, List<RandomRole> randomRoles, string animation = "")
        {
            int n = randomRoles.Count;

            //搜集相应等级的武功
            List<Skill> skills = new List<Skill>(); skills.Clear();
            int[] minSkillLevel = { 0, 5, 7, 10 };
            int[] maxSkillLevel = { 4, 6, 9, 100 };
            foreach (Skill skill in SkillManager.GetSkills())
            {
                if (skill.Hard >= minSkillLevel[level] && skill.Hard <= maxSkillLevel[level])
                {
                    skills.Add(skill);
                }
            }

            //搜集小兵所使用的外形模组
            List<Role> roles = new List<Role>(); roles.Clear();
            roles.Add(RoleManager.GetRole("小混混"));
            roles.Add(RoleManager.GetRole("小混混2"));
            roles.Add(RoleManager.GetRole("小混混3"));
            roles.Add(RoleManager.GetRole("小混混4"));
            roles.Add(RoleManager.GetRole("无量剑弟子"));
            roles.Add(RoleManager.GetRole("全真派入门弟子"));
            roles.Add(RoleManager.GetRole("童姥使者"));
            roles.Add(RoleManager.GetRole("明教徒"));
            roles.Add(RoleManager.GetRole("峨眉弟子"));
            roles.Add(RoleManager.GetRole("青城弟子"));
            roles.Add(RoleManager.GetRole("全真派弟子"));
            roles.Add(RoleManager.GetRole("天龙门弟子"));
            roles.Add(RoleManager.GetRole("丐帮弟子"));
            roles.Add(RoleManager.GetRole("五毒教弟子"));

            //属性等级
            int[] attributes = { 30, 50, 70, 90 };
            int attribute = attributes[level];

            for (int i = 0; i < n; i++)
            {
                //外形决定
                int randomNo = Tools.GetRandomInt(0, roles.Count - 1) % roles.Count;
                Role role = roles[randomNo].Clone();

                //等级决定
                int roleLevel = 1;
                switch (level)
                {
                    case 0:
                        roleLevel = Tools.GetRandomInt(1, 5);
                        break;
                    case 1:
                        roleLevel = Tools.GetRandomInt(6, 10);
                        break;
                    case 2:
                        roleLevel = Tools.GetRandomInt(11, 15);
                        break;
                    case 3:
                        roleLevel = Tools.GetRandomInt(16, 20);
                        break;
                    default:
                        roleLevel = 20;
                        break;
                }

                //各项属性决定
                role.Name = randomName; //+ i.ToString() +"号";
                role.Key = randomName + i.ToString(); //+ i.ToString() + "号";
                role.Level = roleLevel;
                role.Exp = 0;
                role.Attributes["maxhp"] = roleLevel * 70;
                role.Attributes["hp"] = roleLevel * 70;
                role.Attributes["maxmp"] = roleLevel * 70;
                role.Attributes["mp"] = roleLevel * 70;

                role.Attributes["bili"] = attribute;
                role.Attributes["gengu"] = attribute;
                role.Attributes["fuyuan"] = attribute;
                role.Attributes["shenfa"] = attribute;
                role.Attributes["dingli"] = attribute;
                role.Attributes["wuxing"] = attribute;
                role.Attributes["quanzhang"] = attribute;
                role.Attributes["jianfa"] = attribute;
                role.Attributes["daofa"] = attribute;
                role.Attributes["qimen"] = attribute;
                if (animation != "")
                {
                    role.Animation = animation;
                }
                //所用武功
                role.SpecialSkills.Clear();
                role.Skills.Clear();
                role.InternalSkills.Clear();
                //随机指定外功
                SkillInstance sintance = new SkillInstance()
                {
                    Skill = skills[Tools.GetRandomInt(0, skills.Count-1) % skills.Count],
                    Level = Tools.GetRandomInt(1,6),
                    MaxLevel = 10,
                    Owner = role,
                };
                role.Skills.Add(sintance);
                //基本内功
                InternalSkillInstance siintance = new InternalSkillInstance()
                {
                    Skill = SkillManager.GetInternalSkill("基本内功"),
                    Level = 10,
                    Equipped = true,
                    MaxLevel = 10,
                    Owner = role,
                };
                role.InternalSkills.Add(siintance);

                //加入随机天赋
                role.addRandomTalentAndWeapons();

                //在战场上注册
                EnemyTeam.Add(role.Key);

                TotalExp += role.LevelupExp / 15.0;
                foreach (var s in role.Skills)
                {
                    s.CurrentCd = 0;
                }

                AddSpirit(new Spirit(role, this, Visibility.Visible, role.Animation)
                {
                    X = randomRoles[i].x,
                    Y = randomRoles[i].y,
                    Team = 2,
                    FaceRight = (Tools.GetRandomInt(0,1) % 2 == 0) ? true:false,
                });
            }
            
        }

        private void LoadArenaSpirits(List<BattleRole> roles, List<int> friends, List<String> enemies)
        {
            //装载已选择的人物
            if (Spirits.Count > 0)
            {
                foreach (var sp in Spirits)
                {
                    sp.Remove();
                    //Spirits.Remove(sp);
                    this.RootCanvas.Children.Remove(sp);
                }
                Spirits.Clear();
            }
            TotalExp = 0;
            EnemyTeam.Clear();
            FriendTeam.Clear();

            //add
            int friendIndex = 0;
            int enemyIndex = 0;
            foreach (BattleRole role in roles)
            {
                Role r = null;
                if (role.team == 1)//我方角色，从我方选择列表中添加
                {
                    //r = RoleManager.GetRole(role.roleKey).Clone();
                    if (friendIndex < friends.Count)
                    {
                        r = RuntimeData.Instance.Team[friends[friendIndex]];
                        FriendTeam.Add(r.Key);
                        friendIndex++;
                    }
                    else
                    {
                        r = null;
                    }
                }
                else //敌方角色
                {
                    if (enemyIndex < enemies.Count)
                    {
                        double zhoumuFactor = 1 + (RuntimeData.Instance.Round - 1) * 0.1;
                        r = RoleManager.GetRole(enemies[enemyIndex]).Clone();
                        //炼狱难度
                        if (RuntimeData.Instance.GameMode == "crazy")
                        {
                            r.Attributes["maxhp"] = (int)(r.Attributes["maxhp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                            r.Attributes["hp"] = (int)(r.Attributes["hp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                            r.Attributes["maxmp"] = (int)(r.Attributes["maxmp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                            r.Attributes["mp"] = (int)(r.Attributes["mp"] * zhoumuFactor * CommonSettings.CrazyModeEnemyHpMpAdd);
                        }
                        else if (RuntimeData.Instance.GameMode == "hard")
                        {
                            r.Attributes["maxhp"] = (int)(r.Attributes["maxhp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                            r.Attributes["hp"] = (int)(r.Attributes["hp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                            r.Attributes["maxmp"] = (int)(r.Attributes["maxmp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                            r.Attributes["mp"] = (int)(r.Attributes["mp"] * zhoumuFactor * CommonSettings.HardModeEnemyHpMpAdd);
                        }
                        r.addRandomTalentAndWeapons();
                        r.addSkillLevel();
                        enemyIndex++;
                        EnemyTeam.Add(r.Key);
                        TotalExp += (r.LevelupExp / 8.0 + 50);
                    }
                    else
                    {
                        r = null;
                    }
                }
                if (r != null)
                {
                    //重置技能CD，hp等
                    if (r.Attributes["hp"] <= 0) r.Attributes["hp"] = 1;
                    if (r.Attributes["mp"] <= 0) r.Attributes["mp"] = 0;
                    foreach (var s in r.Skills)
                    {
                        s.CurrentCd = 0;
                    }

                    AddSpirit(new Spirit(r, this, Visibility.Visible, r.Animation)
                    {
                        X = role.x,
                        Y = role.y,
                        Team = role.team,
                        FaceRight = role.faceRight,
                    });
                }
            }
        }

        private void LoadMapBlocks()
        {
            //clear
            if (blockMap.Length > 0)
            {
                foreach (var b in blockMap)
                {
                    RootCanvas.Children.Remove(b);
                }
            }
            blockMap.Initialize();
            
            for (int i = 0; i < actualXBlockNo; ++i)
            {
                for (int j = 0; j < actualYBlcokNo; ++j)
                {
                    BattleBlock block = new BattleBlock(this, uiHost) { X = i, Y = j };
                    blockMap[i, j] = block;
                    RootCanvas.Children.Add(block);
                }
            }
        }

        private void InitScenarioMapBlocks()
        {
            for (int i = 0; i < actualXBlockNo; ++i)
            {
                for (int j = 0; j < actualYBlcokNo; ++j)
                {
                    BattleBlock block = new BattleBlock(this, uiHost) { X = i, Y = j };
                    block.IsEnabled = false;
                    blockMap[i, j] = block;
                    RootCanvas.Children.Add(block);
                }
            }
        }
        #endregion
        #endregion

        #region 纸娃娃

        public int battleIDIndex = 0;

        public void AddSpirit(Spirit sp)
        {
            sp.battleID = battleIDIndex;
            battleIDIndex++;
            Spirits.Add(sp);
            RootCanvas.Children.Add(sp);
        }

        public void AddSpirit(Spirit sp, int battleID)
        {
            sp.battleID = battleID;
            Spirits.Add(sp);
            RootCanvas.Children.Add(sp);
        }

        public Spirit GetSpirit(int x, int y)
        {
            foreach (var sp in Spirits)
            {
                if (sp.X == x && sp.Y == y) return sp;
            }
            return null;
        }
        #endregion

        #region AI与寻路

        bool aiTerm;
        AIResult aiResult;
        private void OnAIAction()
        {
            DateTime t1 = DateTime.Now;
            aiResult = ai.GetAIResult();
            DateTime t2 = DateTime.Now;
            if (Configer.Instance.Debug)
            {
                string logInfo = string.Format("******AI，耗时{0}毫秒，计算攻击{1}次，平均每次{2}毫秒",
                    (t2 - t1).TotalMilliseconds,
                    aiResult.totalAttackComputeNum,
                    aiResult.totalAttackComputeNum == 0 ? 0 : (t2 - t1).TotalMilliseconds / aiResult.totalAttackComputeNum);
                this.Log(logInfo);
            }
            RoleMoveTo(aiResult.MoveX, aiResult.MoveY, true);
            
        }

        internal void OnAIMovedFinish()
        {
            DispatcherTimer aiWaitTimer = new DispatcherTimer();
            aiWaitTimer.Interval = TimeSpan.FromMilliseconds(CommonSettings.AI_WAITTIME);
            aiWaitTimer.Tick += (s, e) =>
            {
                aiWaitTimer.Stop();
                aiWaitTimer = null;
                this.IsEnabled = true;
                if (aiResult.skill != null)
                {
                    currentSkill = aiResult.skill;
                    skilltarget_x = aiResult.AttackX;
                    skilltarget_y = aiResult.AttackY;
                    PreCastingSkill();
                }
                else
                {
                    OnSelectRest();
                }
            };
            aiWaitTimer.Start();
        }

        private void OnCurrentRoleMove()
        {
            //for test
            //MessageBox.Show( currentSpirit.Role.Name + ":轮到我了" );

            this.ShowHotKeyText();

            this.battleFieldContainer.BattleFieldScrollable = true;

            //记录本单位初始状态，用于回退
            this.rollbackCurrentX = currentSpirit.X;
            this.rollbackCurrentY = currentSpirit.Y;
            this.rollbackCurrentFace = currentSpirit.FaceRight;

            List<LocationBlock> range = ai.GetMoveRange(currentSpirit.X, currentSpirit.Y);
            foreach (var r in range)
            {
                blockMap[r.X, r.Y].Status = BattleBlockStatus.SelectMove;
            }
        }

        private void ShowHotKeyText()
        {
            battleFieldContainer.skillHotKeysPanel.Show(currentSpirit.Role);
            //battleFieldContainer.hotKeyText.Text = tmp;
            //battleFieldContainer.hotKeyText.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideHotKeyText()
        {
            battleFieldContainer.skillHotKeysPanel.Hide();
            //battleFieldContainer.hotKeyText.Visibility = System.Windows.Visibility.Collapsed;
        }

        public int SwitchX(int x) { return x * CommonSettings.SPIRIT_BLOCK_SIZE; }
        public int SwitchY(int y) { return y * CommonSettings.SPIRIT_BLOCK_SIZE; }

        //角色移动动画,阻塞
        public void RoleMoveTo(int x, int y, bool isAI = false)
        {
            List<MoveSearchHelper> way = ai.GetWay(currentSpirit.X, currentSpirit.Y, x, y);
            currentSpirit.Move(way, isAI);
            this.IsEnabled = false;
        }

        #endregion

        #region 战斗与技能动画
        //显示技能动画
        private void ShowSkillAnimation(SkillBox skill, int x, int y)
        {
            //currentSpirit.Status = Spirit.SpiritStatus.Attacking;
            new SelfManagedAnimation(RootCanvas, 1, skill.Animation, Configer.Instance.SkillAnimtionSwitchTime,
                SwitchX(x) + CommonSettings.SPIRIT_BLOCK_SIZE / 2,
                SwitchY(y) + CommonSettings.SPIRIT_BLOCK_SIZE );
        }

        private void ShowEffectAnimation(CommonSettings.VoidCallBack callback)
        {
            int currentZIndex = Canvas.GetZIndex(currentSpirit);

            if (currentSkill.IsSpecial)
            {
                string skillName = "jiqi1";

                //记录联机信息
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimation = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimationName = skillName;
                }

                effectCover.Visibility = Visibility.Visible;
                
                Canvas.SetZIndex(effectCover, CommonSettings.Z_EFFECTCOVER);
                Canvas.SetZIndex(currentSpirit, CommonSettings.Z_EFFECTROLE);

                CommonSettings.VoidCallBack newcallback = () =>
                    {
                        effectCover.Visibility = Visibility.Collapsed;
                        Canvas.SetZIndex(currentSpirit, currentZIndex);
                        callback();
                    };

                AnimationGroup effect = SkillManager.GetSkillAnimation(skillName);

                if (currentSpirit.Role.Female)
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.女", "音效.女2", "音效.女3", "音效.女4" });
                }
                else
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.男", "音效.男2", "音效.男3", "音效.男4", "音效.男5", "音效.男-哼" });
                }
                new SelfManagedAnimation(RootCanvas, 1, effect, (int)(Configer.Instance.SkillAnimtionSwitchTime / 2),
                    SwitchX(currentSpirit.X) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_WIDTH / 2,
                    SwitchY(currentSpirit.Y) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_HEIGHT / 2,
                    800,
                    600,
                    newcallback, CommonSettings.Z_EFFECT, 0.6, true, false);
            }
            else if (currentSkill.IsAoyi)
            {
                effectCover.Visibility = Visibility.Visible;

                Canvas.SetZIndex(effectCover, CommonSettings.Z_EFFECTCOVER);
                Canvas.SetZIndex(currentSpirit, CommonSettings.Z_EFFECTROLE);

                SelfManagedAnimation anime = null;

                CommonSettings.VoidCallBack newcallback = () =>
                {
                    uiHost.battleFieldContainer.aoyiHead.Source = currentSpirit.Role.Head;
                    uiHost.battleFieldContainer.aoyiHeadCopy.Source = uiHost.battleFieldContainer.aoyiHead.Source;
                    uiHost.battleFieldContainer.aoyiText.Text = currentSkill.AoyiInstance.Aoyi.Name;
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiHead, CommonSettings.Z_EFFECTAOYI);
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiHeadCopy, CommonSettings.Z_EFFECTAOYI);
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiText, CommonSettings.Z_EFFECTAOYI);

                    uiHost.battleFieldContainer.aoyiHead.Visibility = Visibility.Visible;
                    uiHost.battleFieldContainer.aoyiHeadCopy.Visibility = Visibility.Visible;
                    uiHost.battleFieldContainer.aoyiText.Visibility = Visibility.Visible;

                    uiHost.battleFieldContainer.AoyiBoard.Begin();
                    uiHost.battleFieldContainer.AoyiBoard.Completed += (object sender, EventArgs e) =>
                    {
                        effectCover.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiHead.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiHeadCopy.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiText.Visibility = Visibility.Collapsed;
                        anime.remove();

                        Canvas.SetZIndex(currentSpirit, currentZIndex);
                        callback();
                    };
                };
                //string[] aoyiskills = new string[] { "特效.奥义1", "特效.奥义2", "特效.奥义3", "特效.奥义4", "特效.奥义5", "特效.奥义6", "特效.奥义7" };
                //string aoyiskill = aoyiskills[Tools.GetRandomInt(0, aoyiskills.Length - 1)];
                //SkillAnimation effect = SkillManager.GetSkillAnimation(aoyiskill);
                AnimationGroup effect = currentSkill.AoyiInstance.Aoyi.Animations;
                if (effect == null)
                {
                    effect = SkillManager.GetSkillAnimation("aoyi1");
                }

                //记录联机信息
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimation = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimationName = effect.Name;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.isAoyi = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.aoyiName = currentSkill.AoyiInstance.Aoyi.Name;
                }

                if (currentSpirit.Role.Female)
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.女", "音效.女2", "音效.女3", "音效.女4" }, () =>
                    {
                        AudioManager.PlayRandomEffect(new String[] { "音效.内功攻击4", "音效.打雷", "音效.奥义1", "音效.奥义2", "音效.奥义3", "音效.奥义4", "音效.奥义5", "音效.奥义6" });
                    });
                }
                else
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.男", "音效.男2", "音效.男3", "音效.男4", "音效.男5", "音效.男-哼" }, () =>
                    {
                        AudioManager.PlayRandomEffect(new String[] { "音效.内功攻击4", "音效.打雷", "音效.奥义1", "音效.奥义2", "音效.奥义3", "音效.奥义4", "音效.奥义5", "音效.奥义6" });
                    });
                }

                anime = new SelfManagedAnimation(RootCanvas, 1, effect, Configer.Instance.SkillAnimtionSwitchTime / 2,
                    uiHost.battleFieldContainer.fieldMarginLeft/*SwitchX(currentSpirit.X) - 250*/,
                    uiHost.battleFieldContainer.fieldMarginTop/*SwitchY(currentSpirit.Y) - 380*/,
                    800,
                    600,
                    newcallback, CommonSettings.Z_EFFECT,
                    1.0,
                    false, false);
            }
            else if (currentSkill.IsUnique)
            {
                string skillName = "jiqi2";

                //记录联机信息
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimation = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimationName = skillName;
                }

                effectCover.Visibility = Visibility.Visible;
                Canvas.SetZIndex(effectCover, CommonSettings.Z_EFFECTCOVER);
                Canvas.SetZIndex(currentSpirit, CommonSettings.Z_EFFECTROLE);

                CommonSettings.VoidCallBack newcallback = () =>
                {
                    effectCover.Visibility = Visibility.Collapsed;
                    Canvas.SetZIndex(currentSpirit, currentZIndex);
                    callback();
                };

                AnimationGroup effect = SkillManager.GetSkillAnimation(skillName);

                if (currentSpirit.Role.Female)
                {
                    AudioManager.PlayRandomEffect(new string[]{ "音效.女", "音效.女2", "音效.女3", "音效.女4" });
                }
                else
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.男", "音效.男2", "音效.男3", "音效.男4", "音效.男5", "音效.男-哼" });
                }

                new SelfManagedAnimation(RootCanvas, 1, effect, Configer.Instance.SkillAnimtionSwitchTime / 2,
                    SwitchX(currentSpirit.X) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_WIDTH / 2,
                    SwitchY(currentSpirit.Y) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_HEIGHT / 2,
                    800,
                    600,
                    newcallback, CommonSettings.Z_EFFECT, 0.6, true, false);
            }
            else
            {
                callback();
            }
        }


        //攻击信息，如掉血、debuff、buff
        //private void ShowAttackInfo(string info, int x, int y)
        //{
        //    new AttackInfo(RootCanvas, SwitchX(x), SwitchY(y), info, Colors.White);
        //}

        //private void ShowCriticalAttackInfo(string info, int x, int y)
        //{
        //    new AttackInfo(RootCanvas, SwitchX(x), SwitchY(y), info, Colors.Yellow);
        //}

        public void BlockUnselective()
        {
            for (int i = 0; i < actualXBlockNo; ++i)
            {
                for (int j = 0; j < actualYBlcokNo; ++j)
                {
                    blockMap[i, j].Status = BattleBlockStatus.IDLE;
                }
            }
        }

        int skilltarget_x;
        int skilltarget_y;
        
        //order：表明这个对话发生的时点，是行动时候，还是行动前
        public void ShowSmallDialogBox(Spirit s, string[] info, double property=1, TextCastOrder order = TextCastOrder.Action)
        {
            if (s == null || info == null || info.Length == 0 || property < 0) 
                return;
            this.ShowSmallDialogBox(s, info[Tools.GetRandomInt(0, info.Length) % info.Length], property, order);
        }

        public void ShowSmallDialogBox(Spirit s, string info, double property = 1, TextCastOrder order = TextCastOrder.Action)
        {
            if (!Tools.ProbabilityTest(property))
                return;

            //联机模式下，记录当前的显示状态
            if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
            {
                if (order == TextCastOrder.PreAction)
                {
                    if (!OLBattleGlobalSetting.Instance.battleData.preRoleWords.ContainsKey(s.battleID))
                        OLBattleGlobalSetting.Instance.battleData.preRoleWords.Add(s.battleID, info);
                    else
                        OLBattleGlobalSetting.Instance.battleData.preRoleWords[s.battleID] = info;
                }
                else if (order == TextCastOrder.Action)
                {
                    if (!OLBattleGlobalSetting.Instance.battleData.roleWords.ContainsKey(s.battleID))
                        OLBattleGlobalSetting.Instance.battleData.roleWords.Add(s.battleID, info);
                    else
                        OLBattleGlobalSetting.Instance.battleData.roleWords[s.battleID] = info;
                }
            }
            s.ShowSmallDialog(info);
        }


        internal Aoyi ChangeToAoyi()
        {
            List<Aoyi> aoyis = SkillManager.GetAoyis();
            Aoyi aoyi2change = null;
            foreach (Aoyi aoyi in aoyis)
            {
                //奥义概率加成
                double aoyiProbIndex = 1.0;
                aoyiProbIndex *=  (1 + (currentSpirit.Role.AttributesFinal["wuxing"] / 150.0) * 0.2);
                if (currentSpirit.Role.HasTalent("博览群书"))
                {
                    aoyiProbIndex += 0.5;
                }
                if (currentSpirit.Role.HasTalent("屌丝") && currentSkill.Name == "野球拳")
                {
                    aoyiProbIndex += 0.1;
                }

                List<ItemTrigger> triggers = currentSpirit.Role.GetItemTriggers("powerup_aoyi");
                if(triggers.Count>0)
                {
                    foreach(var tr in triggers)
                    {
                        if(tr.Argvs[0] == aoyi.Name)
                        {
                            //aoyi.AddPower *= (1 + int.Parse(tr.Argvs[1]) / 100f);
                            aoyiProbIndex += int.Parse(tr.Argvs[2]) / 100f;
                        }
                    }
                }

                //当前释放的技能是某个奥义的起始技能，且概率上释放技能的可能性成立
                if (currentSkill.Name == aoyi.Start && currentSkill.Level >= aoyi.StartLevel &&
                    Tools.ProbabilityTest(aoyi.Probability * aoyiProbIndex))
                {
                    bool allOK = true;
                    //判断当前奥义释放的条件是否满足
                    foreach (AoyiCondition condition in aoyi.conditions)
                    {
                        bool ok = false;
                        #region 判断是否可以释放奥义
                        if (condition.type == "skill")
                        {
                            foreach (SkillInstance skill in currentSpirit.Role.Skills)
                            {
                                if (skill.Skill.Name == condition.value && skill.Level >= condition.level)
                                {
                                    ok = true; break;
                                }
                            }
                        }

                        if (condition.type == "internalskill")
                        {
                            foreach (InternalSkillInstance skill in currentSpirit.Role.InternalSkills)
                            {
                                if (skill.Skill.Name == condition.value && skill.Level >= condition.level)
                                {
                                    ok = true; break;
                                }
                            }
                        }

                        if (condition.type == "talent")
                        {
                            if (currentSpirit.Role.HasTalent(condition.value))
                                ok = true;
                        }
                        #endregion

                        //如果当前条件不满足，无法释放奥义
                        if (!ok)
                        {
                            allOK = false;
                            break;
                        }
                    }

                    //条件满足，释放奥义
                    if (allOK)
                    {
                        aoyi2change = aoyi;
                        break;
                    }
                }
            }
            return aoyi2change;
        }

        internal void PreCastingSkill()
        {
            if ((!currentSkill.IsSpecial)) //内功也可以发动奥义
            {
                Aoyi aoyi = ChangeToAoyi();
                if (aoyi != null)
                {
                    AoyiInstance aoyiInstance = new AoyiInstance();
                    aoyiInstance.uihost = this.uiHost;
                    aoyiInstance.Owner = currentSpirit.Role;
                    aoyiInstance.skill = currentSkill.Instance;
                    aoyiInstance.Aoyi = aoyi;

                    currentSkill.AoyiInstance = aoyiInstance;
                }
            }
            
            BlockUnselective();
            AttackInfoNew attackInfo = new AttackInfoNew();
            string skillinfo = currentSpirit.Role.GetEquippedInternalSkill().Skill.Name + " + " + currentSkill.Name;

            Color skillColor = Colors.White;
            if (currentSkill.IsSpecial)
            {
                skillinfo = currentSkill.Name;
            }
            else if (currentSkill.IsAoyi)
            {
                skillinfo = currentSkill.AoyiInstance.Aoyi.Name;
            }   
            else if(currentSkill.IsInternalUnique)
            {
                skillinfo = currentSkill.Name;
                if (currentSkill.InternalInstance.Yin > currentSkill.InternalInstance.Yang)
                {
                    skillColor = Colors.Magenta;
                }
                else if (currentSkill.InternalInstance.Yin == currentSkill.InternalInstance.Yang)
                {
                    skillColor = Colors.Yellow;
                }
                else
                {
                    skillColor = Colors.Red;
                }
            }
            else
            {
                if (currentSkill.Tiaohe)
                {
                    skillColor = Colors.Yellow;
                }
                else
                {
                    if (currentSkill.Suit < 0) skillColor = Colors.Magenta;
                    if (currentSkill.Suit > 0) skillColor = Colors.Red;
                }
            }
            attackInfo.Show(skillColor, SwitchX(currentSpirit.X), SwitchY(currentSpirit.Y), skillinfo, RootCanvas);
            currentSpirit.Status = Spirit.SpiritStatus.Attacking;

            //如果是联机模式，则记录当前的攻击信息
            if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
            {
                SkillInfo info = OLBattleGlobalSetting.Instance.battleData.currentSkill;
                info.actionType = "SKILL";
                info.name = skillinfo;
                info.skillAnimationTemplate = currentSkill.AnimationName;
                info.skillCastSize = currentSkill.Size;
                info.skillCoverType = (int)currentSkill.CoverType;
                info.targetx = skilltarget_x;
                info.targety = skilltarget_y;
                info.audio = currentSkill.Audio;
            }

            //如果有特效，先来特效
            CommonSettings.VoidCallBack callback = () =>
            {
                if (skillWaitTimer == null)
                {
                    skillWaitTimer = new DispatcherTimer();
                    skillWaitTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    skillWaitTimer.Tick += (s, e) =>
                    {
                        skillWaitTimer.Stop();
                        skillWaitTimer = null;
                        //attackInfo.Show(skillColor, SwitchX(currentSpirit.X), SwitchY(currentSpirit.Y), skillinfo, RootCanvas);
                        CastSkill();
                        //battleFieldContainer.cameraScrollTo(SwitchX(skilltarget_x), SwitchY(skilltarget_y), () => { CastSkill(); });
                    };
                }
                skillWaitTimer.Start();
            };

            ShowEffectAnimation(callback);
        }

        DispatcherTimer skillWaitTimer = null;
        
        internal void CastSkill()
        {
            
            if(currentSkill.IsAoyi)
            {
                Log(currentSpirit.Role.Name + "施展奥义【" + currentSkill.AoyiInstance.Aoyi.Name + "】");
            }
            else
            {
                Log(currentSpirit.Role.Name + "施展【" + currentSkill.Name + "】");
            }
            currentSkill.AddCastCd();
            int hitNumber = 0;

            int x = skilltarget_x;
            int y = skilltarget_y;
            if (x > currentSpirit.X)
            {
                currentSpirit.FaceRight = true;
            }
            if (x < currentSpirit.X)
            {
                currentSpirit.FaceRight = false;
            }
            
            //播放音效
            AudioManager.PlayEffect(currentSkill.Audio);

            //掉Mp
            currentSpirit.Mp -= currentSkill.CostMp;

            List<LocationBlock> blocks = currentSkill.GetSkillCoverBlocks(x, y, currentSpirit.X, currentSpirit.Y);
            foreach (var b in blocks)
            {
                Spirit attackTarget = Attack(currentSpirit, currentSkill, b.X, b.Y);
                if(attackTarget != null && attackTarget.Team != currentSpirit.Team)
                {
                    hitNumber++;
                }
            }

            if (hitNumber > 0) //有命中目标
            {
                //外功
                if (currentSkill.TryAddExp(10))
                {
                    currentSpirit.AddAttackInfo(currentSkill.Name + " Level up!",Colors.Green);
                    AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));
                    //MessageBox.Show(string.Format("【{0}】的【{1}】升到第【{2}】级!", currentSkill.Owner.Name, currentSkill.Name, currentSkill.Level));
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.type = "DIALOG";
                    dialog.info = string.Format("【{0}】的【{1}】升到第【{2}】级!", currentSkill.Owner.Name, currentSkill.Name, currentSkill.Level);
                    uiHost.dialogPanel.ShowDialog(dialog);
                }

                //内功
                if (currentSpirit.Role.GetEquippedInternalSkill().TryAddExp(10))
                {
                    InternalSkillInstance internalSkillInstance = currentSpirit.Role.GetEquippedInternalSkill();
                    currentSpirit.AddAttackInfo(currentSpirit.Role.GetEquippedInternalSkill().Skill.Name + " Level up!",Colors.Green);
                    AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));

                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.type = "DIALOG";
                    dialog.info = string.Format("【{0}】的【{1}】升到第【{2}】级!", internalSkillInstance.Owner.Name, internalSkillInstance.Skill.Name, internalSkillInstance.Level);
                    uiHost.dialogPanel.ShowDialog(dialog);
                }

                //角色经验
                if (currentSpirit.Role.AddExp(5))
                {
                    currentSpirit.AddAttackInfo(currentSpirit.Role.Name + "Level up!",Colors.Green);
                    AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.type = "DIALOG";
                    dialog.info = string.Format("【{0}】升到第【{1}】级", currentSpirit.Role.Name, currentSpirit.Role.Level);
                    uiHost.dialogPanel.ShowDialog(dialog);
                }

                currentSpirit.Role.Balls -= currentSkill.BallCost;
            }

            Status = (BattleStatus.NextPerson);
            
        }

        
        //攻击目标
        private Spirit Attack(Spirit source,SkillBox skill,int x,int y)
        {
            int targetX = x, targetY = y;
            Spirit target = GetSpirit(x, y);
            if (target == null)
            {
                ShowSkillAnimation(skill, targetX, targetY);
                return null;
            }
            if (source == target && !skill.HitSelf)
            {
                ShowSkillAnimation(skill, targetX, targetY);
                return null;
            }
            if (!RuntimeData.Instance.FriendlyFire && !skill.IsSpecial && source.Team == target.Team)
            {
                ShowSkillAnimation(skill, targetX, targetY);
                return null;
            }
            

            //if ( ((!skill.IsSpecial) || (skill.IsSpecial && (!skill.HitSelf)) ) && (source == target || target == null || target.Team == source.Team))
            //{
            //    //Status = (BattleStatus.NextPerson);
            //    //显示技能动画
            //    ShowSkillAnimation(skill, targetX, targetY);
            //    return null;
            //}

            //attack logic
            AttackResult result = new AttackResult();

            //如果有溜须拍马，因为攻击敌人，溜须拍马取消
            if (target.Team != source.Team && source.Role.GetBuff("溜须拍马")!=null)
            {
                string msg = source.Role.Name + "的溜须拍马状态取消！";
                this.ShowSmallDialogBox(target, "竟然敢打我？！");
                source.Role.DeleteBuff("溜须拍马");
                source.Refresh();
                Log(msg);
            }

            //碎裂的怒吼，震晕周围敌人
            #region 碎裂的怒吼
            //效果：周围距离为3的敌人晕眩，发动奥义情况下30%概率晕眩
            if (source.Role.HasTalent("碎裂的怒吼") && skill.IsAoyi && Tools.ProbabilityTest(0.3))
            {
                this.ShowSmallDialogBox(source, "大地，震裂！");
                Log(source.Role.Name + "天赋【碎裂的怒吼】发动");
                foreach (var s in this.Spirits)
                {
                    if (s.Team != source.Team && Math.Abs(s.X - source.X) + Math.Abs(s.Y - source.Y) <= 3)
                    {
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = new Buff() { Name = "晕眩", Level = 0 },
                            Owner = s.Role,
                            LeftRound = 2
                        };
                        s.Role.AddBuff(newBuff);
                        s.AddAttackInfo("晕眩！", Colors.Red);
                        s.Refresh();
                        Log(s.Role.Name + "晕眩了");
                    }
                }
            }
            #endregion
            //飞向天际 30%概率吹飞
            #region 飞向天际
            if (source.Role.HasTalent("飞向天际") && target.Team != source.Team && Tools.ProbabilityTest(0.3))
            {
                this.ShowSmallDialogBox(source, "给我飞吧！");
                Log(source.Role.Name + "天赋【飞向天际】发动");
                Log(target.Role.Name + "被击飞");
                int dx = 0;
                if ((source.X - target.X) == 0)
                {
                    dx = 0;
                }
                else
                {
                    dx = (source.X - target.X) / Math.Abs(source.X - target.X);
                }
                
                int dy = 0;
                if ((source.Y - target.Y) == 0)
                {
                    dy = 0;
                }
                else
                {
                    dy = (source.Y - target.Y) / Math.Abs(source.Y - target.Y);
                }
                int currentX = target.X;
                int currentY = target.Y;
                int count = 1;
                while (true)
                {
                    int ddx = (dx == 0) ? 0 : -dx * count;
                    int ddy = (dy == 0) ? 0 : -dy * count;
                    int newx = target.X + ddx;
                    int newy = target.Y + ddy;
                    if (ai.IsEmptyBlock(newx, newy))
                    {
                        count++;
                        currentX = newx;
                        currentY = newy;
                    }
                    else
                    {
                        target.X = currentX;
                        target.Y = currentY;
                        break;
                    }
                }
            }
            #endregion

            //处理各种特殊攻击
            #region 天地同寿&&同归剑法&&六合劲
            if (skill.Name == "天地同寿")
            {
                this.ShowSmallDialogBox(source, "狗贼，我跟你拼了！");

                double rd = Tools.GetRandom(0.5, 1.0);
                result.Hp = (int) (source.Role.Attributes["hp"] * rd);
                source.Role.Attributes["hp"] = 0;
                source.Role.Attributes["maxhp"] = (int) (source.Role.Attributes["maxhp"] - 10);
                Log(source.Role.Name + "使用天地同寿，自杀");
            }
            else if (skill.Name == "同归剑法")
            {
                this.ShowSmallDialogBox(source, "来吧！！看我同归剑法", 1);
                result.Hp = 1000 + Tools.GetRandomInt(0, 4000);
                source.Hp -= 1000;
                source.AddAttackInfo("自伤1000", Colors.White);
                Log(source.Role.Name + "使用同归剑法，自伤HP1000");
            }
            else if(skill.Name == "六合劲")
            {
                this.ShowSmallDialogBox(source, "看我内功的劲力！");
                target.Sp = 0;
                target.AddAttackInfo("集气清零", Colors.Yellow);
                Log(source.Role.Name + "使用六合劲，对方集气清零");
            }
            #endregion
            #region 笑傲江湖曲
            else if (skill.Name == "笑傲江湖曲")
            {
                this.ShowSmallDialogBox(source, "铮铮一曲玉石碎，为君且作沧海歌！");
                foreach (Spirit sp in Spirits)
                {
                    if (sp.Team == source.Team)
                    {
                        BuffInstance buff = sp.Role.GetBuff("攻击强化");
                        Buff b = new Buff(); b.Name = "攻击强化"; b.Level = 5; b.Round = 3;
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = b,
                            Owner = source.Role,
                            LeftRound = b.Round
                        };
                        if (buff == null)
                        {
                            sp.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                                buff = newBuff;
                        }
                        sp.AddAttackInfo("攻击力上升！", Colors.Red);
                        ShowSkillAnimation(skill, sp.X, sp.Y);
                        sp.Refresh();
                        Log(sp.Role.Name + "由于笑傲江湖曲，攻击力上升");
                    }
                }
            }
            #endregion
            #region 清心普善咒
            else if (skill.Name == "清心普善咒")
            {
                this.ShowSmallDialogBox(source, "高山流水，韵远流长。");
                foreach (Spirit sp in Spirits)
                {
                    if (sp.Team == source.Team)
                    {
                        Log(sp.Role.Name + "异常状态解除！");
                        sp.AddAttackInfo("异常状态解除！", Colors.White);
                        ShowSkillAnimation(skill, sp.X, sp.Y);

                        if (sp.Role.Buffs == null || sp.Role.Buffs.Count == 0)
                            continue;

                        List<BuffInstance> buffs = new List<BuffInstance>();
                        foreach (var s in sp.Role.Buffs)
                        {
                            if (!s.IsDebuff)
                            {
                                buffs.Add(s);
                            }
                        }
                        sp.Role.Buffs.Clear();
                        sp.Role.Buffs = buffs;

                        sp.Refresh();
                    }
                }
            }
            #endregion
            #region 阿碧的歌声
            else if (skill.Name == "阿碧的歌声")
            {
                this.ShowSmallDialogBox(source, "吴歌一曲醉芙蓉~");
                foreach (Spirit sp in Spirits)
                {
                    if (sp.Team == source.Team)
                    {
                        Log(sp.Role.Name + "异常状态解除！");
                        sp.AddAttackInfo("异常状态解除！", Colors.White);
                        ShowSkillAnimation(skill, sp.X, sp.Y);

                        if (sp.Role.Buffs == null || sp.Role.Buffs.Count == 0)
                            continue;

                        List<BuffInstance> buffs = new List<BuffInstance>();
                        foreach (var s in sp.Role.Buffs)
                        {
                            if (!s.IsDebuff)
                            {
                                buffs.Add(s);
                            }
                        }
                        sp.Role.Buffs.Clear();
                        sp.Role.Buffs = buffs;

                        sp.Refresh();
                    }
                }
            }
            #endregion
            #region 吴侬软语
            else if (skill.Name == "吴侬软语")
            {
                this.ShowSmallDialogBox(source, "风轻轻，柳青青，侬的怒气快平息~");
                foreach (Spirit sp in Spirits)
                {
                    if (sp.Team != source.Team)
                    {
                        Log(sp.Role.Name + "增益状态解除！");
                        sp.AddAttackInfo("增益状态解除！", Colors.White);
                        ShowSkillAnimation(skill, sp.X, sp.Y);

                        if (source.Role.HasTalent("吴侬软语")) //全体晕眩
                        {
                            BuffInstance newBuff = new BuffInstance()
                            {
                                buff = new Buff() { Name = "晕眩", Level = 0 },
                                Owner = sp.Role,
                                LeftRound = 2
                            };
                            BuffInstance buff = sp.Role.GetBuff("晕眩");
                            if (buff == null)
                            {
                                sp.Role.Buffs.Add(newBuff);
                            }
                            else
                            {
                                if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                                    buff = newBuff;
                            }
                            Log(sp.Role.Name + "晕眩");
                            sp.AddAttackInfo("晕眩", Colors.Red);
                            sp.Refresh();
                        }

                        if (sp.Role.Buffs == null || sp.Role.Buffs.Count == 0)
                            continue;

                        List<BuffInstance> buffs = new List<BuffInstance>();
                        foreach (var s in sp.Role.Buffs)
                        {
                            if (s.IsDebuff)
                            {
                                buffs.Add(s);
                            }
                        }
                        sp.Role.Buffs.Clear();
                        sp.Role.Buffs = buffs;

                        sp.Refresh();
                    }
                }
            }
            #endregion
            #region 易容术
            else if (skill.Name == "易容术")
            {
                this.ShowSmallDialogBox(source, "嘻嘻，待我易容改面，藏于暗处~");
                result.Hp = 0;
                BuffInstance buff = source.Role.GetBuff("易容");
                Buff b = new Buff(); b.Name = "易容"; b.Level = 0; b.Round = 3;
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = b,
                    Owner = source.Role,
                    LeftRound = b.Round
                };
                if (buff == null)
                {
                    source.Role.Buffs.Add(newBuff);
                }
                else
                {
                    if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                        buff = newBuff;
                }

                source.Refresh();
            }
            #endregion
            #region 过肩摔&神龙摆尾
            else if (skill.Name == "过肩摔")
            {
                int dx = source.X - target.X;
                int dy = source.Y - target.Y;
                int newx = source.X + dx;
                int newy = source.Y + dy;
                if (ai.IsEmptyBlock(newx, newy))
                {
                    target.X = newx;
                    target.Y = newy;
                }
            }
            else if (skill.Name == "神龙摆尾")
            {
                int dx = source.X - target.X;
                int dy = source.Y - target.Y;
                int currentX = target.X;
                int currentY = target.Y;
                int count = 1;
                while (true)
                {
                    int ddx = (dx == 0) ? 0 : -dx * count;
                    int ddy = (dy == 0) ? 0 : -dy * count;
                    int newx = target.X + ddx;
                    int newy = target.Y + ddy;
                    if (ai.IsEmptyBlock(newx, newy))
                    {
                        count++;
                        currentX = newx;
                        currentY = newy;
                    }
                    else
                    {
                        target.X = currentX;
                        target.Y = currentY;
                        break;
                    }
                }
            }
            #region 武穆兵法
            else if (skill.Name == "武穆兵法")
            {
                foreach (var sp in this.Spirits)
                {
                    if (sp.Team == currentSpirit.Team)
                    {
                        BuffInstance buff = sp.Role.GetBuff("攻击强化");
                        Buff b = new Buff(); b.Name = "攻击强化"; b.Level = 5; b.Round = 3;
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = b,
                            Owner = source.Role,
                            LeftRound = b.Round
                        };
                        if (buff == null)
                        {
                            sp.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                                buff = newBuff;
                        }

                        buff = sp.Role.GetBuff("防御强化");
                        b = new Buff(); b.Name = "防御强化"; b.Level = 5; b.Round = 3;
                        newBuff = new BuffInstance()
                        {
                            buff = b,
                            Owner = source.Role,
                            LeftRound = b.Round
                        };
                        if (buff == null)
                        {
                            sp.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                                buff = newBuff;
                        }

                        buff = sp.Role.GetBuff("神速攻击");
                        b = new Buff(); b.Name = "神速攻击"; b.Level = 5; b.Round = 3;
                        newBuff = new BuffInstance()
                        {
                            buff = b,
                            Owner = source.Role,
                            LeftRound = b.Round
                        };
                        if (buff == null)
                        {
                            sp.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                                buff = newBuff;
                        }
                        Log(sp.Role.Name + "被武穆遗书强化");
                        sp.AddAttackInfo("强化！", Colors.Red);
                        ShowSkillAnimation(skill, sp.X, sp.Y);
                        sp.Refresh();
                    }
                }
            }
            #endregion

            #endregion
            else
            {

                result = skill.Attack(source, target);
                if (result.sourceCastInfo != null)
                {
                    this.ShowSmallDialogBox(source, result.sourceCastInfo, result.sourceCastProperty);
                }
                if (result.targetCastInfo != null)
                {
                    this.ShowSmallDialogBox(target, result.targetCastInfo, result.targetCastProperty);
                }


                #region 真武七截阵.进攻，攻击力叠加，人越多越猛
                if (source.Role.HasTalent("真武七截阵"))
                {
                    foreach (var s in Spirits)
                    {
                        if (s.Role.HasTalent("真武七截阵") && s.Role.Key != source.Role.Key && s.Team == source.Team)
                        {
                            double addPower = Tools.GetRandom(0, 1.0);
                            if (result.Hp > 0)
                                result.Hp = (int)(result.Hp * (1 + addPower / 10.0));
                            this.ShowSmallDialogBox(s, "这一招，我们一起杀敌！（真武七截阵发动！）");
                            Log(s.Role.Name + "真武七截阵发动，叠加攻击效果");
                        }
                    }
                }
                #endregion

                #region 金刚伏魔圈
                if (source.Role.HasTalent("金刚伏魔圈"))
                {
                    int count = 0;
                    foreach (var s in Spirits)
                    {
                        if (s.Role.HasTalent("金刚伏魔圈") && s.Team == source.Team)
                        {
                            count++;
                        }
                    }

                    if (count >= 3) //至少要三个人
                    {
                        result.Hp = (int)(result.Hp * 1.5);
                        foreach (var s in Spirits)
                        {
                            if (s.Role.HasTalent("金刚伏魔圈") && s.Team == source.Team)
                            {
                                this.ShowSmallDialogBox(s, "喝！（金刚伏魔圈发动！）");
                                Log(s.Role.Name + "金刚伏魔圈发动，叠加攻击效果");
                            }
                        }
                    }
                    else
                    {
                        foreach (var s in Spirits)
                        {
                            if (s.Role.HasTalent("金刚伏魔圈") && s.Team == source.Team)
                            {
                                s.AddAttackInfo("金刚伏魔圈解除!", Colors.Red);
                                s.Role.RemoveTalent("金刚伏魔圈");
                                Log(s.Role.Name + "金刚伏魔圈被破阵了！");
                            }
                        }
                    }
                }
                #endregion

                #region 峨眉宗师，技能叠加
                if (source.Role.HasTalent("峨眉宗师"))
                {
                    if (skill.Name.Contains("飘雪穿云掌") || skill.Name.Contains("四象掌") || skill.Name.Contains("佛光普照")
                        || skill.Name.Contains("截手九式") || skill.Name.Contains("峨眉剑法") || skill.Name.Contains("回风拂柳剑")
                        || skill.Name.Contains("灭剑绝剑") || skill.Name.Contains("九阴白骨爪") || skill.Name.Contains("霹雳雷火弹")
                        || skill.Name.Contains("落英神剑掌") || skill.Name.Contains("玉箫剑法") || skill.Name.Contains("弹指神通"))
                    {
                        string[] infos = { 
                                             "庄生晓梦迷蝴蝶，望帝春心托杜鹃...",
                                             "佛光普照空悲切,西子捧心谁能绝!",
                                             "貂蝉拜月望不断,昭君出塞意缠绵!" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.5, 3.0));
                        }
                        Log(source.Role.Name + "天赋【峨眉宗师】发动，增加攻击力");
                    }
                }
                #endregion

                #region 刚柔并济，技能叠加
                if (source.Role.HasTalent("刚柔并济"))
                {
                    if (skill.Name.Contains("太极拳") || skill.Name.Contains("太极剑") || skill.Name.Contains("绵掌") || skill.Name.Contains("玄虚刀法") || skill.Name.Contains("倚天屠龙笔法")
                        || skill.Name.Contains("柔云剑法") || skill.Name.Contains("绕指柔剑") || skill.Name.Contains("神门十三剑") || skill.Name.Contains("绝户虎爪手"))
                    {
                        string[] infos = { 
                                             "以静制动，以柔克刚",
                                             "辩位于尺寸毫厘，制动于擒扑封闭",
                                             "太极生圆" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.3);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.1, 1.2));
                        }
                        Log(source.Role.Name + "天赋【刚柔并济】发动，增加攻击力");
                    }
                }
                #endregion

                #region 易经伐髓，技能叠加
                if (source.Role.HasTalent("易经伐髓"))
                {
                    if (skill.Name.Contains("罗汉拳") || skill.Name.Contains("韦陀棍") || skill.Name.Contains("般若掌")
                        || skill.Name.Contains("拈花指") || skill.Name.Contains("伏魔棍") || skill.Name.Contains("达摩剑法")
                        || skill.Name.Contains("大金刚掌") || skill.Name.Contains("如来千叶手") || skill.Name.Contains("天竺佛指")
                        || skill.Name.Contains("须弥山掌") || skill.Name.Contains("燃木刀法") || skill.Name.Contains("龙爪手"))
                    {
                        string[] infos = { 
                                             "无色无相，无嗔无狂。",
                                             "一花一世界，一叶一菩提。",
                                             "扫地扫地扫心地，心地不扫空扫地。" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 2.0));
                        }
                        Log(source.Role.Name + "天赋【易经伐髓】发动，增加攻击力");
                    }
                }
                #endregion

                #region 天龙.盖世英雄，技能叠加
                if (source.Role.HasTalent("天龙.盖世英雄"))
                {
                    if (skill.Name.Contains("降龙十八掌") || skill.Name.Contains("擒龙功") || skill.Name.Contains("易筋经")
                        || skill.Name.Contains("打狗棒法"))
                    {
                        string[] infos = { 
                                             "龙战于野！",
                                             "或跃在渊!",
                                             "吃我这一招!" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.8));
                        }
                        Log(source.Role.Name + "天赋【天龙.盖世英雄】发动，增加攻击力");
                    }
                }
                #endregion

                #region 铁剑掌门，技能叠加
                if (source.Role.HasTalent("铁剑掌门"))
                {
                    if (skill.Name.Contains("铁剑剑法") || skill.Name.Contains("漫天花雨") || skill.Name.Contains("碧落苍穹") || skill.Name.Contains("铁血大旗功"))
                    {
                        string[] infos = { 
                                             "看我铁剑门的厉害！",
                                             "吃我这一招!" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * 1.4);
                        }
                        Log(source.Role.Name + "天赋【铁剑掌门】发动，增加攻击力");
                    }
                }
                #endregion

                #region 射雕英雄
                if (source.Role.HasTalent("射雕英雄"))
                {
                    if (skill.Name.Contains("降龙十八掌") || skill.Name.Contains("打狗棒法") || skill.Name.Contains("九阴真经"))
                    {
                        string[] infos = {
                                             "蓉儿，看我的!",
                                             "侠之大者，为国为民",
                                             "呵！！"
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.5));
                        }
                        Log(source.Role.Name + "天赋【射雕英雄】发动，增加攻击力");
                    }
                }
                #endregion

                #region 玲珑璇玑
                if (source.Role.HasTalent("玲珑璇玑"))
                {
                    if (skill.Name.Contains("打狗棒法") || skill.Name.Contains("九阴真经"))
                    {
                        string[] infos = {
                                             "靖哥哥，看我的!",
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.35);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.5));
                        }
                        Log(source.Role.Name + "天赋【玲珑璇玑】发动，增加攻击力");
                    }
                }
                #endregion

                #region 大理世家，技能叠加
                if (source.Role.HasTalent("大理世家"))
                {
                    if (skill.Name.Contains("六脉神剑") || skill.Name.Contains("一阳指"))
                    {
                        string[] infos = { 
                                             "无形剑气！",
                                             "大理段氏的威名!",
                                             "我戳死你!" 
                                         };
                        this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                        if (result.Hp > 0)
                        {
                            result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.5));
                        }
                        Log(source.Role.Name + "天赋【大理世家】发动，增加攻击力");
                    }
                }
                #endregion

                #region 木婉清的眷恋，技能叠加
                bool mwqInTeam = false;
                foreach (var s in FriendTeam)
                {
                    if (s == "木婉清")
                    {
                        mwqInTeam = true;
                        break;
                    }
                }
                if (source.Role.Name == "段誉" && source.Role.HasTalent("木婉清的眷恋") && mwqInTeam)
                {
                    string[] infos = { 
                                      "婉妹，我绝不辜负你的期望！",
                                      "婉妹，看我段誉大显神通!",
                                      "婉妹，有我在！" 
                                     };
                    this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                    if (result.Hp > 0)
                    {
                        result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.8));
                    }
                    Log(source.Role.Name + "天赋【木婉清的眷恋】发动，增加攻击力");
                }
                #endregion

                #region 长平公主的眷恋，技能叠加
                if (source.Role.Name == "袁承志" && source.Role.HasTalent("长平公主的眷恋"))
                {
                    string[] infos = { 
                                      "阿九，我绝不辜负你的期望！",
                                      "阿九妹子...你如今真的出家了么？！",
                                      "阿九..." 
                                     };
                    this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 0.5);
                    if (result.Hp > 0)
                    {
                        result.Hp = (int)(result.Hp * Tools.GetRandom(1.2, 1.5));
                    }
                    Log(source.Role.Name + "天赋【长平公主的眷恋】发动，增加攻击力");
                }
                #endregion

                #region 段王爷的电眼，技能叠加
                if (source.Role.HasTalent("段王爷的电眼") && (target.Role.Female || target.Role.HasTalent("阉人")) && Tools.ProbabilityTest(0.5))
                {
                    string[] infos = { 
                                      "我段某人帅么~",
                                      "哎呀，看我段某人给你秀秀~",
                                      "我就是镇南王爷段正淳~" 
                                     };
                    this.ShowSmallDialogBox(source, infos[Tools.GetRandomInt(0, infos.Length) % infos.Length], 1.0);
                    Buff buff = new Buff();
                    buff.Name = "晕眩";
                    buff.Level = 0;
                    buff.Round = 2;
                    result.Debuff.Add(buff);
                    Log(source.Role.Name + "天赋【段王爷的电眼】发动，" + target.Role.Name + "被秀晕了");
                }
                #endregion

                #region 至空至明
                if (target.Role.HasTalent("至空至明") && Tools.ProbabilityTest(0.15))
                {
                    target.Role.SkillCdRecover();
                    this.ShowSmallDialogBox(target,
                        new string[] { "浅斟低吟浮名尽 (天赋*至空至明发动)", 
                            "流觞曲水入梦来 (天赋*至空至明发动)" },
                            0.5);
                    Log(target.Role.Name + "天赋【至空至明】发动，所有技能冷却");
                }
                #endregion

                #region 无形剑气
                if(source.Role.HasTalent("无形剑气") && currentSkill.Name == "六脉神剑")
                {
                    int delta = Math.Abs(source.Mp - target.Mp);
                    int poweradd = (int)(delta * Tools.GetRandom(0.3, 0.5));
                    result.Hp += poweradd;
                    Log(source.Role.Name + "天赋【无形剑气】发动，造成额外伤害" + poweradd);
                }
                #endregion
            }

            string attackinfo = "";

            #region 闪躲
            bool isSkip = false;
            
            BuffInstance piaomiao = target.Role.GetBuff("飘渺");
            bool shouldSkip = false;
            if (target.Team != source.Team)
                shouldSkip = true;
            if (target.Team == source.Team && (result.Hp > 0 || result.Mp > 0))
                shouldSkip = true;
            if (shouldSkip && piaomiao != null && Tools.ProbabilityTest((double)piaomiao.Level * 0.07))
            {
                isSkip = true;
            }
            if (shouldSkip && target.Role.HasTalent("飘然") && Tools.ProbabilityTest(0.08))
            {
                this.ShowSmallDialogBox(target, "我闪！ (天赋*飘然发动)", 0.5);
                Log(target.Role.Name + "天赋【飘然】发动，躲避攻击");
                isSkip = true;
            }
            if (target.Role.HasTalent("赵敏的眷念") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(target, "敏敏，我没事的！ (天赋*赵敏的眷念发动)", 0.5);
                Log(target.Role.Name + "天赋【赵敏的眷念】发动，躲避攻击");
                isSkip = true;
            }
            if (target.Team != source.Team && target.Role.HasTalent("鹿鼎.一品鹿鼎公") && Tools.ProbabilityTest(0.3))
            {
                this.ShowSmallDialogBox(target, "好汉，饶命饶命饶命饶命（念晕你！）", 1.0);
                Log(target.Role.Name + "天赋【鹿鼎.一品鹿鼎公】发动，躲避攻击。" + source.Role.Name + "被念晕了");
                isSkip = true;

                Buff b = new Buff();
                b.Name = "晕眩";
                b.Level = 0;
                b.Round = 3;
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = b,
                    Owner = source.Role,
                    Level = b.Level,
                    LeftRound = b.Round
                };
                source.Role.Buffs.Add(newBuff);
                source.AddAttackInfo("晕眩", Colors.White);
                source.Refresh();
            }
            if (target.Team != source.Team && target.Role.HasTalent("沾衣十八跌") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(target, "沾衣十八跌！", 1.0);
                isSkip = true;
                Log(target.Role.Name + "天赋【沾衣十八跌】发动，躲避攻击。" + source.Role.Name + "被震晕了");
                Buff b = new Buff();
                b.Name = "晕眩";
                b.Level = 0;
                b.Round = 2;
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = b,
                    Owner = source.Role,
                    Level = b.Level,
                    LeftRound = b.Round
                };
                source.Role.AddBuff(newBuff);
                source.AddAttackInfo("晕眩", Colors.White);
                source.Refresh();
            }
            if (target.Team != source.Team && target.Role.HasTalent("段王爷的电眼") && ( source.Role.Female || source.Role.HasTalent("阉人") ) &&  Tools.ProbabilityTest(0.5))
            {
                this.ShowSmallDialogBox(source, "他好帅，真舍不得伤害他...(天赋*段王爷的电眼发动)", 0.5);
                Log(target.Role.Name + "天赋【段王爷的电眼】发动，躲避攻击");
                isSkip = true;
            }
            if (shouldSkip && target.Role.HasTalent("孤独求败") && Tools.ProbabilityTest(0.05))
            {
                this.ShowSmallDialogBox(target, "我立于天下武学的巅峰！ (天赋*孤独求败发动)", 0.2);
                Log(target.Role.Name + "天赋【孤独求败】发动，躲避攻击");
                isSkip = true;
            }
            if (shouldSkip && target.Role.HasTalent("雪山飞狐") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(target, "雪山飞狐！", 0.5);
                Log(target.Role.Name + "天赋【雪山飞狐】发动，躲避攻击");
                isSkip = true;
            }
            if (shouldSkip && target.Role.HasTalent("神行百变"))
            {
                if ((float)target.Role.Attributes["hp"] / (float)target.Role.Attributes["maxhp"] >= 0.8)
                {
                    if (Tools.ProbabilityTest(0.15))
                    {
                        this.ShowSmallDialogBox(target, "看我神行百变！", 0.5);
                        Log(target.Role.Name + "天赋【神行百变】发动，躲避攻击");
                        isSkip = true;
                    }
                }
                else
                {
                    if (Tools.ProbabilityTest(0.07))
                    {
                        this.ShowSmallDialogBox(target, "看我神行百变！", 0.5);
                        Log(target.Role.Name + "天赋【神行百变】发动，躲避攻击");
                        isSkip = true;
                    }
                }
            }
            if (shouldSkip && target.Role.HasTalent("奇门遁甲") && Tools.ProbabilityTest(0.09))
            {
                this.ShowSmallDialogBox(target, "看我五行之术！", 0.5);
                Log(target.Role.Name + "天赋【奇门遁甲】发动，躲避攻击");
                isSkip = true;
            }
            if (isSkip && shouldSkip && source.Role.HasTalent("奇门遁甲") && Tools.ProbabilityTest(0.8))
            {
                this.ShowSmallDialogBox(source, "看我五行之术！", 0.5);
                Log(source.Role.Name + "天赋【奇门遁甲】发动，必定命中");
                isSkip = false;
            }
            if(isSkip && shouldSkip && source.Role.HasTalent("铁口直断") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(source, "我掐指一算，哎呀，你有血光之灾！", 0.5);
                Log(source.Role.Name + "天赋【铁口直断】发动，命中");
                isSkip = false;
            }
            if (isSkip && shouldSkip && source.Role.HasTalent("锐眼") && Tools.ProbabilityTest(0.05))
            {
                this.ShowSmallDialogBox(source, "哼！我看到了真相！", 0.5);
                Log(source.Role.Name + "天赋【锐眼】发动，命中");
                isSkip = false;
            }
            if(shouldSkip && source.Role.HasTalent("白内障") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(source, "让我穿针眼？算了吧！", 0.5);
                Log(source.Role.Name + "天赋【白内障】发动，MISS");
                isSkip = true;
            }

            //装备命中
            List<ItemTrigger> triggers = source.Role.GetItemTriggers("mingzhong");
            if(triggers != null && triggers.Count>0 && isSkip)
            {
                double p = 0;
                foreach(var t in triggers)
                {
                    p += int.Parse(t.Argvs[0]) / 100f;
                }
                if (Tools.ProbabilityTest(p)) isSkip = false;
            }

            double douzhuanProb = 0.12;
            if (target.Role.HasTalent("慕容世家"))
                douzhuanProb *= 2.0;
            if (target.Role.HasTalent("斗转星移") && Tools.ProbabilityTest(douzhuanProb) && target.Team != source.Team)
            {
                this.ShowSmallDialogBox(target, "以彼之道，还施彼身！（天赋*斗转星移发动！）");
                
                target.AddAttackInfo("斗转星移", Colors.White);
                isSkip = true;

                int tanHP = (int)(result.Hp * 0.5f);
                int tanMP = (int)(result.Mp * 0.5f);

                if (tanHP > 0)
                    attackinfo = string.Format("-{0}", tanHP);
                else if (tanHP < 0)
                    attackinfo = string.Format("+{0}", -tanHP);
                if (tanHP != 0)
                {
                    source.AddAttackInfo(attackinfo, Colors.White);
                    source.Hp -= tanHP;
                }

                if (tanMP > 0)
                    attackinfo = string.Format("-内力{0}", tanMP);
                else if (tanMP < 0)
                    attackinfo = string.Format("+内力{0}", -tanMP);
                if (tanMP != 0)
                {
                    source.AddAttackInfo(attackinfo, Colors.Blue);
                    source.Mp -= tanMP;
                }

                Log(target.Role.Name + "天赋【斗转星移】发动，躲避攻击，反弹伤害" + tanHP.ToString() + ",反弹内力" + tanMP.ToString());

                //显示技能动画
                ShowSkillAnimation(skill, source.X, source.Y);
            }
            if (isSkip)
            {
                result.Hp = 0;
                result.Mp = 0;
                result.costBall = 0;
                result.Critical = false;
                result.Buff.Clear();
                result.Debuff.Clear();
                target.AddAttackInfo("MISS",Colors.White);
            }
            #endregion

            #region 不老长春
            float addBLCCProb = 0.0f;
            if (target.Role.HasTalent("万年长春"))
                addBLCCProb = 0.1f;
            if (result.Hp>0 && target.Role.HasTalent("不老长春") && Tools.ProbabilityTest(0.1+addBLCCProb))
            {
                int hpRecover = (int)(result.Hp*0.35f);
                Log(target.Role.Name + "天赋【不老长春】发动，回血" + hpRecover);
                if (target.Role.GetBuff("重伤") != null)
                {
                    Log(target.Role.Name + "由于重伤，回血效果减半");    
                    hpRecover /= 2;
                }
                target.AddAttackInfo(string.Format("不老长春回血{0}", hpRecover), Colors.Green);

                target.Hp += hpRecover;
                result.Hp = 0;
                result.Mp = 0;
                result.costBall = 0;
                result.Critical = false;
                result.Buff.Clear();
                result.Debuff.Clear();
            }
            #endregion


            #region 解毒

            if (skill.Name == "解毒" && source.Team == target.Team)
            {
                if (target.Role.GetBuff("中毒") != null)
                {
                    target.Role.DeleteBuff("中毒");
                    target.AddAttackInfo("解毒", Colors.White);
                    Log(target.Role.Name + "中毒解除");
                }
            }
            #endregion

            //部分BT技能直接减生命上限
            #region 死生茫茫
            if (source.Role.HasTalent("死生茫茫") && result.Hp > 0 && Tools.ProbabilityTest(0.1))
            {
                Log(source.Role.Name + "天赋【生死茫茫】发动");

                //死生茫茫：10%几率按照所受伤害的5%减生命上限，攻击方吸收50%的气血（非上限），最多减当前血量上限的10%
                //损血
                attackinfo = string.Format("-{0}", (int)(result.Hp * 0.05) );
                target.AddAttackInfo(attackinfo, Colors.Yellow);

                int maxHpDesc = Math.Min((int)(result.Hp * 0.05), (int)(target.Role.Attributes["maxhp"] * 0.1));
                if (target.Role.Attributes["maxhp"] > maxHpDesc)
                    target.Role.Attributes["maxhp"] -= maxHpDesc;
                else
                    target.Role.Attributes["maxhp"] = 1;

                if (target.Role.Attributes["hp"] > target.Role.Attributes["maxhp"])
                    target.Role.Attributes["hp"] = target.Role.Attributes["maxhp"];
                Log(target.Role.Name + "生命值上限" + attackinfo);
                
                //吸血
                int hpRecover = (int)(result.Hp * 0.5); ;
                if (source.Role.GetBuff("重伤") != null)
                {
                    Log(target.Role.Name + "由于重伤，回血效果减半"); 
                    hpRecover /= 2;
                }
                source.Hp += hpRecover;
                Log(source.Role.Name + "天赋【生死茫茫】发动，吸血" + hpRecover);

                attackinfo = string.Format("吸血{0}", hpRecover);
                source.AddAttackInfo(attackinfo, Colors.Red);

                //显示战斗信息
                string info = "啊！好疼！" + target.Role.Name + "气血上限减少" + ((int)(result.Hp * 0.15)).ToString() + "！！";
                this.ShowSmallDialogBox(target, info);

                string info2 = "嘿嘿...（" + source.Role.Name + "天赋【死生茫茫】发动！）";
                this.ShowSmallDialogBox(source, info2);
            }
            #endregion

            #region 真武七截阵.防御
            if (target.Role.HasTalent("真武七截阵"))
            {
                foreach (var s in Spirits)
                {
                    if (s.Role.HasTalent("真武七截阵") && s.Team == target.Team && s.Role.Key != target.Role.Key && Tools.GetRandom(0, 1.0) <= 0.5)
                    {
                        Log(s.Role.Name + "阵法【真武七截阵】发动，增加防御力");
                        this.ShowSmallDialogBox(s, "这一招，我来挡！（真武七截阵发动！）");
                        result.Hp = (int)(result.Hp * 0.3f);
                        target = s;
                        targetX = target.X; targetY = target.Y;
                        break;
                    }
                }
            }
            #endregion

            #region 五行阵.防御
            if(result.Hp > 0)
            {
                List<Spirit> wuxingSpirit = new List<Spirit>();
                wuxingSpirit.Clear();
                foreach (var s in Spirits)
                {
                    if (s.Role.HasTalent("五行阵") && s.Team == target.Team && s.Role.Key != target.Role.Key && Math.Abs(s.X - target.X) + Math.Abs(s.Y - target.Y) <= 5 && Tools.ProbabilityTest(0.5))
                    {
                        wuxingSpirit.Add(s);
                    }
                }

                int resultHP = (int) (result.Hp / (double)(wuxingSpirit.Count + 1) );
                result.Hp = resultHP;

                foreach (var s in wuxingSpirit)
                {
                    Log(s.Role.Name + "阵法【五行阵】发动，增加防御力");
                    this.ShowSmallDialogBox(s, "五行迷阵，我来帮你挡！（五行阵发动！）");
                    s.Hp -= resultHP;
                    s.AddAttackInfo(string.Format("-{0}", resultHP), Colors.Yellow, AttackInfoType.CriticalHit);

                    if (s.Hp <= 0)
                        Die(s, result);
                }
            }       
            #endregion

            #region 八卦阵.防御
            if (result.Hp > 0 && target.Role.HasTalent("八卦阵") && Tools.ProbabilityTest(0.5))
            {
                List<Spirit> baguaSpirit = new List<Spirit>();
                baguaSpirit.Clear();
                foreach (var s in Spirits)
                {
                    if (s.Team == target.Team && s.Role.Key != target.Role.Key && Math.Abs(s.X - target.X) + Math.Abs(s.Y - target.Y) <= 5)
                    {
                        baguaSpirit.Add(s);
                    }
                }

                if (baguaSpirit.Count > 0)
                {
                    int damage = (int)(result.Hp * 0.8);
                    result.Hp = (int)(result.Hp * 0.2);

                    Spirit baguaS = baguaSpirit[Tools.GetRandomInt(0, baguaSpirit.Count) % baguaSpirit.Count];
                    this.ShowSmallDialogBox(baguaS, "八卦阵发动，替我挡着！（八卦阵发动！）");
                    Log(baguaS.Role.Name + "阵法【八卦阵】发动，增加防御力");
                    baguaS.Hp -= damage;
                    baguaS.AddAttackInfo(string.Format("-{0}", damage), Colors.Yellow, AttackInfoType.CriticalHit);

                    if (baguaS.Hp <= 0)
                        Die(baguaS, result);
                }
            }
            #endregion


            #region 如果有易容，因为攻击敌人，易容取消
            if (target.Team != source.Team && source.Role.GetBuff("易容") != null)
            {
                this.ShowSmallDialogBox(source, "看招！（" + source.Role.Name + "易容后，发动奇袭！）");
                Log(source.Role.Name + "发动奇袭，攻击力增加，易容取消");
                if (result.Hp > 0)
                    result.Hp = (int)(result.Hp * Tools.GetRandom(1.1, 1.5));
                source.Role.DeleteBuff("易容");
                source.Refresh();
            }
            #endregion

            if (result.Hp > 0)
                attackinfo = string.Format("-{0}", result.Hp);
            else if (result.Hp < 0)
                attackinfo = string.Format("+{0}", -result.Hp);

            if (result.Hp != 0)
            {
                if (result.Critical)
                {
                    target.AddAttackInfo(attackinfo, Colors.Yellow, AttackInfoType.CriticalHit);
                    Log("暴击！！" + target.Role.Name + "受到伤害" + result.Hp);
                }
                else
                {
                    target.AddAttackInfo(attackinfo, Colors.White);
                    Log(target.Role.Name + "受到伤害【" + result.Hp + "】");
                }
                target.Hp -= result.Hp;
            }

            #region 吸血
            float xiRatio = 0;

            //血刀大法:吸血效果
            if ( (skill.Name=="血刀大法" || skill.Name=="血刀大法.吸") && result.Hp > 0 )
            {
                if (skill.Name == "血刀大法")
                    xiRatio = 0.1f;
                else if (skill.Name == "血刀大法.吸")
                    xiRatio = 0.5f;
   
                if (source.Role.HasTalent("血海魔功"))
                {
                    this.ShowSmallDialogBox(source, "嘿嘿嘿，吸光你们！", 0.3);
                    xiRatio += 0.2f;
                }
                if (source.Role.HasTalent("血魔刀法"))
                {
                    this.ShowSmallDialogBox(source, "血...", 0.3);
                    xiRatio += 0.15f;
                }
            }
            if (source.Role.HasTalent("嗜血狂魔"))
            {
                this.ShowSmallDialogBox(source, "看我血刀门的厉害！", 0.3);
                xiRatio += 0.05f + (float)(0.05f * source.Role.Level / 30.0);
            }
            List<ItemTrigger> itemTriggers = source.Role.GetItemTriggers("xi");
            if (itemTriggers.Count > 0)
            {
                foreach (var t in itemTriggers)
                {
                    xiRatio += (float)(int.Parse(t.Argvs[0]) / 100.0);
                }
            }

            if (xiRatio > 0 && result.Hp > 0)
            {
                int xiHP = (int)(result.Hp * xiRatio);

                if (source.Role.GetBuff("重伤") != null)
                {
                    xiHP /= 2;
                    Log(source.Role.Name + "由于重伤效果，回复减半");
                }
                attackinfo = string.Format("吸血{0}", xiHP);
                source.AddAttackInfo(attackinfo, Colors.Red);
                source.Hp += xiHP;
                Log(source.Role.Name + "吸血" + xiHP);
            }
            #endregion

            #region 北冥神功
            if (source.Role.HasTalent("北冥神功") && target.Mp > 0 && source != target)
            {
                int xiMp = (int)(source.Role.Attributes["gengu"] * 2 * source.Role.GetEquippedInternalSkill().Level/10
                    * (2 - target.Role.Attributes["dingli"] / 100) * Tools.GetRandom(1, 1.5));
                if (xiMp > target.Mp)
                    xiMp = target.Mp;
                attackinfo = string.Format("吸内{0}", xiMp);
                source.AddAttackInfo(attackinfo, Colors.Blue);
                source.Mp += xiMp;
                target.Mp -= xiMp;
                Log(source.Role.Name + "天赋【北冥神功】发动，吸取内力" + xiMp);
            }
            if (source.Role.HasTalent("鲲跃北溟") && target.Mp > 0 && source != target && Tools.ProbabilityTest(0.5))
            {
                int xiMp = (int)(source.Role.Attributes["gengu"] * source.Role.GetEquippedInternalSkill().Level / 10
                    * (2 - target.Role.Attributes["dingli"] / 100));
                if (xiMp > target.Mp)
                    xiMp = target.Mp;
                attackinfo = string.Format("吸内{0}", xiMp);
                source.AddAttackInfo(attackinfo, Colors.Blue);
                source.Mp += xiMp;
                target.Mp -= xiMp;
                Log(source.Role.Name + "天赋【鲲跃北溟】发动，吸取内力" + xiMp);
            }
            #endregion

            #region 化功大法
            if (source.Role.HasTalent("化功大法") && target.Mp > 0 && source!= target && Tools.ProbabilityTest(0.5))
            {
                int xiMp = (int)(source.Role.Attributes["gengu"] * source.Role.GetEquippedInternalSkill().Level / 10
                    * 3 * (2 - target.Role.Attributes["dingli"] / 100));
                if (xiMp > target.Mp)
                    xiMp = target.Mp;
                attackinfo = string.Format("-内力{0}", xiMp);
                target.AddAttackInfo(attackinfo, Colors.Blue);
                target.Mp -= xiMp;
                Log(source.Role.Name + "天赋【化功大法】发动，化去内力" + xiMp);
            }
            #endregion

            #region 吸星大法
            if (source.Role.HasTalent("吸星大法") && target.Mp > 0 && source != target)
            {
                int xiMp = (int)(source.Role.Attributes["gengu"] * 2 * source.Role.GetEquippedInternalSkill().Level / 10
                    * (2 - target.Role.Attributes["dingli"] / 100) * Tools.GetRandom(1, 2));
                if (xiMp > target.Mp)
                    xiMp = target.Mp;
                attackinfo = string.Format("吸内{0}", xiMp);
                source.AddAttackInfo(attackinfo, Colors.Blue);
                source.Mp += xiMp;
                target.Mp -= xiMp;
                Log(source.Role.Name + "天赋【吸星大法】发动，吸取内力" + xiMp);

                //造成封印内力效果
                BuffInstance buff = target.Role.GetBuff("封穴");
                Buff b = new Buff(); b.Name = "封穴"; b.Level = 0; b.Round = 3;
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = b,
                    Owner = source.Role,
                    LeftRound = b.Round
                };
                if (buff == null)
                {
                    target.Role.Buffs.Add(newBuff);
                }
                else
                {
                    if (newBuff.LeftRound >= buff.LeftRound) //覆盖刷新buff
                        buff = newBuff;
                }
                target.AddAttackInfo("穴位被封！", Colors.Red);
                Log(target.Role.Name + "被吸星大法封穴");
                ShowSkillAnimation(skill, target.X, target.Y);
                target.Refresh();
            }
            #endregion

            #region 玄门罡气

            if(source.Role.HasTalent("玄门罡气") && source.Role.GetEquippedInternalSkill().Skill.Name == "九阴真经")
            {
                int spDesc = Tools.GetRandomInt(10, 40);
                Log(source.Role.Name + "天赋【玄门罡气】发动。" + target.Role.Name + "减少集气" + spDesc.ToString());
                target.Sp -= spDesc;
                target.AddAttackInfo("-集气 " + spDesc, Colors.Yellow);
            }

            #endregion

            //display MP
            if (result.Mp != 0)
            {
                string mpinfo = "";
                if (result.Mp > 0)
                    mpinfo = string.Format("-{0}内力", result.Mp);
                else if (result.Hp < 0)
                    mpinfo = string.Format("+{0}内力", -result.Mp);
            
                if (result.Critical)
                {
                    target.AddAttackInfo(mpinfo, Colors.Blue, AttackInfoType.CriticalHit);
                }
                else
                    target.AddAttackInfo(mpinfo, Colors.Blue);
                target.Mp -= result.Mp;
            }

            //display costBall
            if (result.costBall != 0)
            {
                string ballinfo = "";
                if (result.costBall > 0)
                    ballinfo = string.Format("-{0}怒气", result.costBall);
                else if (result.Hp < 0)
                    ballinfo = string.Format("+{0}怒气", -result.costBall);
                if (result.Critical)
                {
                    target.AddAttackInfo(ballinfo, Colors.Orange, AttackInfoType.CriticalHit);
                }
                else
                {
                    target.AddAttackInfo(ballinfo, Colors.Orange);
                }
                target.Role.Balls -= result.costBall;
            }

            #region buff & debuff
            //上buff
            foreach (var b in result.Buff)
            {
                BuffInstance buff = source.Role.GetBuff(b.Name);
                BuffInstance newBuff = new BuffInstance()
                    {
                        buff = b,
                        Owner = source.Role,
                        Level = b.Level,
                        LeftRound = b.Round
                    };
                //有些buff的level为持续回合数
                //if (b.Name == "醉酒" || b.Name == "溜须拍马" || b.Name == "易容")
                //{
                //    newBuff.LeftRound = b.Level;
                //}
                if (buff == null)
                {
                    source.Role.Buffs.Add(newBuff);
                }
                else
                {
                    if (newBuff.Level >= buff.Level) //覆盖刷新buff
                    {
                        buff.buff = newBuff.buff;
                        buff.Owner = newBuff.Owner;
                        buff.Level = newBuff.Level;
                        buff.LeftRound = newBuff.LeftRound;
                    }
                }
                if (newBuff.buff.Name == "魔神降临")
                {
                    AudioManager.PlayEffect(ResourceManager.Get("音效.咆哮"));
                }
                Log(source.Role.Name + "获得增益状态【" + b.Name + "】，等级" + b.Level);
            }
            //上debuff，有圣战状态就上不了
            bool isSaint = false;
            foreach (var s in target.Role.Buffs)
            {
                if (s.buff.Name == "圣战")
                {
                    isSaint = true;
                    Log(target.Role.Name + "圣战状态，不受一切减益效果影响");
                    break;
                }
            }
            if (!isSaint)
            {
                //计算抵抗不良状态概率
                List<ItemTrigger> antiDebuffItemTriggers = target.Role.GetItemTriggers("anti_debuff");
                double antiProbability = 0;
                foreach (var t in antiDebuffItemTriggers) antiProbability += int.Parse(t.Argvs[0]) / 100.0;

                foreach (var b in result.Debuff)
                {
                    if (Tools.ProbabilityTest(antiProbability))  //抵抗不良状态
                        continue;

                    BuffInstance buff = target.Role.GetBuff(b.Name);
                    BuffInstance newBuff = new BuffInstance()
                    {
                        buff = b,
                        Owner = target.Role,
                        Level = b.Level,
                        LeftRound = b.Round
                    };
                    //有些debuff的level为持续回合数
                    //if (b.Name == "晕眩" || b.Name=="诸般封印" || b.Name=="剑封印" || b.Name=="刀封印" || b.Name=="拳掌封印" || b.Name=="奇门封印")
                    //{
                    //    newBuff.LeftRound = b.Level;
                    //}
                    if (source.Role.HasTalent("毒系精通") && newBuff.buff.Name.Equals("中毒"))
                    {
                        Log(source.Role.Name + "天赋【毒系精通】发动，增强用毒效果");
                        this.ShowSmallDialogBox(source, "我毒！（天赋*毒系精通发动)");
                        newBuff.Level = newBuff.buff.Level + 3;
                        newBuff.LeftRound += 2;
                    }
                    if (source.Role.HasTalent("毒圣") && newBuff.buff.Name.Equals("中毒"))
                    {
                        Log(source.Role.Name + "天赋【毒圣】发动，增强用毒效果");
                        this.ShowSmallDialogBox(source, "无人能解！（天赋*毒圣发动)");
                        newBuff.Level = newBuff.buff.Level + 5;
                        newBuff.LeftRound += 4;
                    }
                    if (buff == null)
                    {
                        target.Role.Buffs.Add(newBuff);
                    }
                    else
                    {
                        //覆盖刷新buff
                        //MessageBox.Show("BF" + newBuff.buff.Name + ":" + newBuff.Level + ";" + newBuff.buff.Round);
                        //MessageBox.Show("BF old" + buff.buff.Name + ":" + buff.Level + ";" + buff.buff.Round);
                        if (newBuff.Level >= buff.Level)
                        {
                            buff.buff = newBuff.buff;
                            buff.Owner = newBuff.Owner;
                            buff.Level = newBuff.Level;
                            buff.LeftRound = newBuff.LeftRound;
                        }
                    }
                    Log(target.Role.Name + "获得减益状态【" + newBuff.buff.Name + "】，等级" + newBuff.Level);
                }
            }
            source.Refresh();
            target.Refresh();
            #endregion

            #region 集气
            //主动攻击方集气概率
            double sourceBallAddProbability =
                0.2 + (float)((float)source.Role.Attributes["fuyuan"] / 100) * 0.4;
            if (source.Role.HasTalent("暴躁"))
            {
                sourceBallAddProbability += 0.15;
            }
            if (Tools.ProbabilityTest(sourceBallAddProbability) && source.Team != target.Team)
            {
                source.Role.Balls++;
                if (source.Role.HasTalent("斗魂")) { 
                    source.Role.Balls++;
                    Log(source.Role.Name + "天赋【斗魂】发动，怒气增益翻倍");
                }
            }

            //被攻击方集气概率
            double targetBallAddProbability = 
                0.5 + (float)((float)target.Role.Attributes["dingli"] / 100 + (float)target.Role.Attributes["fuyuan"] / 100) * 0.25;
            if (target.Role.HasTalent("暴躁"))
            {
                targetBallAddProbability += 0.15;
            }
            if (Tools.ProbabilityTest(targetBallAddProbability) && source.Team != target.Team)
            {
                target.Role.Balls++;
                if (target.Role.HasTalent("斗魂")) { 
                    target.Role.Balls++;
                    Log(target.Role.Name + "天赋【斗魂】发动，怒气增益翻倍");
                }
            }
            #endregion

            #region 左右互博、神速攻击、醉酒、素心神剑
            int repeatTime = 0;
            double probability = 0.0f;
            //float addProbability = 0.0f;
            if (source.Role.HasTalent("幽居"))
            {
                repeatTime = 1;
                probability += 0.005f * (float)source.Role.Level;
            }
            if (source.Role.HasTalent("素心神剑"))
            {
                repeatTime = 1;
                probability += Tools.GetRandom(0, 0.2);
            }
            if (source.Role.HasTalent("左右互搏"))
            {
                repeatTime = 1;
                probability += Tools.GetRandom(0, 0.6);
            }
            foreach (BuffInstance buff in source.Role.Buffs)
            {
                if (buff.buff.Name == "左右互博" || buff.buff.Name == "醉酒" )
                {
                    repeatTime = 1; probability += Tools.GetRandom(0, ((float)buff.Level) * 0.1f);
                }
                else if (buff.buff.Name == "神速攻击")
                {
                    repeatTime = 2; probability += Tools.GetRandom(0, ((float)buff.Level) * 0.1f); 
                    break;
                }
            }

            for (int i = 0; i < repeatTime; i++)
            {
                int hp = (int)(result.Hp * (0.6 - 0.3 * i));
                if (Tools.ProbabilityTest(probability))
                {
                    if (hp > 0)
                        attackinfo = string.Format("多重攻击-{0}", hp);
                    else if (hp < 0)
                        attackinfo = string.Format("多重攻击+{0}", -hp);
                    if (result.Critical)
                    {
                        target.AddAttackInfo(attackinfo, Colors.Yellow, AttackInfoType.CriticalHit);
                    }
                    else
                        target.AddAttackInfo(attackinfo, Colors.White);
                    Log("多重攻击！！" + target.Role.Name + "受到伤害" + hp);
                    target.Hp -= hp;
                }
            }
            #endregion

            //显示技能动画
            ShowSkillAnimation(skill, targetX, targetY);

            //处理角色死亡情况
            if (source.Mp < 0) source.Mp = 0;
            if (target.Mp < 0) target.Mp = 0;
            if (source.Role.Attributes["hp"] <= 0)
            {
                Die(source, result);
            }
            if (target.Role.Attributes["hp"] <= 0)
            {
                Die(target, result);                
            }

            return target;
        }

        private void Die(Spirit sp, AttackResult result)
        {
            if (sp.Role.HasTalent("百足之虫") && (Tools.GetRandom(0, 1.0) <= 0.2))
            {
                Log(sp.Role.Name + "天赋【百足之虫】发动，不死");
                this.ShowSmallDialogBox(sp, "只要还有一口气，就不会轻易死去！");
                sp.Hp = 1;
            }
            else if (sp.Role.HasTalent("真气护体") && (sp.Role.Attributes["mp"] >= result.Hp * 2) && (Tools.ProbabilityTest(0.5)))
            {
                Log(sp.Role.Name + "天赋【真气护体】发动，改为减少内力");
                this.ShowSmallDialogBox(sp, "猛提起一口真气，又有了几分精神！");
                sp.Hp = 1;
                sp.Mp = sp.Mp - result.Hp * 2;
            }
            else if (sp.Role.HasTalent("无尽斗志") && ((sp.Role.HasTalent("我就是神") && sp.FuhuoCount == 0) || Tools.ProbabilityTest(0.1)))
            {
                Log(sp.Role.Name + "天赋【无尽斗志】发动，原地满血复活了！");
                this.ShowSmallDialogBox(sp, "天道不息，斗气不止！（天赋*无尽斗志发动！）");
                sp.Hp = sp.Role.Attributes["maxhp"];
                sp.Mp = sp.Role.Attributes["maxmp"];
                sp.Role.Balls = 6;
                sp.AddAttackInfo("原地满血复活!", Colors.Red);
                sp.FuhuoCount++;
            }
            else
            {
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.die.Add(sp.battleID);
                }

                sp.Hp = 0;
                sp.Remove();
                Spirits.Remove(sp);
                Log(sp.Role.Name + "被击败！");
            }
        }
        #endregion

        #region 控件交互触发

        /// <summary>
        /// 选择block完成
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void OnSelectBlock(int x, int y)
        {
            switch (Status)
            {
                case BattleStatus.SelectMove:
                    BlockUnselective();
                    Status = BattleStatus.Moving;
                    RoleMoveTo(x, y);
                    break;
                case BattleStatus.SelectAttack:
                    skilltarget_x = x;
                    skilltarget_y = y;
                    Status = BattleStatus.Attacking;
                    PreCastingSkill();
                    break;
                case BattleStatus.SelectItemTarget:
                    Spirit sp = GetSpirit(x, y);
                    if (sp != null)
                    {
                        Status = BattleStatus.UsingItem;
                        BlockUnselective();
                        OnUseItemTarget(currentItem, sp);
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnSelectRoleAction(RoleActionType type)
        {
            switch (type)
            {
                case RoleActionType.Attack:
                    OnSelectAttack();
                    break;
                case RoleActionType.Items:
                    OnSelectItems();
                    break;
                case RoleActionType.Rest:
                    OnSelectRest();
                    break;
                case RoleActionType.RoleStatus:
                    uiHost.rolePanel.Show(currentSpirit.Role);
                    Status = BattleStatus.SelectAction;
                    break;
                case RoleActionType.Cancel:
                    SpiritPositionRoolback();
                    Status = BattleStatus.SelectMove;
                    uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                default:
                    MessageBox.Show("error role action");
                    break;
            }
        }
        
        /// <summary>
        /// 角色位置状态回滚
        /// </summary>
        internal void SpiritPositionRoolback()
        {
            currentSpirit.X = rollbackCurrentX;
            currentSpirit.Y = rollbackCurrentY;
            currentSpirit.FaceRight = rollbackCurrentFace;
        }

        /// <summary>
        /// 选择技能完成
        /// </summary>
        /// <param name="skill"></param>
        internal void OnSelectSkill(SkillBox skill)
        {
            uiHost.skillPanel.Visibility = System.Windows.Visibility.Collapsed;
            if (skill == null) //cancel
            {
                Status = BattleStatus.SelectAction;
                return;
            }
            if (skill.IsSwitchInternalSkill) //切换内功
            {
                currentSpirit.Role.EquipInternalSkill(skill.SwitchInternalSkill);
                currentSpirit.AddAttackInfo("切换内功:" + skill.SwitchInternalSkill.Skill.Name, Colors.White);
                Status = BattleStatus.NextPerson;
                return;
            }
            currentSkill = skill;
            Status = (BattleStatus.SelectAttack);
        }

        /// <summary>
        /// 人物移动动画完成
        /// </summary>
        internal void OnMovedFinish()
        {
            this.IsEnabled = true;
            Status = (BattleStatus.SelectAction);
        }

        internal void OnSelectAttack()
        {
            bool isAllSeal = false;
            //List<Dialog> dialogs = new List<Dialog>();
            //dialogs.Clear();
            /*foreach (var s in currentSpirit.Role.Buffs)
            {
                if (s.buff.Name == "诸般封印")
                {
                    isAllSeal = true;
                    break;
                }
                /*if (s.buff.Name == "剑封印")
                {
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.img = currentSpirit.Role.Head;
                    dialog.type = "DIALOG";
                    dialog.info = "剑法遭到封印，无法施展！";
                    dialogs.Add(dialog);
                }
                if (s.buff.Name == "刀封印")
                {
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.img = currentSpirit.Role.Head;
                    dialog.type = "DIALOG";
                    dialog.info = "刀法遭到封印，无法施展！";
                    dialogs.Add(dialog);
                }
                if (s.buff.Name == "拳掌封印")
                {
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.img = currentSpirit.Role.Head;
                    dialog.type = "DIALOG";
                    dialog.info = "空手功夫遭到封印，无法施展！";
                    dialogs.Add(dialog);
                }
                if (s.buff.Name == "奇门封印")
                {
                    Dialog dialog = new Dialog();
                    dialog.role = currentSpirit.Role.Key;
                    dialog.img = currentSpirit.Role.Head;
                    dialog.type = "DIALOG";
                    dialog.info = "奇门武功遭到封印，无法施展！";
                    dialogs.Add(dialog);
                }
            }
            if (isAllSeal)
            {
                Dialog dialog = new Dialog();
                dialog.img = currentSpirit.Role.Head;
                dialog.role = currentSpirit.Role.Key;
                dialog.type = "DIALOG";
                dialog.info = "所有技能均被封印，无法施展！";
                uiHost.dialogPanel.CallBack = null;
                uiHost.dialogPanel.ShowDialog(dialog);
            }
            else
            {*/
                //uiHost.dialogPanel.CallBack = (i) =>
                //    {
                        uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;
                        Status = (BattleStatus.SelectSkill);
                //    };
                //uiHost.dialogPanel.ShowDialogs(dialogs);
            //}
        }

        internal void OnSelectRest()
        {
            uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;

            Log(currentSpirit.Role.Name + "休息。");
            int internalSuit = Math.Max(currentSpirit.Role.GetEquippedInternalSkill().Yin, Math.Abs(currentSpirit.Role.GetEquippedInternalSkill().Yang));
            float recoverInternalFact = ((float)internalSuit + 100) / 150;
            //recover hp
            int HpRecover = (int)(2000 * 0.02 * (1 + 1.5 * (float)currentSpirit.Role.Attributes["gengu"] / 100f) * recoverInternalFact * Tools.GetRandom(0.1, 2));
            if (HpRecover == 0) HpRecover = 1;
            if (currentSpirit.Role.GetBuff("重伤") != null)
            {
                HpRecover /= 2;
                Log("由于重伤效果,恢复减半");
            }
            if (HpRecover + currentSpirit.Hp > currentSpirit.Role.Attributes["maxhp"])
            {
                HpRecover = currentSpirit.Role.Attributes["maxhp"] - currentSpirit.Hp;
                currentSpirit.Hp = currentSpirit.Role.Attributes["maxhp"];
            }
            else
                currentSpirit.Hp += HpRecover;

            if (HpRecover > 0)
            {
                currentSpirit.AddAttackInfo("+" + HpRecover.ToString(), Colors.Green);
                AudioManager.PlayEffect(ResourceManager.Get("音效.休息"));
                Log(currentSpirit.Role.Name + "回复生命值" + HpRecover);
            }

            //recover mp
            int MpRecover = (int)(2000 * 0.03 * (1 + 2 * (float)currentSpirit.Role.Attributes["gengu"] / 100f) * recoverInternalFact * Tools.GetRandom(0.2, 2));
            if (MpRecover == 0) MpRecover = 1;
            if (currentSpirit.Role.GetBuff("重伤") != null)
            {
                MpRecover /= 2;
            }
            if (currentSpirit.Role.GetBuff("封穴") != null)
            {
                MpRecover = 0;
                Log(currentSpirit.Role.Name + "由于被封穴，无法恢复内力");
            }
            if (MpRecover + currentSpirit.Mp > currentSpirit.Role.Attributes["maxmp"])
            {
                MpRecover = currentSpirit.Role.Attributes["maxmp"] - currentSpirit.Mp;
                currentSpirit.Mp = currentSpirit.Role.Attributes["maxmp"];
            }
            else
                currentSpirit.Mp += MpRecover;

            if (MpRecover > 0)
            {
                //new AttackInfo(RootCanvas, SwitchX(currentSpirit.X), SwitchY(currentSpirit.Y), "+" + MpRecover.ToString(), Colors.Blue);

                currentSpirit.AddAttackInfo("+" + MpRecover.ToString(), Colors.Blue);
                AudioManager.PlayEffect(ResourceManager.Get("音效.休息"));
                Log(currentSpirit.Role.Name + "回复内力" + MpRecover);
            }

            //recordAndSendOLData();
            Status = (BattleStatus.NextPerson);
        }



        internal void OnSelectCancel()
        {
            Status = BattleStatus.SelectMove;
        }

        private void OnNextRole()
        {
            if (currentSpirit != null)
            {
                currentSpirit.IsCurrent = false;
                currentSpirit.Sp = 0;
            }
            recordAndSendOLData();
            //OnNextRoleAction(); //回合制
            this.HideHotKeyText();
            Status = BattleStatus.NextActivePersonAction;
        }

        private DispatcherTimer SpTimer = new DispatcherTimer();
        private void OnWatingForNextPerson()
        {
            if (Configer.Instance.JiqiAnimation)
            {
                SpTimer.Start();
            }
            else
            {
                while(!SpRun())
                {

                }
            }
        }
        private void OnNextActivePerson()
        {
            if (!JudgeResult())
            {
                return; //战斗结束
            }
            if(activePersons.Count == 0)
            {
                Status = BattleStatus.WaitingForActivePerson;
                return;
            }
            currentSpirit = activePersons.Dequeue();
            if (currentSpirit.Hp <= 0) //这个角色已经死了
            {
                Status = BattleStatus.NextActivePersonAction;
                return;
            }
            currentSpirit.IsCurrent = true;
            if (currentSpirit.Team == 1)
                myTurn = true;
            else
                myTurn = false;

            uiHost.roleDetailPanel.Show(currentSpirit.Role);
            battleFieldContainer.cameraMoveToCurrentRole();
        }

        private int _battleTimestamp = 0;

        private void SpTimer_Tick(object sender, EventArgs e)
        {
            SpRun();
        }

        /// <summary>
        /// 时间流逝
        /// </summary>
        /// <returns>是否有人触发行动了</returns>
        private bool SpRun()
        {
            _battleTimestamp++;
            if (_battleTimestamp > CommonSettings.DEFAULT_MAX_GAME_SPTIME)
            {
                Status = BattleStatus.LOSE;
                return true;
            }
            battleFieldContainer.RoundText.Text = string.Format("时间限制 {0}/{1}", _battleTimestamp.ToString(), CommonSettings.DEFAULT_MAX_GAME_SPTIME);

            foreach (var sp in Spirits)
            {
                this.BuffsEffect(sp); //计算buff效果
            }

            if (_battleTimestamp % CommonSettings.BUFF_RUN_CYCLE == 0)
            {
                this.MiusSkillCd(); //技能CD
            }

            activePersons.Clear();
            foreach (var sp in Spirits)
            {
                sp.Sp += sp.Role.SpAddSpeed;
                if (sp.Sp >= 100)
                {
                    activePersons.Enqueue(sp);
                }
            }

            if (activePersons.Count > 0)
            {
                this.SpTimer.Stop();
                isBattleFieldActive = true;
                Status = BattleStatus.NextActivePersonAction;
                return true;
            }
            return false;
        }

        private Queue<Spirit> activePersons = new Queue<Spirit>();
        private void PersonAction(Spirit sp)
        {
            this.SpTimer.Stop();

        }

        private void BuffsEffect(Spirit sp)
        {
            List<RoundBuffResult> buffResult = sp.Role.RunBuffs();
            foreach (var br in buffResult)
            {
                
                if (br.AddHp != 0)
                {
                    Log(sp.Role.Name + "由于效果【" + br.buff.buff.Name + "】，生命值" + br.AddHp);
                    sp.Hp += br.AddHp;
                    if (sp.Hp <= 0) sp.Hp = 1;
                    sp.AddAttackInfo(br.AddHp > 0 ? "+" + br.AddHp : "" + br.AddHp, Colors.Green, AttackInfoType.Hit, TextCastOrder.PreAction);
                }
                if (br.AddMp != 0)
                {
                    Log(sp.Role.Name + "由于效果【" + br.buff.buff.Name + "】，内力" + br.AddMp);
                    sp.Mp += br.AddMp;
                    if (sp.Mp <= 0) sp.Mp = 1;
                    sp.AddAttackInfo(br.AddMp > 0 ? "+" + br.AddMp : "" + br.AddMp, Colors.Blue, AttackInfoType.Hit, TextCastOrder.PreAction);
                }
                if (br.AddBall != 0)
                {
                    Log(sp.Role.Name + "由于效果【" + br.buff.buff.Name + "】，怒气" + br.AddBall);
                    sp.Role.Balls += br.AddBall;
                    if (sp.Role.Balls < 0) sp.Role.Balls = 0;
                    sp.AddAttackInfo(br.AddBall > 0 ? "+蓄力 " + br.AddBall : "" + br.AddBall, Colors.Orange, AttackInfoType.Hit, TextCastOrder.PreAction);
                }
            }
            sp.Refresh();
        }

        private void MiusSkillCd()
        {
            foreach (var sp in Spirits)
            {
                foreach (var skill in sp.Role.Skills)
                {
                    if (skill.CurrentCd > 0)
                        skill.CurrentCd--;
                    foreach (var us in skill.UniqueSkillInstances)
                    {
                        if (us.Cd > 0)
                            us.Cd--;
                    }
                }

                foreach (var skill in sp.Role.InternalSkills)
                {
                    foreach (var us in skill.UniqueSkillInstances)
                    {
                        if (us.Cd > 0)
                            us.Cd--;
                    }
                }

                foreach (var skill in sp.Role.SpecialSkills)
                {
                    if (skill.Cd > 0) skill.Cd--;
                }

                if (sp.ItemCd > 0) sp.ItemCd--;
            }
        }
        
        private void OnNewRound()
        {

            ActionedSpirit.Clear();
            GameTurn++;
            if (GameTurn > MaxGameTurn)
            {
                this.OnLose();
                return;
            }
            battleFieldContainer.HeadPanel.Reset(this.Spirits);

            //所有技能冷却--
            this.MiusSkillCd();
            this.Dispatcher.BeginInvoke(() => { OnNextRoleAction(); });
            //MessageBox.Show("game turn = " + GameTurn.ToString());

            //时间流逝,简单难度战斗不耗时
            if (RuntimeData.Instance.GameMode != "normal")
                RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(0.4); //相当于每个回合占1/5个时辰
        }

        private void OnNextRoleAction()
        {
            if (currentSpirit != null)
            {
                this.battleFieldContainer.HeadPanel.RemoveTop();
            }
            this.battleFieldContainer.HeadPanel.PopTop();

            //判断战斗胜负
            if (!this.JudgeResult()) return;
            
            if (currentSpirit != null)
                currentSpirit.IsCurrent = false;

            //寻找行动角色
            currentSpirit = null;
            int maxShenfa = int.MinValue;
            foreach (var sp in Spirits)
            {
                if (ActionedSpirit.Keys.Contains(sp.battleID)) continue;
                if (sp.Role.AttributesFinal["shenfa"] > maxShenfa || (sp.Role.AttributesFinal["shenfa"] == maxShenfa && sp.battleID < currentSpirit.battleID)) //寻找身法最高的角色优先行动
                {
                    maxShenfa = sp.Role.AttributesFinal["shenfa"];

                    if (currentSpirit != null)
                    {
                        currentSpirit.IsCurrent = false;
                    }

                    currentSpirit = sp;
                    currentSpirit.IsCurrent = true;
                }
            }

            if (currentSpirit == null) //所有角色都已经行动过
            {
                OnNewRound();
            }
            else
            {
                if (currentSpirit.Team == 1)
                    myTurn = true;
                else
                    myTurn = false;

                uiHost.roleDetailPanel.Show(currentSpirit.Role);
                battleFieldContainer.cameraMoveToCurrentRole();
            }
        }

        private void OnNextRoleStartAct()
        {
            this.battleFieldContainer.BattleFieldScrollable = false;
            /*if (currentSpirit != null)
            {
                MessageBox.Show(currentSpirit.Role.Name + "是现在行动角色");
            }
            else
            {
                MessageBox.Show("当前角色已经挂了");
            }*/

            //如果是联机游戏，且当前行动的并非自己，则进入接收数据模式
            if (OLBattleGlobalSetting.Instance.OLGame && (!myTurn))
            {
                Status = BattleStatus.OLNextPerson;
                return;
            }

            //当前行动的角色已经挂了
            if (currentSpirit == null)
            {
                Status = BattleStatus.NextPerson;
                return;
            }

            #region 普照
            //普照效果：40%概率2格以内队友回复效果
            if (currentSpirit.Role.HasTalent("普照")&&Tools.ProbabilityTest(0.4))
            {
                this.ShowSmallDialogBox(currentSpirit, "佛光普照！(天赋*普照发动)", 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【普照】发动");
                foreach(var s in this.Spirits)
                {
                    if( s.Team == currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 2)
                    {
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = new Buff() { Name = "恢复", Level = 2 },
                            Owner = s.Role,
                            LeftRound = 3
                        };
                        BuffInstance buff = s.Role.GetBuff("恢复");
                        if(buff==null)
                        {
                            s.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.Level >= buff.Level) //覆盖刷新buff
                            buff = newBuff;
                        }
                        Log(s.Role.Name + "获得增益效果【" + newBuff.buff.Name +"】，等级" + newBuff.Level);
                        s.AddAttackInfo("进入回复状态！", Colors.Red, AttackInfoType.Hit, TextCastOrder.PreAction);
                        s.Refresh();
                    }
                }
            }
            #endregion

            #region 无相
            if (currentSpirit.Role.HasTalent("无相") && Tools.ProbabilityTest(0.3))
            {
                this.ShowSmallDialogBox(currentSpirit, new string[]{
                    "阿弥陀佛(天赋*无相发动)",
                }, 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【无相】发动");
                int level = currentSpirit.Role.Level;
                int HpAdd = Tools.GetRandomInt(0, (level) * 30 + 50);
                if (HpAdd + currentSpirit.Role.Attributes["hp"] > currentSpirit.Role.Attributes["maxhp"])
                    HpAdd = currentSpirit.Role.Attributes["maxhp"] - currentSpirit.Role.Attributes["hp"];
                currentSpirit.Hp += HpAdd;
                currentSpirit.AddAttackInfo("恢复生命" + HpAdd, Colors.Green);
            }

            #endregion

            #region 琴胆剑心
            //琴胆剑心：40%概率2格以内队友攻击强化30%效果
            if (currentSpirit.Role.HasTalent("琴胆剑心") && Tools.ProbabilityTest(0.4))
            {
                this.ShowSmallDialogBox(currentSpirit, "笑傲江湖，琴胆剑心！(天赋*琴胆剑心发动)", 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【琴胆剑心】发动");
                foreach (var s in this.Spirits)
                {
                    if (s.Team == currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 2)
                    {
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = new Buff() { Name = "攻击强化", Level = 3 },
                            Owner = s.Role,
                            LeftRound = 3
                        };
                        BuffInstance buff = s.Role.GetBuff("攻击强化");
                        if (buff == null)
                        {
                            s.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.Level >= buff.Level) //覆盖刷新buff
                                buff = newBuff;
                        }
                        Log(s.Role.Name + "获得增益效果【" + newBuff.buff.Name + "】，等级" + newBuff.Level);
                        s.AddAttackInfo("攻击强化！", Colors.Red, AttackInfoType.Hit, TextCastOrder.PreAction);
                        s.Refresh();
                    }
                }
            }
            #endregion

            #region 医仙
            //医仙效果：四格以内队友回血
            if (currentSpirit.Role.HasTalent("医仙"))
            {
                this.ShowSmallDialogBox(currentSpirit, new string[]{
                    "看我的医术！(天赋*医仙发动)",
                    "神医！(天赋*医仙发动)",
                }, 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【医仙】发动");
                foreach (var s in this.Spirits)
                {
                    if (s.Team == currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 4)
                    {
                        int dHp = (int)((currentSpirit.Role.Attributes["gengu"]) * Tools.GetRandom(1,2));
                        s.Hp = s.Hp + dHp;
                        s.AddAttackInfo("【医仙】 +" + dHp + "HP", Colors.Green, AttackInfoType.Hit, TextCastOrder.PreAction);
                        Log(s.Role.Name + "恢复生命值" + dHp);
                    }
                }
            }
            #endregion

            #region 救死扶伤
            //救死扶伤效果：四格以内一名随机队友回血，回复量是医仙的两倍
            if (currentSpirit.Role.HasTalent("救死扶伤"))
            {
                List<Spirit> rangeFriend = new List<Spirit>();
                foreach (var s in this.Spirits)
                {
                    if (s.Team == currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 4)
                    {
                        rangeFriend.Add(s);
                    }
                }

                if (rangeFriend.Count > 0)
                {
                    this.ShowSmallDialogBox(currentSpirit, new string[]{
                        "救死扶伤，医者本分也！(天赋*救死扶伤发动)",
                    }, 1.0, TextCastOrder.PreAction);
                    Log(currentSpirit.Role.Name + "天赋【救死扶伤】发动");
                    Spirit sSpirit = rangeFriend[Tools.GetRandomInt(0, rangeFriend.Count) % rangeFriend.Count];
                    int dHp = (int)((currentSpirit.Role.Attributes["gengu"]) * Tools.GetRandom(1, 2));
                    sSpirit.Hp = sSpirit.Hp + 2 * dHp;
                    sSpirit.AddAttackInfo("【救死扶伤】 +" + dHp + "HP", Colors.Green, AttackInfoType.Hit, TextCastOrder.PreAction);
                    Log(sSpirit.Role.Name + "恢复生命值" + dHp);
                }
            }
            #endregion

            #region 哀歌
            //哀歌效果：三格以内对方攻击减弱两成，持续三个回合
            if (currentSpirit.Role.HasTalent("哀歌"))
            {
                this.ShowSmallDialogBox(currentSpirit, "生涯，何其哀！(天赋*哀歌发动)", 0.1, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【哀歌】发动");
                foreach (var s in this.Spirits)
                {
                    if (s.Team != currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 3)
                    {
                        BuffInstance newBuff = new BuffInstance()
                        {
                            buff = new Buff() { Name = "攻击弱化", Level = 2 },
                            Owner = s.Role,
                            LeftRound = 3
                        };
                        BuffInstance buff = s.Role.GetBuff("攻击弱化");
                        if (buff == null)
                        {
                            s.Role.Buffs.Add(newBuff);
                        }
                        else
                        {
                            if (newBuff.Level >= buff.Level) //覆盖刷新buff
                                buff = newBuff;
                        }
                        Log(s.Role.Name + "获得减益效果【" + newBuff.buff.Name + "】，等级" + newBuff.Level);
                        s.Refresh();
                    }
                }
            }
            #endregion

            #region 悲酥清风
            //悲酥清风效果：三格以内对方内力减半，40%概率发动
            if (currentSpirit.Role.HasTalent("悲酥清风") && Tools.ProbabilityTest(0.3))
            {
                this.ShowSmallDialogBox(currentSpirit, "悲酥清风，无色无形！(天赋*悲酥清风发动)", 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【悲酥清风】发动");
                foreach (var s in this.Spirits)
                {
                    if (s.Team != currentSpirit.Team && Math.Abs(s.X - currentSpirit.X) + Math.Abs(s.Y - currentSpirit.Y) <= 3)
                    {
                        s.AddAttackInfo("-内力" + (int)(s.Mp * 0.5f), Colors.Blue, AttackInfoType.Hit, TextCastOrder.PreAction);
                        s.Mp = (int)(s.Mp * 0.5f);
                        Log(s.Role.Name + "内力减少" + (int)(s.Mp * 0.5));
                        s.Refresh();
                    }
                }
            }
            #endregion

            #region 嗜酒如命

            if (currentSpirit.Role.HasTalent("嗜酒如命") && Tools.ProbabilityTest(0.15))
            {
                this.ShowSmallDialogBox(currentSpirit, "好酒，好酒！", 1.0, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【嗜酒如命】发动");
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = new Buff() { Name = "醉酒", Level = 2 },
                    Owner = currentSpirit.Role,
                    LeftRound = 3
                };
                BuffInstance buff = currentSpirit.Role.GetBuff("恢复");
                if (buff == null)
                {
                    currentSpirit.Role.Buffs.Add(newBuff);
                }
                else
                {
                    if (newBuff.Level >= buff.Level) //覆盖刷新buff
                        buff = newBuff;
                }
                currentSpirit.AddAttackInfo("进入醉酒状态！", Colors.Red, AttackInfoType.Hit, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "获得增益效果【" + newBuff.buff.Name + "】，等级" + newBuff.Level);
                currentSpirit.Refresh();
            }

            #endregion

            #region 百穴归一
            if (currentSpirit.Role.HasTalent("百穴归一"))
            {
                Log(currentSpirit.Role.Name + "天赋【百穴归一】发动");
                this.ShowSmallDialogBox(
                    currentSpirit, 
                    new string[]{
                        "啊，我感觉全身发热（天赋【百穴归一】发动）",
                        "百穴归一！（天赋【百穴归一】发动）",
                        "全身穴道畅通（天赋【百穴归一】发动）",
                    },
                    0.7, TextCastOrder.PreAction);
                currentSpirit.Role.Balls++;
                int dmp = Tools.GetRandomInt(50, 200);
                currentSpirit.Mp += dmp;
                Log(currentSpirit.Role.Name + "恢复内力" + dmp);
                currentSpirit.AddAttackInfo("回内+" + dmp, Colors.Blue, AttackInfoType.Hit, TextCastOrder.PreAction);
            }
            #endregion

            #region 右臂有伤
            if (currentSpirit.Role.HasTalent("右臂有伤"))
            {
                this.ShowSmallDialogBox(
                    currentSpirit,
                    new string[]{
                        "小伤而已，不碍事！（天赋【右臂有伤】发动）",
                    },
                    0.15, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【右臂有伤】发动，怒气+1");
                currentSpirit.Role.Balls++;
            }

            #endregion

            #region 魔神降临
            if (currentSpirit.Role.HasTalent("魔神降临") && Tools.ProbabilityTest(0.12))
            {
                this.ShowSmallDialogBox(
                    currentSpirit,
                    new string[]{
                        "吼！！！！（天赋【魔神降临】发动）",
                    },
                    0.7, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【魔神降临】发动，变身为魔神");
                AudioManager.PlayEffect(ResourceManager.Get("音效.咆哮"));
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = new Buff() { Name = "魔神降临", Level = 2 },
                    Owner = currentSpirit.Role,
                    LeftRound = 3
                };
                currentSpirit.Role.AddBuff(newBuff);
                currentSpirit.Refresh();
            }
            #endregion

            #region 百变千幻
            if (currentSpirit.Role.HasTalent("百变千幻") && Tools.ProbabilityTest(0.1))
            {
                this.ShowSmallDialogBox(
                    currentSpirit,
                    new string[]{
                        "变！（天赋【百变千幻】发动）",
                        "看我藏于暗处（天赋【百变千幻】发动）",
                    },
                    0.7, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【百变千幻】发动，启动易容术");
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = new Buff() { Name = "易容", Level = 2 },
                    Owner = currentSpirit.Role,
                    LeftRound = 3
                };
                currentSpirit.Role.AddBuff(newBuff);
                currentSpirit.Refresh();
            }
            #endregion

            #region 倚天屠龙
            if (currentSpirit.Role.HasTalent("倚天屠龙") && Tools.ProbabilityTest(0.2))
            {
                this.ShowSmallDialogBox(
                    currentSpirit,
                    new string[]{
                        "看我倚天屠龙功！（天赋【倚天屠龙】发动）",
                    },
                    0.7, TextCastOrder.PreAction);
                Log(currentSpirit.Role.Name + "天赋【倚天屠龙】发动，获得圣战状态");
                BuffInstance newBuff = new BuffInstance()
                {
                    buff = new Buff() { Name = "圣战", Level = 2 },
                    Owner = currentSpirit.Role,
                    LeftRound = 3
                };
                currentSpirit.Role.AddBuff(newBuff);
                currentSpirit.Refresh();
            }
            #endregion

            #region 光明圣火阵
            if (currentSpirit.Role.HasTalent("光明圣火阵"))
            {
                double p = 0;
                foreach (var s in Spirits)
                {
                    if (s.Team == currentSpirit.Team && s != currentSpirit)
                    {
                        p += 0.2;
                    }
                    if (Tools.ProbabilityTest(p))
                    {
                        List<BuffInstance> toberemove = new List<BuffInstance>();
                        foreach (var buff in currentSpirit.Role.Buffs)
                        {
                            if (buff.IsDebuff)
                            {
                                toberemove.Add(buff);
                            }
                        }
                        foreach (var buff in toberemove) { currentSpirit.Role.Buffs.Remove(buff); }
                        if (toberemove.Count > 0)
                        {
                            Log(currentSpirit.Role.Name + "阵法【光明圣火阵】发动，清除负面效果");
                            currentSpirit.AddAttackInfo("光明圣火阵,清除负面效果", Colors.Red);
                            currentSpirit.Refresh();
                        }
                    }
                }
            }
            #endregion

            //眩晕效果
            BuffInstance faintBuff = currentSpirit.Role.GetBuff("晕眩");
            bool isFaint = false;
            if (faintBuff != null) isFaint = true;

            #region 醉酒buff
            foreach (BuffInstance buff in currentSpirit.Role.Buffs)
            {
                if (buff.buff.Name == "醉酒")
                {
                    if (Tools.GetRandom(0, 1.0) <= 0.2)
                    {
                        Log(currentSpirit.Role.Name + "由于醉酒，无法行动。");
                        isFaint = true;
                        this.ShowSmallDialogBox(currentSpirit, "但愿...长...醉不复...醒...", 1.0, TextCastOrder.PreAction);
                    }
                    else
                    {
                        Log(currentSpirit.Role.Name + "醉酒发动，怒气全满。");
                        currentSpirit.Role.Balls = 6;
                        this.ShowSmallDialogBox(currentSpirit,
                             "长风破浪会有时，直挂云帆济沧海！", 1.0, TextCastOrder.PreAction);
                    }
                    break;
                }
            }
            #endregion

            uiHost.roleDetailPanel.Show(currentSpirit.Role);
            currentSpirit.Refresh();

            //联机模式下，记录此时的全人物状态
            if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
            {
                foreach (Spirit sp in Spirits)
                {
                    OLBattleGlobalSetting.Instance.battleData.preRoles[sp.battleID] = sp.Role;

                    SpiritInfo spInfo = new SpiritInfo();
                    spInfo.X = sp.X;
                    spInfo.Y = sp.Y;
                    spInfo.itemCD = sp.ItemCd;
                    spInfo.faceright = sp.FaceRight==false?0:1;
                    OLBattleGlobalSetting.Instance.battleData.preSpiritInfos[sp.battleID] = spInfo;
                }
            }

            if (!isFaint)
            {
                #region 令狐冲的怪病
                if (currentSpirit.Role.HasTalent("令狐冲的怪病") && Tools.GetRandom(0, 1) <= 0.2)
                {
                    this.ShowSmallDialogBox(currentSpirit,
                             "咳咳，该死，这病又发作了。", 1.0, TextCastOrder.PreAction);
                    Log(currentSpirit.Role.Name + "由于【令狐冲的怪病】无法行动");
                    bool pingAlive = false;
                    foreach (Spirit s in Spirits)
                    {
                        if (s.Role.Key == "平一指" && s.Team == currentSpirit.Team && s.Hp > 1)
                        {
                            this.ShowSmallDialogBox(s,
                                "唉，真是造孽，让我给你缓缓吧。", 1.0, TextCastOrder.PreAction);
                            Log("由于平一指存在，治疗了令狐冲的怪病");
                            pingAlive = true;
                            break;
                        }
                    }

                    if (!pingAlive)
                    {
                        ActionedSpirit[currentSpirit.battleID] = true;
                        Status = BattleStatus.NextPerson;
                        return;
                    }
                }
                #endregion

                if (currentSpirit.Team == 1 && !Configer.Instance.AutoBattle && !ForceAI) //玩家控制
                {
                    aiTerm = false;
                    Status = BattleStatus.SelectMove;
                }
                else //AI
                {
                    aiTerm = true;
                    Status = BattleStatus.AI;
                }
            }
            else
            {
                Status = BattleStatus.NextPerson;                
            }
            ActionedSpirit[currentSpirit.battleID] = true;
        }

        #region 联机战斗

        private void SendLoadingFinish()
        {
            string cmd = "LOADING_FINISH";
            BattleNetManager.Instance.Chat(OLBattleGlobalSetting.Instance.channel, cmd);
        }

        private void OnWaitingPlayer()
        {
            SendLoadingFinish();

            if (!OLBattleGlobalSetting.Instance.enemyLoadFinish)
                uiHost.onlineGameLoadingPanel.startShow("等待对手加载战斗");

            waitingPlayerTimer = new DispatcherTimer();
            waitingPlayerTimer.Interval = TimeSpan.FromMilliseconds(CommonSettings.AI_WAITTIME);
            waitingPlayerTimer.Tick += new EventHandler(Timer_WaitingPlayerTick);
            waitingPlayerTimer.Start();
        }

        private DispatcherTimer waitingPlayerTimer;

        private void Timer_WaitingPlayerTick(object sender, EventArgs e)
        {
            if (OLBattleGlobalSetting.Instance.enemyLoadFinish)
            {
                waitingPlayerTimer.Stop();
                uiHost.onlineGameLoadingPanel.stopShow();
                //MessageBox.Show("你的对手已经就绪，开始战斗吧！");
                Status = BattleStatus.Starting;
            }
        }

        private void OnOLNextPerson()
        {
            //MessageBox.Show("现在是对方行动！");
        }

        private void recordAndSendOLData()
        {
            //如果是我方回合，且为联机战斗，则传送数据
            if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
            {
                //记录人物移动信息
                OLBattleGlobalSetting.Instance.battleData.x = currentSpirit.X;
                OLBattleGlobalSetting.Instance.battleData.y = currentSpirit.Y;
                OLBattleGlobalSetting.Instance.battleData.faceright = currentSpirit.FaceRight;
                
                //记录此时的全人物状态
                foreach (Spirit sp in Spirits)
                {
                    OLBattleGlobalSetting.Instance.battleData.roles[sp.battleID] = sp.Role;

                    SpiritInfo spInfo = new SpiritInfo();
                    spInfo.X = sp.X;
                    spInfo.Y = sp.Y;
                    spInfo.itemCD = sp.ItemCd;
                    spInfo.faceright = sp.FaceRight == false ? 0 : 1;
                    OLBattleGlobalSetting.Instance.battleData.spiritInfos[sp.battleID] = spInfo;
                }

                sendOLBattleData();
            }
        }

        private void sendOLBattleData()
        {
            string data = OLBattleGlobalSetting.Instance.battleData.toXMLData().ToString();
            string cmd = "OL_BATTLE_DATA";
            //MessageBox.Show(data.Length.ToString());
            BattleNetManager.Instance.Chat(OLBattleGlobalSetting.Instance.channel, cmd + "$" + data);
            OLBattleGlobalSetting.Instance.battleData.clear();
        }

        public void OLDataDisplay(string battleDataString)
        {
            OLBattleGlobalSetting.Instance.battleData.parse(battleDataString);

            //显示服务器回传的结果
            OLBattleGlobalSetting.Instance.battleData.displayPreBattleEffect(this.uiHost);
            OLBattleGlobalSetting.Instance.battleData.displayMove(this.uiHost);
        }

        public void OnOLDisplayMoveFinish()
        {
            CommonSettings.VoidCallBack callback = () =>
            {
                OLBattleGlobalSetting.Instance.battleData.displayBattleEffect(this.uiHost);

                //清除battleData
                OLBattleGlobalSetting.Instance.battleData.clear();

                //判定胜负，下一个人物
                this.IsEnabled = true;
                ActionedSpirit[currentSpirit.battleID] = true;
                NextPerson();
            };

            OLBattleGlobalSetting.Instance.battleData.displaySkill(this.uiHost, callback);

        }

        public void ShowOLSkillAnimation(CommonSettings.VoidCallBack skillCallback)
        {
            AttackInfoNew attackInfo = new AttackInfoNew();
            SkillInfo info = OLBattleGlobalSetting.Instance.battleData.currentSkill;
            attackInfo.Show(info.color, SwitchX(currentSpirit.X), SwitchY(currentSpirit.Y), info.name, RootCanvas);
            currentSpirit.Status = Spirit.SpiritStatus.Attacking;
            //如果有特效，先来特效
            CommonSettings.VoidCallBack callback = () =>
            {
                if (OLSkillWaitTimer == null)
                {
                    OLSkillWaitTimer = new DispatcherTimer();
                    OLSkillWaitTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    OLSkillWaitTimer.Tick += (s, e) =>
                    {
                        OLSkillWaitTimer.Stop();
                        //attackInfo.Show(skillColor, SwitchX(currentSpirit.X), SwitchY(currentSpirit.Y), skillinfo, RootCanvas);
                        CastOLBattleSkill(OLBattleGlobalSetting.Instance.battleData.currentSkill.targetx, OLBattleGlobalSetting.Instance.battleData.currentSkill.targety);
                        skillCallback();
                    };
                }
                OLSkillWaitTimer.Start();
            };

            ShowOLEffectAnimation(callback);
        }

        private DispatcherTimer OLSkillWaitTimer = null;

        private void ShowOLEffectAnimation(CommonSettings.VoidCallBack callback)
        {
            int currentZIndex = Canvas.GetZIndex(currentSpirit);

            SkillInfo info = OLBattleGlobalSetting.Instance.battleData.currentSkill;

            if (info.fullScreenAnimation == 1 && info.isAoyi == 0)
            {
                string skillName = info.fullScreenAnimationName;

                //记录联机信息
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimation = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimationName = skillName;
                }

                effectCover.Visibility = Visibility.Visible;

                Canvas.SetZIndex(effectCover, CommonSettings.Z_EFFECTCOVER);
                Canvas.SetZIndex(currentSpirit, CommonSettings.Z_EFFECTROLE);

                CommonSettings.VoidCallBack newcallback = () =>
                {
                    effectCover.Visibility = Visibility.Collapsed;
                    Canvas.SetZIndex(currentSpirit, currentZIndex);
                    callback();
                };

                AnimationGroup effect = SkillManager.GetSkillAnimation(skillName);

                if (currentSpirit.Role.Female)
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.女", "音效.女2", "音效.女3", "音效.女4" });
                }
                else
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.男", "音效.男2", "音效.男3", "音效.男4", "音效.男5", "音效.男-哼" });
                }
                new SelfManagedAnimation(RootCanvas, 1, effect, Configer.Instance.SkillAnimtionSwitchTime / 2,
                    SwitchX(currentSpirit.X) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_WIDTH / 2,
                    SwitchY(currentSpirit.Y) + CommonSettings.SPIRIT_BLOCK_SIZE / 2 - CommonSettings.SCREEN_HEIGHT / 2,
                    800,
                    600,
                    newcallback, CommonSettings.Z_EFFECT, 0.6, true, false);
            }
            else if (info.isAoyi == 1)
            {
                //AoyiInstance aoyiInstance = new AoyiInstance();
                //aoyiInstance.uihost = this.uiHost;
                //aoyiInstance.Owner = currentSpirit.Role;
                //aoyiInstance.skill = currentSkill.Instance;
                //aoyiInstance.Aoyi = SkillManager.GetAoyi(info.aoyiName);
                //currentSkill.AoyiInstance = aoyiInstance;

                effectCover.Visibility = Visibility.Visible;

                Canvas.SetZIndex(effectCover, CommonSettings.Z_EFFECTCOVER);
                Canvas.SetZIndex(currentSpirit, CommonSettings.Z_EFFECTROLE);

                SelfManagedAnimation anime = null;

                CommonSettings.VoidCallBack newcallback = () =>
                {
                    uiHost.battleFieldContainer.aoyiHead.Source = currentSpirit.Role.Head;
                    uiHost.battleFieldContainer.aoyiHeadCopy.Source = uiHost.battleFieldContainer.aoyiHead.Source;
                    uiHost.battleFieldContainer.aoyiText.Text = info.aoyiName;
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiHead, CommonSettings.Z_EFFECTAOYI);
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiHeadCopy, CommonSettings.Z_EFFECTAOYI);
                    Canvas.SetZIndex(uiHost.battleFieldContainer.aoyiText, CommonSettings.Z_EFFECTAOYI);

                    uiHost.battleFieldContainer.aoyiHead.Visibility = Visibility.Visible;
                    uiHost.battleFieldContainer.aoyiHeadCopy.Visibility = Visibility.Visible;
                    uiHost.battleFieldContainer.aoyiText.Visibility = Visibility.Visible;

                    uiHost.battleFieldContainer.AoyiBoard.Begin();
                    uiHost.battleFieldContainer.AoyiBoard.Completed += (object sender, EventArgs e) =>
                    {
                        effectCover.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiHead.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiHeadCopy.Visibility = Visibility.Collapsed;
                        uiHost.battleFieldContainer.aoyiText.Visibility = Visibility.Collapsed;
                        anime.remove();

                        Canvas.SetZIndex(currentSpirit, currentZIndex);
                        callback();
                    };
                };
                //string[] aoyiskills = new string[] { "特效.奥义1", "特效.奥义2", "特效.奥义3", "特效.奥义4", "特效.奥义5", "特效.奥义6", "特效.奥义7" };
                //string aoyiskill = aoyiskills[Tools.GetRandomInt(0, aoyiskills.Length - 1)];
                //SkillAnimation effect = SkillManager.GetSkillAnimation(aoyiskill);
                AnimationGroup effect = SkillManager.GetAoyi(info.aoyiName).Animations;
                if (effect == null)
                {
                    effect = SkillManager.GetSkillAnimation("aoyi1");
                }

                //记录联机信息
                if (OLBattleGlobalSetting.Instance.OLGame && myTurn)
                {
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimation = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.fullScreenAnimationName = effect.Name;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.isAoyi = 1;
                    OLBattleGlobalSetting.Instance.battleData.currentSkill.aoyiName = currentSkill.AoyiInstance.Aoyi.Name;
                }

                if (currentSpirit.Role.Female)
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.女", "音效.女2", "音效.女3", "音效.女4" }, () =>
                    {
                        AudioManager.PlayRandomEffect(new String[] { "音效.内功攻击4", "音效.打雷", "音效.奥义1", "音效.奥义2", "音效.奥义3", "音效.奥义4", "音效.奥义5", "音效.奥义6" });
                    });
                }
                else
                {
                    AudioManager.PlayRandomEffect(new string[] { "音效.男", "音效.男2", "音效.男3", "音效.男4", "音效.男5", "音效.男-哼" }, () =>
                    {
                        AudioManager.PlayRandomEffect(new String[] { "音效.内功攻击4", "音效.打雷", "音效.奥义1", "音效.奥义2", "音效.奥义3", "音效.奥义4", "音效.奥义5", "音效.奥义6" });
                    });
                }

                anime = new SelfManagedAnimation(RootCanvas, 1, effect, Configer.Instance.SkillAnimtionSwitchTime/2,
                    uiHost.battleFieldContainer.fieldMarginLeft/*SwitchX(currentSpirit.X) - 250*/,
                    uiHost.battleFieldContainer.fieldMarginTop/*SwitchY(currentSpirit.Y) - 380*/,
                    800,
                    600,
                    newcallback, CommonSettings.Z_EFFECT,
                    1.0,
                    false, false);
            }
            else
            {
                callback();
            }
        }

        internal void CastOLBattleSkill(int x, int y)
        {
            //currentSkill.AddCastCd();
            //int hitNumber = 0;

            if (x > currentSpirit.X)
            {
                currentSpirit.FaceRight = true;
            }
            if (x < currentSpirit.X)
            {
                currentSpirit.FaceRight = false;
            }

            SkillInfo info = OLBattleGlobalSetting.Instance.battleData.currentSkill;

            //currentSpirit.Status = Spirit.SpiritStatus.Attacking;

            //播放音效
            AudioManager.PlayEffect(info.audio);

            List<LocationBlock> blocks = SkillBox.GetSkillCoverBlocks(x, y, currentSpirit.X, currentSpirit.Y, info.skillCastSize, (SkillCoverType)info.skillCoverType);
            foreach (var b in blocks)
            {
                //Spirit attackTarget = Attack(currentSpirit, currentSkill, b.X, b.Y);
                new SelfManagedAnimation(RootCanvas, 1, SkillManager.GetSkillAnimation(info.skillAnimationTemplate), 100,
                SwitchX(b.X) + CommonSettings.SPIRIT_BLOCK_SIZE / 2,
                SwitchY(b.Y) + CommonSettings.SPIRIT_BLOCK_SIZE);
            }
        }

        #endregion


        private void OnSelectRoleAction()
        {
            Spirit sp = currentSpirit;
            Canvas.SetLeft(uiHost.roleActionPanel, SwitchX(sp.X) + 30 - battleFieldContainer.fieldMarginLeft);
            Canvas.SetTop(uiHost.roleActionPanel, SwitchY(sp.Y) + 20 - battleFieldContainer.fieldMarginTop);

            if (SwitchX(sp.X) >= this.backgroundSize.Width - 100)
            {
                Canvas.SetLeft(uiHost.roleActionPanel, SwitchX(sp.X) - battleFieldContainer.fieldMarginLeft - 30);
            }
            if (SwitchX(sp.X) <= 100)
            {
                Canvas.SetLeft(uiHost.roleActionPanel, SwitchX(sp.X) + 60);
            }
            if (SwitchY(sp.Y) >= this.backgroundSize.Height - 100)
            {
                Canvas.SetTop(uiHost.roleActionPanel, SwitchY(sp.Y) - battleFieldContainer.fieldMarginTop - 20);
            }
            if (SwitchY(sp.Y) <= 100)
            {
                Canvas.SetTop(uiHost.roleActionPanel, SwitchY(sp.Y) + 60);
            }


            uiHost.roleActionPanel.Show();
            //mainGameUI.roleActionPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnSelectSkill()
        {
            Spirit sp = currentSpirit;

            uiHost.skillPanel.Show(sp.Role);

            int x = SwitchX(sp.X) + 60 - battleFieldContainer.fieldMarginLeft;
            int y = SwitchY(sp.Y) - 10 - battleFieldContainer.fieldMarginTop;

            if (y + uiHost.skillPanel.Height > CommonSettings.SCREEN_HEIGHT)
            {
                y = (int)(CommonSettings.SCREEN_HEIGHT - uiHost.skillPanel.Height);
            }
            if (x + uiHost.skillPanel.Width > CommonSettings.SCREEN_WIDTH)
            {
                x = (int)(CommonSettings.SCREEN_WIDTH - uiHost.skillPanel.Width);
            }

            Canvas.SetLeft(uiHost.skillPanel, x);
            Canvas.SetTop(uiHost.skillPanel, y);
            uiHost.skillPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnRoleAttack()
        {
            Spirit sp = currentSpirit;
            SkillBox skill = currentSkill;
            List<LocationBlock> skillCover = skill.GetSkillCastBlocks(sp.X, sp.Y);

            foreach (var b in skillCover)
            {
                if (b.X >= 0 && b.X < actualXBlockNo && b.Y >= 0 && b.Y < actualYBlcokNo)
                {
                    BattleBlock block = blockMap[b.X, b.Y];
                    block.Status = BattleBlockStatus.SelectAttack;
                    block.RelatedBlocks = skill.GetSkillCoverBlocks(b.X, b.Y, sp.X, sp.Y);
                }
            }
        }

        //玩家选择了物品
        //菜单中选择“物品”项
        internal void OnSelectItems()
        {
            uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;
            Status = (BattleStatus.SelectItem);
        }

        internal void OnUseItemTarget(Item item, Spirit target)
        {
            Log(currentSpirit.Role.Name + "对" + target.Role.Name + "使用了物品【" + item.Name + "】");
            if (item == null) //on cancel
            {
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Visible;
                Status = BattleStatus.SelectItem;
                return;
            }

            if (target.ItemCd > 0 && (!currentSpirit.Role.HasTalent("妙手仁心")) ) //没CD
            {
                MessageBox.Show("少侠，你吃药太频繁了会气血失调的！【还需要" + target.ItemCd + "回合】");
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Visible;
                Status = BattleStatus.SelectItem;
                return;
            }

            ItemResult result = ItemManager.TryUseItem(target.Role, null, item);
            bool recover = false;
            if (target.Role.GetBuff("重伤") != null)
            {
                result.Hp /= 2;
                result.Mp /= 2;
                if (result.Hp > 0 || result.Mp > 0)
                {
                    Log("由于重伤效果,恢复效果减半！");
                }
            }
            if (result.Hp > 0)
            {
                target.Hp += result.Hp;
                Log(target.Role.Name + "生命值恢复" + result.Hp);
                if (result.Hp > 0)
                {
                    target.AddAttackInfo("+" + result.Hp, Colors.Green);
                }
                recover = true;
            }
            if (result.Mp > 0)
            {
                target.Mp += result.Mp;
                Log(target.Role.Name + "内力恢复" + result.Mp);
                if (result.Mp > 0)
                {
                    target.AddAttackInfo("+" + result.Mp, Colors.Blue);
                }
                recover = true;
            }
            if (result.Balls > 0)
            {
                target.Role.Balls += result.Balls;
                if (target.Role.Balls > 6)
                    target.Role.Balls = 6;
                target.AddAttackInfo("怒气+" + result.Balls, Colors.Orange);
                Log(target.Role.Name + "怒气增加" + result.Balls);
                recover = true;
            }
            if (result.Buffs.Count > 0)
            {
                //上buff
                foreach (var b in result.Buffs)
                {
                    BuffInstance buff = target.Role.GetBuff(b.Name);
                    BuffInstance newBuff = new BuffInstance()
                    {
                        buff = b,
                        Owner = target.Role,
                        Level = b.Level,
                        LeftRound = b.Round
                    };
                    if (buff == null)
                    {
                        target.Role.Buffs.Add(newBuff);
                    }
                    else
                    {
                        if (newBuff.Level >= buff.Level) //覆盖刷新buff
                        {
                            buff.buff = newBuff.buff;
                            buff.Owner = newBuff.Owner;
                            buff.Level = newBuff.Level;
                            buff.LeftRound = newBuff.LeftRound;
                        }
                    }
                    Log(target.Role.Name + "获得效果【" + b.Name + "】，等级" + b.Level);
                }
                target.Refresh();
            }
            BuffInstance poisonBuff = target.Role.GetBuff("中毒");
            if (poisonBuff != null && (result.DescPoisonLevel != 0 || result.DescPoisonDuration != 0))
            {
                poisonBuff.Level -= result.DescPoisonLevel;
                poisonBuff.LeftRound -= result.DescPoisonDuration;
                AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                target.Refresh();
            }

            if (recover)
            {
                AudioManager.PlayEffect(ResourceManager.Get("音效.恢复类物品"));
            }
            RuntimeData.Instance.Items.Remove(item);

            if (RuntimeData.Instance.GameMode != "normal")
            {
                target.ItemCd += item.Cooldown; //增加药物CD
            }

            uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;

            //recordAndSendOLData();
            NextPerson();
        }

        internal void OnUseItem(Item item)
        {
            if (item == null) //on cancel
            {
                currentItem = null;
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                Status = BattleStatus.SelectAction;
                return;
            }

            if (currentSpirit.Role.HasTalent("隔空取物"))
            {
                currentItem = item;
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                Status = BattleStatus.SelectItemTarget;
            }
            else
            {
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                currentItem = item;
                OnUseItemTarget(item, currentSpirit);
            }
        }

        public void OnSelectItem()
        {
            uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items,true);
        }

        public void OnSelectItemTarget()
        {
            Spirit sp = currentSpirit;
            int size = 2;

            List<LocationBlock> rst = new List<LocationBlock>();
            for (int i = 0; i < actualXBlockNo; ++i)
            {
                for (int j = 0; j < actualYBlcokNo; ++j)
                {
                    if (mapCoverLayer[i, j]) continue;
                    if (Math.Abs(sp.X - i) + Math.Abs(sp.Y - j) <= size)
                    {
                        rst.Add(new LocationBlock() { X = i, Y = j });
                    }
                }
            }

            foreach (var r in rst)
            {
                blockMap[r.X, r.Y].Status = BattleBlockStatus.SelectMove;
            }
        }

        #endregion

        #region 胜负判定
        /// <summary>
        /// 判断战斗胜负
        /// </summary>
        /// <returns>是否继续战场逻辑</returns>
        private bool JudgeResult()
        {
            if (isEnd) return false;
            if (!this.IsBattle) return true;

            //目前的算法是纯以团队人数，之后应该加入各种情况
            int team1 = 0;
            int otherteam = 0;
            foreach (var s in this.Spirits)
            {
                if (s.Team == 1) team1++;
                else otherteam++;
            }
            if (team1 == 0)
            {
                isEnd = true;
                //OLBattleGlobalSetting.Instance.close();
                //isBattleFieldActive = false;
                Status = BattleStatus.LOSE;
                return false;
            }
            if (otherteam == 0)
            {
                isEnd = true;
                //OLBattleGlobalSetting.Instance.close();
                //isBattleFieldActive = false;
                Status = BattleStatus.WIN;
                return false;
            }
            return true;
        }

        //判断联机战斗胜负（五局三胜 or 三局两胜）并处理
        private void JudgeOLBattleResult()
        {
            int totalCount = TowerManager.getTower(uiHost.towerSelectScene.currentTower).Count;

            //胜利了
            if (OLBattleGlobalSetting.Instance.winCount * 2 > totalCount)
            {
                Dialog dialog = new Dialog();
                dialog.role = "南贤";
                dialog.info = "嗯...总共" + totalCount.ToString() + "场战斗。目前你赢了" + OLBattleGlobalSetting.Instance.winCount + "场，输了" + OLBattleGlobalSetting.Instance.loseCount + "场。恭喜你赢了！";
                dialog.type = "DIALOG";
                uiHost.dialogPanel.ShowDialog(dialog, (i) =>
                {
                    OLBattleGlobalSetting.Instance.close();
                    uiHost.onlineGamePanel.Win();
                    uiHost.onlineGamePanel.Status = OnlineStatus.InHost;
                });
            }
            //挂了
            else if (OLBattleGlobalSetting.Instance.loseCount * 2 > totalCount)
            {
                Dialog dialog = new Dialog();
                dialog.role = "南贤";
                dialog.info = "嗯...总共" + totalCount.ToString() + "场战斗。目前你赢了" + OLBattleGlobalSetting.Instance.winCount + "场，输了" + OLBattleGlobalSetting.Instance.loseCount + "场。哦！你已经挂了。下次再努力吧。";
                dialog.type = "DIALOG";
                uiHost.dialogPanel.ShowDialog(dialog, (i) =>
                {
                    OLBattleGlobalSetting.Instance.close();
                    uiHost.onlineGamePanel.Lose();
                    uiHost.onlineGamePanel.Status = OnlineStatus.InHost;
                 });
            }
            else//打下一局
            {
                Dialog dialog = new Dialog();
                dialog.role = "南贤";
                dialog.info = "嗯...总共" + totalCount.ToString() + "场战斗。目前你赢了" + OLBattleGlobalSetting.Instance.winCount + "场，输了" + OLBattleGlobalSetting.Instance.loseCount + "场。让我们继续吧！";
                dialog.type = "DIALOG";
                uiHost.dialogPanel.ShowDialog(dialog, (i) =>
                    {
                        uiHost.Dispatcher.BeginInvoke(() =>
                            {
                                OLBattleGlobalSetting.Instance.clear();
                                RuntimeData.Instance.gameEngine.OLBattle(false, OLBattleGlobalSetting.Instance.myTeamIndex, OLBattleGlobalSetting.Instance.channel);
                            });
                    });
            }
        }

        private void OnWin()
        {
            isBattleFieldActive = false;
            //MessageBox.Show("战斗胜利!");
            if (currentScenario == "test_ARENA")
            {
                uiHost.bonus.confirmBack = OnArenaBonus;
                uiHost.bonus.ArenaShow();

            }
            else if (currentScenario == "test_HUASHAN")
            {
                string battleKey = TowerManager.getTower(uiHost.towerSelectScene.currentTower)[uiHost.towerSelectScene.currentIndex].Key;
                //string bonusItem = TowerManager.GetRandomBonus(battleKey);
                //uiHost.towerSelectScene.bonuses.Add(bonusItem);

                //闯天关顺利结束
                if (uiHost.towerSelectScene.currentIndex == TowerManager.getTower(uiHost.towerSelectScene.currentTower).Count - 1)
                {
                    Dialog dialog = new Dialog();
                    dialog.role = "南贤";
                    dialog.info = "恭喜你挑战华山论剑成功！";
                    dialog.type = "DIALOG";

                    uiHost.dialogPanel.ShowDialog(dialog, (i) =>
                    {
                        RuntimeData.Instance.gameEngine.CallScence(
                            this, 
                            new NextGameState() { Type = "story", Value = "original_华山论剑分枝判断" }
                        );
                   });
                }
                else
                {
                    Dialog dialog = new Dialog();
                    dialog.role = "北丑";
                    dialog.info = "恭喜你取得了本场战斗的胜利！让我们马上开始下一场吧！";
                    dialog.type = "DIALOG";
                    uiHost.dialogPanel.ShowDialog(dialog, (i) =>
                    {
                        if (uiHost.towerSelectScene.cannotSelected.Count < RuntimeData.Instance.Team.Count)
                        {
                            NextGameState next = new NextGameState();
                            next.Type = "nextHuashan";
                            RuntimeData.Instance.gameEngine.CallScence(this, next);
                        }
                        else
                        {
                            Dialog diagQuit = new Dialog();
                            diagQuit.info = "下一战你已经无人可以上场了，本次华山论剑，你输了！";
                            diagQuit.type = "DIALOG";
                            diagQuit.role = "北丑";
                            uiHost.dialogPanel.ShowDialog(diagQuit, (j) =>
                            {
                                uiHost.dialogPanel.CallBack = null;
                                RuntimeData.Instance.gameEngine.CallScence(
                                    this,
                                    new NextGameState() { Type = "gameOver", Value = "gameOver" });
                            });
                        }
                    });
                }
            }
            else if (currentScenario == "test_TOWER")
            {
                string battleKey = TowerManager.getTower(uiHost.towerSelectScene.currentTower)[uiHost.towerSelectScene.currentIndex].Key;
                string bonusItem = TowerManager.GetRandomBonus(battleKey);
                uiHost.towerSelectScene.bonuses.Add(bonusItem);

                //闯天关顺利结束
                if (uiHost.towerSelectScene.currentIndex == TowerManager.getTower(uiHost.towerSelectScene.currentTower).Count - 1)
                {
                    Dialog dialog = new Dialog();
                    dialog.role = "北丑";
                    dialog.info = "恭喜你挑战天关【" + uiHost.towerSelectScene.currentTower + "】成功！";
                    dialog.type = "DIALOG";

                    uiHost.dialogPanel.ShowDialog(dialog,(i) =>
                    {
                        uiHost.towerSelectScene.bonusBack = () =>
                        {
                            RuntimeData.Instance.gameEngine.CallScence(this, null);
                        };
                        uiHost.towerSelectScene.showBonus();
                    });
                }
                else
                {
                    List<Dialog> dialogs = new List<Dialog>();
                    dialogs.Clear();
                    Dialog dialog = new Dialog();
                    dialog.role = "北丑";
                    dialog.info = "恭喜你取得了胜利！你本场战斗的奖励为【" + bonusItem + "】！";
                    dialog.type = "DIALOG";
                    dialogs.Add(dialog);

                    Dialog dialog2 = new Dialog();
                    dialog2.role = "北丑";
                    dialog2.info = "截止目前，你的奖励有：";
                    for (int i = 0; i < uiHost.towerSelectScene.bonuses.Count; i++)
                    {
                        if (i != uiHost.towerSelectScene.bonuses.Count - 1)
                            dialog2.info += "【" + uiHost.towerSelectScene.bonuses[i] + "】、";
                        else
                            dialog2.info += "【" + uiHost.towerSelectScene.bonuses[i] + "】！";
                    }
                    dialog2.type = "DIALOG";
                    dialogs.Add(dialog2);

                    uiHost.dialogPanel.ShowDialogs(dialogs, (tmp) =>
                    {
                        string title = "要继续挑战下一关吗？";
                        List<string> opts = new List<string>();
                        opts.Add("挑战！（注意：下一场战斗失败则失去所有奖励！）");
                        opts.Add("算了，拿着现在的奖励走人吧。");
                        uiHost.multiSelectBox.Show(title, opts, (selected) =>
                        {
                            if (selected == 0 && uiHost.towerSelectScene.cannotSelected.Count < RuntimeData.Instance.Team.Count)
                            {
                                NextGameState next = new NextGameState();
                                next.Type = "nextTower";
                                RuntimeData.Instance.gameEngine.CallScence(this, next);
                            }
                            else
                            {
                                uiHost.towerSelectScene.bonusBack = () =>
                                {
                                    RuntimeData.Instance.gameEngine.CallScence(this, null);
                                };
                                Dialog diagQuit = new Dialog();
                                if (uiHost.towerSelectScene.cannotSelected.Count >= RuntimeData.Instance.Team.Count)
                                    diagQuit.info = "你已经无人可以应战了，我只能强制结束本次天关挑战了！";
                                else
                                    diagQuit.info = "知难而退，也是一种勇气！";
                                diagQuit.type = "DIALOG";
                                diagQuit.role = "北丑";
                                uiHost.dialogPanel.ShowDialog(diagQuit,(j) =>
                                {
                                    uiHost.dialogPanel.CallBack = null;
                                    uiHost.towerSelectScene.showBonus();
                                });
                            }
                        });
                    });
                }
            }
            else if (OLBattleGlobalSetting.Instance.OLGame)
            {
                OLBattleGlobalSetting.Instance.winCount++;

                //联机战斗后续处理
                JudgeOLBattleResult();
                return;
            }
            else
            {
                uiHost.bonus.confirmBack = OnWinEnd;
                uiHost.bonus.Show(true, currentBattle.bonus);
            }
        }

        public void OnArenaBonus()
        {
            uiHost.arenaSelectScene.bonusBack = OnWinEnd;
            uiHost.arenaSelectScene.bonus();
        }

        public void OnWinEnd()
        {
            RuntimeData.Instance.KeyValues[currentScenario] = "win";
            //RuntimeData.Instance.gameEngine.CallScence(
            //    this, ScenarioManager.getNextScenario(currentScenario, "win"));
            if (battleCallback != null)
            {
                battleCallback(1);
                battleCallback = null;
            }
            else
            {
                RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = "map", Value = RuntimeData.Instance.CurrentBigMap });
            }
        }

        private void OnLose()
        {
            isBattleFieldActive = false;
            RuntimeData.Instance.KeyValues[currentScenario] = "lose";
            if (OLBattleGlobalSetting.Instance.OLGame)
            {
                OLBattleGlobalSetting.Instance.loseCount++;
                JudgeOLBattleResult();
                return;
            }

            uiHost.bonus.confirmBack = () =>
            {
                if (currentScenario == "test_TOWER")
                {
                    RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = "story", Value = "original_天关.失败" });
                    return;
                }
                else if (currentScenario == "test_HUASHAN")
                {
                    RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = "gameOver", Value = "gameOver" });
                    return;
                }
                else if (currentScenario == "test_ARENA")
                {
                    RuntimeData.Instance.gameEngine.CallScence(null, new NextGameState() { Type = "map", Value = RuntimeData.Instance.CurrentBigMap });
                    return;
                }
                else if (battleCallback != null)
                {
                    battleCallback(0);
                    battleCallback = null;
                }
                else
                {
                    RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = "map", Value = RuntimeData.Instance.CurrentBigMap });

                }
            };
            uiHost.bonus.Show(false);
        }

        #endregion 
    }
}
