using JyGame.GameData;
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

namespace JyGame
{
    public partial class MusicSelectWindow : ChildWindow
    {
        public MusicSelectWindow()
        {
            InitializeComponent();
        }

        public void BindMusic(string music)
        {
            MusicListBox.Items.Clear();
            int index = 0;
            foreach (var r in ResourceManager.ResourceMap)
            {
                if (r.Key.StartsWith("音乐."))
                {
                    MusicListBox.Items.Add(new ListBoxItem() { Content = r.Key + "(" + ResourceManager.Get(r.Key) + ")" , Tag = r.Key });
                    if (r.Key == music)
                        MusicListBox.SelectedIndex = index;
                    index++;
                }
            }
            
            Music = music;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ChildWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
           
        }

        private void MusicListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string music = (MusicListBox.SelectedItem as ListBoxItem).Tag.ToString();
            AudioManager.PlayMusic(ResourceManager.Get(music));
            Music = music;
        }

        public string Music = "";

        private void SearchText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = (sender as TextBox).Text.Trim();
            foreach (ListBoxItem item in MusicListBox.Items)
            {
                if(item.Content.ToString().Contains(keyword))
                {
                    item.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    item.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            MusicListBox.UpdateLayout();
        }
    }
}

