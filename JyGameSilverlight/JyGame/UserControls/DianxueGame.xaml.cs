using System;
using System.Windows;
using System.Windows.Controls;
using JyGame.GameData;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;

namespace JyGame.UserControls
{
    public partial class DianxueGame : UserControl
    {
        int times = 0;
        int onetime = 0;
        int keepTimes = 0;
        Image bom;
        int point = 0;
        int dahuandanChecked = 0;


        
        DispatcherTimer time = new DispatcherTimer();
        DispatcherTimer keepTime = new DispatcherTimer();
        //public CommonSettings.IntCallBack callBack = null;
        public CommonSettings.ItemCallBack callBack = null;
        List<Image> imageList = new List<Image>();
        TextBox tb = new TextBox();

        Dictionary<String, int> itemsNum = new Dictionary<String, int>();
        

        public double X
        {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }
        public double Y
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }

        public DianxueGame()
        {
            InitializeComponent();
            itemsNum.Add("物品.大还丹", 0);
            itemsNum.Add("物品.大蟠桃", 0);
            itemsNum.Add("物品.冬虫夏草", 0);
            itemsNum.Add("物品.九转熊蛇丸", 0);
            itemsNum.Add("物品.生生造化丹", 0);

            itemsNum.Add("物品.柳叶刀", 0);
            itemsNum.Add("物品.金丝道袍", 0);
            itemsNum.Add("物品.黄金项链", 0);
            itemsNum.Add("物品.血刀", 0);
            itemsNum.Add("物品.乌蚕衣", 0);
        }

        void move_finish(object sender, EventArgs e)
        {
            imageList[0].Visibility = Visibility.Collapsed;
        }

        void headClick(object sender, RoutedEventArgs e)
        {
            //(sender as Rect).Tag 
            string typeString = (string)(sender as Image).Tag;
            if (typeString.Length > 1)
            {
                if (dahuandanChecked == 0)
                {
                    dahuandanChecked = 1;
                    Storyboard storyBoard = new Storyboard();

                    DoubleAnimation da = new DoubleAnimation();
                    da.From = Canvas.GetLeft((sender as Image));
                    da.To = 2400;
                    da.Duration = new Duration(TimeSpan.FromMilliseconds(800));

                    Storyboard.SetTarget(da, (sender as Image));
                    Storyboard.SetTargetProperty(da, new PropertyPath("(Canvas.Left)"));
                    storyBoard.Children.Add(da);

                    DoubleAnimation da1 = new DoubleAnimation();
                    da1.From = Canvas.GetTop((sender as Image));
                    da1.To = 1800;
                    da1.Duration = new Duration(TimeSpan.FromMilliseconds(800));

                    Storyboard.SetTarget(da1, (sender as Image));
                    Storyboard.SetTargetProperty(da1, new PropertyPath("(Canvas.Top)"));
                    storyBoard.Children.Add(da1);



                    if (!Resources.Contains("itemAnimation"))
                    {
                        Resources.Add("itemAnimation", storyBoard);
                    }
                    storyBoard.Begin();
                    //itemNum++;
                    itemsNum[typeString]++;
                    //imageList[0].Visibility = Visibility.Collapsed;
                    storyBoard.Completed += new EventHandler(move_finish);
                }
            }
            else{
                int type = int.Parse(typeString);
                switch(type)
                {
                    case 1:
                        AudioManager.PlayEffect(ResourceManager.Get("音效.男惨叫"));
                        point += 4 - type;
                        break;
                    case 2:
                        AudioManager.PlayEffect(ResourceManager.Get("音效.男惨叫"));
                        point += 4 - type;
                        break;
                    case 3:
                        AudioManager.PlayEffect(ResourceManager.Get("音效.男惨叫"));
                        point += 4 - type;
                        break;
                    case 4:
                        AudioManager.PlayEffect(ResourceManager.Get("音效.敢点老娘"));
                        point -= 2;
                        break;
                    default:
                        break;
                }

                
                tb.Text = "得分:" + point.ToString();
                tb.Visibility = Visibility.Visible;

                if (bom != null && bom.Visibility==Visibility.Visible)
                {
                    bom.Visibility = Visibility.Collapsed;
                }
                imageList[type].Visibility = Visibility.Collapsed;
                int top = (int)Canvas.GetTop((sender as Image)) - 10;
                int left = (int)Canvas.GetLeft((sender as Image)) - 10;
                bom = new Image();

                bom.Width = (sender as Image).Width + 20;
                bom.Height = (sender as Image).Height + 20;
                bom.Source = ResourceManager.GetImage("物品.点击爆炸");
                LayoutRoot.Children.Add(bom);
                Canvas.SetLeft(bom, left);
                Canvas.SetTop(bom, top);
                //Canvas.SetZIndex(bom, 10);
                bom.Visibility = Visibility.Visible;

                int speed = 200;
                keepTime.Tick += new EventHandler(hit_Tick);
                keepTime.Interval = TimeSpan.FromMilliseconds(speed);
                keepTime.Start();
            }
        }

        void hit_Tick(object sender, EventArgs e)
        {
            
            if(keepTimes>=1)
            {
                bom.Visibility = Visibility.Collapsed;
                keepTime.Stop();
            }
            keepTimes++;
        }

        public void changeCursor()
        {
            //Cursor myCursor = new Cursor(Cursor.Current.Handle);
            
        }

        public Color getColor(int r, int g, int b)
        {
            Color color = new Color();
            color.R = (byte)r;
            color.G = (byte)g;
            color.B = (byte)b;
            color.A = 255;
            return color;
        }


        public void start()
        {
            itemsNum["物品.大还丹"] = 0;
            itemsNum["物品.大蟠桃"] = 0;
            itemsNum["物品.冬虫夏草"] = 0;
            itemsNum["物品.九转熊蛇丸"] = 0;
            itemsNum["物品.生生造化丹"] = 0;

            itemsNum["物品.柳叶刀"] = 0;
            itemsNum["物品.金丝道袍"] = 0;
            itemsNum["物品.黄金项链"] = 0;
            itemsNum["物品.血刀"] = 0;
            itemsNum["物品.乌蚕衣"] = 0;

            Dictionary<int, string> imageName = new Dictionary<int, string>();
            imageName.Add(0, "物品.大还丹");
            imageName.Add(1, "头像.西");
            imageName.Add(2, "头像.南");
            imageName.Add(3, "头像.北");
            imageName.Add(4, "头像.东");
            //imageName.Add(5, "头像.东");

            for (int i = 0; i < 5; ++i)
            {
                Image image = new Image();
                image.Height = (i + 1) * 20+20;
                image.Width = (i + 1) * 20+20;

                LayoutRoot.Children.Add(image);
                image.Source = ResourceManager.GetImage(imageName[i]);
                image.Visibility = Visibility.Collapsed;

                image.Tag = i.ToString();


                imageList.Add(image);
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                Canvas.SetZIndex(image, 100);
                image.MouseLeftButtonUp += new MouseButtonEventHandler(headClick);
            }

            tb.Height = 50;
            tb.Width = 100;
            LayoutRoot.Children.Add(tb);
            tb.Text = "得分:0";
            tb.FontSize = 20;
            Color forecolor = getColor(0, 0, 0);
            tb.Foreground = new SolidColorBrush(forecolor);
            Color backcolor = getColor(255, 255, 255);
            tb.Background = new SolidColorBrush(backcolor);
            tb.BorderBrush = new SolidColorBrush(backcolor);
            tb.TextAlignment = TextAlignment.Center;
            tb.Visibility = Visibility.Visible;
            Canvas.SetLeft(tb,-100);
            Canvas.SetTop(tb, 0);


            
            LayoutRoot.MouseMove += new MouseEventHandler(DianxueGame_MouseMove);
            
            int speed = 100;
            time.Tick += new EventHandler(gameLoop_Tick);
            time.Interval = TimeSpan.FromMilliseconds(speed);
            time.Start();
            
        }


        public void DianxueGame_MouseMove(Object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        Boolean pointInRect(Point point, Rect rect)
        {
            if (point.X > rect.Left && point.X < rect.Right && point.Y > rect.Top && point.Y < rect.Bottom)
            {
                return true;
            }
            return false;
        }

        Boolean overlap(Rect rect1, Rect rect2)
        {
            if (pointInRect(new Point(rect1.Left, rect1.Top), rect2))
            {
                return true;
            }
            if (pointInRect(new Point(rect1.Left, rect1.Bottom), rect2))
            {
                return true;
            }
            if (pointInRect(new Point(rect1.Right, rect1.Top), rect2))
            {
                return true;
            }
            if (pointInRect(new Point(rect1.Right, rect1.Bottom), rect2))
            {
                return true;
            }
            if (pointInRect(new Point(rect2.Left, rect2.Top), rect1))
            {
                return true;
            }
            if (pointInRect(new Point(rect2.Left, rect2.Bottom), rect1))
            {
                return true;
            }
            if (pointInRect(new Point(rect2.Right, rect2.Top), rect1))
            {
                return true;
            }
            if (pointInRect(new Point(rect2.Right, rect2.Bottom), rect1))
            {
                return true;
            }
            return false;
        }

        Boolean rectOverlap(int top, int left, int height, int width, List<Rect> rectList)
        {
            if (top < 10 || top > 470 || left < 10 || left > 670)
            {
                return true;
            }
            Rect newRect = new Rect(new Point(top, left), new Point(top + height, left + width));
            foreach(var rect in rectList)
            {
                if (overlap(newRect, rect))
                {
                    return true;
                }
            }
            return false;
        }

        void gameOver()
        {

            if (callBack != null)
            {
                if (point < 0)
                {
                    point = 0;
                }
                callBack(itemsNum,point);
                callBack = null;
            }

            onetime = 0;
            keepTimes = 0;
            point = 0;
            //itemNum = 0;
            times = 0;
            time = new DispatcherTimer();

            foreach (var image in imageList)
            {
                LayoutRoot.Children.Remove(image);
            }
            LayoutRoot.Children.Remove(tb);
            imageList.Clear();

        }

        void gameLoop_Tick(object sender, EventArgs e)
        {
            //changeCursor();
            
            if (onetime == 20)
            {
                onetime = 0;
                times++;
            }

            if (times >= 10)
            {
                time.Stop();
                gameOver();
            }

            if (onetime % 20 == 0)
            {
                List<Rect> rectList = new List<Rect>();
                foreach (var image in imageList)
                {

                    int top,left;
                    do
                    {
                        int ranNum = new Random().Next(0, 480000);
                        top = ranNum / 800;
                        left = ranNum % 800;
                        
                        //top = new Random().Next(10, 470);
                        //left = new Random().Next(10, 670);
                    } while (rectOverlap(top, left, (int)image.Height, (int)image.Width, rectList));


                    Rect rect = new Rect(new Point(top, left), new Point(top+image.Height, left+image.Width));
                    rectList.Add(rect);

                    if (((string)image.Tag).Length > 1 || ((string)image.Tag)=="0")
                    {

                        string randomItemName = "";
                        int ra = new Random().Next(0, 1000);
                        if (ra < 3)
                        {
                            randomItemName = "物品.血刀";
                        }
                        else if (ra < 6)
                        {
                            randomItemName = "物品.乌蚕衣";
                        }
                        else if (ra < 12)
                        {
                            randomItemName = "物品.金丝道袍";
                        }
                        else if (ra < 18)
                        {
                            randomItemName = "物品.柳叶刀";
                        }
                        else if (ra < 23)
                        {
                            randomItemName = "物品.黄金项链";
                        }
                        else if (ra < 33)
                        {
                            randomItemName = "物品.生生造化丹";
                        }
                        else if (ra < 83)
                        {
                            randomItemName = "物品.九转熊蛇丸";
                        }
                        else if (ra < 133)
                        {
                            randomItemName = "物品.冬虫夏草";
                        }
                        else if (ra < 550)
                        {
                            randomItemName = "物品.大蟠桃";
                        }
                        else
                        {
                            randomItemName = "物品.大还丹";
                        }


                        image.Source = ResourceManager.GetImage(randomItemName);
                        image.Tag = randomItemName;
                    }

                    
                    
                    Canvas.SetLeft(image,left);
                    Canvas.SetTop(image, top);
                    Canvas.SetZIndex(image, 100);
                    image.Visibility = Visibility.Visible;
                }
                dahuandanChecked = 0;
            }
            else if (onetime % 16 == 0)
            {
                imageList[4].Visibility = Visibility.Collapsed;
            }
            else if (onetime % 14 == 0)
            {
                imageList[3].Visibility = Visibility.Collapsed;
            }
            else if (onetime % 12 == 0)
            {
                imageList[2].Visibility = Visibility.Collapsed;
            }
            else if (onetime % 10 == 0)
            {
                imageList[1].Visibility = Visibility.Collapsed;
            }
            else if (onetime % 6 == 0 && dahuandanChecked==0)
            {
                imageList[0].Visibility = Visibility.Collapsed;
            }
            
            onetime++;
        }
    }
}
