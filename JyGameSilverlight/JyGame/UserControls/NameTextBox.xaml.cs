using System;
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
    public enum NameTextBoxType
    {
        NameBox,
        Danmu
    }

    public partial class NameTextBox : UserControl
	{
        
		public NameTextBox()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}
        public void Show(string title, NameTextBoxType type, string text, JyGame.GameData.CommonSettings.VoidCallBack callback,int maxLength = 8)
        {
            this.callback = callback;
            this.title.Text = title;
            this.Type = type;
            this.text.Text = text;
            this.Visibility = System.Windows.Visibility.Visible;
            this.text.MaxLength = maxLength;
        }

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            if (Type == NameTextBoxType.NameBox)
            {
                if (text.Text == "" || text.Text.Length > 8)
                {
                    MessageBox.Show("名字不能为空，或不能超过8个字");
                    return;
                }
                if(CommonSettings.IsBanWord(text.Text))
                {
                    MessageBox.Show("错误：名字中含有非法字符！");
                    return;
                }
                if (text.Text != "小虾米" && text.Text != "玲兰")
                {
                    foreach (var r in RoleManager.GetRoles())
                    {
                        if (text.Text == r.Name)
                        {
                            MessageBox.Show("错误，不能和已有NPC重名");
                            return;
                        }
                    }
                }
             
            }
            callback();
        }

        private JyGame.GameData.CommonSettings.VoidCallBack callback;
        private NameTextBoxType Type = NameTextBoxType.NameBox;

	}
}