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
    public partial class SkillSelectPanel : UserControl
    {
        public delegate void OnSelectSkillDelegate(SkillBox skill);

        public OnSelectSkillDelegate Callback;
        public SkillSelectPanel()
        {
            InitializeComponent();
            SkillContainer.Orientation = Orientation.Vertical;
        }

        public void Show(Role r)
        {
            this.SkillContainer.Children.Clear();

            List<SkillBox> avaliableSkills = r.GetAvaliableSkills();
            foreach (var s in avaliableSkills)
            {
                this.AddSkill(s);
            }

            foreach (var s in r.InternalSkills)
            {
                if (s != r.GetEquippedInternalSkill())
                {
                    this.AddSkill(new SkillBox() { IsSwitchInternalSkill = true, SwitchInternalSkill = s });
                }
                else
                {
                    this.AddSkill(new SkillBox() { IsSwitchInternalSkill = true, SwitchInternalSkill = s }, false);
                }
            }
        }

        //显示可以被洗练的武功
        public void ShowXilian(Role r)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.SkillContainer.Children.Clear();
            foreach(var s in r.Skills)
            {
                this.AddSkill(new SkillBox() { Instance = s });
            }

            foreach (var s in r.InternalSkills)
            {
                if (!s.Equipped)
                    this.AddSkill(new SkillBox() { IsSwitchInternalSkill = true, SwitchInternalSkill = s, XilianTag = true});
            }
        }

        private void AddSkill(SkillBox box, bool isEnable = true)
        {
            TextBlock skillButton = new TextBlock()
            {
                Text = string.Format("{0}",box.Name) ,
                Foreground = null,
                FontSize = 12,
                FontFamily = new FontFamily("SimHei")
             };
            if(box.StatusInfo != string.Empty)
                ToolTipService.SetToolTip(skillButton, box.StatusInfo);
            if (box.IsSwitchInternalSkill) skillButton.Foreground = new SolidColorBrush(Colors.Purple);
            else if (box.IsUnique) skillButton.Foreground = new SolidColorBrush(Colors.Red);
            else if (box.IsSpecial) skillButton.Foreground = new SolidColorBrush(Colors.Cyan);
            else skillButton.Foreground = new SolidColorBrush(Colors.White);

            if (box.Status == SkillStatus.Ok && isEnable)
            {
                skillButton.MouseLeftButtonUp += (s, e) =>
                    {
                        Callback(box);
                    };
            }
            else
            {
                skillButton.Opacity = 0.35;
            }
            skillButton.MouseEnter += (s, e) =>
                {
                    skillButton.Foreground = new SolidColorBrush(Colors.Orange);

                    skillinfo.Xaml = box.GenerateToolTip(false).Xaml.Replace("#FF000000", "#FFFFFFFF"); //将黑色的字变成白色的
                    //skillinfo.Blocks.Add(box.GenerateToolTip().Blocks[0]);
                };
            skillButton.MouseLeave += (s,e)=>
                {
                    if (box.IsSwitchInternalSkill) skillButton.Foreground = new SolidColorBrush(Colors.Purple);
                    else if (box.IsUnique) skillButton.Foreground = new SolidColorBrush(Colors.Red);
                    else if (box.IsSpecial) skillButton.Foreground = new SolidColorBrush(Colors.Cyan);
                    else skillButton.Foreground = new SolidColorBrush(Colors.White);
                    skillinfo.Blocks.Clear();
                };

            SkillContainer.Children.Add(skillButton);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Callback(null);
        }
    }
}
