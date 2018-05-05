using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.UserControls;
using System.Collections.Generic;
using JyGame.GameData;
using System.Collections.ObjectModel;

namespace JyGame.UserControls
{
	public partial class Log : UserControl
	{
        public Log()
		{
			InitializeComponent();
		}

        public void Show()
        {
            
            this.Visibility = Visibility.Visible;

            logPanel.Children.Clear();
            nickPanel.Children.Clear();
            string[] logs = RuntimeData.Instance.Log.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i=logs.Length-1;i>=0;--i)
            {
                logPanel.Children.Add(new TextBlock() { Text = logs[i], FontSize = 12 });
            }

            StackPanel sp = null;
            int k = 0;
            foreach(var rm in ResourceManager.ResourceMap)
            {
                if (rm.Key.StartsWith("nick."))
                {
                    if (k % 5 == 0)
                    {
                        sp = new StackPanel() { 
                            Orientation = Orientation.Horizontal, 
                            Margin = new Thickness(2, 2, 2, 2),
                            
                        };
                        nickPanel.Children.Add(sp);
                        k = 0;
                    }
                    k++;
                    string nick = rm.Key.Replace("nick.", "");
                    string detail = rm.Value;
                    if (RuntimeData.Instance.Nicks.Contains(nick))
                    {
                        TextBlock tb = new TextBlock() { Text = nick + "\t", FontSize = 16 };
                        if (nick == RuntimeData.Instance.CurrentNick)
                        {
                            tb.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        ToolTipService.SetToolTip(tb, detail);
                        sp.Children.Add(tb);
                        tb.MouseLeftButtonUp += (s, e) =>
                        {
                            RuntimeData.Instance.CurrentNick = nick;
                            RuntimeData.Instance.gameEngine.uihost.mapUI.nickLabel.Text = nick;
                            this.Dispatcher.BeginInvoke(() => { this.Show(); });
                        };
                    }
                    else
                    {
                        TextBlock tb = new TextBlock() { Text = nick + "\t", FontSize = 16 };
                        tb.Opacity = 0.3;
                        ToolTipService.SetToolTip(tb, detail);
                        sp.Children.Add(tb);
                    }
                }
                
            }
        }

        private void copyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(RuntimeData.Instance.Log);
            MessageBox.Show("已复制日志，快show给你的朋友们吧！");
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void nickCopyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string tmp = "";
            foreach (var nick in RuntimeData.Instance.Nicks)
            {
                string detail = ResourceManager.Get("nick." + nick);
                if (detail == null) detail = "";
                tmp += string.Format("{0} : {1}\n", nick, detail);
            }
            Clipboard.SetText(tmp);
            MessageBox.Show("已复制称号，快show给你的朋友们吧！");
        }
	}
}