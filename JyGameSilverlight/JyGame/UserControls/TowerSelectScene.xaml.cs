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
	public partial class TowerSelectScene : UserControl
	{
        private List<String> towers = new List<String>();
        public CommonSettings.VoidCallBack confirmBack = null;
        public CommonSettings.VoidCallBack cancelBack = null;
        public CommonSettings.VoidCallBack bonusBack = null;
        public string currentTower = "";
        public int currentIndex = 0;
        public int maxFriendNo = 0;
        public bool inited = false;
        public Collection<string> cannotSelected = new Collection<string>();
        public List<string> bonuses = new List<string>(); 

        public UIHost uiHost = null;
        
		public TowerSelectScene()
		{
			InitializeComponent();
		}

        public void Init()
        {
            towers = TowerManager.getTowers();
            towerList.Items.Clear();

            preview.Background = null;
            currentIndex = 0;
            cannotSelected.Clear();
            bonuses.Clear();
            inited = true;
            friendNo.Text = " 场";
            //enemyNo.Text = " 人";
        }

        public void load()
        {
            if(!inited)
                Init(); 
            foreach (String tower in towers)
            {
                bool addTower = true;
                foreach (EventCondition condition in TowerManager.getCondition(tower))
                {
                    if (!TriggerManager.judge(condition))
                    {
                        addTower = false;
                        break;
                    }
                }

                if (addTower && (!towerList.Items.Contains(tower)) )
                    towerList.Items.Add(tower);
            }
            currentIndex = 0;
            cannotSelected.Clear();
            bonuses.Clear();
            this.Visibility = Visibility.Visible;
        }

        public void loadHuashan()
        {
            if (!inited)
                Init();
            foreach (String tower in towers)
            {
                bool addTower = true;
                foreach (EventCondition condition in TowerManager.getCondition(tower))
                {
                    if (!TriggerManager.judge(condition))
                    {
                        addTower = false;
                        break;
                    }
                }

                if (addTower && (!towerList.Items.Contains(tower)))
                    towerList.Items.Add(tower);
            }
            currentIndex = 0;
            cannotSelected.Clear();
            bonuses.Clear();
            this.Visibility = Visibility.Collapsed;
        }


        private void towerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentTower = (string)((ListBox)sender).SelectedValue;
            Battle firstBattle = TowerManager.getTower(currentTower)[0];

            MapTemplate mapTemplate = BattleManager.GetMapTemplate(firstBattle.templateKey);

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = mapTemplate.Background;
            brush.Stretch = Stretch.Uniform;
            preview.Background = brush;

            friendNo.Text = TowerManager.getTower(currentTower).Count.ToString() + "场";
            descText.Text = TowerManager.getTowerDesc(currentTower);
            calcFriend(firstBattle);
        }

        public void calcFriend(Battle battle, int myTeamIndex = 1)
        {
            int friendTeamNo = 0;
            int enemyTeamNo = 0;
            foreach (BattleRole role in battle.battleRoles)
            {
                if (role.team == myTeamIndex && role.roleKey == null)
                {
                    friendTeamNo++;
                }
                else
                {
                    enemyTeamNo++;
                }
            }
            maxFriendNo = friendTeamNo;
        }

        public void bonus()
        {
            showBonus();
        }

        public void showBonus()
        {
            /**************************
             *  天关的奖励，从第一层一直奖励到完成的最后一层
             * *************************/

            List<Dialog> dialogs = new List<Dialog>();
            for (int i = 0; i <= currentIndex; i++)
            {
                Battle battle = TowerManager.getTower(currentTower)[i];
                string BonusItem = bonuses[i];
                RuntimeData.Instance.Items.Add(ItemManager.GetItem(BonusItem).Clone(true));
                Dialog dialog = new Dialog();
                dialog.role = "北丑";
                dialog.type = "DIALOG";
                dialog.info = "这是你在第【" + (i+1).ToString()+ "】关【" + battle.Key + "】所获得的奖励！";

                Dialog dialog2 = new Dialog();
                dialog2.role = "主角";
                dialog2.type = "DIALOG";
                dialog2.info = "获得【" + BonusItem + "】。";

                dialogs.Add(dialog);
                dialogs.Add(dialog2);
                if (i == currentIndex)
                {
                    uiHost.dialogPanel.ShowDialogs(dialogs, (j) =>
                    {
                        uiHost.dialogPanel.CallBack = null;
                        bonusBack();
                    });
                }
            }
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (currentTower == null || currentTower == "")
            {
                MessageBox.Show("请先选择一个要挑战的天关！");
            }
            else
            {
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