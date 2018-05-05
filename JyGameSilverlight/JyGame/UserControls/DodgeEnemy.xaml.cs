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

namespace JyGame.UserControls
{
    public partial class DodgeEnemy : UserControl
    {
        public int ContainerWidth { get; set; }
        public int ContainerHeight { get; set; }
        public DodgeEnemy()
        {
            InitializeComponent();
        }
        public double X
        {
            get { return BlueRectPoint.X; }
            set { BlueRectPoint.X = value; }
        }
        public double Y
        {
            get { return BlueRectPoint.Y; }
            set { BlueRectPoint.Y = value; }
        }
        public double Speed { get; set; }

        private bool hDirect = true;
        private bool wDirect = true;

        public double InitSpeedY { get; set; }
        public double InitSpeedX { get; set; }
        public void Dimension(double _w, double _h)
        {
            this.Width = _w;
            this.Height = _h;
            BlueRect.Width = _w;
            BlueRect.Height = _h;
        }
        public bool Move
        {
            set
            {
                if (value)
                {
                    if (this.Y <= 0) hDirect = true;

                    if (hDirect && (this.Y < (ContainerHeight - this.Height)))
                    {
                        this.Y += Speed * InitSpeedY;

                    }
                    else
                    {
                        this.Y -= Speed * InitSpeedY;
                        hDirect = false;

                    }

                    if (this.X <= 0) wDirect = true;

                    if (wDirect && (this.X < (ContainerWidth - this.Width)))
                    {
                        this.X += Speed * InitSpeedX;

                    }
                    else
                    {
                        this.X -= Speed * InitSpeedX;
                        wDirect = false;

                    }

                }

            }
        }
    }
}
