using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.GameData;

namespace JyGame
{
	public partial class StudioPanel : UserControl
	{
		public StudioPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void InitStorys(List<Story> storys)
        {
            CurrentScriptCombo.Items.Clear();
            foreach(var s in storys)
            {
                CurrentScriptCombo.Items.Add(s.Name);
            }
        }

        public void InitMaps(List<BigMap> bigMaps)
        {
            CurrentMapCombo.Items.Clear();
            foreach (var s in bigMaps)
            {
                CurrentMapCombo.Items.Add(s.Name);
            }
        }

        public void PlayStory(Story story)
        {
            this.JubenTab.IsSelected = true;
            foreach (var item in CurrentScriptCombo.Items)
            {
                if(item.ToString()==story.Name)
                {
                    CurrentScriptCombo.SelectedItem = item;
                    break;
                }
            }
        }

        private void CurrentScriptCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null) return;
            string storyKey = (sender as ComboBox).SelectedItem.ToString();
            CurrentScript.Text = StoryManager.GetStory(storyKey).GenerateXml().ToString();
        }

        private void CurrentMapCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null) return;
            string mapKey = (sender as ComboBox).SelectedItem.ToString();
            CurrentMapScript.Text = MapEventsManager.GetBigMap(mapKey).GenerateXml().ToString();
        }

        public void LoadMap(BigMap map)
        {
            this.MapTab.IsSelected = true;
            foreach (var item in CurrentMapCombo.Items)
            {
                if (item.ToString() == map.Name)
                {
                    CurrentMapCombo.SelectedItem = item;
                    break;
                }
            }
        }

        public void LoadBattle(Battle battle)
        {
            this.BattleTab.IsSelected = true;
            this.CurrentBattleName.Text = battle.Key;
            this.CurrentBattleScript.Text = battle.GenerateXml().ToString();

            this.CurrentBattleTemplateName.Text = battle.Template.Key;
            this.CurrentBattleTemplateScript.Text = battle.Template.GenerateXml().ToString();
        }

        private void EditStoryButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CurrentScriptCombo.SelectedItem != null)
            {
                StoryEditWindow win = new StoryEditWindow();
                string storyKey = CurrentScriptCombo.SelectedItem.ToString();
                win.BindData(StoryManager.GetStory(storyKey));
                win.Show();
            }
            else
            {
                MessageBox.Show("请先选择要编辑的剧本");
            }
        }

        private void NewStoryButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StoryEditWindow win = new StoryEditWindow();
            win.Show();
        }
	}
}