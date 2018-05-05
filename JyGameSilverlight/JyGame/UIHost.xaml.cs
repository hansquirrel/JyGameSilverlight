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

using JyGame.UserControls;
using JyGame.GameData;
using System.Windows.Threading;
using JyGame.Logic;


namespace JyGame
{
    public partial class UIHost : UserControl
    {
        public UIHost()
        {
            InitializeComponent();

            mapUI.uiHost = this;
            battleFieldContainer.uiHost = this;
            scence.uiHost = this;
            rolePanel.uiHost = this;
            roleListPanel.uiHost = this;
            dialogPanel.uihost = this;
            arenaSelectRole.uiHost = this;
            arenaSelectScene.uiHost = this;
            towerSelectRole.uiHost = this;
            towerSelectScene.uiHost = this;
            bonus.uiHost = this;
            systemOptionsPanel.uiHost = this;
            saveLoadPanel.uiHost = this;
            mainMenu.uiHost = this;
            multiSelectBox.uiHost = this;
            rollRolePanel.uiHost = this;
            envsetPanel.uiHost = this;
            shopPanel.uiHost = this;
            onlineGamePanel.uiHost = this;
        }

        //每次返回大地图时候需要做的事情（补血、补内力、消CD...）
        private void returnToMapInit()
        {
            //部队补满血、内力，武功CD清零
            foreach (Role role in RuntimeData.Instance.Team)
            {
                role.Attributes["hp"] = role.Attributes["maxhp"];
                role.Attributes["mp"] = role.Attributes["maxmp"];
                foreach (SkillInstance skill in role.Skills)
                {
                    skill.CurrentCd = 0;
                }
            }
        }

        #region 控件管理

        public void reset()
        {
            //物品选择面板
            itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;

            //角色详细信息面板
            roleDetailPanel.Visibility = System.Windows.Visibility.Collapsed;
            roleDetailPanel_float.Visibility = System.Windows.Visibility.Collapsed;

            //对话面板
            dialogCover.Visibility = Visibility.Collapsed;

            //角色面板
            this.rolePanel.Visibility = Visibility.Collapsed;
            this.roleListPanel.Visibility = System.Windows.Visibility.Collapsed;

            //选项面板
            selectPanel.Visibility = Visibility.Collapsed;

            //商店面板
            //storePanel.Visibility = Visibility.Collapsed;

            //江湖日志面板
            logPanel.Visibility = Visibility.Collapsed;

            //系统选项
            systemOptionsPanel.Visibility = System.Windows.Visibility.Collapsed;

            //存储/读取对话框
            saveLoadPanel.Visibility = System.Windows.Visibility.Collapsed;

            //商店
            shopPanel.Visibility = System.Windows.Visibility.Collapsed;

            //战场
            battleFieldContainer.Visibility = System.Windows.Visibility.Collapsed;

            //角色行动选择框
            roleActionPanel.Visibility = System.Windows.Visibility.Collapsed;

            //技能选择框
            skillPanel.Visibility = System.Windows.Visibility.Collapsed;

            //联机游戏
            onlineGamePanel.Visibility = System.Windows.Visibility.Collapsed;

            //人物头像
            scence.HideHeads();
            mapUI.resetHead();

            //audiopanel
            audioPanel.Hide();
        }


        private Color[] danmuColors = new Color[]{
            Colors.Red,
            Colors.Yellow,
            Colors.Blue,
            Colors.Cyan,
            Colors.Orange,
            Colors.Magenta
        };

        /// <summary>
        /// 增加一条弹幕
        /// </summary>
        /// <param name="info"></param>
        public void AddDanMu(string info)
        {
            TextBlock tb = new TextBlock()
            {
                FontFamily = new FontFamily("SimHei"),
                FontSize = Tools.GetRandomInt(20,30),
                IsHitTestVisible = false,
                Text = info,
                Foreground = new SolidColorBrush(danmuColors[Tools.GetRandomInt(0,danmuColors.Length - 1)]),
                Opacity = 0.75
            };
            
            DanmuCanvas.Children.Add(tb);
            double startLeft = 800 + Tools.GetRandomInt(0,800);
            Canvas.SetLeft(tb, startLeft);
            Canvas.SetTop(tb, Tools.GetRandomInt(0, 500));
           
            Storyboard storyboard = new Storyboard();
            DoubleAnimation animationX = new DoubleAnimation()
            {
                From = startLeft,
                To = -tb.ActualWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(Tools.GetRandomInt(10000,16000))),
                
            };
            Storyboard.SetTarget(animationX, tb);
            Storyboard.SetTargetProperty(animationX, new PropertyPath("(Canvas.Left)"));
            storyboard.Children.Add(animationX);
            
            storyboard.Completed += (ss, e) =>
            {
                storyboard.Stop();
                storyboard = null;
                DanmuCanvas.Children.Remove(tb);
            };
            storyboard.Begin();
        }

        public void playSmallGame(string game)
        {
            
            #region 轻功游戏
            if (game == "dodge")
            {
                dodgeGame.callBack = (s) =>
                {
                    dodgeGame.Visibility = Visibility.Collapsed;
                    RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(2 * 3);

                    List<Dialog> dialogs = new List<Dialog>();

                    Dialog dialog = new Dialog();
                    dialog.role = "佟湘玉";
                    dialog.type = "DIALOG";
                    dialog.info = "坚持了" + s.ToString() + "秒！";
                    dialogs.Add(dialog);
                    if (dodgeGame.IsCheat)
                    {
                        dialogs.Add(new Dialog()
                        {
                            role = "佟湘玉",
                            type = "DIALOG",
                            info = "小子！用糖果浏览器了吧？不给奖励物品！想获得奖励物品，就堂堂正正的玩吧！"
                        });
                    }
                    else
                    {
                        if (s >= 5 && s < 10)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "干得不错，奖你一点小礼物。";


                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得特制鸡腿 x 1";

                            RuntimeData.Instance.Items.Add(ItemManager.GetItem("特制鸡腿").Clone());
                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }

                        if (s >= 10 && s < 14)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "太牛了，少侠我对你的敬意如滔滔江水不绝...一点小礼物，不成敬意。";

                            List<BonusItem> bonusItems = new List<BonusItem>();
                            bonusItems.Add(new BonusItem("冬虫夏草"));
                            bonusItems.Add(new BonusItem("金丝道袍"));
                            bonusItems.Add(new BonusItem("阔剑"));
                            bonusItems.Add(new BonusItem("精钢拳套"));
                            bonusItems.Add(new BonusItem("金刚杵"));
                            bonusItems.Add(new BonusItem("柳叶刀"));

                            string ItemName = BonusItem.GetRandomBonus(bonusItems);
                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得" + ItemName + " x 1";

                            RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));
                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }

                        if (s >= 14 && s < 17)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "OMG...少侠我好崇拜你哦。";


                            List<BonusItem> bonusItems = new List<BonusItem>();
                            bonusItems.Add(new BonusItem("生生造化丹"));
                            bonusItems.Add(new BonusItem("冬虫夏草"));
                            bonusItems.Add(new BonusItem("罗汉拳谱", 1, 1));
                            bonusItems.Add(new BonusItem("天山掌法谱", 1, 1));
                            bonusItems.Add(new BonusItem("松风剑法秘籍", 1, 1));
                            bonusItems.Add(new BonusItem("华山剑法秘籍", 1, 1));
                            bonusItems.Add(new BonusItem("三分剑术", 1, 1));
                            bonusItems.Add(new BonusItem("雷震剑法秘籍", 1, 1));
                            bonusItems.Add(new BonusItem("南山刀法谱", 1, 1));
                            bonusItems.Add(new BonusItem("袖箭秘诀", 1, 1));
                            bonusItems.Add(new BonusItem("拂尘秘诀", 1, 1));
                            bonusItems.Add(new BonusItem("蛇鹤八打", 1, 1));
                            string ItemName = BonusItem.GetRandomBonus(bonusItems);
                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得" + ItemName + " x 1";

                            RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));
                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }

                        if (s >= 17 && s < 20)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "少侠，你真的是人类么？";

                            List<BonusItem> bonusItems = new List<BonusItem>();
                            bonusItems.Add(new BonusItem("生生造化丹"));
                            bonusItems.Add(new BonusItem("黑玉断续膏"));
                            bonusItems.Add(new BonusItem("君子剑", 1, 1));
                            bonusItems.Add(new BonusItem("淑女剑", 1, 1));
                            string ItemName = BonusItem.GetRandomBonus(bonusItems);
                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得" + ItemName + " x 1";
                            RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));

                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }

                        if (s >= 20 && s < 23)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "...你已经是God Like了。";

                            List<BonusItem> bonusItems = new List<BonusItem>();
                            bonusItems.Add(new BonusItem("生生造化丹"));
                            bonusItems.Add(new BonusItem("黑玉断续膏"));
                            bonusItems.Add(new BonusItem("天王保命丹"));
                            bonusItems.Add(new BonusItem("乌蚕衣", 1, 1));
                            string ItemName = BonusItem.GetRandomBonus(bonusItems);
                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得" + ItemName + " x 1";
                            RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));

                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }

                        if (s >= 23)
                        {
                            Dialog dialogBonus = new Dialog();
                            dialogBonus.role = "佟湘玉";
                            dialogBonus.type = "DIALOG";
                            dialogBonus.info = "Oh, S**t！你已经超神了！";

                            List<BonusItem> bonusItems = new List<BonusItem>();
                            bonusItems.Add(new BonusItem("生生造化丹"));
                            bonusItems.Add(new BonusItem("黑玉断续膏"));
                            bonusItems.Add(new BonusItem("天王保命丹"));
                            bonusItems.Add(new BonusItem("凌波微步图谱", 1, 1));
                            bonusItems.Add(new BonusItem("天下轻功总决", 1, 1));
                            string ItemName = BonusItem.GetRandomBonus(bonusItems);
                            Dialog dialogBonus2 = new Dialog();
                            dialogBonus2.role = "佟湘玉";
                            dialogBonus2.type = "DIALOG";
                            dialogBonus2.info = "获得" + ItemName + " x 1";
                            RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));
                            dialogs.Add(dialogBonus);
                            dialogs.Add(dialogBonus2);
                        }
                    }

                    RuntimeData.Instance.DodgePoint = RuntimeData.Instance.DodgePoint + s * 2;

                    if (RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"] >= CommonSettings.SMALLGAME_MAX_ATTRIBUTE)
                    {
                        RuntimeData.Instance.DodgePoint = 0;
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "貌似练这个已经没什么长进了...";
                        dialogs.Add(dialog2);
                    }
                    else if (RuntimeData.Instance.DodgePoint >= RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"])
                    {
                        RuntimeData.Instance.DodgePoint = 0;
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "你的身法进步了！身法从【" + RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"].ToString() + "】提高至【" + (RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"] + 5).ToString() + "】！";
                        dialogs.Add(dialog2);
                        RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"] = RuntimeData.Instance.GetTeamRole("主角").Attributes["shenfa"] + 5;
                    }
                    else
                    {
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "你练习了一会儿，对轻身功夫似乎有了一些心得...";
                        dialogs.Add(dialog2);
                    }

                    dialogPanel.ShowDialogs(
                        dialogs,
                        (rst) =>
                        {
                            dialogPanel.CallBack = null;
                            RuntimeData.Instance.gameEngine.LoadMap(this.mapUI.currentMap.Name);
                        }
                    );
                };
                dodgeGame.Visibility = Visibility.Visible;
                dodgeGame.start();
            }
            #endregion 
            #region 点穴
            if (game == "dianxue")
            {
                TimeSpan ts1 = new TimeSpan(RuntimeData.Instance.Date.Ticks);
                dianxueGame.callBack = (items, point) =>
                {
                    double duration = new TimeSpan(RuntimeData.Instance.Date.Ticks).Subtract(ts1).Duration().TotalSeconds;
                    
                    //MessageBox.Show(duration.ToString());

                    dianxueGame.Visibility = Visibility.Collapsed;
                    RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(2 * 3);
                    //MessageBox.Show(duration.ToString());
                    List<Dialog> dialogs = new List<Dialog>();
                    if (duration > 25)
                    {
                        dialogs.Add(new Dialog()
                        {
                            role = "白展堂",
                            type = "DIALOG",
                            info = "你这小子，别以为我不知道你用了糖果浏览器！作弊不给物品奖励！想获得奖励物品，堂堂正正的玩吧！"
                        });
                    }
                    else
                    {
                        //奖励物品
                        foreach (KeyValuePair<string, int> item in items)
                        {
                            if (item.Value > 0)
                            {
                                string ItemName = item.Key.Replace("物品.", "");
                                Dialog dialogBonus2 = new Dialog();
                                dialogBonus2.role = "白展堂";
                                dialogBonus2.type = "DIALOG";
                                dialogBonus2.info = "获得" + ItemName + " x " + item.Value;
                                for (int i = 0; i < item.Value; ++i)
                                {
                                    RuntimeData.Instance.Items.Add(ItemManager.GetItem(ItemName).Clone(true));
                                }
                                dialogs.Add(dialogBonus2);
                            }
                        }
                    }

                    RuntimeData.Instance.biliPoint = RuntimeData.Instance.biliPoint + (int)point;

                    if (RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"] >= CommonSettings.SMALLGAME_MAX_ATTRIBUTE)
                    {
                        RuntimeData.Instance.biliPoint = 0;
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "貌似现在练这个已经没法提高臂力了。。";
                        dialogs.Add(dialog2);
                    }
                    else if (RuntimeData.Instance.biliPoint >= RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"])
                    {
                        RuntimeData.Instance.biliPoint = 0;
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "你的臂力进步了！臂力从【" + RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"].ToString() + "】提高至【" + (RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"] + 5).ToString() + "】！";
                        dialogs.Add(dialog2);
                        RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"] = RuntimeData.Instance.GetTeamRole("主角").Attributes["bili"] + 5;
                    }
                    else
                    {
                        Dialog dialog2 = new Dialog();
                        dialog2.role = "主角";
                        dialog2.type = "DIALOG";
                        dialog2.info = "你练习了一会儿，对臂力功夫似乎有了一些心得...";
                        dialogs.Add(dialog2);
                    }

                    dialogPanel.ShowDialogs(dialogs,(rst) =>
                    {
                        dialogPanel.CallBack = null;
                        RuntimeData.Instance.gameEngine.LoadMap(this.mapUI.currentMap.Name);
                    });
                };
                dianxueGame.Visibility = Visibility.Visible;
                dianxueGame.start();

                
                
            }
            #endregion
            #region 直接升级
            if (game == "levelup")
            {
                int maxLevel = 12;
                if (TriggerManager.judge(new EventCondition() { type = "should_finish", value = "mainStory_黑暗的阴影1" }))
                {
                    maxLevel = 18;
                }
                if (TriggerManager.judge(new EventCondition() { type = "should_finish", value = "mainStory_神秘剑客1" }))
                {
                    maxLevel = 22;
                }
                if (TriggerManager.judge(new EventCondition() { type = "should_finish", value = "mainStory_紧急1" }))
                {
                    maxLevel = 25;
                }

                List<string> levelUpedRoles = new List<string>();
                //升级
                foreach (Role role in RuntimeData.Instance.Team)
                {
                    if (role.Level < maxLevel)
                    {
                        int exp2add = role.LevelupExp - role.Exp + 1;
                        role.AddExp(exp2add);
                        levelUpedRoles.Add(role.Name);
                    }
                }
                AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));
                //MessageBox.Show(string.Format("【{0}】升到第【{1}】级", currentSpirit.Role.Name, currentSpirit.Role.Level) );
                Dialog dialog = new Dialog();
                dialog.role = "主角";
                dialog.type = "DIALOG";
                if (levelUpedRoles.Count == 0)
                {
                    dialog.info = string.Format("没有人满足升级条件，全员大于等于" + maxLevel + "级，钱浪费了");

                }
                else
                {
                    string tmp = "";
                    foreach (var s in levelUpedRoles)
                    {
                        tmp += s + "、";
                    }
                    tmp.TrimEnd(new char[]{'、'});
                    dialog.info = string.Format(tmp + "等级提升一级！");
                }
                dialogPanel.ShowDialog(dialog, (rst) =>
                {
                    dialogPanel.CallBack = null;
                    RuntimeData.Instance.gameEngine.LoadMap(this.mapUI.currentMap.Name);
                });

                
            }
            #endregion
        }

        #endregion

        #region 对话系统
        //public void Chat(string story_dialogs)
        //{
        //    dialogPanel.CallBack = dialogOver;
        //    dialogPanel.ShowDialogs(DialogManager.GetDialogList(story_dialogs));
        //}

        private void dialogOver(int result)
        {
            dialogCover.Visibility = Visibility.Collapsed;
        }
        #endregion
        private void UserControl_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RightClickCallback != null)
            {
                RightClickCallback();
            }
            e.Handled = true;
        }

        public JyGame.GameData.CommonSettings.VoidCallBack RightClickCallback = null;

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }

        private void FullScreenButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Application.Current.Host.Content.IsFullScreen)
                Application.Current.Host.Content.IsFullScreen = true;
        }

        private void hideInfoCover(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.infoCanvas.Visibility = Visibility.Collapsed;
            this.mainMenu.Storyboard1.Begin();
        }

        private void DanmuButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.textBox.Visibility == System.Windows.Visibility.Visible)
                return;
        	if((DateTime.Now - lastDanmuTime).TotalMinutes<1)
            {
                MessageBox.Show("对不起，您每分钟只能发表一次弹幕");
                return;
            }
            this.textBox.Show("输入弹幕", NameTextBoxType.Danmu, "", () => {
                string danmu = this.textBox.text.Text;
                if (!string.IsNullOrEmpty(danmu))
                {
                    GameServerManager.Instance.SendDanmu(danmu);
                    lastDanmuTime = DateTime.Now;
                }
                this.textBox.Visibility = System.Windows.Visibility.Collapsed;
                
            }, 20);
        }
        private DateTime lastDanmuTime = DateTime.MinValue;

        private void DanmuOptionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(Configer.Instance.Danmu)
            {
                Configer.Instance.Danmu = false;
                DanmuCanvas.Visibility = Visibility.Collapsed;
            }
            else
            {
                Configer.Instance.Danmu = true;
                DanmuCanvas.Visibility = Visibility.Visible;
            }
        }

        private void danmuCheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool? isSet = (sender as CheckBox).IsChecked;
            if (isSet != null)
            {
                Configer.Instance.Danmu = (bool)isSet;
                if (Configer.Instance.Danmu)
                {
                    RuntimeData.Instance.gameEngine.uihost.DanmuCanvas.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    RuntimeData.Instance.gameEngine.uihost.DanmuCanvas.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void danmuCheckBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            danmuCheckBox.IsChecked = Configer.Instance.Danmu;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
            this.OpenLogo.Begin();
            this.OpenLogo.Completed += OpenLogo_Completed;
        }

        void OpenLogo_Completed(object sender, EventArgs e)
        {
            this.OpenLogo.Completed -= OpenLogo_Completed;
            this.OpenLogo.Stop();

            infoCanvas.Visibility = System.Windows.Visibility.Collapsed;
            LayoutRoot.Visibility = System.Windows.Visibility.Visible;

            this.mainMenu.Storyboard1.Begin();
        }
    }
}
