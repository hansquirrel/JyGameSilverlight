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
    public enum AttackInfoType
    {
        Hit, //攻击
        CriticalHit, //暴击
    }

    public enum TextCastOrder
    {
        PreAction,
        Action,
    }

    public class AttackInfoInstance
    {
        public string Info;
        public AttackInfoType Type;
        public Color Color;
    }
   
    public partial class Spirit : UserControl
    {
        public int battleID = 0;

        public enum SpiritStatus
        { 
            Standing = 0,
            Moving,
            Attacking,
        }
        private string[] StatusMap = new string[] { "stand", "move", "attack" };

        //public Dictionary<SpiritStatus, List<ImageSource>> Images
        //{
        //    get
        //    {
        //        return RoleManager.GetAnimationTempalte(RoleAnimationTempalte).Images;
        //    }
        //}

        //public Dictionary<SpiritStatus, int> ImageWidths = null;
        //public Dictionary<SpiritStatus, int> ImageHeights = null;

        #region AttackInfo

        private void AddAttackInfo(AttackInfoInstance attackInfo)
        {
            if (AttackInfoControls.Count == 0)
            {
                SpiritAttackInfo sp = new SpiritAttackInfo();
                sp.Go(this, attackInfo);
            }
            else
            {
                int waitTime = AttackInfoControls.Count * 500;
                //int waitTime = 0;
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, waitTime);
                timer.Tick += (s,e) =>
                {
                    SpiritAttackInfo sp = new SpiritAttackInfo();
                    sp.Go(this, attackInfo);
                    timer.Stop();
                };
                timer.Start();
            }
        }

        public void AddAttackInfo(string info, Color color,AttackInfoType type = AttackInfoType.Hit, TextCastOrder order = TextCastOrder.Action)
        {
            //联机模式下记录信息
            if (OLBattleGlobalSetting.Instance.OLGame && ParentBattleField.myTurn)
            {
                BattleWord word = new BattleWord();
                word.word = info; word.color = color;

                if (order == TextCastOrder.PreAction)
                {
                    if (!OLBattleGlobalSetting.Instance.battleData.preRoleAttackInfo.ContainsKey(battleID))
                        OLBattleGlobalSetting.Instance.battleData.preRoleAttackInfo.Add(battleID, new List<BattleWord>());

                    OLBattleGlobalSetting.Instance.battleData.preRoleAttackInfo[battleID].Add(word);
                }
                else if (order == TextCastOrder.Action)
                {
                    if (!OLBattleGlobalSetting.Instance.battleData.roleAttackInfo.ContainsKey(battleID))
                        OLBattleGlobalSetting.Instance.battleData.roleAttackInfo.Add(battleID, new List<BattleWord>());

                    OLBattleGlobalSetting.Instance.battleData.roleAttackInfo[battleID].Add(word);
                }
            }

            this.AddAttackInfo(new AttackInfoInstance() { Info = info, Type = type, Color = color });
        }

        private List<SpiritAttackInfo> _attackInfoControls = new List<SpiritAttackInfo>();
        public List<SpiritAttackInfo> AttackInfoControls
        {
            get
            {
                lock (this) { return _attackInfoControls; }
            }
            set
            {
                lock (this) { _attackInfoControls = value; }
            }
        }
        #endregion

        #region 对话框
        public void ShowSmallDialog(string info)
        {
            smallDialogBox.Visibility = System.Windows.Visibility.Visible;
            smallDialogBox.content.Text = info;
            smallDialogBox.Storyboard1.Begin();
        }

        void SmallDialogStoryboard1_Completed(object sender, EventArgs e)
        {
            smallDialogBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        public void ScrollIntoView(CommonSettings.VoidCallBack callback)
        {
            this.ParentBattleField.battleFieldContainer.cameraScrollTo(this.CanvasLeft, this.CanvasTop, callback);
        }

        public int ItemCd = 0; //药物CD

        /// <summary>
        /// 角色动画模板
        /// </summary>
        public string RoleAnimationTempalte;

        public Spirit(Role role, BattleField parentBattleField, Visibility isVisible, string RoleAnimationTempalte)
        {
            InitializeComponent();
            Status = SpiritStatus.Standing;
            this.IsEnabled = false;
            this.ItemCd = 0;
            Role = role;
            ParentBattleField = parentBattleField;

            this.smallDialogBox.head.Source = role.Head;
            this.smallDialogBox.Storyboard1.Completed += new EventHandler(SmallDialogStoryboard1_Completed);
            this.smallDialogBox.Visibility = System.Windows.Visibility.Collapsed;

            this.RoleAnimationTempalte = RoleAnimationTempalte;
            //ImageWidths = RoleManager.GetAnimationTempalte(RoleAnimationTempalte).imageWidths;
            //ImageHeights = RoleManager.GetAnimationTempalte(RoleAnimationTempalte).imageHeights;
            //Body.Width = 75 * ImageWidths[Status] / ImageHeights[Status];
            ////Body.Height = ImageHeights[Status];
            //Body.Height = 75;

            this.HPTip.Visibility = isVisible;
            this.MPTip.Visibility = isVisible;
            this.SPTip.Visibility = isVisible;
            this.HpTipCanvas.Visibility = isVisible;
            this.MpTipCanvas.Visibility = isVisible;
            if (/*Configer.Instance.JiqiAnimation && */isVisible == System.Windows.Visibility.Visible)
                this.SpTipCanvas.Visibility = System.Windows.Visibility.Visible;
            else
                this.SpTipCanvas.Visibility = System.Windows.Visibility.Collapsed;

            this.Hp = role.Attributes["hp"];
            this.Mp = role.Attributes["mp"];

            this.Sp = 0;

            foreach(var k in StatusMap)
            { 
                List<AnimationImage> tmp = Role.GetAnimation(k);
                if( tmp != null && tmp.Count > 0)
                {
                    AnimationImages.Add(k, tmp);
                }
            }

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(CommonSettings.SPRITE_SWITCH_WAITTIME);
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Start();
            FocusRingAimation.RepeatBehavior = RepeatBehavior.Forever;
            FocusRingAimation.AutoReverse = true;

            this.nameText.Visibility = System.Windows.Visibility.Visible;
            this.nameText.Text = role.Name;

            this.Die.Completed += (s, e) =>
            {
                this.Timer.Stop();
                ParentBattleField.RootCanvas.Children.Remove(this);
            };
        }

        ~Spirit()
        {
            Remove();
        }

        /// <summary>
        /// 已经复活的次数
        /// </summary>
        public int FuhuoCount = 0;

        public void Remove()
        {
            try
            {
                HpTipCanvas.Visibility = System.Windows.Visibility.Collapsed;
                MpTipCanvas.Visibility = System.Windows.Visibility.Collapsed;
                SpTipCanvas.Visibility = System.Windows.Visibility.Collapsed;
                nameText.Visibility = System.Windows.Visibility.Collapsed;
                this.Die.Begin();
            }
            catch (Exception e)
            {

            }
        }

        int picCurrent = 0;
        int standSwitchIndex = 0;

        public List<string> GetAnimationImages()
        {
            List<string> rst = new List<string>();

            //角色图片
            for (int i = 0; i < StatusMap.Length; ++i)
            {
                List<AnimationImage> images = Role.GetAnimation(StatusMap[i]);
                if (images != null)
                {
                    foreach (var img in images)
                    {
                        rst.Add(img.Url);
                    }
                }
            }

            //技能图片
            foreach (var s in this.Role.Skills)
            {
                if (s.Animation.Images != null)
                {
                    foreach (var img in s.Animation.Images)
                    {
                        rst.Add(img.Url);

                        //相关奥义
                        foreach (var aoyi in SkillManager.GetAoyis())
                        {
                            if (aoyi.Start == s.Skill.Name)
                            {
                                if (aoyi.Animations != null)
                                {
                                    foreach (var imgaoyi in aoyi.Animations.Images)
                                    {
                                        rst.Add(imgaoyi.Url);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return rst;
        }

        private Dictionary<string, List<AnimationImage>> AnimationImages = new Dictionary<string, List<AnimationImage>>();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Status == SpiritStatus.Standing) //站立的时候4个回调切换一次，否则像抽风
            {
                if (standSwitchIndex != 0)
                {
                    standSwitchIndex++;
                    if (standSwitchIndex > 2)
                        standSwitchIndex = 0;
                    return;
                }
                standSwitchIndex++;
            }

            List<AnimationImage> images = null;
            string statusStr = StatusMap[(int)Status];
            if (!AnimationImages.ContainsKey(statusStr))
            {
                images = AnimationImages["stand"];
            }
            else
            {
                images = AnimationImages[statusStr];
            }
            if (images == null)
                MessageBox.Show(string.Format("error, animation {0}/stand not exist",Role.Animation));
            if (picCurrent >= images.Count)
            {
                picCurrent = 0;
                if (Status == SpiritStatus.Attacking)
                {
                    Status = SpiritStatus.Standing;
                }
            }
            AnimationImage currentImage = images[picCurrent];
            Body.Source = currentImage.Image;
            Body.Height = currentImage.H * CommonSettings.SpiritScaleRate;
            Body.Width = currentImage.W * CommonSettings.SpiritScaleRate;
            
            if (FaceRight)
            {
                Canvas.SetLeft(Body, this.ActualWidth / 2 - currentImage.AnchorX * CommonSettings.SpiritScaleRate);
            }
            else
            {
                Canvas.SetLeft(Body, this.ActualWidth / 2 - (Body.Width - currentImage.AnchorX * CommonSettings.SpiritScaleRate));
            }
            Canvas.SetTop(Body, this.ActualHeight - currentImage.AnchorY * CommonSettings.SpiritScaleRate);

            picCurrent++;
        }

        public SpiritStatus Status
        {
            get { return _status; }
            set
            {
                standSwitchIndex = 0;
                _status = value;
                picCurrent = 0;
            }
        }
        private SpiritStatus _status;
        public Role Role { get; set; }

        public BattleField ParentBattleField { get; set; }
        public int MoveAbility 
        {
            get 
            {
                if (this.Role.GetBuff("定身") != null)
                    return 0;
                if (this.Role.HasTalent("金刚伏魔圈"))
                    return 0;
                BuffInstance buffInstance = this.Role.GetBuff("缓速"); //缓速buff
                double slowMove = 0;
                if (buffInstance != null)
                {
                    slowMove = buffInstance.Level * 1.5;
                }
                int rst = Role.AttributesFinal["shenfa"] / 40 + 2 - (int)slowMove;
                BuffInstance qingshenBuff = this.Role.GetBuff("轻身");
                if (qingshenBuff != null)
                {
                    rst += qingshenBuff.Level + 1;
                }
                if (this.Role.HasTalent("轻功高超"))
                {
                    rst += 1;
                }
                if (this.Role.HasTalent("瘸子"))
                {
                    rst -= 1;
                }
                if (rst > 5) rst = 5;
                return rst <= 0 ? 1 : rst;
            } 
        }

        private DispatcherTimer Timer;
        private bool isAI = false;
        public void Move(List<MoveSearchHelper> way, bool isAI)
        {
            Status = SpiritStatus.Moving;
            this.isAI = isAI;
            MoveWay = way;
            moved = 0;
            Move(moved);
        }
        List<MoveSearchHelper> MoveWay;
        int moved;
        Storyboard moveStoryboard = null;

        private bool _faceRight = true;
        public bool FaceRight
        {
            set
            {
                if (value == _faceRight) return;
                if (value)
                {
                    Body.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                }
                else
                {
                    Body.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                    Body.RenderTransformOrigin = new Point(0.4, 0.5); //0.4是因为图片本身不左右绝对对齐
                }
                _faceRight = value;
            }
            get
            {
                return _faceRight;
            }
        }

        private void Move(int count)
        {
            if (MoveWay.Count == 0)
            {
                moved = 1;
                MoveWay.Clear();
                if (OLBattleGlobalSetting.Instance.OLGame && (!ParentBattleField.myTurn))
                {
                    ParentBattleField.OnOLDisplayMoveFinish();
                }
                else if (isAI)
                {
                    ParentBattleField.OnAIMovedFinish();
                }
                else
                {
                    ParentBattleField.OnMovedFinish();
                }
                Status = SpiritStatus.Standing;
                return;
            }
                
            int x = MoveWay[moved].X;
            int y = MoveWay[moved].Y;

            if (x<X)
            {
                FaceRight = false;
            }
            if (x>X)
            {
                FaceRight = true;
            }

            DoubleAnimation MoveAnimX = new DoubleAnimation();
            MoveAnimX.From = SwitchX(X);
            MoveAnimX.To = SwitchX(x);
 
            MoveAnimX.Duration = new Duration(new TimeSpan(0, 0, 0, 0, CommonSettings.MOVE_WAITTIME));
            DoubleAnimation MoveAnimY = new DoubleAnimation();
            MoveAnimY.From = SwitchY(Y);
            MoveAnimY.To = SwitchY(y);
            MoveAnimY.Duration = new Duration(new TimeSpan(0, 0, 0, 0, CommonSettings.MOVE_WAITTIME));

            moveStoryboard = new Storyboard();
            moveStoryboard.Duration = new Duration(new TimeSpan(0, 0, 0, 0, CommonSettings.MOVE_WAITTIME));
            moveStoryboard.AutoReverse = false;
            moveStoryboard.Children.Add(MoveAnimY);
            moveStoryboard.Children.Add(MoveAnimX);

            Storyboard.SetTarget(MoveAnimX, this);
            Storyboard.SetTarget(MoveAnimY, this);
            Storyboard.SetTargetProperty(MoveAnimY, new PropertyPath("(Canvas.Top)"));
            Storyboard.SetTargetProperty(MoveAnimX, new PropertyPath("(Canvas.Left)"));

            moveStoryboard.Completed += (s, e) => 
            {
                moved++;
                if (moved < MoveWay.Count)
                {
                    Move(moved);
                }
                else
                {
                    moved = 1;
                    MoveWay.Clear();
                    if(OLBattleGlobalSetting.Instance.OLGame && (!ParentBattleField.myTurn))
                    {
                        ParentBattleField.OnOLDisplayMoveFinish();
                    }
                    else if (isAI)
                    {
                        ParentBattleField.OnAIMovedFinish();
                    }
                    else
                    {
                        ParentBattleField.OnMovedFinish();
                    }
                    Status = SpiritStatus.Standing;
                }
            };
            moveStoryboard.Begin();
            X = x;
            Y = y;
        }

        private double SwitchX(int x)
        {
            return CommonSettings.SPIRIT_X_MARGIN + (x) * CommonSettings.SPIRIT_BLOCK_SIZE + CommonSettings.SPIRIT_BLOCK_SIZE/2 - LayoutRoot.ActualWidth / 2;
        }

        private double SwitchY(int y)
        {
            return CommonSettings.SPIRIT_Y_MARGIN + (y + 1) * CommonSettings.SPIRIT_BLOCK_SIZE - LayoutRoot.ActualHeight - 20;
        }

        public double CanvasLeft
        {
            get { return SwitchX(X); }
        }

        public double CanvasTop
        {
            get { return SwitchY(Y); }
        }

        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
                Canvas.SetLeft(this, SwitchX(_x));
            }
        }
        private int _x;

        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                Canvas.SetTop(this, SwitchY(_y));
                Canvas.SetZIndex(this, CommonSettings.Z_SPIRIT + value);
            }
        }
        private int _y;

        public int Team
        {
            get { return _team; }
            set
            {
                _team = value;
                this.nameText.Foreground = new SolidColorBrush(CommonSettings.TeamColor[_team]);
            }
        }
        private int _team;

        public int Hp
        {
            get { return Role.Attributes["hp"]; }
            set
            {
                Role.Attributes["hp"] = value;
                if (Role.Attributes["hp"] > Role.Attributes["maxhp"])
                    Role.Attributes["hp"] = Role.Attributes["maxhp"];
                if (Role.Attributes["hp"] >= 0)
                {
                    this.HPTip.Width = (double)Role.Attributes["hp"] / (double)Role.Attributes["maxhp"] * (double)HpTipCanvas.Width;
                }
            }
        }

        public int Mp
        {
            get { return Role.Attributes["mp"]; }
            set
            {
                Role.Attributes["mp"] = value;
                if (Role.Attributes["mp"] > Role.Attributes["maxmp"])
                    Role.Attributes["mp"] = Role.Attributes["maxmp"];
                if (Role.Attributes["mp"] >= 0)
                {
                    this.MPTip.Width = (double)Role.Attributes["mp"] / (double)Role.Attributes["maxmp"] * (double)MpTipCanvas.Width;
                }
            }
        }

        private double _sp = 0;
        public double Sp
        {
            get { return _sp; }
            set
            {
                _sp = value;
                if (_sp > 100) _sp = 100;
                if (_sp < 0) _sp = 0;
                this.SPTip.Width = _sp / 100 * (double)SpTipCanvas.Width;
            }
        }

        public void Refresh() //主要是处理ROLE的属性变化，而spirit所属的界面没有更新
        {
            this.Mp = this.Role.Attributes["mp"];
            this.Hp = this.Role.Attributes["hp"];
            this.Sp = this._sp;

            this.FillBuffPanel();   
        }

        private void FillBuffPanel()
        {
            this.buffPanel.Children.Clear();
            foreach (var buffInstance in this.Role.Buffs)
            {
                TextBlock tb = new TextBlock() { 
                    FontSize = 10
                };
                tb.Foreground = buffInstance.IsDebuff ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Yellow);

                tb.Text = buffInstance.ToString();
                buffPanel.Children.Add(tb);
            }

            //BUFF特效显示区
            if (this.Role.GetBuff("易容") != null)
            {
                this.Body.Opacity = 0.3;
            }
            else
            {
                this.Body.Opacity = 1;
            }
        }

        private bool _focus = false;
        public bool IsCurrent
        {
            get { return _focus; }
            set
            {
                _focus = value;
                if (_focus)
                {
                    FocusRingAimation.Begin();
                    FocusRing.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    FocusRingAimation.Stop();
                    FocusRing.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
