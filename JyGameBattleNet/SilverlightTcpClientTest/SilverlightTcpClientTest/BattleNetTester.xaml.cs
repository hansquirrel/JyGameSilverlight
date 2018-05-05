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

namespace SilverlightTcpClientTest
{
	public partial class BattleNetTester : UserControl
	{
        public enum BattleNetStatus
        {
            NotLogin, //没登陆
            Normal, //在大厅中
            Ready, //游戏已经准备好，等待确认
            InGame, //游戏中
        }

		public BattleNetTester()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}


        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            bnStatus = BattleNetStatus.NotLogin;
        }

        BattleNetStatus bnStatus
        {
            get
            {
                return _bnStatus;
            }
            set
            {
                _bnStatus = value;
                switch (value)
                {
                    case BattleNetStatus.NotLogin:
                        hostStatusTextbox.Text = "未登录";
                        loginButton.IsEnabled = true;
                        logoutButton.IsEnabled = false;
                        refreshOnlineUsersButton.IsEnabled = false;
                        createGameButton.IsEnabled = false;
                        chatButton.IsEnabled = false;
                        refreshOnlineUsersButton.IsEnabled = false;
                        refreshSaveButton.IsEnabled = false;
                        userText.IsEnabled = true;
                        passwordText.IsEnabled = true;
                        saveButton.IsEnabled = false;
                        break;
                    case BattleNetStatus.Normal:
                        hostStatusTextbox.Text = "已登陆";
                        loginButton.IsEnabled = false;
                        logoutButton.IsEnabled = true;
                        refreshOnlineUsersButton.IsEnabled = true;
                        createGameButton.IsEnabled = true;
                        chatButton.IsEnabled = true;
                        refreshOnlineUsersButton.IsEnabled = true;
                        refreshSaveButton.IsEnabled = true;
                        userText.IsEnabled = false;
                        passwordText.IsEnabled = false;
                        saveButton.IsEnabled = true;
                        break;
                    case BattleNetStatus.Ready:
                        hostStatusTextbox.Text = "等待确认进入游戏";
                        break;
                    case BattleNetStatus.InGame:
                        hostStatusTextbox.Text = "游戏中";
                        break;
                    default:
                        break;
                }
            }
        }
        BattleNetStatus _bnStatus = BattleNetStatus.NotLogin;

        BattleNetManager bnManager = null;
        private void loginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (bnManager == null)
            {
                bnManager = new BattleNetManager("www.jy-x.com", 4502, this.Dispatcher);
                //bnManager = new BattleNetManager("127.0.0.1", 4502, this.Dispatcher);
                bnManager.OnNewChatNotify += (s, ee) =>
                {
                    TextBlock tb = new TextBlock() { Text = string.Format("[{0}]{1}:{2}", ee.Channel, ee.User.Name, ee.Message) };
                    chatBox.Items.Add(tb);
                    chatBox.ScrollIntoView(tb);
                };
                bnManager.OnDroppedNotify += (s, ee) =>
                {
                    MessageBox.Show("与服务器失去连接!");
                    bnStatus = BattleNetStatus.NotLogin;
                };
            }
            string username = userText.Text;
            string password = passwordText.Password;
            bnManager.Login(username, password, (b, timeout) => 
            {
                if (timeout)
                {
                    MessageBox.Show("登陆超时");
                }
                else if (b)
                {
                    MessageBox.Show("登陆成功");
                    bnStatus = BattleNetStatus.Normal;

                    string channel = "PRIVATE_" + username;
                    bnManager.JoinChannel(new string[] { "ALL", channel }, (isOk, to) =>
                    {
                        RefreshSave();
                        RefreshOnlineUser();
                    });
                }
                else
                {
                    MessageBox.Show("登陆失败，用户名或密码错误");
                    bnManager = null;
                }
            });
        }

        private void logoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bnManager.Logout();
            bnStatus = BattleNetStatus.NotLogin;
        }

        private void createGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBlock tb = onlineUserListbox.SelectedItem as TextBlock;
            if (tb == null) return;
            BattleNetUser user = tb.Tag as BattleNetUser;
            string channel = "PRIVATE_" + user.Name;
            string uuid = System.Guid.NewGuid().ToString();
            bnManager.Chat(channel, string.Format("can we play in channel:{0} ?",uuid));
        }

        private void chatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string info = chatTextBox.Text;
            string channel = channelText.Text;
            bnManager.Chat(channel, info);
        }

        private void refreshOnlineUsersButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshOnlineUser();
        }

        private void RefreshOnlineUser()
        {
            onlineUserListbox.Items.Clear();
            bnManager.GetOnlineUsers((users, timeout) =>
            {
                foreach (var u in users)
                {
                    onlineUserListbox.Items.Add(new TextBlock() { Text = string.Format("{0}/{1}", u.Name, u.Score), Tag = u });
                }
            });
        }

        private void clearChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            chatBox.Items.Clear();
        }

        private void refreshSaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshSave();
        }

        private void RefreshSave()
        {
            saveListBox.Items.Clear();
            bnManager.GetSaves((saves, timeout) =>
            {
                for (int i = 0; i < saves.Count; ++i)
                {
                    string save = saves[i];
                    TextBlock tb = new TextBlock() { Text = i.ToString() };
                    tb.Tag = new SaveItem() { index = i, content = save };
                    saveListBox.Items.Add(tb);
                }
            });
        }

        private void saveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (saveListBox.SelectedItem == null)
            {
                MessageBox.Show("请选择要保存的存档位");
                return;
            }
            SaveItem si = ((saveListBox.SelectedItem as TextBlock).Tag as SaveItem);
            int index = si.index;
            string content = saveContent.Text;
            
            bnManager.Save(index, content, (isOk, timeout) =>
            {
                if (isOk)
                {
                    MessageBox.Show("保存成功");
                    RefreshSave();
                }
                else
                    MessageBox.Show("保存失败");
            });
        }

        private void saveListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (saveListBox.SelectedItem == null) return;
            SaveItem si = ((saveListBox.SelectedItem as TextBlock).Tag as SaveItem);
            saveContent.Text = si.content;
        }

        private void chatTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                chatButton_Click(null,null);
                chatTextBox.Text = "";
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// 在此处添加事件处理程序实现。
            string win = "user1";
            string lose = "user2";
            string info = "1213";
            string uuid = "1111-1111-1111-111";
            bnManager.CommitBattleResult(uuid, win, lose, info, (isOk, timeout) =>
            {
                MessageBox.Show("提交成功");
            });
        }
	}

    class SaveItem
    {
        public int index;
        public string content;
    }
}