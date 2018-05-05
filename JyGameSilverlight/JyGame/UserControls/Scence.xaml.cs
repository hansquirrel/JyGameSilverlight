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
using JyGame.Interface;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public partial class Scence : UserControl, IScence
    {
        //private List<Spirit> Spirits = new List<Spirit>();
        public List<SceneHead> heads = new List<SceneHead>();

        public Scence()
        {
            InitializeComponent();
        }

        //by cg 2013-10-26
        public void SetRoles(List<string> roles)
        {
            HideHeads();
            int index = 0;
            foreach (var role in roles)
            {
                if (index >= 5) //最多显示5个
                    break;
                SceneHead sceneHead = new SceneHead(role, "", "original_nothing", "story", "这是" + "【" + role + "】", 0);
                sceneHead.head.Width = CommonSettings.MAPUI_ROLEHEAD_WIDTH;
                sceneHead.head.Height = CommonSettings.MAPUI_ROLEHEAD_HEIGHT;
                sceneHead.Margin = new Thickness(CommonSettings.MAPUI_ROLEHEAD_X, CommonSettings.MAPUI_ROLEHEAD_Y + index * (CommonSettings.MAPUI_ROLEHEAD_HEIGHT + CommonSettings.MAPUI_ROLEHEAD_GAP), 0, 0);
                Canvas.SetZIndex(sceneHead, CommonSettings.Z_MAPUI_ROLEHEAD);
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = ResourceManager.GetImage("ui.头像框.普通");
                brush.Stretch = Stretch.Uniform;
                sceneHead.LayoutRoot.Background = brush;

                uiHost.LayoutRoot.Children.Add(sceneHead);
                heads.Add(sceneHead);
                index++;
            }
        }

        public void HideHeads()
        {
            if (heads.Count > 0)
            {
                foreach (var head in heads)
                {
                    uiHost.LayoutRoot.Children.Remove(head);
                }
                heads.Clear();
            }
        }

        public void SetBackground(string background)
        {
            if (background == "BLACK")
            {
                this.backgroundCanvas.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.backgroundCanvas.Background = new ImageBrush()
                {
                    ImageSource = Tools.GetImage(ResourceManager.Get(background)),
                    Stretch = Stretch.Fill,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center,
                    Opacity = CommonSettings.timeOpacity[RuntimeData.Instance.Date.Hour / 2]
                };
            }
            this.Visibility = Visibility.Visible;
            //this.FadeOut.Stop();
            //this.IsEnabled = false;
            //this.FadeIn.Begin();
            //this.FadeIn.Completed += new EventHandler(FadeIn_Completed);
            this.IsEnabled = true;
        }
        public void SetBackground(ImageSource background)
        {
            this.backgroundCanvas.Background = new ImageBrush()
            {
                ImageSource = background,
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
                Opacity = CommonSettings.timeOpacity[RuntimeData.Instance.Date.Hour / 2]
            };
            this.Visibility = Visibility.Visible;
            //this.FadeOut.Stop();
            //this.IsEnabled = false;
            //this.FadeIn.Begin();
            //this.FadeIn.Completed += new EventHandler(FadeIn_Completed);
            this.IsEnabled = true;
        }
        //end by cg 2013-10-26

        public void Load(string scenceName)
        {
            this.Visibility = Visibility.Visible;
            currentScenario = scenceName;
            string battleKey = DialogManager.GetDialogsMapKey(scenceName);
            Battle battle = BattleManager.GetBattle(battleKey);
            LoadMap(battle.templateKey, battle);

            string scenceType = DialogManager.GetDialogsType(scenceName);
            //类型为普通对话（无流程控制）
            //if (scenceType == "DIALOGS")
            //{
            //    uiHost.dialogPanel.ShowDialogs(DialogManager.GetDialogList(scenceName));
            //    uiHost.dialogPanel.CallBack = (selected) =>
            //    {
            //        string result = selected.ToString();
            //        RuntimeData.Instance.KeyValues[currentScenario] = result;
            //        RuntimeData.Instance.gameEngine.CallScence(
            //            this, ScenarioManager.getNextScenario(currentScenario, result));
            //    };
            //}
            //类型为带选择的（对话结尾处有流程控制）
            /*
            if (scenceType == "DIALOGS_OPTION")
            {
                uiHost.selectPanel.CallBack = () =>
                {
                    RuntimeData.Instance.KeyValues[currentScenario] = uiHost.selectPanel.currentSelection;
                    RuntimeData.Instance.gameEngine.CallScence(
                        this, ScenarioManager.getNextScenario(currentScenario, uiHost.selectPanel.currentSelection));
                };

                uiHost.dialogPanel.CallBack = () =>
                {
                    uiHost.dialogPanel.Visibility = Visibility.Visible;
                    uiHost.selectPanel.title.Text = "选项";
                    uiHost.selectPanel.yes.Content = "是";
                    uiHost.selectPanel.no.Content = "否";
                    uiHost.selectPanel.ShowSelection();
                };

                uiHost.dialogPanel.ShowDialogs(DialogManager.GetDialogList(scenceName));
            }
             */
            //this.FadeOut.Stop();
            //this.IsEnabled = false;
            //this.FadeIn.Begin();
            //this.FadeIn.Completed += new EventHandler(FadeIn_Completed);
            this.IsEnabled = true;
        }

        void FadeIn_Completed(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }

        private void LoadMap(string key, Battle battle)
        {
            MapTemplate mapTemplate = BattleManager.GetMapTemplate(key);

            /*if (Spirits.Count > 0)
            {
                foreach (var sp in Spirits)
                {
                    this.backgroundCanvas.Children.Remove(sp);
                }
                Spirits.Clear();
            }*/
            if (heads.Count > 0)
            {
                foreach (var head in heads)
                {
                    this.backgroundCanvas.Children.Remove(head);
                }
                heads.Clear();
            }

            //纯图，没人物
            if (battle.battleRoles.Count == 0)
            {
                this.backgroundCanvas.Background = new ImageBrush() { ImageSource = mapTemplate.Background, Stretch = Stretch.Uniform, AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Opacity = CommonSettings.timeOpacity[RuntimeData.Instance.Date.Hour / 2] };
            }
            //图+人物
            else
            {
                this.backgroundCanvas.Background = new ImageBrush() { ImageSource = mapTemplate.Background, Stretch = Stretch.Uniform, AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, Opacity = CommonSettings.timeOpacity[RuntimeData.Instance.Date.Hour / 2] };

                int index = 0;
                foreach (var role in battle.battleRoles)
                {
                    SceneHead sceneHead = new SceneHead(role.roleKey, "", "original_nothing", "story", "这是" + "【" + role.roleKey +"】", 0);
                    sceneHead.head.Width = CommonSettings.MAPUI_ROLEHEAD_WIDTH;
                    sceneHead.head.Height = CommonSettings.MAPUI_ROLEHEAD_HEIGHT;
                    sceneHead.Margin = new Thickness(CommonSettings.MAPUI_ROLEHEAD_X, CommonSettings.MAPUI_ROLEHEAD_Y + index * (CommonSettings.MAPUI_ROLEHEAD_HEIGHT + CommonSettings.MAPUI_ROLEHEAD_GAP), 0, 0);
                    Canvas.SetZIndex(sceneHead, CommonSettings.Z_MAPUI_ROLEHEAD);
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = ResourceManager.GetImage("ui.头像框.普通");
                    brush.Stretch = Stretch.Uniform;
                    sceneHead.LayoutRoot.Background = brush;

                    this.backgroundCanvas.Children.Add(sceneHead);
                    heads.Add(sceneHead);
                    index++;
                }
            }

            AudioManager.PlayMusic(mapTemplate.Music);
        }

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            //this.FadeIn.Stop();
            //this.IsEnabled = false;
            //this.FadeOut.Begin();
            //this.FadeOut.Completed += new EventHandler(FadeOut_Completed);
        }

        void FadeOut_Completed(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            this.IsEnabled = true;
        }

        private string currentScenario = string.Empty;
        public UIHost uiHost;
    }
}
