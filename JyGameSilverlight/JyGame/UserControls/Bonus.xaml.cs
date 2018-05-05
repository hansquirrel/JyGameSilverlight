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

namespace JyGame.UserControls
{
	public partial class Bonus : UserControl
	{
        public CommonSettings.VoidCallBack confirmBack = null;
        public List<Item> bonusItems = new List<Item>();
        public UIHost uiHost = null;
        Random rd = new Random();

        public int money = 0;
        public int exp = 0;

        public Bonus()
		{
			InitializeComponent();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="win">是否胜利</param>
        /// <param name="bonus">是否有奖励</param>
        public void Show(bool win = true, bool bonus = true)
        {
            //this.enemyNoText.Text = uiHost.battleFieldContainer.field.EnemyTeam.Count.ToString();
            TitleTextBox.Text = win ? "战斗胜利" : "战斗失败";
            if (win)
            {
                if (bonus)
                {
                    calcMoney();
                    calcExp();
                    calcBattleBonusItems();
                }
                else
                {
                    moneyText.Text = "0";
                    expText.Text = "0";
                    bonusItemsPanel.Children.Clear();
                }
            }
            else
            {
                moneyText.Text = "0";
                expText.Text = "0";
                bonusItemsPanel.Children.Clear();
            }
            this.Visibility = Visibility.Visible;
        }

        public void ArenaShow(bool win = true)
        {
            //this.enemyNoText.Text = uiHost.arenaSelectScene.selectedEnemies.Count.ToString();
            TitleTextBox.Text = win ? "战斗胜利" : "战斗失败";
            if (win)
            {
                calcMoney();
                calcExp();
                calcBattleBonusItems();
            }
            else
            {
                moneyText.Text = "0";
                expText.Text = "0";
                bonusItemsPanel.Children.Clear();
            }
            this.Visibility = Visibility.Visible;
        }

        public void calcExp()
        {
            //角色经验
            int totalExp = (int)uiHost.battleFieldContainer.field.TotalExp;
            //foreach (string roleKey in uiHost.battleFieldContainer.field.EnemyTeam)
            //{
            //    if(RoleManager.GetRole(roleKey) != null)
            //        totalExp += (int)((double)RoleManager.GetRole(roleKey).LevelupExp / 15.0);
            //}
            int exp2add = (int)((double)totalExp / (double)uiHost.battleFieldContainer.field.FriendTeam.Count);
            if (exp2add < 5)
                exp2add = 5;
            this.expText.Text = exp2add.ToString();

            foreach (string roleKey in uiHost.battleFieldContainer.field.FriendTeam)
            {
                for(int i=0; i<RuntimeData.Instance.Team.Count; i++)
                {
                    if (RuntimeData.Instance.Team[i].Key == roleKey)
                    {
                        if (RuntimeData.Instance.Team[i].AddExp(exp2add))
                        {
                            AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));
                            string info = string.Format("【{0}】升到第【{1}】级", RuntimeData.Instance.Team[i].Name, RuntimeData.Instance.Team[i].Level);
                            uiHost.dialogPanel.ShowDialog(roleKey,info,null);
                        }
                    }
                }
            }
        }

        public void calcMoney()
        {
            money = 0;

            //根据对方等级计算获得的金钱
            foreach (String roleKey in uiHost.battleFieldContainer.field.EnemyTeam)
            {
                //if (roleKey.Contains("杂鱼")) continue;

                Role role = RoleManager.GetRole(roleKey);
                if(role != null)
                    money += (int)Math.Pow(1.2, (double)role.Level);
            }

            //money = (int)(money * 0.6);
            if (money < 10)
                money = 10;

            this.moneyText.Text = money.ToString();
            RuntimeData.Instance.Money += money;
        }

        //计算普通Battle获得的物品
        public void calcBattleBonusItems()
        {
            bonusItems.Clear();

            foreach (String roleKey in uiHost.battleFieldContainer.field.EnemyTeam)
            {
                List<Item> itemList = new List<Item>();
                Role role = RoleManager.GetRole(roleKey);
                if (role != null)
                {
                    int roleLevel = role.Level;
                    foreach (Item item in ItemManager.Items)
                    {
                        if (item.IsDrop && roleLevel >= (item.level - 1) * 5) 
                        {
                            itemList.Add(item);
                        }
                    }
                }

                //计算取得的物品概率，常规战斗下每个人掉落物品的概率均为10%
                if (itemList.Count > 0 && Tools.ProbabilityTest(0.1))
                {
                    bonusItems.Add(itemList[Tools.GetRandomInt(0,itemList.Count -1)]);
                }

                double roleHard = 2;
                if(role != null){
                    roleHard += role.Level / 3.0;
                    if (role.Level == 30) roleHard = 99999; //30级的角色可以掉落所有残章
                }

                double canzhangProperty = 0;
                if(RuntimeData.Instance.GameMode == "hard")
                {
                    canzhangProperty = 0.03;
                }else if(RuntimeData.Instance.GameMode == "crazy") //炼狱难度，掉率提高
                {
                    canzhangProperty = 0.08 + (RuntimeData.Instance.Round - 1) * 0.04;
                }

                //外功残章
                if (Tools.ProbabilityTest(canzhangProperty))
                {
                    Skill s = null;

                    int c = 0;
                    while (c < 100)
                    {
                        c++;
                        s = SkillManager.GetRandomSkill();
                        if (role != null && role.Level == 30 && s.Hard < 7) continue; //高级角色不掉落低级残章
                        if (role != null && role.Level >= 20 && s.Hard < 4) continue;
                        if (s.Hard < roleHard) break;
                    }
                    bonusItems.Add(ItemManager.GetItem(s.Name + "残章"));
                }
                //内功残章
                if (Tools.ProbabilityTest(canzhangProperty/2.0))
                {
                    InternalSkill s = null;
                    while (true)
                    {
                        s = SkillManager.GetRandomInternalSkill();
                        if (s.Hard < roleHard) break;
                    }
                    bonusItems.Add(ItemManager.GetItem(s.Name + "残章"));
                }
            }

            //显示物品
            int count = 0;
            this.bonusItemsPanel.Children.Clear();
            StackPanel currentPanel = null;
            for (int i = 0; i < bonusItems.Count; i++)
            {
                if (count > 6) count = 0;

                if (count == 0)
                {
                    currentPanel = new StackPanel();
                    currentPanel.Orientation = Orientation.Horizontal;
                    currentPanel.Height = 60;
                    currentPanel.Margin = new Thickness(2, 2, 2, 0);
                    this.bonusItemsPanel.Children.Add(currentPanel);
                }
                Item newItem = bonusItems[i].Clone(true);
                ItemUnit itemUnit = new ItemUnit();
                itemUnit.BindItem(newItem);
                itemUnit.Margin = new Thickness(2, 2, 2, 8);
                currentPanel.Children.Add(itemUnit);
                count++;

                //添加物品
                RuntimeData.Instance.Items.Add(newItem);
            }
        }

        ////计算Arena获得的礼物
        //public void calcArenaBonusItems()
        //{
        //    bonusItems.Clear();
        //    List<Item> itemList = new List<Item>();

        //    //首先取得该难度下的物品清单
        //    foreach (Item item in ItemManager.Items)
        //    {
        //        if (item.level == uiHost.arenaSelectScene.hardLevel)
        //        {
        //            itemList.Add(item);
        //        }
        //    }

        //    //计算取得的物品数目和概率，Arena战斗下掉落物品的概率为1/（我方人数+1）
        //    int itemCount = (int)uiHost.arenaSelectScene.selectedEnemies.Count;
        //    float prob = 1.0f / ((float)uiHost.arenaSelectRole.selectedFriends.Count + 1.0f);

        //    //判断以概率取得的物品
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        //rd = new Random();
        //        int probItemIndex = rd.Next(itemList.Count - 1);
        //        //rd = new Random();
        //        int probability = rd.Next(100);
        //        if (probability <= prob * 100)
        //        {
        //            bonusItems.Add(itemList[probItemIndex]);
        //        }

        //        //残章,每个角色有2%的概率获得外功残章
        //        if (Tools.ProbabilityTest(0.02))
        //        {
        //            Skill s = SkillManager.GetRandomSkill();
        //            bonusItems.Add(ItemManager.GetItem(s.Name + "残章"));
        //        }
        //        //有1%的概率获得内功残章
        //        if (Tools.ProbabilityTest(0.01))
        //        {
        //            InternalSkill s = SkillManager.GetRandomInternalSkill();
        //            bonusItems.Add(ItemManager.GetItem(s.Name + "残章"));
        //        }
        //    }

        //    //显示物品
        //    int count = 0;
        //    this.bonusItemsPanel.Children.Clear();
        //    StackPanel currentPanel = null;
        //    for (int i = 0; i < bonusItems.Count; i++)
        //    {
        //        if (count > 6) count = 0;

        //        if (count == 0)
        //        {
        //            currentPanel = new StackPanel();
        //            currentPanel.Orientation = Orientation.Horizontal;
        //            currentPanel.Height = 55;
        //            currentPanel.Margin = new Thickness(2, 2, 2, 0);
        //            this.bonusItemsPanel.Children.Add(currentPanel);
        //        }

        //        Canvas c = new Canvas();
        //        c.Width = 50;
        //        c.Height = 50;
        //        ImageBrush brush = new ImageBrush();
        //        brush.ImageSource = bonusItems[i].Pic;
        //        brush.Stretch = Stretch.Uniform;
        //        c.Background = brush;
        //        c.Margin = new Thickness(2, 2, 2, 5);
        //        string info = bonusItems[i].ToString();
        //        Item tmp = bonusItems[i];
        //        ToolTipService.SetToolTip(c, tmp.GenerateTooltip());

        //        currentPanel.Children.Add(c);
        //        count++;

        //        //添加物品
        //        RuntimeData.Instance.Items.Add(bonusItems[i].Clone());
        //    }
        //}

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            confirmBack();
        }
	}
}