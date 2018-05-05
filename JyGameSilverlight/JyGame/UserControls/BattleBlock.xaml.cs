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
using JyGame.Logic;

namespace JyGame.UserControls
{
    public enum BattleBlockStatus
    {
        IDLE,
        SelectMove,
        SelectAttack,
        SelectItem,
    };

    public partial class BattleBlock : UserControl
    {
        public BattleBlock(BattleField bfield, UIHost uihost)
        {
            InitializeComponent();
            this.MouseEnter += new MouseEventHandler(BattleBlock_MouseEnter);
            this.MouseLeave += new MouseEventHandler(BattleBlock_MouseLeave);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(BattleBlock_MouseLeftButtonUp);
            battleField = bfield;
            this.uiHost = uihost;

            ResetColor();
        }

        private static SolidColorBrush DefaultColor = new SolidColorBrush(Colors.Transparent);
        private static SolidColorBrush MouseMoveColor = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush SelectiveColor = new SolidColorBrush(Colors.Blue);
        private static SolidColorBrush AttackColor = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush RelatedColor = new SolidColorBrush(Colors.Purple);
        private static SolidColorBrush ItemColor = new SolidColorBrush(Colors.Red);

        public void ResetColor()
        {
            this.Status = BattleBlockStatus.IDLE;
        }

        public List<LocationBlock> RelatedBlocks = null;
        private BattleBlockStatus _status = BattleBlockStatus.IDLE;

        //block.SelectiveColor = Colors.Red;
        public BattleBlockStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                switch (value)
                {
                    case BattleBlockStatus.IDLE:
                        this.Rect.Fill = DefaultColor;
                        break;
                    case BattleBlockStatus.SelectAttack:
                        this.Rect.Fill = AttackColor;
                        break;
                    case BattleBlockStatus.SelectMove:
                        this.Rect.Fill = SelectiveColor;
                        break;
                    case BattleBlockStatus.SelectItem:
                        this.Rect.Fill = ItemColor;
                        break;
                    default:
                        MessageBox.Show("invalid battle block status");
                        break;
                }
                _status = value;
            }
        }

        public void ResetStatus()
        {
            this.Status = _status;
        }

        public BattleField battleField { get; set; }
        public UIHost uiHost { get; set; }

        void BattleBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.Status == BattleBlockStatus.IDLE)
            {
                Spirit s = battleField.GetSpirit(X, Y);
                if (s != null)
                {
                    uiHost.rolePanel.Show(s.Role);
                }
                return;
            }

            battleField.OnSelectBlock(X, Y);

            //MessageBox.Show(String.Format("{0}-{1}", X, Y));
        }

        void BattleBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Status == BattleBlockStatus.SelectMove)
                Rect.Fill = SelectiveColor;
            if (Status == BattleBlockStatus.SelectItem)
                Rect.Fill = ItemColor;

            else if (Status == BattleBlockStatus.SelectAttack)
            {
                HideRelatedBlocks();
                Rect.Fill = AttackColor;
            }

            if (battleField.currentSpirit == null) return;
            if (battleField.currentSpirit.Role != null)
            {
                uiHost.roleDetailPanel_float.Hide();
                uiHost.battleFieldContainer.suggestInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void Suggest(string info)
        {
            uiHost.battleFieldContainer.suggestInfo.Text = info;
            uiHost.battleFieldContainer.suggestInfo.Visibility = System.Windows.Visibility.Visible;
        }

        void ShowRelatedBlocks()
        {
            foreach (var b in this.RelatedBlocks)
            {
                if (b.X >= 0 && b.X < battleField.actualXBlockNo && b.Y >= 0 && b.Y < battleField.actualYBlcokNo)
                {
                    battleField.blockMap[b.X, b.Y].Rect.Fill = RelatedColor;
                }
            }
        }

        public void HideRelatedBlocks()
        {
            foreach (var b in this.RelatedBlocks)
            {
                if (b.X >= 0 && b.X < battleField.actualXBlockNo && b.Y >= 0 && b.Y < battleField.actualYBlcokNo)
                {
                    battleField.blockMap[b.X, b.Y].ResetStatus();
                }
            }
        }

        void BattleBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Status == BattleBlockStatus.SelectMove)
            {
                Rect.Fill = MouseMoveColor;
            }
            else if (Status == BattleBlockStatus.SelectAttack)
            {
                ShowRelatedBlocks();
            }


            if (Man != null && Status == BattleBlockStatus.IDLE)
            {
                uiHost.roleDetailPanel_float.Show(Man.Role);
                Suggest(string.Format("点击获取[{0}]的状态", Man.Role.Name));
            }
            if (battleField.Status == BattleStatus.SelectMove && Status == BattleBlockStatus.SelectMove)
            {
                Suggest("移动到该点");
            }
            if (battleField.Status == BattleStatus.SelectAttack && Status == BattleBlockStatus.SelectAttack)
            {
                Suggest("选择该点为技能施展目标");
            }
        }

        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
                Canvas.SetLeft(this, CommonSettings.SPIRIT_X_MARGIN + _x * CommonSettings.SPIRIT_BLOCK_SIZE);
            }
        }
        private int _x;

        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                Canvas.SetTop(this, CommonSettings.SPIRIT_Y_MARGIN + _y * CommonSettings.SPIRIT_BLOCK_SIZE);
            }
        }
        private int _y;

        /// <summary>
        /// 获取当前格上的人物
        /// </summary>
        public Spirit Man
        {
            get
            {
                return battleField.GetSpirit(X, Y);
            }
        }
    }
}
