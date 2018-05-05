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
using System.Collections.Generic;
using JyGame.GameData;
using System.Collections.ObjectModel;
using JyGame.Logic;

namespace JyGame.UserControls
{
	public partial class ArenaSelectScene : UserControl
	{
        private Dictionary<string, Battle> arenas = new Dictionary<string, Battle>();
        private bool inited = false;
        public Battle battle = null;
        public int maxFriendNo = 0;
        public int maxEnemyNo = 0;
        public CommonSettings.VoidCallBack confirmBack = null;
        public CommonSettings.VoidCallBack cancelBack = null;
        public CommonSettings.VoidCallBack bonusBack = null;
        Random rd = new Random();

        public int hardLevel = 1;
        public List<string> selectedEnemies = new List<string>();
        public List<string> joinEnemies = new List<string>();
        public int currentJoinIndex = 0;
        Random randomJoin = new Random();
        public UIHost uiHost = null;

        
		public ArenaSelectScene()
		{
			InitializeComponent();
		}

        public void Init()
        {
            List<Battle> arenasNameList = BattleManager.getArenas();
            foreach (Battle battle in arenasNameList)
            {
                arenas.Add(battle.Key, battle);
                arenaList.Items.Add(battle.Key);
            }

            this.battle = null;
            preview.Background = null;
            maxFriendNo = 0;
            maxEnemyNo = 0;
            friendNo.Text = " 人";
            enemyNo.Text = " 人";

        }

        public void load()
        {
            if (!inited)
            {
                Init();
                inited = true;
            }
            this.Visibility = Visibility.Visible;
        }

        private void arenaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string battleKey = (string)((ListBox)sender).SelectedValue;
            battle = arenas[battleKey];

            MapTemplate mapTemplate = BattleManager.GetMapTemplate(battle.templateKey);

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = mapTemplate.Background;
            brush.Stretch = Stretch.Uniform;
            preview.Background = brush;

            int team1No = 0;
            int team2No = 0;
            foreach (BattleRole role in battle.battleRoles)
            {
                if (role.team == 1)
                {
                    team1No++;
                }
                else
                {
                    team2No++;
                }
            }
            maxFriendNo = team1No;
            maxEnemyNo = team2No;
            friendNo.Text = team1No.ToString() + "人";
            enemyNo.Text = team2No.ToString() + "人";
        }

        private void arenaHard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hardLevel = ((ListBox)sender).SelectedIndex + 1;
            /*switch (hardKey)
            {
                case "江湖宵小":
                    hardLevel = 1;
                    break;
                case "小有名气":
                    hardLevel = 2;
                    break;
                case "成名高手":
                    hardLevel = 3;
                    break;
                case "威震四方":
                    hardLevel = 4;
                    break;
                case "惊世骇俗":
                    hardLevel = 5;
                    break;
                case "天人合一":
                    hardLevel = 6;
                    break;
                default:
                    hardLevel = 1;
                    break;
            }*/
        }

        public void GenerateEnemy()
        {
            selectedEnemies.Clear();

            List<Role> roles = RoleManager.GetRoles();
            List<string> roleKeys = new List<string>();
            foreach (var role in roles)
            {
                if (role.Attributes["arena"] == 1)
                {
                    if (hardLevel < 5 && role.Level <= hardLevel * 5 && role.Level > (hardLevel - 1) * 5)
                    {
                        roleKeys.Add(role.Key);
                    }
                    else if (hardLevel == 5 && role.Level >= 25 && role.Level < 30)
                    {
                        roleKeys.Add(role.Key);
                    }
                    else if (hardLevel == 6 && role.Level >= 30)
                    {
                        roleKeys.Add(role.Key);
                    }
                }
            }

            //if (roleKeys.Count <= maxEnemyNo)
            //{
            //    selectedEnemies.AddRange(roleKeys);
           // }
           // else
           // {

                for (int i = 0; i < maxEnemyNo; i++)
                {
                    //rd = new Random();
                    int randamIndex = rd.Next(roleKeys.Count);
                    string roleKey = roleKeys[randamIndex];
                    selectedEnemies.Add(roleKey);
                }
           // }
        }

        public void bonus()
        {
            /*joinEnemies.Clear();
            foreach (string enemy in selectedEnemies)
            {
                int prob = RoleManager.GetRole(enemy).Attributes["arenaJoinProb"];
                //randomJoin = new Random();
                if (randomJoin.Next(100) <= prob && (!joinEnemies.Contains(enemy)) && (!RuntimeData.Instance.InTeam(enemy)))
                {
                    joinEnemies.Add(enemy);
                }
            }

            currentJoinIndex = 0;
            showJoinInfo(0);*/

            showBonus();
        }

        public void showBonus()
        {
            /**************************
             * 
             *  竞技场的奖励
             *  
             * 
             * *************************/

            //List<string> itemList = new List<string>();

            //奖励物品的map, key是物品名，value是否只能得到一次
            List<BonusItem> itemList = new List<BonusItem>();

            itemList.Clear();

            #region 单挑战 & 4v4 礼物

            if (battle.Key == "arena_单挑战" || battle.Key == "arena_沙漠4v4")
            {
                //Arena奖励

                if (hardLevel >= 1 && hardLevel <=2)
                {
                    itemList.Add(new BonusItem("大还丹"));
                    itemList.Add(new BonusItem("道口烧鸡"));
                    itemList.Add(new BonusItem("月饼"));
                }
                if (hardLevel >= 2 && hardLevel <= 3)
                {
                    itemList.Add(new BonusItem("九花玉露丸"));
                    itemList.Add(new BonusItem("冬虫夏草"));
                    itemList.Add(new BonusItem("特制鸡腿"));
                }
                if (hardLevel >= 3 && hardLevel <= 4)
                {
                    itemList.Add(new BonusItem("罗汉拳谱", 1, 0.5));
                    itemList.Add(new BonusItem("松风剑法秘籍", 1, 0.5));
                    itemList.Add(new BonusItem("南山刀法谱", 1, 0.5));
                    itemList.Add(new BonusItem("蛇鹤八打", 1, 0.5));
                    itemList.Add(new BonusItem("四象掌掌谱",1,0.5));
                    itemList.Add(new BonusItem("绕指柔剑剑谱",1,0.5));
                    itemList.Add(new BonusItem("草原刀法", 1,0.5));
                    itemList.Add(new BonusItem("玉蜂针", 1,0.5));
                }
                if (hardLevel >= 4 && hardLevel <= 5)
                {
                    itemList.Add(new BonusItem("燃木刀法", 1,0.4));
                    itemList.Add(new BonusItem("黑血神针秘籍", 1,0.4));
                    itemList.Add(new BonusItem("天王保命丹", 0,0.5));
                    itemList.Add(new BonusItem("生生造化丹", 0,0.5));
                    itemList.Add(new BonusItem("黑玉断续膏", 0,0.5));
                    itemList.Add(new BonusItem("精制匕首"));
                    itemList.Add(new BonusItem("铲刀"));
                }
                if (hardLevel >= 5)
                {
                    itemList.Add(new BonusItem("太极拳谱", 1, 0.4));
                    itemList.Add(new BonusItem("太极剑法谱", 1, 0.4));
                    itemList.Add(new BonusItem("天王保命丹"));
                    itemList.Add(new BonusItem("生生造化丹"));
                    itemList.Add(new BonusItem("黑玉断续膏"));
                    itemList.Add(new BonusItem("檀香佛珠"));
                    itemList.Add(new BonusItem("金丝护腕"));
                }
                if (hardLevel >= 6)
                {
                    itemList.Add(new BonusItem("天王保命丹"));
                    itemList.Add(new BonusItem("续命八丸"));
                    //itemList.Add(new BonusItem("笑傲江湖曲", 1,0.2));
                    //itemList.Add(new BonusItem("凌波微步图谱", 1,0.2));
                    //itemList.Add(new BonusItem("青莲诗集", 1,0.3));
                    itemList.Add(new BonusItem("天地同寿", 1,0.3));
                    itemList.Add(new BonusItem("王母蟠桃", 5,0.5));
                    itemList.Add(new BonusItem("道家仙丹", 5,0.5));
                    itemList.Add(new BonusItem("霓裳羽衣"));
                    itemList.Add(new BonusItem("幽梦衣"));
                    itemList.Add(new BonusItem("蓝宝戒指"));
                    itemList.Add(new BonusItem("水晶护符"));
                    //itemList.Add(new BonusItem("天王保命丹"));
                    //itemList.Add(new BonusItem("生生造化丹"));
                    //itemList.Add(new BonusItem("黑玉断续膏"));
                    //itemList.Add(new BonusItem("松果"));
                }

            }
            #endregion
            #region 血战重阳宫

            if (battle.Key == "arena_血战重阳宫")
            {
                //Arena奖励

                if (hardLevel >= 1 && hardLevel <= 2)
                {
                    itemList.Add(new BonusItem("九花玉露丸"));
                    itemList.Add(new BonusItem("冬虫夏草"));
                    itemList.Add(new BonusItem("特制鸡腿"));
                }
                if (hardLevel >= 2 && hardLevel <= 3)
                {
                    itemList.Add(new BonusItem("草原刀法", 1,0.5));
                    itemList.Add(new BonusItem("玉蜂针", 1,0.5));
                }
                if (hardLevel >= 3 && hardLevel <= 4)
                {
                    itemList.Add(new BonusItem("四象掌掌谱", 1, 0.5));
                    itemList.Add(new BonusItem("绕指柔剑剑谱", 1, 0.5));
                    itemList.Add(new BonusItem("天王保命丹"));
                    itemList.Add(new BonusItem("生生造化丹"));
                    itemList.Add(new BonusItem("黑玉断续膏"));
                    itemList.Add(new BonusItem("全真心法秘籍", 1, 0.5));
                }
                if (hardLevel >= 4 && hardLevel <= 5)
                {
                    //itemList.Add(new BonusItem("灭仙爪", 1,0.3));
                    //itemList.Add(new BonusItem("倚天剑", 1,0.3));
                    //itemList.Add(new BonusItem("屠龙刀", 1,0.3));
                    //itemList.Add(new BonusItem("打狗棒", 1,0.3));
                    itemList.Add(new BonusItem("太极拳谱", 1, 0.5));
                    itemList.Add(new BonusItem("太极剑法谱", 1, 0.5));
                    itemList.Add(new BonusItem("燃木刀法", 1, 0.5));
                    itemList.Add(new BonusItem("黑血神针秘籍", 1, 0.5));
                    itemList.Add(new BonusItem("天王保命丹"));
                    itemList.Add(new BonusItem("生生造化丹"));
                    itemList.Add(new BonusItem("黑玉断续膏"));
                }
                if (hardLevel >= 5)
                {
                    //itemList.Add(new BonusItem("降龙十八掌谱", 1,0.3));
                    //itemList.Add(new BonusItem("独孤九剑剑谱", 1,0.3));
                    //itemList.Add(new BonusItem("鸳鸯刀", 1,0.3));
                    //itemList.Add(new BonusItem("打狗棒法入门", 1,0.3));
                    //itemList.Add(new BonusItem("笑傲江湖曲", 1,0.3));
                    //itemList.Add(new BonusItem("天下轻功总决", 1, 0.2));
                    //itemList.Add(new BonusItem("青莲诗集", 1,0.3));
                    itemList.Add(new BonusItem("天地同寿", 1,0.3));
                    itemList.Add(new BonusItem("王母蟠桃", 5,0.5));
                    itemList.Add(new BonusItem("道家仙丹", 5,0.5));
                    itemList.Add(new BonusItem("乌蚕衣", 1, 0.5));
                    itemList.Add(new BonusItem("回力跑鞋", 1, 0.5));
                    itemList.Add(new BonusItem("续命八丸"));
                    itemList.Add(new BonusItem("流星锤"));
                    itemList.Add(new BonusItem("搏击爪套"));
                }
                if (hardLevel >= 6)
                {
                    //itemList.Add(new BonusItem("黑暗遗篇之黑天死炎", 1,0.2));
                    //itemList.Add(new BonusItem("黑暗遗篇之死生茫茫", 1,0.2));
                    itemList.Add(new BonusItem("乾坤大挪移心法", 1,0.2));
                    itemList.Add(new BonusItem("素心神剑心得", 1,0.2));
                    //itemList.Add(new BonusItem("太极心得手抄本", 1,0.2));
                    //itemList.Add(new BonusItem("华佗遗篇", 1,0.2));
                    //itemList.Add(new BonusItem("凌波微步图谱", 1, 0.3));
                    //itemList.Add(new BonusItem("无尽斗志", 1,0.2));
                    itemList.Add(new BonusItem("神行百变秘籍", 1, 0.2));
                    itemList.Add(new BonusItem("厚黑学", 1, 0.2));
                    itemList.Add(new BonusItem("松果", 5, 0.4));
                    itemList.Add(new BonusItem("续命八丸"));
                    itemList.Add(new BonusItem("霓裳羽衣"));
                    itemList.Add(new BonusItem("幽梦衣"));
                    itemList.Add(new BonusItem("蓝宝戒指"));
                    itemList.Add(new BonusItem("水晶护符"));
                }
            }
            #endregion
            #region 三吃一

            if (battle.Key == "arena_三吃一")
            {
                //Arena奖励
                if (hardLevel >= 1 && hardLevel <= 2)
                {
                    itemList.Add(new BonusItem("大还丹"));
                    itemList.Add(new BonusItem("道口烧鸡"));
                    itemList.Add(new BonusItem("月饼"));
                }
                if (hardLevel >= 2 && hardLevel <= 3)
                {
                    itemList.Add(new BonusItem("九花玉露丸"));
                    itemList.Add(new BonusItem("冬虫夏草"));
                    itemList.Add(new BonusItem("特制鸡腿"));
                }
                if (hardLevel >= 3 && hardLevel <= 4)
                {
                    itemList.Add(new BonusItem("罗汉拳谱", 1, 0.5));
                    itemList.Add(new BonusItem("松风剑法秘籍", 1, 0.5));
                    itemList.Add(new BonusItem("南山刀法谱", 1, 0.5));
                    itemList.Add(new BonusItem("蛇鹤八打", 1, 0.5));
                    itemList.Add(new BonusItem("特制鸡腿"));
                }
                if (hardLevel >= 4 && hardLevel <= 5)
                {
                    itemList.Add(new BonusItem("四象掌掌谱", 1, 0.5));
                    itemList.Add(new BonusItem("绕指柔剑剑谱", 1, 0.5));
                    itemList.Add(new BonusItem("草原刀法", 1, 0.5));
                    itemList.Add(new BonusItem("玉蜂针", 1, 0.5));
                    itemList.Add(new BonusItem("冬虫夏草"));
                }
                if (hardLevel >= 5)
                {
                    itemList.Add(new BonusItem("燃木刀法", 1, 0.4));
                    itemList.Add(new BonusItem("黑血神针秘籍", 1, 0.4));
                    itemList.Add(new BonusItem("天王保命丹"));
                    itemList.Add(new BonusItem("生生造化丹"));
                    itemList.Add(new BonusItem("黑玉断续膏"));
                }
                if (hardLevel >= 6)
                {
                    itemList.Add(new BonusItem("太极拳谱", 1, 0.4));
                    itemList.Add(new BonusItem("太极剑法谱", 1, 0.4));
                    itemList.Add(new BonusItem("续命八丸"));
                }
            }
            #endregion

            if (itemList.Count > 0)
            {
                string BonusItemItem = BonusItem.GetRandomBonus(itemList);
                RuntimeData.Instance.Items.Add(ItemManager.GetItem(BonusItemItem).Clone(true));
                Dialog dialog = new Dialog();
                dialog.role = "燕小六";
                dialog.type = "DIALOG";
                dialog.info = "干得不错！这个，奖给你了！";

                Dialog dialog2 = new Dialog();
                dialog2.role = "主角";
                dialog2.type = "DIALOG";
                dialog2.info = "获得【" + BonusItemItem + "】！";

                List<Dialog> dialogs = new List<Dialog>();
                dialogs.Add(dialog);
                dialogs.Add(dialog2);
                uiHost.dialogPanel.ShowDialogs(dialogs, (i) =>
                {
                    bonusBack();
                });
            }
            else
            {
                bonusBack();
            }

        }

        public void showJoinInfo(int selected)
        {
            /*if (currentJoinIndex >= joinEnemies.Count)
            {
                enemyJoinBack();
                return;
            }

            string enemy = joinEnemies[currentJoinIndex];
            currentJoinIndex++;

            Dialog dialog = new Dialog();
            dialog.info = RoleManager.GetRole(enemy).Name + "想加入你的队伍。是否接收？";
            dialog.img = RoleManager.GetRole(enemy).Head;
            dialog.type = "DIALOG";
            dialog.role = RoleManager.GetRole(enemy).Key;
            List<Dialog> dialogs = new List<Dialog>();
            dialogs.Add(dialog);

            uiHost.selectPanel.CallBack = () =>
            {
                uiHost.dialogPanel.CallBack = null;
                if (uiHost.selectPanel.currentSelection == "yes")
                {
                    RuntimeData.Instance.addTeamMember(enemy);
                    dialog.info = "【" + RoleManager.GetRole(enemy).Name + "】加入队伍。";
                    dialog.img = RoleManager.GetRole(enemy).Head;
                    dialog.type = "DIALOG";
                    dialog.role = RoleManager.GetRole(enemy).Key;
                    dialogs.Clear();
                    dialogs.Add(dialog);
                    String date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";
                    RuntimeData.Instance.Log += date + "，" + "【" + RoleManager.GetRole(enemy).Name + "】加入队伍。" + "\r\n";
                }
                else
                {
                    dialog.info = "不要我算了，狗眼看人低。";
                    dialog.img = RoleManager.GetRole(enemy).Head;
                    dialog.type = "DIALOG";
                    dialog.role = RoleManager.GetRole(enemy).Key;
                    dialogs.Clear();
                    dialogs.Add(dialog);
                }

                uiHost.dialogPanel.CallBack = showJoinInfo;
                uiHost.dialogPanel.ShowDialogs(dialogs);
            };

            uiHost.dialogPanel.CallBack = (tmp) =>
            {
                uiHost.dialogPanel.Visibility = Visibility.Visible;
                uiHost.selectPanel.title.Text = "是否邀请加入?";
                uiHost.selectPanel.yes.Content = "邀请";
                uiHost.selectPanel.no.Content = "拒绝";
                uiHost.selectPanel.ShowSelection();
            };

            uiHost.dialogPanel.ShowDialogs(dialogs);*/
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (battle == null)
            {
                MessageBox.Show("请先选择一个竞技场场景！");
            }
            else
            {
                GenerateEnemy();
                this.Visibility = Visibility.Collapsed;
                confirmBack();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            cancelBack();
        }
	}
}