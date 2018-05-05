using JyGame.GameData;
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

namespace JyGame
{
	public partial class SkillHotKeysPanel : UserControl
	{
		public SkillHotKeysPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}
        public bool IsActive = false;

        public CommonSettings.ObjectCallBack Callback = null;

        private const string _specialHotKeys = "1234567890";
        private const string _hotKeys = "QWERTASDFGZXCVB";

        private Dictionary<char, SkillBox> _mapping = new Dictionary<char, SkillBox>();

        public void Hide()
        {
            IsActive = false;
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        public void Show(Role r)
        {
            IsActive = true;
            _mapping.Clear();
            this.HotKeyPanel.Children.Clear();
            this.SpecialSkillHotKeyPanel.Children.Clear();

            int skillIndex = 0;
            int spIndex = 0;
            foreach(var s in r.GetAvaliableSkills())
            {
                if(s.Instance != null || s.IsInternalUnique)
                {
                    if (s.Status == SkillStatus.Ok && skillIndex < _hotKeys.Length - 1)
                    {
                        this.HotKeyPanel.Children.Add(SkillHotKeysItem.Create(s, _hotKeys[skillIndex], this));
                        _mapping[_hotKeys[skillIndex]] = s;
                    }
                    skillIndex++;
                }else if(s.SpecialSkill != null && spIndex < _specialHotKeys.Length - 1)
                {
                    if (s.Status == SkillStatus.Ok)
                    {
                        this.SpecialSkillHotKeyPanel.Children.Add(SkillHotKeysItem.Create(s, _specialHotKeys[spIndex], this));
                        _mapping[_specialHotKeys[spIndex]] = s;
                    }
                    spIndex++;
                }
            }

            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void SelectSkill(Key key)
        {
            char k = (key.ToString()[key.ToString().Length - 1]);
            if (_mapping.ContainsKey(k))
            {
                SkillBox s = _mapping[k];
                if (Callback != null)
                    Callback(s);
            }
        }
	}
}