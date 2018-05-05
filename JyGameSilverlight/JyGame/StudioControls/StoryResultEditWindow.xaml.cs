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
using System.Xml.Linq;

namespace JyGame
{
    public partial class StoryResultEditWindow : ChildWindow
    {
        public StoryResultEditWindow()
        {
            InitializeComponent();
        }


        public List<JyGame.GameData.Result> Results
        {
            get
            {
                return _results;
            }
            set
            {
                _results = value;
                this.Refresh();
            }
        }
        private List<JyGame.GameData.Result> _results = null;

        private void Refresh()
        {
            XElement root = new XElement("results");
            foreach (var r in _results)
            {
                root.Add(r.GenerateXml());
            }
            this.ResultContentText.Text = root.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = this.ResultContentText.Text;
                XElement root = XElement.Parse(text);
                List<JyGame.GameData.Result> newResults = new List<GameData.Result>();
                foreach (var r in root.Elements("result"))
                {
                    newResults.Add(GameData.Result.Parse(r));
                }
                _results = newResults;

                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("格式错误！");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        
    }
}

