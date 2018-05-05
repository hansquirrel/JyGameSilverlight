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

using JyGame.UserControls;
using JyGame.GameData;
using System.Windows.Threading;

using JyGame.BattleNet;
using System.Reflection;
using System.IO.IsolatedStorage;
using System.Globalization;

namespace JyGame
{
    public partial class MainPage : UserControl
    {
        private void TouchHostPage()
        {
            //用于统计计数
            WebClient webClient = new WebClient();
            Uri versionUrl = new Uri("http://www.jy-x.com/0.5/version.xml", UriKind.Absolute);
            webClient.OpenReadAsync(versionUrl);
            webClient.OpenReadCompleted += (s, e) =>
                {
                    if (e.Error == null && e.Result != null)
                    {
                        byte[] result = new byte[e.Result.Length];
                        e.Result.Read(result, 0, (int)(e.Result.Length));
                        MessageBox.Show(result.ToString());
                    }
                };
        }

        public void GameInit()
        {
            //if (Configer.Instance.Debug)
            //{
            //    Application.Current.Host.Settings.EnableFrameRateCounter = true;
            //}

            //this.TouchHostPage();

            if (!GameProject.IsLoaded)
                GameProject.LoadGameProject();

            GameServerManager.Instance.Init();
            BattleNetManager.Instance.SetConnectArgs("www.jy-x.com", 4502, this.Dispatcher);
            //BattleNetManager.Instance.SetConnectArgs("211.100.49.136", 4502, this.Dispatcher);
            //BattleNetManager.Instance.SetConnectArgs("127.0.0.1", 4502, this.Dispatcher);
            if (!Configer.Instance.Debug)
            {
                uiHost.battleFieldContainer.debugPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                uiHost.battleFieldContainer.debugPanel.Visibility = System.Windows.Visibility.Visible;
            }

            string rAssembly = Assembly.GetExecutingAssembly().FullName.Split(',')[1].Split('=')[1];
            uiHost.mainMenu.versionText.Text = rAssembly;
            uiHost.VersionInfoText.Text = "金X " + rAssembly;

            height = this.LayoutRoot.Height;
            width = this.LayoutRoot.Width;
            //Application.Current.Host.Content.Resized += new EventHandler(Content_Resized);
            Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_Resized);
        }

        void Content_Resized(object sender, EventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen) 
            { 
                double currentWidth = Application.Current.Host.Content.ActualWidth;
                double currentHeight = Application.Current.Host.Content.ActualHeight;
                double uniformScaleAmount = Math.Min((currentWidth / width), (currentHeight / height));
                //RootLayoutScaleTransform.ScaleX = uniformScaleAmount;
                //RootLayoutScaleTransform.ScaleY = uniformScaleAmount;
                RootLayoutScaleTransform.ScaleX  = currentWidth / width;
                RootLayoutScaleTransform.ScaleY = currentHeight / height;
                LayoutRoot.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                LayoutRoot.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                uiHost.FullScreenButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RootLayoutScaleTransform.ScaleX = 1;
                RootLayoutScaleTransform.ScaleY = 1;
                LayoutRoot.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                LayoutRoot.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                uiHost.FullScreenButton.Visibility = Visibility.Visible;
            }
            
        }

        double height;
        double width;

        public MainPage()
        {
            Configer.Instance.Init();
            InitializeComponent();
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space && uiHost.dialogPanel.CallBack != null && uiHost.dialogPanel.Visibility == System.Windows.Visibility.Visible)
            {
                uiHost.dialogPanel.CallBack(0);
            }
            else if (e.Key == Key.Space || e.Key==Key.Enter)
            {
                e.Handled = true;
            }
            else if (uiHost.battleFieldContainer.skillHotKeysPanel.IsActive)
            {
                uiHost.battleFieldContainer.skillHotKeysPanel.SelectSkill(e.Key);
            }
            if(e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.GameInit();
            RuntimeData.Instance.gameEngine = new GameEngine(this.uiHost);
            uiHost.mainMenu.Load();
        }
    }
}
