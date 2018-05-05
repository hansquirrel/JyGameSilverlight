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
    public partial class BuyItemWindow : ChildWindow
    {
        public BuyItemWindow()
        {
            InitializeComponent();
            
        }

        public void Bind(Item item, string title,CommonSettings.IntCallBack callback, int max = -1)
        {
            ItemImage.Source = item.Pic;
            ItemDetail.Content = item.GenerateTooltip();
            NumberText.Text = "1";
            this.callback = callback;
            this.MaxLimit = max;

            if(max > 0)
            {
                this.NumberLimitText.Text = max.ToString();
            }
        }

        private int MaxLimit = -1;
        private CommonSettings.IntCallBack callback;
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            int number = GetNumber;
            if ( number <= 0)
            {
                return;
            }else if(number > MaxLimit && MaxLimit != -1)
            {
                MessageBox.Show("错误，超出了数量上限");
                return;
            }
            else
            {
                callback(number);
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void MiusButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int number = int.Parse(NumberText.Text) - 1;
            if(number <0 ) number = 0;
            NumberText.Text = (number).ToString();
        }

        private void PlusButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int number = int.Parse(NumberText.Text) + 1;
            if (number > MaxLimit && MaxLimit != -1) number = MaxLimit;
            NumberText.Text = (number).ToString();
        }

        private int GetNumber
        {
            get
            {
                try
                {
                    return int.Parse(NumberText.Text);
                }catch(Exception e)
                {
                    MessageBox.Show("输入数量格式错误");
                    return 0;
                }
            }
        }
    }
}

