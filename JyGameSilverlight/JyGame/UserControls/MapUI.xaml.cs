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
using System.Windows.Threading;
using System.Collections.ObjectModel;
using JyGame.Interface;
using System.Windows.Browser;

namespace JyGame.UserControls
{
    public partial class MapUI : UserControl, IScence
    {
        public BigMap currentMap = null;

        public bool isOver = false;
        public string nextScenario = "";
        public bool isDialogOver = false;

        public Dictionary<string, string> locationDescription = new Dictionary<string, string>();
        public Dictionary<string, Event> locationEvents = new Dictionary<string, Event>();
        public Dictionary<string, int> locationLv = new Dictionary<string, int>();

        public List<MapEvent> locationButtons = new List<MapEvent>();
        public List<SceneHead> heads = new List<SceneHead>();

        public UIHost uiHost;

        public double currentPosTop = 0.0;
        public double currentPosLeft = 0.0;

        #region 初始化

        public MapUI()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            info.Visibility = System.Windows.Visibility.Collapsed;
            Canvas.SetZIndex(mapPointer, CommonSettings.Z_MAPUI_MAPPOINTER);
            Canvas.SetZIndex(info, CommonSettings.Z_MAPUI_INFO);
            Image[] buttonList = new Image[] { ButtonRizhi, ButtonXitong, ButtonWupin, ButtonZhuangtai, ButtonGonglue, ButtonDonate };
            foreach (var btn in buttonList)
            {
                btn.MouseEnter += (ss, ee) =>
                {
                    Image img = ss as Image;
                    Canvas.SetTop(img, Canvas.GetTop(img) - 15);
                };
                btn.MouseLeave += (ss, ee) =>
                {
                    Image img = ss as Image;
                    Canvas.SetTop(img, Canvas.GetTop(img) + 15);
                };
                btn.MouseLeftButtonUp += (ss, ee) =>
                {
                    AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                    Image img = ss as Image;
                    if (img == ButtonRizhi)
                    {
                        this.LogButton_Click(null, null);
                    }
                    else if (img == ButtonXitong)
                    {
                        this.EnvButton_Click(null, null);
                    }
                    else if (img == ButtonWupin)
                    {
                        this.ItemsButton_Click(null, null);
                    }
                    else if (img == ButtonZhuangtai)
                    {
                        this.RoleButton_Click(null, null);
                    }
                    else if (img == ButtonGonglue)
                    {
                        HtmlPage.Window.Navigate(new Uri(CommonSettings.GonglueUrl, UriKind.RelativeOrAbsolute), "_blank");
                    }
                    else if (img == ButtonDonate)
                    {
                        showDonate();
                    }
                };
            }
        }

        public void resetTeam()
        {
            foreach (Role role in RuntimeData.Instance.Team)
            {
                role.Reset();
            }
        }

        public void resetHead()
        {
            foreach (SceneHead head in heads)
            {
                this.LayoutRoot.Children.Remove(head);
            }
            heads.Clear();
            info.Visibility = Visibility.Collapsed;
        }

        private List<string> GetCurrentMapImages()
        {
            List<string> rst = new List<string>();
            rst.Add(currentMap.BackgroundUrl);
            List<MapRole> mapRoles = currentMap.getMapRoles();
            foreach (MapRole mapRole in mapRoles)
            {
                string roleKey = mapRole.roleKey;
                if (RoleManager.GetRole(roleKey) != null)
                    rst.Add(RoleManager.GetRole(roleKey).HeadPicPath);
                else if(mapRole.pic !=null)
                    rst.Add(mapRole.pic);
            }
            return rst;
        }

        public void resetMap()
        {
            resetTeam();

            isOver = false;
            nextScenario = "";
            isDialogOver = false;
            
            locationDescription.Clear();
            locationEvents.Clear();
            locationLv.Clear();
            uiHost.scence.HideHeads();

            //PointArrow.Visibility = System.Windows.Visibility.Collapsed;

            foreach (var l in locationButtons)
            {
                this.LayoutRoot.Children.Remove(l);
            }
            locationButtons.Clear();
            resetHead();

            //初始化地图回调
            initCallback();
            
            //读入地图背景
            this.background.Source = this.currentMap.Background;
            this.background.Opacity = CommonSettings.timeOpacity[RuntimeData.Instance.Date.Hour / 2];

            //主角头像
            this.zhujueHead.Source = RuntimeData.Instance.Team[0].Head;

            //称号
            this.nickLabel.Text = RuntimeData.Instance.CurrentNick;

            //读入背景音乐
            this.initMusic();

            //读入当前日期
            this.initDate();

            //读入难度
            if (RuntimeData.Instance.GameMode == "normal")
            {
                gameModeText.Text = "难度:简单（周目：" + RuntimeData.Instance.Round +"）";
                gameModeText.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (RuntimeData.Instance.GameMode == "hard")
            {
                gameModeText.Text = "难度:进阶（周目：" + RuntimeData.Instance.Round + "）";
                gameModeText.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (RuntimeData.Instance.GameMode == "crazy")
            {
                gameModeText.Text = "难度:炼狱（周目：" + RuntimeData.Instance.Round + "）";
                gameModeText.Foreground = new SolidColorBrush(Colors.Red);
            }

            if (currentMap.HasLocation)
            {
                //当前位置浮标等
                mapPointer.Visibility = Visibility.Visible;
                mapPointer.image.Source = RuntimeData.Instance.Team[0].Head;
                
                //读入当前每个地点的事件
                initLocationEvents();

                //地点/事件初始化
                initLocations();

                //当前位置描述信息
                currentLocateText.Text = string.Format("{0}:{1}",
                    RuntimeData.Instance.CurrentBigMap,
                    RuntimeData.Instance.GetLocation(RuntimeData.Instance.CurrentBigMap));
            }
            else //场景
            {
                //初始化角色的事件
                initRoleEvents();

                //初始化场景
                initScene();

                //对于迷宫，不能显示mapkey，只能显示description，详见MapEvents.xml中关于五毒教山中的部分
                if (currentMap.desc == null || currentMap.desc == "")
                {
                    currentLocateText.Text = string.Format("{0}", RuntimeData.Instance.CurrentBigMap);
                }
                else
                {
                    currentLocateText.Text = string.Format("{0}", currentMap.desc);
                }
            }
        }

        private void initDate()
        {
            TimeBlock.Text = string.Format("江湖{0}年{1}月{2}日{3}时",
                CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year],
                CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month],
                CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day],
                CommonSettings.chineseTime[RuntimeData.Instance.Date.Hour / 2]);
        }

        private void initMusic()
        {
            if (currentMap.Musics.Count > 0)
            {
                AudioManager.PlayMusic(ResourceManager.Get(currentMap.GetRandomMusic()));
            }
        }

        public void initRoleEvents()
        {
            List<MapRole> maproles = currentMap.getMapRoles();
            foreach (MapRole maprole in maproles)
            {
                List<Event> events = currentMap.getEvents(maprole.roleKey);

                //TODO 判断事件触发条件

                if (events == null)
                {
                    locationEvents.Add(maprole.roleKey, null);
                    locationDescription.Add(maprole.roleKey, maprole.description);
                    locationLv.Add(maprole.roleKey, 0);
                }
                else
                {
                    //TODO 现在是直接触发第一个尚未执行过的事件，这肯定还是有问题的...
                    int i = 0;

                    for (i = 0; i < events.Count; i++)
                    {
                        //如果是只能执行一次的事件，需要检查这个事件是否已经被执行过了。
                        if (events[i].RepeatType == EventRepeatType.Once)
                        {
                            if (RuntimeData.Instance.KeyValues.ContainsKey(events[i].Value))
                            {
                                continue;
                            }
                        }

                        int randomNo = Tools.GetRandomInt(0, 100);
                        if (randomNo > events[i].probability)
                        {
                            continue;
                        }

                        //检查事件触发的各种条件
                        //TODO:更多条件判定有待完善
                        bool conditionOK = true;
                        foreach (EventCondition condition in events[i].conditions)
                        {
                            if (!TriggerManager.judge(condition))
                            {
                                conditionOK = false;
                                break;
                            }
                        }
                        if (!conditionOK)
                        {
                            continue;
                        }

                        locationEvents.Add(maprole.roleKey, events[i]);

                        string desc = events[i].description;
                        if (desc == null || desc == string.Empty)
                            desc = maprole.description;
                        locationDescription.Add(maprole.roleKey, desc);
                        locationLv.Add(maprole.roleKey, events[i].lv);
                        break;
                    }
                    //没有一个符合触发条件
                    if (i >= events.Count)
                    {
                        locationEvents.Add(maprole.roleKey, null);
                        locationDescription.Add(maprole.roleKey, maprole.description);
                        locationLv.Add(maprole.roleKey, 0);
                    }
                }
            }
        }

        public void initLocationEvents()
        {
            List<Location> locations = currentMap.getLocations();
            foreach (Location location in locations)
            {
                List<Event> events = currentMap.getEvents(location.name);

                if (events == null)
                {
                    locationEvents.Add(location.name, null);
                    locationDescription.Add(location.name, location.description);
                    locationLv.Add(location.name, 0);
                }
                else
                {

                    //foreach(var evt in )

                    //TODO 现在是直接触发第一个尚未执行过的事件，这肯定还是有问题的...
                    int i = 0;

                    for (i = 0; i < events.Count; i++)
                    {
                        //如果是只能执行一次的事件，需要检查这个事件是否已经被执行过了。
                        if (events[i].RepeatType == EventRepeatType.Once)
                        {
                            if (RuntimeData.Instance.KeyValues.ContainsKey(events[i].Value))
                            {
                                continue;
                            }
                        }

                        int randomNo = Tools.GetRandomInt(0, 100);
                        if (randomNo > events[i].probability)
                        {
                            continue;
                        }

                        //检查事件触发的各种条件
                        //TODO:更多条件判定有待完善
                        bool conditionOK = true;
                        foreach (EventCondition condition in events[i].conditions)
                        {
                            if (!TriggerManager.judge(condition))
                            {
                                conditionOK = false;
                                break;
                            }
                        }
                        if (!conditionOK)
                        {
                            continue;
                        }


                        locationEvents.Add(location.name, events[i]);
                        string desc =  events[i].description;
                        if(desc == null||desc == string.Empty)
                            desc = location.description;
                        locationDescription.Add(location.name, desc );
                        locationLv.Add(location.name, events[i].lv);
                        break;
                    }
                    //没有一个符合触发条件
                    if ( i >= events.Count)
                    {
                        locationEvents.Add(location.name, null);
                        locationDescription.Add(location.name, location.description);
                        locationLv.Add(location.name, 0);
                    }
                }
            }
        }

        public void initScene()
        {
            mapPointer.Visibility = Visibility.Collapsed;
            //PointArrow.Visibility = System.Windows.Visibility.Collapsed;
            List<MapRole> mapRoles = currentMap.getMapRoles();
            Canvas.SetZIndex(info, CommonSettings.Z_MAPUI_INFO);

            int index = 0;
            foreach (MapRole mapRole in mapRoles)
            {
                SceneHead sceneHead;
                if (!(isActive(mapRole.roleKey)))
                {
                    if (mapRole.hide)
                    {
                        continue;
                    }

                    sceneHead = new SceneHead(mapRole.roleKey, mapRole.pic, "original_nothing", "story", mapRole.description, 0);
                }
                else
                {
                    Event ev = locationEvents[mapRole.roleKey];
                    
                    sceneHead = new SceneHead(
                        mapRole.roleKey, mapRole.pic, ev.Value, ev.Type, ev.description != null? ev.description: mapRole.description, ev.lv,
                        ev.RepeatType == EventRepeatType.Once);
                }

                sceneHead.head.Width = CommonSettings.MAPUI_ROLEHEAD_WIDTH;
                sceneHead.head.Height = CommonSettings.MAPUI_ROLEHEAD_HEIGHT;
                sceneHead.Margin = new Thickness(CommonSettings.MAPUI_ROLEHEAD_X, CommonSettings.MAPUI_ROLEHEAD_Y + index * (CommonSettings.MAPUI_ROLEHEAD_HEIGHT + CommonSettings.MAPUI_ROLEHEAD_GAP), 0, 0);
                Canvas.SetZIndex(sceneHead, CommonSettings.Z_MAPUI_ROLEHEAD);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = ResourceManager.GetImage("ui.头像框.普通");
                brush.Stretch = Stretch.Uniform;
                sceneHead.LayoutRoot.Background = brush;

                sceneHead.MouseEnter += showRoleInfo;
                sceneHead.MouseLeave += hideRoleInfo;

                this.LayoutRoot.Children.Add(sceneHead);
                heads.Add(sceneHead);
                index++;
            }
        }

        public void initLocations()
        {
            
            List<Location> locations = currentMap.getLocations();
            
            foreach (Location location in locations)
            {
                MapEvent locButton = new MapEvent();
                locButton.Name = location.name;
                locButton.Opacity = 0.75;
                locButton.locateName.Text = location.name;
                if (background.Opacity >= 0.9) //当前是大中午，为了突出字体颜色
                {
                    locButton.locateName.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (background.Opacity <= 0.5) //当前是大中午
                {
                    locButton.locateName.Foreground = new SolidColorBrush(Colors.White);
                }

                if (RuntimeData.Instance.GetLocation(currentMap.Name).Equals(location.name))
                {
                    mapPointer.Margin = new Thickness(location.x - 15, location.y - 40, location.x + 5  , location.y  );
                    currentPosLeft = (double)location.x;
                    currentPosTop = (double)location.y;
                }

                locButton.Margin = new Thickness(location.x - 20, location.y , location.x + 1, location.y + 22);
                locButton.image.Width = 21;
                locButton.image.Height = 22;
                locButton.Tag = location.name;
                if (!(isActive(location.name)))
                {
                    locButton.switch2noevent();
                }
                else
                {
                    Event ev = locationEvents[location.name];
                    if (ev.image == "")
                    {
                        locButton.switch2event();
                    }
                    else
                    {
                        locButton.image.Width = 30;
                        locButton.image.Height = 30;
                        locButton.switch2selfimage(ResourceManager.GetImage(ev.image));
                        if(ev.RepeatType == EventRepeatType.Once)
                            locButton.ShowEventTag();
                    }
                }

                locButton.Visibility = Visibility.Visible;
                Canvas.SetZIndex(locButton, CommonSettings.Z_MAPUI_LOCATION);
                locButton.MouseEnter += showInfo;
                locButton.MouseLeave += hideInfo;
                locButton.MouseLeftButtonUp += enterScenario;
                this.LayoutRoot.Children.Add(locButton);
                locationButtons.Add(locButton);
            }
        }

        public void initCallback()
        {
            uiHost.itemSelectPanel.Callback = this.OnSelectItem;
            uiHost.RightClickCallback = this.OnRightClick;
        }

        #endregion 

        #region 地点/事件触发系统

        //判断一个地点/点击一个头像是否有可触发的事件
        public bool isActive(string locationName)
        {
            if (locationEvents.ContainsKey(locationName))
            {
                if (locationEvents[locationName] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void showInfo(object sender, EventArgs e)
        {
            if( isActive((string) ((MapEvent)sender).Tag ) )
            {
                Event ev = locationEvents[(string)((MapEvent)sender).Tag];
                //if (ev.image == "")
                //{
                    ((MapEvent)sender).switch2enter();
                //}
                //else
                //{
                //    ((MapEvent)sender).image.Source = ResourceManager.GetImage(ev.image);
                //}
                if(!string.IsNullOrEmpty(ev.image))
                {
                    infoHeadImage.Source = ResourceManager.GetImage(ev.image);
                    infoHeadImage.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    infoHeadImage.Visibility = System.Windows.Visibility.Collapsed;
                }
                double x = ((MapEvent)sender).Margin.Left + 60;
                double y = ((MapEvent)sender).Margin.Top;

                if (x + 377 > LayoutRoot.ActualWidth)
                {
                    x = x - 40 - 377 - 15;
                }

                if (y + 103 > LayoutRoot.ActualHeight)
                {
                    y = y - 10 - 103 - 15;
                }

                location.Text = ((MapEvent)sender).Name;

                if (locationLv[((MapEvent)sender).Name] != 0)
                {
                    string level = "战斗难度： ";
                    for (int i = 0; i < locationLv[((MapEvent)sender).Name] / 5 + 1;++i )
                    {
                        level += "★";
                    }
                    lv.Text = level;
                }
                else
                {
                    lv.Text = "";
                }
                description.Text = locationDescription[((MapEvent)sender).Name];

                //计算耗时
                double nextX = ((MapEvent)sender).Margin.Left;
                double nextY = ((MapEvent)sender).Margin.Top;
                double distance = Math.Sqrt((currentPosLeft - nextX) * (currentPosLeft - nextX) + (currentPosTop - nextY) * (currentPosTop - nextY));
                double costHour = 24 + (distance / 50.0) * 10;
                description.Text += string.Format(" \n(预计花费{0:0}天)", costHour / 24);
                
                info.Margin = new Thickness(x, y, x + info.ActualWidth, y + info.ActualHeight);
                info.Visibility = Visibility.Visible;
            }else
            {
                infoHeadImage.Visibility = System.Windows.Visibility.Collapsed;
                double x = ((MapEvent)sender).Margin.Left + 60;
                double y = ((MapEvent)sender).Margin.Top;

                if (x + 377 > LayoutRoot.ActualWidth)
                {
                    x = x - 40 - 377 - 15;
                }

                if (y + 103 > LayoutRoot.ActualHeight)
                {
                    y = y - 10 - 103 -15 ;
                }

                location.Text = ((MapEvent)sender).Name;
                lv.Text = "";
                description.Text = locationDescription[((MapEvent)sender).Name];
                info.Margin = new Thickness(x, y, x + info.ActualWidth, y + info.ActualHeight);
                info.Visibility = Visibility.Visible;
            }

            //this.ShowArrow(
            //    currentPosLeft,
            //    currentPosTop,
            //    ((MapEvent)sender).Margin.Left + ((MapEvent)sender).ActualWidth / 2,
            //    ((MapEvent)sender).Margin.Top + ((MapEvent)sender).ActualHeight / 2);
        }


        //private void ShowArrow(double x1,double y1,double x2,double y2)
        //{
        //    if (x1 <= x2 && y1 <= y2) PointArrow.StartCorner = Microsoft.Expression.Media.CornerType.TopLeft;
        //    else if (x1 <= x2 && y1 >= y2) PointArrow.StartCorner = Microsoft.Expression.Media.CornerType.BottomLeft;
        //    else if (x1 >= x2 && y1 <= y2) PointArrow.StartCorner = Microsoft.Expression.Media.CornerType.TopRight;
        //    else
        //        PointArrow.StartCorner = Microsoft.Expression.Media.CornerType.BottomRight;

        //    double x = Math.Min(x1, x2);
        //    double y = Math.Min(y1, y2);
        //    double w = Math.Abs(x1 - x2);
        //    double h = Math.Abs(y1 -  y2);
        //    Canvas.SetLeft(PointArrow, x);
        //    Canvas.SetTop(PointArrow, y);
        //    PointArrow.Width = w;
        //    PointArrow.Height = h;
        //    PointArrow.Visibility = Visibility.Visible;
        //}

        private void showRoleInfo(object sender, EventArgs e)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = ResourceManager.GetImage("ui.头像框.激活");
            ((SceneHead)sender).LayoutRoot.Background = brush;

            double x = ((SceneHead)sender).Margin.Left + 90;
            double y = ((SceneHead)sender).Margin.Top;

            if (x + 377 > LayoutRoot.ActualWidth)
            {
                x = x - 40 - 377;
            }

            if (y + 103 > LayoutRoot.ActualHeight)
            {
                y = y - 10 - 103;
            }

            location.Text = ((SceneHead)sender).roleKey;
            
            description.Text = ((SceneHead)sender).description;
            //((SceneHead)sender).
            int level = ((SceneHead)sender).level;
            if (level != 0)
            {
                string levelInfo = "战斗难度： ";
                for (int i = 0; i < level / 5 + 1; ++i)
                {
                    levelInfo += "★";
                }
                lv.Text = levelInfo;
            }
            else
            {
                lv.Text = "";
            }
            info.Margin = new Thickness(x, y, x + info.ActualWidth, y + info.ActualHeight);
            infoHeadImage.Source = ((SceneHead)sender).head.Source;
            infoHeadImage.Visibility = System.Windows.Visibility.Visible;
            info.Visibility = Visibility.Visible;
        }

        private void hideInfo(object sender, EventArgs e)
        {
            if (isActive((string)((MapEvent)sender).Tag))
            {
                Event ev = locationEvents[(string)((MapEvent)sender).Tag];
                if (ev.image == "")
                {
                    ((MapEvent)sender).switch2event();
                }
                else
                {
                    ((MapEvent)sender).switch2selfimage(ResourceManager.GetImage(ev.image));
                }
                info.Visibility = Visibility.Collapsed;
                //PointArrow.Visibility = Visibility.Collapsed;
            }
            else
            {
                ((MapEvent)sender).switch2noevent();
                info.Visibility = Visibility.Collapsed;
            }
        }

        private void hideRoleInfo(object sender, EventArgs e)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = ResourceManager.GetImage("ui.头像框.普通");
            ((SceneHead)sender).LayoutRoot.Background = brush;

            info.Visibility = Visibility.Collapsed;
        }

        private void enterScenario(object sender, EventArgs e)
        {
            string locationName = ((MapEvent)sender).Name;
            if(!locationEvents.ContainsKey(locationName))
            {
                return;
            }

            Event eventX = locationEvents[locationName];
            if (eventX == null)
            {
                return;
            }

            //根据与当前位置的距离计算需要消耗在路上的天数，大地图至少为1天，小地图至少1个小时
            double costHour = 1;
            if (currentMap.Name.Equals("大地图"))
            {
                costHour = 24;
            }
            double nextX = ((MapEvent)sender).Margin.Left;
            double nextY = ((MapEvent)sender).Margin.Top;
            double distance = Math.Sqrt((currentPosLeft - nextX) * (currentPosLeft - nextX) + (currentPosTop - nextY) * (currentPosTop - nextY));
            costHour += (distance / 50.0) * 10;
            RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(costHour);

            //开始填充下一个场景的loader参数
            string nextScenarioType = eventX.Type ;

            nextScenario = eventX.Value;
            isOver = true;
            RuntimeData.Instance.SetLocation(currentMap.Name, locationName);
            RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = nextScenarioType, Value = nextScenario });
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        public void Load(string bigMapName)
        {
            BigMap bigMap = MapEventsManager.GetBigMap(bigMapName);
            if (bigMap == null)
            {
                MessageBox.Show("错误,地图" + bigMap + "不存在!");
                return;
            }
            RuntimeData.Instance.CurrentBigMap = bigMap.Name;
            currentMap = bigMap;
            if(Configer.Instance.Debug)
            {
                App.DebugPanel.LoadMap(bigMap);
            }

            uiHost.loadingPanel.Show(this.GetCurrentMapImages(), () =>
            {
                resetMap();
                this.Visibility = Visibility.Visible;
            });
        }
        #endregion

        #region 功能

        private void RoleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (uiHost.rolePanel.Visibility == System.Windows.Visibility.Collapsed)
            //    uiHost.rolePanel.Show(RuntimeData.Instance.Team);
            //else
            //    uiHost.rolePanel.Visibility = System.Windows.Visibility.Collapsed;

            if (uiHost.roleListPanel.Visibility == System.Windows.Visibility.Collapsed)
                uiHost.roleListPanel.Show();
            else
                uiHost.roleListPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ItemsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            if (uiHost.itemSelectPanel.Visibility == System.Windows.Visibility.Collapsed)
                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
            else
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        void OnSelectItem(Item item)
        {
            //TODO...
            if (item == null)
            {
                uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            if (item.Type == (int)ItemType.Costa)
            {
                MessageBox.Show("你选择了" + item.Name + "，目前大地图暂不支持使用战斗消耗品");
                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                return;
            }
            else if (item.Type == (int)ItemType.Mission)
            {
                //任务物品
            }
            #region 增加上限物品
            else if (item.Type == (int)ItemType.Upgrade) //增加上限物品
            {
                if (uiHost.roleListPanel.Visibility == System.Windows.Visibility.Collapsed)
                {
                    MessageBox.Show("请打开人物状态面板，并切换到需要使用物品的角色。");
                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                    return;
                }
                else
                {
                    Role selectRole = uiHost.roleListPanel.GetSelectRole();
                    Dialog dialog1 = new Dialog();
                    dialog1.type = "DIALOG";
                    dialog1.role = "南贤";
                    dialog1.info = item.Name + "为一次性消耗品！确定使用吗？";
                    uiHost.dialogPanel.ShowDialog(dialog1, (select) =>
                    {
                        uiHost.selectPanel.title.Text = "确定使用吗？";
                        uiHost.selectPanel.yes.Content = "确定";
                        uiHost.selectPanel.CallBack = () =>
                        {
                            if (uiHost.selectPanel.currentSelection == "yes")
                            {
                                ItemResult result = ItemManager.TryUseItem(selectRole, selectRole, item);

                                List<Dialog> resultDialogs = new List<Dialog>();
                                resultDialogs.Clear();
                                if (result.MaxHp != 0)
                                {
                                    selectRole.Attributes["maxhp"] = selectRole.Attributes["maxhp"] + result.MaxHp;
                                    if (selectRole.Attributes["maxhp"] >= 10000)
                                        selectRole.Attributes["maxhp"] = 9999;
                                    selectRole.Attributes["hp"] = selectRole.Attributes["maxhp"];
                                    uiHost.roleListPanel.Refresh();

                                    Dialog dialog = new Dialog();
                                    dialog.info = selectRole.Name + "的气血上限增加了【" + result.MaxHp.ToString() + "】！";
                                    dialog.type = "DIALOG";
                                    dialog.role = selectRole.Key;
                                    dialog.img = selectRole.Head;
                                    resultDialogs.Add(dialog);
                                }
                                if (result.MaxMp != 0)
                                {
                                    selectRole.Attributes["maxmp"] = selectRole.Attributes["maxmp"] + result.MaxHp;
                                    if (selectRole.Attributes["maxmp"] >= 10000)
                                        selectRole.Attributes["maxmp"] = 9999;
                                    selectRole.Attributes["mp"] = selectRole.Attributes["maxmp"];
                                    uiHost.roleListPanel.Refresh();

                                    Dialog dialog = new Dialog();
                                    dialog.info = selectRole.Name + "的内力上限增加了【" + result.MaxMp.ToString() + "】！";
                                    dialog.type = "DIALOG";
                                    dialog.role = selectRole.Key;
                                    dialog.img = selectRole.Head;
                                    resultDialogs.Add(dialog);
                                }

                                RuntimeData.Instance.Items.Remove(item);
                                uiHost.dialogPanel.CallBack = null;
                                uiHost.dialogPanel.ShowDialogs(resultDialogs);
                                AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                                   
                            }
                            uiHost.roleListPanel.Refresh();
                            uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                        };
                        uiHost.selectPanel.ShowSelection();
                    });
                }
            }
            #endregion
            else
            {
                if (uiHost.roleListPanel.Visibility == System.Windows.Visibility.Collapsed)
                {
                    MessageBox.Show("请打开人物列表状态面板，并切换到需要使用物品的角色。");
                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                    return;
                }
                else
                {
                    Role selectRole = uiHost.roleListPanel.GetSelectRole();
                    if (item.CanEquip(selectRole))
                    {
                        #region 特殊技能学习物品
                        if (item.Type == 6) //特殊攻击学习，一次性消耗品
                        {
                            //如果有了，不能学习
                            foreach (var specilSkill in selectRole.SpecialSkills)
                            {
                                if (specilSkill.Skill.Name.Equals(item.GetItemSkill().SkillName))
                                {
                                    MessageBox.Show("不能使用,该角色已经学会该项技能");
                                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    return;
                                }
                            }

                            Dialog dialog1 = new Dialog();
                            dialog1.type = "DIALOG";
                            dialog1.role = "南贤";
                            dialog1.info = item.Name + "为一次性消耗品！确定使用吗？";
                            uiHost.dialogPanel.ShowDialog(dialog1, (select) =>
                                {
                                    uiHost.selectPanel.title.Text = "确定使用吗？";
                                    uiHost.selectPanel.yes.Content = "确定";
                                    uiHost.selectPanel.CallBack = () =>
                                    {
                                        if (uiHost.selectPanel.currentSelection == "yes")
                                        {
                                            SpecialSkillInstance instance = new SpecialSkillInstance()
                                            {
                                                Owner = selectRole,
                                            };
                                            instance.Skill = SkillManager.GetSpecialSkill(item.GetItemSkill().SkillName);
                                            selectRole.SpecialSkills.Add(instance);
                                            RuntimeData.Instance.Items.Remove(item);

                                            Dialog dialog = new Dialog();
                                            dialog.info = selectRole.Name + "领悟了特殊技能：【" + item.GetItemSkill().SkillName + "】！";
                                            dialog.type = "DIALOG";
                                            dialog.role = selectRole.Key;
                                            dialog.img = selectRole.Head;
                                            uiHost.dialogPanel.CallBack = null;
                                            uiHost.dialogPanel.ShowDialog(dialog);
                                            AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                                        }
                                        uiHost.roleListPanel.Refresh();
                                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    };
                                    uiHost.selectPanel.ShowSelection();
                                });
                        }
                        #endregion

                        #region 天赋之书
                        else if (item.Type == 7) //天赋之书，一次性消耗品
                        {
                            //如果有了，不能学习
                            foreach (var talent in selectRole.Talents)
                            {
                                if (talent.Equals(item.GetItemSkill().SkillName))
                                {
                                    MessageBox.Show("不能使用,该角色已经拥有该项天赋");
                                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    return;
                                }
                            }

                            //如果武学常识不够，不能学习
                            int needWuxue = 0;
                            if (!selectRole.CanLearnTalent(item.GetItemSkill().SkillName, ref needWuxue))
                            {
                                MessageBox.Show("不能使用,该角色剩余武学常识不够，需要" + needWuxue.ToString());
                                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                return;
                            }

                            Dialog dialog1 = new Dialog();
                            dialog1.type = "DIALOG";
                            dialog1.role = "南贤";
                            dialog1.info = item.Name + "为一次性消耗品！确定使用吗？";
                            uiHost.selectPanel.yes.Content = "确定";
                            uiHost.dialogPanel.ShowDialog(dialog1, (select) =>
                                {
                                    uiHost.selectPanel.title.Text = "确定使用吗？";
                                    uiHost.selectPanel.CallBack = () =>
                                    {
                                        if (uiHost.selectPanel.currentSelection == "yes")
                                        {
                                            string talentName = item.GetItemSkill().SkillName;
                                            if (selectRole.Talent == null || selectRole.Talent == "")
                                                selectRole.Talent = talentName;
                                            else
                                                selectRole.Talent = selectRole.Talent + "#" + talentName;
                                            RuntimeData.Instance.Items.Remove(item);

                                            Dialog dialog = new Dialog();
                                            dialog.info = selectRole.Name + "领悟了天赋：【" + item.GetItemSkill().SkillName + "】！";
                                            dialog.type = "DIALOG";
                                            dialog.role = selectRole.Key;
                                            dialog.img = selectRole.Head;
                                            uiHost.dialogPanel.CallBack = null;
                                            uiHost.dialogPanel.ShowDialog(dialog);
                                            AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                                        }
                                        uiHost.roleListPanel.Refresh();
                                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    };
                                    uiHost.selectPanel.ShowSelection();
                                });
                        }
                        #endregion

                        else if (item.Type == (int)ItemType.Special) //自宫小刀
                        {
                            #region 自宫小刀
                            if (item.Name == "刀")
                            {
                                //如果是女人，不能学习
                                if (selectRole.Female)
                                {
                                    MessageBox.Show("女性角色无法自宫！");
                                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    return;
                                }
                                //如果是野兽，不能学习
                                if (selectRole.Female)
                                {
                                    MessageBox.Show("兽类无法自宫！");
                                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    return;
                                }
                                //已经阉割了的，不能学习
                                if (selectRole.HasTalent("阉人"))
                                {
                                    MessageBox.Show("已经阉割过了！想割也没得割喽~");
                                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    return;
                                }

                                Dialog dialog1 = new Dialog();
                                dialog1.type = "DIALOG";
                                dialog1.role = "南贤";
                                dialog1.info = "割了要大损功力！当太监不容易的！确定要把" + selectRole.Name + "阉割了吗？";
                                uiHost.selectPanel.yes.Content = "割了！";
                                uiHost.dialogPanel.ShowDialog(dialog1, (select) =>
                                {
                                    uiHost.selectPanel.title.Text = "真的要割吗？";
                                    uiHost.selectPanel.CallBack = () =>
                                    {
                                        if (uiHost.selectPanel.currentSelection == "yes")
                                        {
                                            string talentName = "阉人";
                                            if (selectRole.Talent == null || selectRole.Talent == "")
                                                selectRole.Talent = talentName;
                                            else
                                                selectRole.Talent = selectRole.Talent + "#" + talentName;
                                            RuntimeData.Instance.Items.Remove(item);

                                            List<Dialog> dialogs = new List<Dialog>();
                                            dialogs.Clear();

                                            Dialog dialog = new Dialog();
                                            dialog.info = selectRole.Name + "已经变成了太监！从今以后可以重新做人，开启第二人生了。";
                                            dialog.type = "DIALOG";
                                            dialog.role = "南贤";
                                            dialog.img = selectRole.Head;
                                            dialogs.Add(dialog);

                                            int minusHP = (int)(selectRole.Attributes["maxhp"] / 3.0);
                                            int minusMP = (int)(selectRole.Attributes["maxmp"] / 2.0);
                                            selectRole.Attributes["maxhp"] -= minusHP;
                                            selectRole.Attributes["hp"] -= minusHP;
                                            selectRole.Attributes["maxmp"] -= minusMP;
                                            selectRole.Attributes["mp"] -= minusMP;

                                            Dialog diagHP = new Dialog();
                                            diagHP.info = selectRole.Name + "减少最大气血" + minusHP + "点！ T_T";
                                            diagHP.type = "DIALOG";
                                            diagHP.role = selectRole.Key;
                                            diagHP.img = selectRole.Head;
                                            dialogs.Add(diagHP);

                                            Dialog diagMP = new Dialog();
                                            diagMP.info = "...T_T " + selectRole.Name + "减少最大内力" + minusMP + "点！";
                                            diagMP.type = "DIALOG";
                                            diagMP.role = selectRole.Key;
                                            diagMP.img = selectRole.Head;
                                            dialogs.Add(diagMP);

                                            uiHost.dialogPanel.CallBack = null;
                                            uiHost.dialogPanel.ShowDialogs(dialogs);
                                            AudioManager.PlayEffect(ResourceManager.Get("音效.男惨叫"));
                                        }
                                        uiHost.roleListPanel.Refresh();
                                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                    };
                                    uiHost.selectPanel.ShowSelection();
                                });
                            }
                            #endregion

                            else if (item.Name == "洗练书")
                            {
                                Dialog dialog1 = new Dialog();
                                dialog1.type = "DIALOG";
                                dialog1.role = "南贤";
                                dialog1.info = "洗练书可以忘却技能，确定要把" + selectRole.Name + "的技能忘却吗？";
                                uiHost.selectPanel.yes.Content = "是的！";
                                uiHost.dialogPanel.ShowDialog(dialog1, (select) =>
                                {
                                    uiHost.selectPanel.title.Text = "真的要忘却技能吗？";
                                    uiHost.selectPanel.CallBack = () =>
                                    {
                                        if (uiHost.selectPanel.currentSelection == "yes")
                                        {
                                            uiHost.skillPanel.Callback = (selectSkill) =>
                                                {
                                                    uiHost.skillPanel.Visibility = System.Windows.Visibility.Collapsed;
                                                    if(selectSkill!=null)
                                                    {
                                                        RuntimeData.Instance.Items.Remove(item);
                                                        AudioManager.PlayEffect(ResourceManager.Get("音效.恢复2"));
                                                        if(selectSkill.Instance != null)
                                                        {
                                                            selectRole.Skills.Remove(selectSkill.Instance);
                                                        }else if(selectSkill.SwitchInternalSkill != null)
                                                        {
                                                            selectRole.InternalSkills.Remove(selectSkill.SwitchInternalSkill);
                                                        }

                                                        uiHost.roleListPanel.Refresh();
                                                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                                    }
                                                };
                                            uiHost.skillPanel.ShowXilian(selectRole);
                                        }
                                    };
                                    uiHost.selectPanel.ShowSelection();
                                });
                            }
                        }
                        #region 装备品
                        else //装备品（武器、护甲、首饰、经书）
                        {
                            Item formalItem = item.EquipToRole(selectRole);
                            RuntimeData.Instance.Items.Remove(item);
                            if (formalItem != null)
                            {
                                RuntimeData.Instance.Items.Add(formalItem);
                            }

                            uiHost.roleListPanel.Refresh();
                            uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                        }
                        #endregion
                    }
                    #region 残章
                    else if(item.Type == (int)ItemType.Canzhang)
                    {
                        string skillName = item.CanzhangSkill;
                        foreach(var s in selectRole.Skills)
                        {
                            if (s.Skill.Name == skillName && s.MaxLevel < CommonSettings.MAX_SKILL_LEVEL)
                            {
                                AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                                MessageBox.Show(string.Format("{0}的{1}等级上限提高！", selectRole.Name, s.Skill.Name));
                                s.MaxLevel++;
                                RuntimeData.Instance.Items.Remove(item);
                                uiHost.roleListPanel.Refresh();
                                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                return;
                            }
                            else if (s.Skill.Name == skillName && s.MaxLevel >= CommonSettings.MAX_SKILL_LEVEL)
                            {
                                MessageBox.Show(string.Format("{0}的{1}等级已经达到上限，不能再提高了", selectRole.Name, s.Skill.Name));
                                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                return;
                            }
                        }
                        foreach (var s in selectRole.InternalSkills)
                        {
                            if (s.Skill.Name == skillName && s.MaxLevel < CommonSettings.MAX_INTERNALSKILL_LEVEL)
                            {
                                AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                                MessageBox.Show(string.Format("{0}的{1}等级上限提高！", selectRole.Name, s.Skill.Name));
                                s.MaxLevel++;
                                RuntimeData.Instance.Items.Remove(item);
                                uiHost.roleListPanel.Refresh();
                                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                return;
                            }
                            else if (s.Skill.Name == skillName && s.MaxLevel >= CommonSettings.MAX_INTERNALSKILL_LEVEL)
                            {
                                MessageBox.Show(string.Format("【{0}】的【{1}】等级已经达到上限，不能再提高了", selectRole.Name, s.Skill.Name));
                                uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                                return;
                            }
                        }
                        MessageBox.Show(string.Format("错误,【{0}】没有技能【{1}】", selectRole.Name, skillName));
                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                        return;
                    }
                    #endregion
                    else
                    {
                        MessageBox.Show("该角色不能使用该物品，需要" + item.EquipCase);
                        uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                    }
                }
            }
        }

        void OnRightClick()
        {
            uiHost.rolePanel.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.itemSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.systemOptionsPanel.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.saveLoadPanel.Visibility = System.Windows.Visibility.Collapsed;
            uiHost.roleListPanel.Visibility = System.Windows.Visibility.Collapsed;
            //uiHost.storePanel.Visibility = Visibility.Collapsed;
            uiHost.logPanel.Visibility = Visibility.Collapsed;
            uiHost.envsetPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void EnvButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            if (uiHost.systemOptionsPanel.Visibility == System.Windows.Visibility.Collapsed)
                uiHost.systemOptionsPanel.Visibility = System.Windows.Visibility.Visible;
            else
                uiHost.systemOptionsPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        private void ArenaButton_Click(object sender, RoutedEventArgs e)
        {
            RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { Type = "arena", Value = "" });
        }

        /*
        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (uiHost.storePanel.Visibility == System.Windows.Visibility.Collapsed)
                uiHost.storePanel.Show();
            else
                uiHost.storePanel.Visibility = System.Windows.Visibility.Collapsed;
        }
         */

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (uiHost.logPanel.Visibility == Visibility.Collapsed)
                uiHost.logPanel.Show();
            else
                uiHost.logPanel.Visibility = Visibility.Collapsed;
        }

        private void PracButton_Click(object sender, RoutedEventArgs e)
        {
            Dialog dialogInfo = new Dialog();
            dialogInfo.info = "闭关练功可以帮助你迅速提升等级和武学修为，但是需要消耗十天时间。";
            dialogInfo.role = "邢捕头";
            dialogInfo.type = "DIALOG";

            Dialog dialogSelect = new Dialog();
            dialogSelect.info = "要闭关练功吗？#闭关修炼！#外面的世界多精彩，才不要。";
            dialogSelect.role = "主角";
            dialogSelect.type = "SELECT";

            List<Dialog> dialogs = new List<Dialog>();
            dialogs.Clear();
            dialogs.Add(dialogInfo);
            dialogs.Add(dialogSelect);
            uiHost.dialogPanel.CallBack = (selected) =>
            {
                switch (selected)
                {
                    //练功，根据团队当前的经验值，自动升至下一等级
                    case 0:
                        //升级
                        foreach (Role role in RuntimeData.Instance.Team)
                        {
                            int exp2add = role.LevelupExp - role.Exp + 1;
                            role.AddExp(exp2add);
                        }
                        AudioManager.PlayEffect(ResourceManager.Get("音效.升级"));
                        //MessageBox.Show(string.Format("【{0}】升到第【{1}】级", currentSpirit.Role.Name, currentSpirit.Role.Level) );
                        Dialog dialog = new Dialog();
                        dialog.role = "主角";
                        dialog.type = "DIALOG";
                        dialog.info = string.Format("全体队员等级提升一级！");
                        uiHost.dialogPanel.CallBack = null;
                        uiHost.dialogPanel.ShowDialog(dialog);

                        //日历走十天
                        RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddDays(10);
                        uiHost.mapUI.resetMap();

                        RuntimeData.Instance.gameEngine.LoadMap(RuntimeData.Instance.CurrentBigMap);
                        break;
                    case 1:
                        //uiHost.dialogPanel.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            };
            uiHost.dialogPanel.ShowDialogs(dialogs);

        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            uiHost.dodgeGame.callBack = (s) =>
            {
                uiHost.dodgeGame.Visibility = Visibility.Collapsed;
                this.resetMap();

                Dialog dialog = new Dialog();
                dialog.role = "佟湘玉";
                dialog.type = "DIALOG";
                dialog.info = "你坚持了" + s.ToString() + "秒！";
                uiHost.dialogPanel.CallBack = null;
                uiHost.dialogPanel.ShowDialog(dialog);
            };
            uiHost.dodgeGame.Visibility = Visibility.Visible;
            uiHost.dodgeGame.start();
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
        	// 在此处添加事件处理程序实现。
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 在此处添加事件处理程序实现。
        }

        public void showDonate()
        {
            MessageBox.Show("功能关闭。");
            //this.IsEnabled = false;

            //List<Dialog> dialogs = new List<Dialog>();

            //Dialog dialog1 = new Dialog();
            //dialog1.role = "汉家松鼠";
            //dialog1.type = "DIALOG";
            //dialog1.info = "(举着一面小铜锣)各位乡亲，各位乡亲！";
            //dialogs.Add(dialog1);

            //Dialog dialog2 = new Dialog();
            //dialog2.role = "汉家松鼠";
            //dialog2.type = "DIALOG";
            //dialog2.info = "捐款啦，捐款啦！";
            //dialogs.Add(dialog2);

            //Dialog dialog3 = new Dialog();
            //dialog3.role = "汉家松鼠";
            //dialog3.type = "DIALOG";
            //dialog3.info = "感谢您一直以来对于金X的喜欢。我们深知，自己做得还很不足，还有太多地方不尽如人意...您的一点美意，将帮助我们更好地完善这个金庸世界，使它更加丰富多彩！";
            //dialogs.Add(dialog3);

            //Dialog dialog4 = new Dialog();
            //dialog4.role = "汉家松鼠";
            //dialog4.type = "DIALOG";
            //dialog4.info = "也许是一包烟钱、也许是一顿饭钱...这对我们来说都会是莫大的鼓舞和支持！亲爱的玩家，我们需要您！让我们一起携手努力，使金X可以走得更高、更远吧！";
            //dialogs.Add(dialog4);

            //Dialog dialog5 = new Dialog();
            //dialog5.role = "汉家松鼠";
            //dialog5.type = "DIALOG";
            //dialog5.info = "我们设立了一个支付宝账号，并进行了实名认证。滴水之恩，涌泉相报。捐赠时请留下您的QQ、邮箱等联系方式，方便我们与您取得联系！";
            //dialogs.Add(dialog5);

            //Dialog dialog6 = new Dialog();
            //dialog6.role = "汉家松鼠";
            //dialog6.type = "DIALOG";
            //dialog6.info = "祝您游戏愉快！再次感谢您的大力支持！（以下将展示支付宝二维码，请用支付宝客户端扫描捐款，实名认证户名“庞杨”即汉家松鼠.子尹童鞋）";
            //dialogs.Add(dialog6);

            ////Dialog dialog7 = new Dialog();
            ////dialog7.role = "汉家松鼠";
            ////dialog7.type = "DIALOG";
            ////dialog7.info = "如果您的浏览器拦截了弹出的支付宝窗口，您可以手动允许该窗口弹出。对于没有显示就直接拦截支付窗口的浏览器...";
            ////dialogs.Add(dialog7);

            //Dialog dialog8 = new Dialog();
            //dialog8.role = "支付宝二维码";
            //dialog8.type = "DIALOG";
            //dialog8.info = "请使用支付宝客户端扫描我来为汉家松鼠捐款！";
            //dialogs.Add(dialog8);

            //Dialog dialog9 = new Dialog();
            //dialog9.role = "支付宝二维码";
            //dialog9.type = "DIALOG";
            //dialog9.info = "谢谢您对金X的大力支持！";
            //dialogs.Add(dialog9);

            //dialogs.Add(new Dialog() { role = "汉家松鼠", type = "DIALOG", info = "您也可以通过支付宝转账，账号：hanjiasongshu@163.com" });
            //dialogs.Add(new Dialog() { role = "汉家松鼠", type = "DIALOG", info = "如果你是国际友人，可以通过paypal转账，账号：hanjiasongshu@163.com，实名认证“成功”（汉家松鼠.cg同学）" });


            ////Clipboard.SetText("https://me.alipay.com/hanjiasongshu");
            //this.IsEnabled = true;
            //uiHost.dialogPanel.ShowDialogs(dialogs);
        }

        void toDonateURL(int i)
        {
            uiHost.dialogPanel.CallBack = null;
            HtmlPage.Window.Navigate(new Uri(CommonSettings.DonateUrl, UriKind.RelativeOrAbsolute), "_blank");
        }

    }
}
