using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.GameData;
using System.Collections.Generic;

namespace JyGame
{
	public partial class SaveLoadPanel : UserControl
	{
        public UIHost uiHost;
        private bool IsSave;

		public SaveLoadPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show(bool isSave)
        {
            this.IsSave = isSave;
            if (isSave)
            {
                okButton.Content = "保存";
                suggestInfo.Text = "请选择或输入要保存的存档";
            }
            else
            {
                okButton.Content = "读取";
                suggestInfo.Text = "请选择或输入要读取的存档";
            }

            List<SaveInfo> savefilelist = RuntimeData.Instance.GetSaveList();
            listPanel.Children.Clear();
            foreach (var s in savefilelist)
            {
                string filepath = s.Name;
                SaveInfo info = s;
                TextBlock saveFile = new TextBlock()
                {
                    Text = filepath,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 22
                };
                saveFile.MouseEnter += (ss,e) =>
                {
                    saveFile.Foreground = new SolidColorBrush(Colors.Yellow);
                };
                saveFile.MouseLeave += (ss, e) =>
                {
                    saveFile.Foreground = new SolidColorBrush(Colors.White);
                };
                saveFile.MouseLeftButtonUp += (ss, e) =>
                {
                    fileText.Text = filepath;
                    saveDetail.Text = info.ToString();
                };

                listPanel.Children.Add(saveFile);
            }

            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string file = fileText.Text;
            if (file.Contains("\\") || file.Contains("/"))
            {
                MessageBox.Show("文件名格式错误，不能包含/和\\");
                return;
            }

            if (IsSave)
            {
                try
                {
                    if(RuntimeData.Instance.Save(file))
                        MessageBox.Show("保存成功!");
                }
                catch
                {
                    MessageBox.Show("保存失败!");
                }
            }
            else
            {
                try
                {
                    RuntimeData.Instance.Load(file);
                    if (this.LoadCallback != null)
                        LoadCallback();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("读取失败!文件可能损坏!");
                }
            }
            this.Hide();
        }

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void Refresh()
        {
            this.Show(IsSave);
        }

        public CommonSettings.VoidCallBack LoadCallback = null;

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string file = fileText.Text;
            MessageBoxResult rst = MessageBox.Show("确认要删除吗？", "删除确认", MessageBoxButton.OKCancel);
            if (rst == MessageBoxResult.OK)
            {
                try
                {
                    RuntimeData.Instance.DeleteSave(file);
                    this.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("删除失败");
                    return;
                }
            }
            else
            {

            }
        }
	}
}