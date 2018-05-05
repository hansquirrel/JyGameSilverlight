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
    public partial class ImageSelectWindow : ChildWindow
    {
        public ImageSelectWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void BindData(List<string> imageResources, string currentImage)
        {
            ImageResources = imageResources;
            ImageListBox.Items.Clear();
            foreach(var img in imageResources)
            {
                ImageSelectItem c = new ImageSelectItem();
                c.Image.Source = ResourceManager.GetImage(img);
                c.InfoText.Text = string.Format("{0}({1})", img, ResourceManager.Get(img));
                c.Path = img;
                ListBoxItem item = new ListBoxItem() { Content = c };
                ImageListBox.Items.Add(item);
                if(img.Equals(currentImage))
                {
                    ImageListBox.SelectedItem = item;
                }
            }
        }

        List<string> ImageResources = null;

        private void SearchText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = SearchText.Text.Trim();
            foreach (ListBoxItem c in ImageListBox.Items)
            {
                ImageSelectItem d = c.Content as ImageSelectItem;
                if(d.InfoText.Text.Contains(keyword))
                {
                    c.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    c.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            ImageListBox.UpdateLayout();
        }

        private void ImageListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Image = ((ImageListBox.SelectedItem as ListBoxItem).Content as ImageSelectItem).Path.ToString();
        }

        public string Image = "";
    }
}

