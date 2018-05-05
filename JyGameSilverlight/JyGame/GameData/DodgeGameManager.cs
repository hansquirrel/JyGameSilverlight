using System;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public class DodgeGameManager
    {
        /// <summary>
        /// 游戏状态 ready Play Pause over
        /// </summary>
        public enum Gamestatus
        {
            Ready,
            Pause,
            Play,
            Over
        }
        public Gamestatus gamestatus { get; set; }
        /// <summary>
        /// 使用时间
        /// </summary>
        private DispatcherTimer time;
        /// <summary>
        /// 移动速度
        /// </summary>
        public double MoveSpeed { get; set; }
        public List<DodgeEnemy> enemies;
        private DodgeMan Owner;
        private Canvas layoutRoot;
        DodgeEnemy enemy1;
        DodgeEnemy enemy2;
        DodgeEnemy enemy3;
        DodgeEnemy enemy4;

        public const int TIME_INTERVAL = 10;
        
        public DodgeGameManager(Canvas _layoutRoot, DodgeMan _Owner)
        {
            enemies = new List<DodgeEnemy>();
            this.Owner = _Owner;
            this.layoutRoot = _layoutRoot;
            time = new DispatcherTimer();
            time.Interval = TimeSpan.FromMilliseconds(TIME_INTERVAL);
            time.Tick += new EventHandler(time_Tick);
            
            enemy1 = new DodgeEnemy();
            enemy2 = new DodgeEnemy();
            enemy3 = new DodgeEnemy();
            enemy4 = new DodgeEnemy();

            enemies.Add(enemy1);
            enemies.Add(enemy2);
            enemies.Add(enemy3);
            enemies.Add(enemy4);

            InitEnemisPoint();
        }
        /// <summary>
        /// 初始化蓝色（敌人）的位置
        /// </summary>
        void InitEnemisPoint()
        {
            timecount = 0;
            PassedTimeInMs = 0;

            enemy1.X = 70; //初始化位置
            enemy1.Y = 70;
            enemy1.InitSpeedY = 2;
            enemy1.InitSpeedX = 6;
            enemy1.Dimension(80, 80);
            enemy1.ContainerWidth = (int)layoutRoot.Width;
            enemy1.ContainerHeight = (int)layoutRoot.Height;
            ImageBrush brush1 = new ImageBrush();
            brush1.ImageSource = ResourceManager.GetImage("头像.东");
            brush1.Stretch = Stretch.Fill;
            enemy1.BlueRect.Fill = brush1;

            enemy2.X = 570; //初始化位置
            enemy2.Y = 60;
            enemy2.InitSpeedY = 5;
            enemy2.InitSpeedX = 3;
            enemy2.Dimension(80, 80);
            enemy2.ContainerWidth = (int)layoutRoot.Width;
            enemy2.ContainerHeight = (int)layoutRoot.Height;
            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = ResourceManager.GetImage("头像.南");
            brush2.Stretch = Stretch.Fill;
            enemy2.BlueRect.Fill = brush2;

            enemy3.X = 70; //初始化位置
            enemy3.Y = 450;
            enemy3.InitSpeedY = 3;
            enemy3.InitSpeedX = 5;
            enemy3.Dimension(80, 80);
            enemy3.ContainerWidth = (int)layoutRoot.Width;
            enemy3.ContainerHeight = (int)layoutRoot.Height;
            ImageBrush brush3 = new ImageBrush();
            brush3.ImageSource = ResourceManager.GetImage("头像.西");
            brush3.Stretch = Stretch.Fill;
            enemy3.BlueRect.Fill = brush3;

            enemy4.X = 580; //初始化位置
            enemy4.Y = 430;
            enemy4.InitSpeedY = 4;
            enemy4.InitSpeedX = 4;
            enemy4.Dimension(80, 80);
            enemy4.ContainerWidth = (int)layoutRoot.Width;
            enemy4.ContainerHeight = (int)layoutRoot.Height;
            ImageBrush brush4 = new ImageBrush();
            brush4.ImageSource = ResourceManager.GetImage("头像.北");
            brush4.Stretch = Stretch.Fill;
            enemy4.BlueRect.Fill = brush4;

            gamestatus = Gamestatus.Ready;
        }
        /// <summary>
        /// 游戏开始
        /// </summary>
        public void Play()
        {
            gamestatus = Gamestatus.Play;
            time.Start();
        }
        /// <summary>
        /// 游戏重新开始
        /// </summary>
        public void Resart()
        {
            InitEnemisPoint();
        }
        public void Pause()
        {
            time.Stop();
            gamestatus = Gamestatus.Pause;
        }
        private int timecount = 0;
        public int PassedTimeInMs = 0;
        private void time_Tick(object sender, EventArgs e)
        {
            OnGameRun(null);
            foreach (DodgeEnemy enemy in enemies)
                MoveEnemy(enemy);
            timecount++;
            PassedTimeInMs += TIME_INTERVAL;
            if (timecount == 100)
            {
                MoveSpeed += 0.1;
                timecount = 0;
            }
        }
        void MoveEnemy(DodgeEnemy enemy)
        {
            if (gamestatus != Gamestatus.Play)
                return;
            if (IsImpact(enemy))
            {
                enemy.Move = true;
                enemy.Speed = MoveSpeed;
            }
            else
            {
                OnGameOver(null);
            }
        }
        bool IsImpact(DodgeEnemy enemy)
        {
            return IsImpactForOwner(enemy.X, enemy.Y, enemy.Width, enemy.Height);
        }
        bool IsImpactForOwner(double x, double y, double w, double h)
        {
            bool bCollision = true;
            double PointX = Owner.X - Convert.ToInt32(x + w);
            double PointY = Owner.Y - Convert.ToInt32(y + h);
            if ((PointX <= 0 && x <= (Owner.X + Owner.Width)) && (PointY <= 0 && y <= (Owner.Y + Owner.Height)))
            {
                bCollision = false;
            }
            return bCollision;
        }
        public event EventHandler GameOver;
        public event EventHandler GameRun;
        public void OnGameOver(EventArgs e)
        {
            time.Stop(); gamestatus = Gamestatus.Over;
            EventHandler handler = GameOver;
            if (handler != null)
                handler(this, e);
        }
        void OnGameRun(EventArgs e)
        {
            EventHandler handler = GameRun;
            if (handler != null)
                handler(this, e);
        }

    }
}
