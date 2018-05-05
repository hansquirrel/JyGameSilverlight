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

namespace JyGame
{
    public partial class StoryEditWindow : ChildWindow
    {


        public StoryEditWindow()
        {
            InitializeComponent();
            this.ResetView();

            this.Init();
        }

        private string[] ActionTypeList = GameData.Action.TypeList.ToArray();

        private void ComboSelect(string actionName)
        {
            foreach (var item in ActionTypeCombo.Items)
            {
                if (item.ToString() == actionName)
                {
                    ActionTypeCombo.SelectedItem = item;
                    return;
                }
            }
        }

        private void Init()
        {
            foreach(var a in ActionTypeList)
            {
                ActionTypeCombo.Items.Add(a);
            }
        }

        private void Refresh()
        {
            StoryNameTextBox.Text = story.Name;
            StoryListBox.Items.Clear();
            ResultListBox.Items.Clear();
            foreach (var a in story.Actions)
            {
                StoryListBox.Items.Add(a.ToString());
            }
            foreach (var r in story.Results)
            {
                ResultListBox.Items.Add(r.ToString());
            }
        }

        public void BindData(Story story)
        {
            this.story = story;
            Refresh();
        }

        private Story story = null;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ResetView()
        {
            DialogEditCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ActionEditCanvas.Visibility = System.Windows.Visibility.Collapsed;
            MusicEditCanvas.Visibility = System.Windows.Visibility.Collapsed;
            MapEditCanvas.Visibility = System.Windows.Visibility.Collapsed;
            SelectEditCanvas.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowDialogEditor(GameData.Action action)
        {
            DialogEditCanvas.Visibility = System.Windows.Visibility.Visible;
            this.ShowDialogPreview(action);
            if (action.Value != string.Empty)
            {
                DialogRoleNameTextBox.Text = action.Params[0];
                DialogInfoTextBox.Text = action.Params[1];
            }
            else
            {
                DialogRoleNameTextBox.Text = "";
                DialogInfoTextBox.Text = "";
            }
        }

        private void ShowActionEditor(GameData.Action action)
        {
            ActionEditCanvas.Visibility = System.Windows.Visibility.Visible;
            ActionValueTextBox.Text = action.Value;
        }

        private void ShowMusicEditor(GameData.Action action)
        {
            MusicEditCanvas.Visibility = System.Windows.Visibility.Visible;
            MusicValueTextBox.Text = action.Value;
            MusicActualPathText.Text = ResourceManager.Get(action.Value);
            
        }

        private void ShowMapEditor(GameData.Action action)
        {
            MapEditCanvas.Visibility = System.Windows.Visibility.Visible;
            MapValueTextBox.Text = action.Value;
            MapActualPathText.Text = ResourceManager.Get(action.Value);
            MapPreviewImage.Source = ResourceManager.GetImage(action.Value);
        }

        private void ShowSelectEditor(GameData.Action action)
        {
            string[] p = action.Value.Split(new char[] { '#' });
            SelectEditCanvas.Visibility = System.Windows.Visibility.Visible;
            if (p.Length <= 2)
            {
                //MessageBox.Show("SELECT脚本格式解析错误！");
                //this.ShowActionEditor(action);
                SelectEditTitle.Text = "";
                SelectRoleText.Text = "";
                SelectContent.Text = "";
                return;
            }
            string title = p[1];
            string role = p[0];
            string temp = string.Empty;
            for (int i = 2; i < p.Length;++i )
            {
                temp += p[i] + '\n';
            }
            SelectEditTitle.Text = title;
            SelectRoleText.Text = role;
            SelectContent.Text = temp;
        }

        private string GetSelectValue()
        {
            string title = SelectEditTitle.Text.Trim();
            string role = SelectRoleText.Text.Trim();
            string content = SelectContent.Text.Trim();
            List<string> list = new List<string>();
            foreach(var t in content.Split(new char[]{'\n','\r'}))
            {
                if(t.Trim() != string.Empty)
                    list.Add(t.Trim());
            }
            if (title == string.Empty || role == string.Empty || list.Count == 0)
                return null;
            string tmp = string.Empty;
            tmp += role + "#" + title;
            foreach (var t in list)
            {
                tmp += "#" + t;
            }
            return tmp;
        }

        private void ShowDialogPreview(GameData.Action action)
        {
            if (action.Params.Length == 2)
            {
                string[] paras = action.Value.Split(new char[] { '#' });
                string roleName = paras[0];
                string info = paras[1];
                info = info.Replace("$FEMALE$", "女主");
                info = info.Replace("$MALE$", "主角");
                this.Head.Source = RoleManager.GetRole(roleName).Head;

                this.Text.Text = info;
                if (roleName == "女主")
                    this.RoleName.Text = "女主" + ":";
                else if (roleName == "主角")
                {
                    this.RoleName.Text = "主角" + ":";
                    this.Head.Source = ResourceManager.GetImage("头像.主角");
                }
                else
                    this.RoleName.Text = RoleManager.GetRole(roleName).Name + ":";
            }
            else
            {
                this.RoleName.Text = "";
                this.Head.Source = null;
                this.Text.Text = "";
            }
        }

        private GameData.Action CurrentAction = null;

        private void StoryListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ResetView();
            if (StoryListBox.SelectedItem == null)
                return;
            int index = StoryListBox.SelectedIndex;
            
            if (index + 1 <= story.Actions.Count)
            {
                GameData.Action action = story.Actions[index];
                CurrentAction = action;
                ActionTypeText.Text = action.Type;
                if (action.Type == "DIALOG")
                {
                    this.ShowDialogEditor(action);
                }else if(action.Type == "MUSIC")
                {
                    this.ShowMusicEditor(action);
                }else if(action.Type == "BACKGROUND")
                {
                    this.ShowMapEditor(action);
                }else if(action.Type == "SELECT")
                {
                    this.ShowSelectEditor(action);
                }
                else //否则使用默认编辑器
                {
                    this.ShowActionEditor(action);
                }
            }
        }

        private void CommitActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CurrentAction == null) return;
            string CurrentType = ActionTypeText.Text;
            switch(CurrentType)
            {
                case "DIALOG":
                    string roleName = DialogRoleNameTextBox.Text;
                    string dialogInfo = DialogInfoTextBox.Text;
                    CurrentAction.Value = string.Format("{0}#{1}", roleName, dialogInfo);
                    Refresh();
                    break;
                case "MUSIC":
                    CurrentAction.Value = MusicValueTextBox.Text;
                    Refresh();
                    break;
                case "BACKGROUND":
                    CurrentAction.Value = MapValueTextBox.Text;
                    Refresh();
                    break;
                case "SELECT":
                    string tmp = this.GetSelectValue();
                    if (tmp == null)
                    {
                        MessageBox.Show("SELECT脚本保存格式错误！");
                    }
                    else
                    {
                        CurrentAction.Value = tmp;
                    }
                    Refresh();
                    break;
                default:
                    string value = ActionValueTextBox.Text;
                    CurrentAction.Value = value;
                    Refresh();
                    break;
            }
        }

        private void NewActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(ActionTypeCombo.SelectedItem == null)
            {
                MessageBox.Show("请先选择要增加的剧本类型");
                return;
            }
            string type = ActionTypeCombo.SelectedItem.ToString();
            GameData.Action newAction = new GameData.Action() { Type = type, Value = "" };
            if (StoryListBox.SelectedItem != null)
            {
                int index = StoryListBox.SelectedIndex;
                story.Actions.Insert(index + 1,newAction);
                this.Refresh();
                StoryListBox.SelectedIndex = index + 1;
            }
            else
            {
                story.Actions.Add(newAction);
                this.Refresh();
                StoryListBox.SelectedIndex = story.Actions.Count - 1;
            }
        }

        private void UpActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StoryListBox.SelectedItem == null)
            {
                return;
            }
            int index = StoryListBox.SelectedIndex;
            if (index == 0) return;
            story.Actions.Reverse(index - 1, 2);
            this.Refresh();
            this.StoryListBox.SelectedIndex = index - 1;
        }

        private void DownActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StoryListBox.SelectedItem == null)
            {
                return;
            }
            int index = StoryListBox.SelectedIndex;
            if (index == story.Actions.Count - 1)
                return;
            story.Actions.Reverse(index, 2);
            this.Refresh();
            this.StoryListBox.SelectedIndex = index + 1;
        }

        private void DeleteActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StoryListBox.SelectedItem == null)
            {
                return;
            }
            int index = StoryListBox.SelectedIndex;
            story.Actions.RemoveAt(index);
            this.Refresh();
        }

        private void CancelActionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void DebugButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NextGameState next = new NextGameState();
            next.Type = "story";
            next.Value = story.Name;
            if (RuntimeData.Instance.gameEngine != null)
            {
                RuntimeData.Instance.gameEngine.CallScence(RuntimeData.Instance.gameEngine.uihost.mapUI, next);

                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("启动调试错误！你必须启动游戏，并新建或载入存档。");
            }
        }

        private void MusicSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string music = MusicValueTextBox.Text;
            MusicSelectWindow win = new MusicSelectWindow();
            win.BindMusic(music);
            win.Show();
            win.Closed += (ss, ee) =>
            {
                if (win.DialogResult != null && (bool)win.DialogResult == true)
                {
                    MusicValueTextBox.Text = win.Music;
                    MusicActualPathText.Text = ResourceManager.Get(win.Music);
                }
            };
        }

        private void ListenButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string music = MusicValueTextBox.Text;
            if(ResourceManager.Get(music) == null)
            {
                MessageBox.Show("错误，音乐不存在");
                return;
            }
            AudioManager.PlayMusic(ResourceManager.Get(music));
        }

        private void MapSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string map = MapValueTextBox.Text;
            ImageSelectWindow win = new ImageSelectWindow();
            List<string> maps = ResourceManager.GetResourceKeyStartsWith("地图.");
            win.BindData(maps, map);
            win.Show();

            win.Closed += (ss, ee) =>
                {
                    if (win.DialogResult != null && (bool)win.DialogResult == true)
                    {
                        MapValueTextBox.Text = win.Image;
                        MapActualPathText.Text = ResourceManager.Get(win.Image);
                        MapPreviewImage.Source = ResourceManager.GetImage(win.Image);
                    }
                };
        }

        private void ResultEditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StoryResultEditWindow win = new StoryResultEditWindow();

            win.Results = story.Results;
            win.Show();
            win.Closed += (ss, ee) =>
            {
                if(win.Results != null && win.Results.Count > 0 && win.DialogResult == true)
                {
                    story.Results = win.Results;
                    this.Refresh();
                }
            };
        }

        private void StoryNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            story.Name = StoryNameTextBox.Text;
            NameConflictText.Visibility = Visibility.Collapsed;
            foreach (var s in StoryManager.storys)
            {
                if(story != s && s.Name == story.Name)
                {
                    NameConflictText.Visibility = Visibility.Visible;
                    return;
                }
            }
        }
    }
}

