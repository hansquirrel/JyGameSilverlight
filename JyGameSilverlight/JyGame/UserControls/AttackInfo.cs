using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public class AttackInfo
    {
        private AttackInfo(Canvas rootCanvas, int x, int y, string Info, Color fontColor,int yoffset=80)
        {
            this.Info = Info;
            X = x;
            Y = y;
            RootCanvas = rootCanvas;
            FontColor = fontColor;
            _yoffset = yoffset;
            this.Start();
        }
        private int _yoffset;

        private void Start()
        {
            int FromX = X;
            int FromY = Y;
            int TargetY = FromY - _yoffset;

            AttackInfoText = new TextBlock();
            AttackInfoText.Text = Info.ToString();
            AttackInfoText.FontSize = 20;
            AttackInfoText.IsHitTestVisible = false;
            AttackInfoText.FontWeight = FontWeights.ExtraBold;
            
            AttackInfoText.Foreground = new SolidColorBrush(FontColor);

            RootCanvas.Children.Add(AttackInfoText);
            Canvas.SetLeft(AttackInfoText, FromX);
            Canvas.SetTop(AttackInfoText, FromY);
            Canvas.SetZIndex(AttackInfoText, CommonSettings.Z_ATTACKINFO);

            //创建动画
            DoubleAnimation MoveAnimY = new DoubleAnimation();
            MoveAnimY.From = FromY;
            MoveAnimY.To = TargetY;
            MoveAnimY.Duration = new Duration(new TimeSpan(0, 0, 0, 2, 0));

            DoubleAnimation AnimOpc = new DoubleAnimation();
            AnimOpc.From = 1;
            AnimOpc.To = 0;
            AnimOpc.Duration = new Duration(new TimeSpan(0, 0, 0, 2, 0));

            Storyboard Sb = new Storyboard();
            Sb.Duration = new Duration(new TimeSpan(0, 0, 0, 2, 0));
            Sb.AutoReverse = false;
            Sb.Children.Add(MoveAnimY);
            Sb.Children.Add(AnimOpc);

            Storyboard.SetTarget(MoveAnimY, AttackInfoText);
            Storyboard.SetTarget(AnimOpc, AttackInfoText);
            Storyboard.SetTargetProperty(MoveAnimY, new PropertyPath("(Canvas.Top)"));
            Storyboard.SetTargetProperty(AnimOpc, new PropertyPath("Opacity"));

            Sb.Completed += (s, e) => 
            {
                Sb.Stop();
                RootCanvas.Children.Remove(AttackInfoText);
            };
            Sb.Begin();
        }

        private string Info;
        private int X { get; set; }
        private int Y { get; set; }
        private TextBlock AttackInfoText;
        private Canvas RootCanvas { get; set; }
        private Color FontColor { get; set; }
    }
}
