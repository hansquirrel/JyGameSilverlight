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
using JyGame.Interface;

namespace JyGame
{
	public partial class ProjectPanel : UserControl
	{
		public ProjectPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Refresh()
        {
            ProjectTreeView.Items.Clear();

            TreeViewItem tree = new TreeViewItem() { Tag = "剧本" };
            ProjectTreeView.Items.Add(tree);
            foreach(var story in StoryManager.storys)
            {
                tree.Items.Add(new TreeViewItem() { Header = story.Name, Tag = story });
            }

            tree = new TreeViewItem() { Tag = "地图" };
            ProjectTreeView.Items.Add(tree);
            foreach (var map in MapEventsManager.bigMaps)
            {
                tree.Items.Add(new TreeViewItem() { Header = map.Name, Tag = map });
            }

            tree = new TreeViewItem() { Tag = "物品" };
            ProjectTreeView.Items.Add(tree);
            foreach (var item in ItemManager.Items)
            {
                tree.Items.Add(new TreeViewItem() { Header = item.Name, Tag = item });
            }

            tree = new TreeViewItem() { Tag = "外功" };
            ProjectTreeView.Items.Add(tree);
            foreach (var skill in SkillManager.Skills)
            {
                tree.Items.Add(new TreeViewItem() { Header = skill.Name, Tag = skill });
            }

            tree = new TreeViewItem() { Tag = "内功" };
            ProjectTreeView.Items.Add(tree);
            foreach (var skill in SkillManager.InternalSkills)
            {
                tree.Items.Add(new TreeViewItem() { Header = skill.Name,Tag = skill });
            }

            tree = new TreeViewItem() { Tag = "特殊攻击" };
            ProjectTreeView.Items.Add(tree);
            foreach (var skill in SkillManager.SpecialSkills)
            {
                tree.Items.Add(new TreeViewItem() { Header = skill.Name, Tag = skill });
            }

            tree = new TreeViewItem() { Tag = "奥义" };
            ProjectTreeView.Items.Add(tree);
            foreach (var skill in SkillManager.Aoyis)
            {
                tree.Items.Add(new TreeViewItem() { Header = skill.Name, Tag = skill });
            }

            tree = new TreeViewItem() { Tag = "战斗" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in BattleManager.Battles)
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Key,Tag = obj });
            }

            tree = new TreeViewItem() { Tag = "战斗模板" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in BattleManager.MapTemplates)
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Key, Tag = obj });
            }

            tree = new TreeViewItem() { Tag = "资源" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in ResourceManager.ResourceMap)
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Key, Tag = obj });
            }

            tree = new TreeViewItem() { Tag = "人物" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in RoleManager.GetRoles())
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Key, Tag = obj });
            }

            tree = new TreeViewItem() { Tag = "人物成长模板" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in RoleManager.RoleGrowTemplates)
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Name, Tag = obj });
            }

            tree = new TreeViewItem() { Tag = "动画序列" };
            ProjectTreeView.Items.Add(tree);
            foreach (var obj in AnimationManager.project.Units)
            {
                tree.Items.Add(new TreeViewItem() { Header = obj.Name, Tag = obj });
            }

            string keyword = KeywordText.Text;
            foreach (var item in ProjectTreeView.Items)
            {
                SetKeyword(item as TreeViewItem, keyword.Trim());
            }
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void KeywordText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = KeywordText.Text;
            foreach (var item in ProjectTreeView.Items)
            {
                SetKeyword(item as TreeViewItem, keyword.Trim());
            }
        }

        private bool SetKeyword(TreeViewItem item,string keyword)
        {
            bool rst = false;
            int total = 0;
            if(item.Header == null || item.Header.ToString().Contains(keyword) || string.IsNullOrEmpty(item.Header as string))
            {
                rst = true;
            }
            foreach(var it in item.Items)
            {
                if (SetKeyword(it as TreeViewItem, keyword))
                {
                    rst = true;
                    total++;
                }
            }
            if (rst)
                item.Visibility = System.Windows.Visibility.Visible;
            else
                item.Visibility = System.Windows.Visibility.Collapsed;
            if (total > 0)
            {
                item.Header = item.Tag as string + "(" + total + ")";
            }
            return rst;
        }

        private void OpenProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "游戏工程文件(project.xml)|*.xml";
            dialog.InitialDirectory = (Configer.Instance.ScriptRootMenu + "\\Scripts\\");
            dialog.ShowDialog();

            try
            {
                GameProject.LoadGameProject(dialog.File.FullName);
                this.Refresh();
                CurrentProjectText.Text = dialog.File.FullName;
            }
            catch(Exception ex)
            {
                MessageBox.Show("游戏工程载入错误" + ex.ToString());
            }
            
        }

        private void SaveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string projectFilePos = CurrentProjectText.Text;
            ExportScriptsTo(projectFilePos.Replace("project.xml","/project/"));
        }

        //导出工程
        private void ExportScriptsTo(string dir)
        {
            //资源
            ResourceManager.Export(dir);

            //地图
            MapEventsManager.Export(dir);

            //剧本
            StoryManager.Export(dir);

            //人物
            RoleManager.Export(dir);

            //技能
            SkillManager.Export(dir);

            //物品
            ItemManager.Export(dir);

            //战斗
            BattleManager.Export(dir);

            MessageBox.Show("保存成功！");
        }

        private void ModifyObjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem item = ProjectTreeView.SelectedItem as TreeViewItem;
            if (item == null)
                return;
            if(item.Tag is string)
            {
                return;
            }
            if(item.Tag is Story)
            {
                StoryEditWindow win = new StoryEditWindow();
                win.BindData(item.Tag as Story);
                win.Show();
            }
            this.Refresh();
        }

        
        private void NewObjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem item = ProjectTreeView.SelectedItem as TreeViewItem;

            if (item == null || item.Tag == null || !(item.Tag is string))
            {
                MessageBox.Show("请先选定你要增加的类型");
                return;
            }
            string newObjType = item.Tag as string;
            switch(newObjType)
            {
                case "剧本":
                    Story story = new Story();
                    story.Name = "未命名剧本" + StoryManager.storys.Count.ToString();
                    StoryManager.storys.Add(story);
                    StoryEditWindow win = new StoryEditWindow();
                    win.BindData(story);
                    win.Show();
                    
                    break;
                default:
                    break;
            }
            this.Refresh();
        }

        private void DeleteObjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem item = ProjectTreeView.SelectedItem as TreeViewItem;
            if (item == null)
                return;
            if (item.Tag is string)
            {
                return;
            }
            
        	if( MessageBox.Show("确认要删除吗？", "确认", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (item.Tag is Story)
                {
                    Story story = item.Tag as Story;
                    StoryManager.storys.Remove(story);
                    this.Refresh();
                }    
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string path = Configer.Instance.ScriptRootMenu + "\\Scripts\\project.xml";
                GameProject.LoadGameProject(path);
                this.Refresh();
                CurrentProjectText.Text = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("游戏工程载入错误" + ex.ToString());
            }
        }
	}
}