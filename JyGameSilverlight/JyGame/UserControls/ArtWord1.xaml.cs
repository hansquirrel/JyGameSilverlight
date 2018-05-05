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
using Effects.Shader;

namespace JyGame.UserControls
{
    public partial class ArtWord1 : UserControl
    {
        public ArtWord1(string text)
        {
            InitializeComponent();

            artWordText.Text = text;

            this.Visibility = Visibility.Visible;
        }
    }
}
