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
	public partial class ItemUnit : UserControl
	{
		public ItemUnit()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void SetColor(Color r)
        {
            this.BianKuang.Stroke = new SolidColorBrush(r);
        }

        public void Reset()
        {
            count.Text = "";
            name.Text = "";
            this.BianKuang.Stroke = new SolidColorBrush(Colors.LightGray);
            this.image.Source = null;
            ToolTipService.SetToolTip(this, null);
        }

        public void BindItem(Item item)
        {
            if (item == null)
            {
                Reset();
                return;
            }
            this.image.Source = item.Pic;
            string name = item.Name;
            if(name.Length>4)
            {
                name = name.Substring(0, 3) + "..";
            }
            this.name.Text = name;
            this.SetColor(item.GetColor());

            ToolTipService.SetToolTip(this, item.GenerateTooltip());
        }

        public string ItemName
        {
            set
            {
                if (value.Length > 4)
                {
                    name.Text = value.Substring(0, 3) + "..";
                }
                else
                {
                    name.Text = value;
                }
            }
        }

        public int ItemCount
        {
            set
            {
                if (value > 1)
                {
                    count.Text = value.ToString();
                }
                else
                {
                    count.Text = "";
                }
            }
        }
	}
}