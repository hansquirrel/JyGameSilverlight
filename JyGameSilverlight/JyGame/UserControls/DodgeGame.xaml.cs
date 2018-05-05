using System;
using System.Windows;
using System.Windows.Controls;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public partial class DodgeGame : UserControl
    {
        DodgeGameManager gm;
        DodgeMan me;
        DodgeDragManager dm;
        DateTime startTime;
        public CommonSettings.IntCallBack callBack = null;
        public bool IsCheat = false;
        public DodgeGame()
        {
            InitializeComponent();
        }

        private bool isComponentInited = false;
        private void ComponentInit()
        {
            if (isComponentInited) return;
            dm = new DodgeDragManager(LayoutRoot);
            me = new DodgeMan();
            LayoutRoot.Children.Add(me);
            gm = new DodgeGameManager(LayoutRoot, me);
            gm.GameRun += new EventHandler(gm_GameRun);
            gm.GameOver += new EventHandler(gm_GameOver);

            foreach (DodgeEnemy enemy in gm.enemies)
                LayoutRoot.Children.Add(enemy);
            isComponentInited = true;
        }

        void Init()
        {
            ComponentInit();
            IsCheat = false;
            me.X = 400;
            me.Y = 300;
            gm.MoveSpeed = 1.0;
            dm.EnableDragableElement(me);
            dm.OnDragMove += dm_OnDragMove;
            dm.OnFirstTimeMove += dm_MoveEventArgs;
        }
        void dm_OnDragMove(object sender, EventArgs e)
        {
            if (((me.X + me.Width >= 800) || (me.X <= 0)) || (((me.Y + me.Height >= 600) || (me.Y <= 0))))
            {
                gm.OnGameOver(e);
                //gm_GameOver(sender, e);
            }
        }
        void dm_MoveEventArgs(object sender, EventArgs e)
        {
            startTime = DateTime.Now;
            gm.Play();
        }
        void gm_GameRun(object sender, EventArgs e)
        {
        }
        void gm_GameOver(object sender, EventArgs e)
        {
            dm.DisableDragableElement();
            gm.Pause();
            //resultBtn.Visibility = Visibility.Visible;
            double s = (DateTime.Now - this.startTime).TotalSeconds;
            double s2 = (double)gm.PassedTimeInMs / 1000.0;
            if (s > s2 * 4)
                IsCheat = true;
            //string info = string.Format("s={0} , s2={1}", s, s2);
            //MessageBox.Show(info);
            //resultBtn.Content = "你坚持了" + s.ToString() + "秒";
            if (callBack != null)
            {
                callBack((int)s);
                callBack = null;
            }
        }
        void resultBtn_Click(object sender, RoutedEventArgs e)
        {
            //resultBtn.Visibility = Visibility.Collapsed;
            Init();
            gm.Resart();
        }

        public void start()
        {
            Init();
            gm.Resart();
        }
    }
}
