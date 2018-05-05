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
	public partial class ArenaSelectRole : UserControl
	{
        public CommonSettings.VoidCallBack confirmBack = null;
        public CommonSettings.VoidCallBack cancelBack = null;

        public int maxFriendNo = 0;
        public List<int> selectedFriends = new List<int>();
        public List<Role> selectedMyFriends
        {
            get
            {
                List<Role> roles = new List<Role>();
                foreach(int index in selectedFriends)
                {
                    roles.Add(RuntimeData.Instance.Team[index]);
                }
                return roles;
            }
        }

        public UIHost uiHost = null;

		public ArenaSelectRole()
		{
			InitializeComponent();
		}

        public void load(int maxFriend, string[] musts)
        {
            this.maxFriendNo = maxFriend;

            Init(musts);

            this.Visibility = Visibility.Visible;
        }

        public void Init(string[] mustsRoles)
        {
            List<string> musts = new List<string>();
            if (mustsRoles != null && mustsRoles.Length > 0)
            {
                foreach (var r in mustsRoles) 
                    musts.Add(r);
            }
            arenaRolePanel.layoutRoot.Background = null;
            arenaRolePanel.closeButton.Visibility = Visibility.Collapsed;
            arenaRolePanel.AllowDrop = false;
            arenaRolePanel.Visibility = Visibility.Collapsed;

            int count = 0;
            friendTeam.Children.Clear();
            selectedFriends.Clear();

            StackPanel currentFriendPanel = null;

            maxFriendNoText.Text = maxFriendNo.ToString() + "人";
            friendNoText.Text = " 人";
            for (int i = 0; i < RuntimeData.Instance.Team.Count; i++ )
            {
                if (count >= 5) count = 0;

                if (count == 0)
                {
                    currentFriendPanel = new StackPanel();
                    currentFriendPanel.Orientation = Orientation.Horizontal;
                    currentFriendPanel.Height = 55;
                    currentFriendPanel.Margin = new Thickness(2, 2, 2, 0);
                    friendTeam.Children.Add(currentFriendPanel);
                }

                string roleKey = RuntimeData.Instance.Team[i].Key;

                Border bc = new Border();
                bc.Width = 50; bc.Height = 50;
                bc.BorderThickness = new Thickness(0);
                bc.BorderBrush = new SolidColorBrush(Colors.White);
                Canvas c = new Canvas();
                c.Width = bc.Width - 5; c.Height = bc.Height - 5;

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = RuntimeData.Instance.Team[i].Head;
                brush.Opacity = 0.5;
                brush.Stretch = Stretch.Uniform;
                c.Background = brush;

                c.Margin = new Thickness(2, 2, 2, 5);
                c.Tag = i;

                //必须出场的人物
                if (musts.Contains(roleKey))
                {
                    bc.BorderThickness = new Thickness(1);
                    bc.BorderBrush = new SolidColorBrush(Colors.Red);
                    selectedFriends.Add((int)(c.Tag));
                    c.Background.Opacity = 1.0;
                    friendNoText.Text = selectedFriends.Count.ToString() + "人";
                }

                c.MouseLeftButtonUp += (s, e) =>
                {
                    //this.Callback(tmp);
                    if (!selectedFriends.Contains((int)(c.Tag)))
                    {
                        if (selectedFriends.Count < maxFriendNo)
                        {
                            bc.BorderThickness = new Thickness(1);
                            selectedFriends.Add((int)(c.Tag));
                        }
                    }
                    else if (selectedFriends.Contains((int)(c.Tag)) && !musts.Contains(RuntimeData.Instance.Team[(int)(c.Tag)].Key))
                    {
                        bc.BorderThickness = new Thickness(0);
                        selectedFriends.Remove((int)(c.Tag));
                    }
                    friendNoText.Text = selectedFriends.Count.ToString() + "人";
                };
                c.MouseEnter += (s, e) =>
                {
                    //this.ItemInfo.Text = info;
                    c.Background.Opacity = 1.0;
                    arenaRolePanel.Show(RuntimeData.Instance.Team[(int)(c.Tag)]);
                };
                c.MouseLeave += (s, e) =>
                {
                    //this.ItemInfo.Text = "";
                    if (!selectedFriends.Contains((int)c.Tag))
                    {
                        c.Background.Opacity = 0.5;
                    }
                };

                bc.Child = c;
                currentFriendPanel.Children.Add(bc);
                count++;
            }
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFriends.Count == 0)
            {
                MessageBox.Show("至少需要选择一个参战角色");
                return;
            }
            this.Visibility = Visibility.Collapsed;
            confirmBack();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            cancelBack();
        }


	}
}