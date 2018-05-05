using JyGame.GameData;
using System;
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
	public partial class SkillHotKeysItem : UserControl
	{
		public SkillHotKeysItem()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}
        
        public static SkillHotKeysItem Create(SkillBox box, char hotKey, SkillHotKeysPanel parent)
        {
            SkillHotKeysItem rst = new SkillHotKeysItem();
            ToolTipService.SetToolTip(rst, box.GenerateToolTip());
            if (box.Name.Length > 14)
            {
                rst.NameText.Text = box.Name.Substring(0, 3) + "...";
            }
            else
            {
                rst.NameText.Text = box.Name;
            }
            rst.HotKeyText.Text = hotKey.ToString();
            if(box.Instance != null && !box.IsUnique) //外功
            {                
                switch(box.Instance.Skill.Type)
                {
                    case CommonSettings.SKILLTYPE_JIAN:
                        rst.TypeImageJian.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_DAO:
                        rst.TypeImageDao.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_QUAN:
                        rst.TypeImageQuan.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_QIMEN:
                        rst.TypeImageQimen.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }else if(box.IsUnique) //绝技
            {
                rst.NameText.Foreground = new SolidColorBrush(Colors.Red);
                switch (box.Type)
                {
                    case CommonSettings.SKILLTYPE_JIAN:
                        rst.TypeImageJian.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_DAO:
                        rst.TypeImageDao.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_QUAN:
                        rst.TypeImageQuan.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_QIMEN:
                        rst.TypeImageQimen.Visibility = Visibility.Visible;
                        break;
                    case CommonSettings.SKILLTYPE_NEIGONG:
                        rst.TypeImageNeigong.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            else if(box.IsSpecial)
            {
                rst.NameText.Foreground = new SolidColorBrush(Colors.Cyan);
                rst.TypeImageSpecial.Visibility = Visibility.Visible;
            }

            rst.MouseLeftButtonDown += (ss, ee) =>
            {
                parent.Callback(box);
            };

            return rst;
        }
	}
}