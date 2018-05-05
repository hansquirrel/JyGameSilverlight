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
using System.Windows.Media.Imaging;
using JyGame.GameData;
using System.Windows.Threading;
using JyGame.Logic;

namespace JyGame.UserControls
{
    public partial class SceneHead : UserControl
    {
        public string roleKey;
        public string storydialogs;
        public string type;
        public string description;
        public int level;

        public SceneHead() 
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }


        public SceneHead(string roleKey, string pic, string storydialogs, string type, string description,int level,bool showTanhao = false)
        {
            InitializeComponent();
            this.level = level;
            if(showTanhao)
            {
                TanHaoStory.AutoReverse = true;
                TanHaoStory.RepeatBehavior = RepeatBehavior.Forever;
                TanHaoStory.Begin();
            }
            else
            {
                tanHaoImage.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.roleKey = roleKey;
            this.storydialogs = storydialogs;
            this.type = type;
            this.description = description;

            if (RoleManager.GetRole(roleKey) != null)
            {
                if (roleKey == "主角")
                    head.Source = RuntimeData.Instance.Team[0].Head;
                else
                    head.Source = RoleManager.GetRole(roleKey).Head;
            }
            else
                head.Source = ResourceManager.GetImage(pic);
            head.Stretch = Stretch.Uniform;
            this.MouseLeftButtonUp += RoleButton_Click;
        }

        private void RoleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NextGameState next = new NextGameState();
            next.Value = storydialogs;
            next.Type = type;

            RuntimeData.Instance.gameEngine.CallScence(null, next);
        }

        
    }
}
