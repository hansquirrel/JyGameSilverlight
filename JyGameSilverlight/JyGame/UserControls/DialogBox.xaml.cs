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
using System.Windows.Threading;
using JyGame.GameData;

namespace JyGame.UserControls
{
    public partial class DialogBox : UserControl
    {
        public UIHost uihost = null;

        public DialogBox()
        {
            InitializeComponent();
        }

        //by cg 2013-10-26
        public JyGame.GameData.CommonSettings.IntCallBack CallBack;

        public void ShowDialog(string role, string info, CommonSettings.IntCallBack callback)
        {
            this.Focus();
            showDialogFlagNew = true;
            this.Head.Source = RoleManager.GetRole(role).Head;

            this.Text.Text = info;
            //if (this.Text.Text.Contains("$FEMALE$"))
            //    this.Text.Text.Replace("$FEMALE$", RuntimeData.Instance.femaleName);

            if (role == "女主")
                this.RoleName.Text = RuntimeData.Instance.femaleName + ":";
            else if (role == "主角")
            {
                this.RoleName.Text = RuntimeData.Instance.maleName + ":";
                this.Head.Source = RuntimeData.Instance.Team[0].Head;
            }
            else
                this.RoleName.Text = RoleManager.GetRole(role).Name + ":";
            
            this.CallBack = callback;
            this.Visibility = System.Windows.Visibility.Visible;

            foreach (SceneHead head in uihost.scence.heads)
            {
                if (head.roleKey == role)
                {
                    uihost.dialogIndicator.Margin = new Thickness(head.Margin.Left + CommonSettings.MAPUI_ROLEHEAD_WIDTH + 20, head.Margin.Top, 0, 0);
                    uihost.dialogIndicator.Visibility = Visibility.Visible;
                    uihost.dialogIndicator.start_tick();
                    break;
                }
            }
        }

        //以下是为了兼容老的调用代码

        private bool showDialogFlagNew = false;
        private CommonSettings.VoidCallBack oldCallback = null;
        public void ShowDialog(Dialog dialog, CommonSettings.IntCallBack callback = null,bool isBattle = false)
        {
            showDialogFlagNew = false;
            List<Dialog> dialogs = new List<Dialog>();
            dialogs.Add(dialog);
            this.ShowDialogs(dialogs, callback);
        }

        public void ShowDialogs(List<Dialog> dialogs, CommonSettings.IntCallBack callback = null, bool isBattle = false)
        {
            this.Focus();
            showDialogFlagNew = false;
            this.CallBack = callback;
            currentDialogs = dialogs;
            currentIndex = -1;
            this.Dispatcher.BeginInvoke(() => { this.NextDialog(isBattle); });
        }
        private void NextDialog(bool isBattle = false)
        {
            currentIndex++;
            if (currentIndex >= currentDialogs.Count)
            {
                if (CallBack != null)
                {
                    this.CallBack(0);
                }
            }
            else
            {
                Dialog dialog = currentDialogs[currentIndex];
                if (isBattle)//如果是战斗，先需要滚屏
                {
                    bool find = false;
                    foreach(var sp in uihost.battleFieldContainer.field.Spirits)
                    {
                        sp.IsCurrent = false;
                        string roleName = dialog.role;
                        if (roleName == "主角")
                        {
                            roleName = RuntimeData.Instance.maleName;
                        }
                        else if (roleName == "铃兰")
                        {
                            roleName = RuntimeData.Instance.femaleName;
                        }
                        if (sp.Role.Name == roleName)
                        {
                            sp.IsCurrent = true;
                            uihost.battleFieldContainer.field.currentSpirit = sp;
                            sp.ScrollIntoView(() => {
                                //ShowDialogOld(dialog.role, dialog.info, () => { this.Dispatcher.BeginInvoke(() => { this.NextDialog(isBattle); }); });
                                ShowDialogOld(dialog.role, dialog.info, () => { this.NextDialog(isBattle); });
                            });
                            find = true;
                        }
                    }
                    if (!find)
                    {
                        ShowDialogOld(dialog.role, dialog.info, () => { this.NextDialog(isBattle); });
                    }
                }
                else
                {
                    ShowDialogOld(dialog.role, dialog.info, () => { this.NextDialog(isBattle); });
                }
            }
        }
        private void ShowDialogOld(string role, string info, CommonSettings.VoidCallBack callback)
        {
            this.Focus();
            this.Head.Source = RoleManager.GetRole(role).Head;
            this.Text.Text = info;
            string roleName = RoleManager.GetRole(role).Name;
            if (roleName == "小虾米")
            {
                roleName = RuntimeData.Instance.maleName;
                this.Head.Source = RuntimeData.Instance.Team[0].Head;
            }
            else if (role == "铃兰")
            {
                roleName = RuntimeData.Instance.femaleName;
            }
            this.RoleName.Text = roleName + ":";

            this.Visibility = System.Windows.Visibility.Visible;
            this.oldCallback = callback;
        }
        private List<Dialog> currentDialogs = null;
        private int currentIndex = -1;

        public void Hide()
        {
            uihost.dialogPanel.Visibility = Visibility.Collapsed;
            uihost.dialogIndicator.stop_tick();
            uihost.dialogIndicator.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            uihost.dialogIndicator.stop_tick();
            uihost.dialogIndicator.Visibility = Visibility.Collapsed;
            this.Visibility = System.Windows.Visibility.Collapsed;
            if (showDialogFlagNew)
            {
                if (CallBack != null)
                {
                    this.CallBack(0);
                }
            }
            else
            {
                if (oldCallback != null)
                {
                    this.oldCallback();
                }
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Storyboard1.RepeatBehavior = RepeatBehavior.Forever;
            this.Storyboard1.Begin();
        }
    }
}
