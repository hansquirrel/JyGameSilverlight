using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using JyGame.GameData;
namespace JyGame
{
	public partial class RoleListPanel : UserControl
	{
		public RoleListPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public UIHost uiHost = null;
        private Role CurrentRole
        {
            get
            {
                return _currentRole;
            }
            set
            {
                if(_currentRole!=null && _roleImageMap.ContainsKey(_currentRole))
                    _roleImageMap[_currentRole].Opacity = 0.5;
                this.rolePanel.Show(value,true);
                _currentRole = value;
                _roleImageMap[_currentRole].Opacity = 1;
            }
        }
        private Role _currentRole = null;
        private Dictionary<Role, Image> _roleImageMap = new Dictionary<Role, Image>();

        public void Refresh()
        {
            this.rolePanel.Refresh();
        }
        public void Show()
        {
            rolePanel.uiHost = this.uiHost;
            roleStackPanel.Children.Clear();
            _roleImageMap.Clear();
            teamLabel.Text = "当前队伍" + RuntimeData.Instance.Team.Count.ToString() + "人";
            foreach (var r in RuntimeData.Instance.Team)
            {
                Image img = new Image() { Source = r.Head, Width = 70, Height = 70, Tag = r, Opacity = 0.5 };
                _roleImageMap.Add(r, img);
                ToolTipService.SetToolTip(img, r.Name);
                roleStackPanel.Children.Add(img);
                img.MouseLeftButtonUp += (s, e) =>
                {
                    AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                    Image me = (s as Image);
                    this.CurrentRole = me.Tag as Role;
                };
            }
            this.Visibility = System.Windows.Visibility.Visible;

            this.CurrentRole = RuntimeData.Instance.Team[0];
        }

        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        public Role GetSelectRole()
        {
            return CurrentRole;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.rolePanel.HideUI();
        }
	}
}