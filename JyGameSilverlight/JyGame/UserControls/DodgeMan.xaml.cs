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

namespace JyGame.UserControls
{
    public partial class DodgeMan : UserControl
    {
        public DodgeMan()
        {
            InitializeComponent();
            this.Width = 80;
            this.Height = 80;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = ResourceManager.GetImage("头像.主角");
            brush.Stretch = Stretch.Fill;
            this.RedRect.Fill = brush;
        }

        public void changeHead(string roleKey)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = RoleManager.GetRole(roleKey).Head;
            brush.Stretch = Stretch.Fill;
            this.RedRect.Fill = brush;
        }

        public double X
        {
            get { return RedRectPoint.X; }
            set { RedRectPoint.X = value; }
        }
        public double Y
        {
            get { return RedRectPoint.Y; }
            set { RedRectPoint.Y = value; }
        }
    }
}
