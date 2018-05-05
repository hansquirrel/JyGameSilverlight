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
    public partial class RolePanel : UserControl
    {
        public UIHost uiHost;
        public RolePanel()
        {
            InitializeComponent();

            //this.wuqi.MouseLeave += hideSkillInfo;
            //this.fangju.MouseLeave += hideSkillInfo;
            //this.teshu.MouseLeave += hideSkillInfo;
            //this.jingshu.MouseLeave += hideSkillInfo;
            //this.icon_daofa.MouseLeave += hideSkillInfo;
            //this.icon_jianfa.MouseLeave += hideSkillInfo;
            //this.icon_qimen.MouseLeave += hideSkillInfo;
            //this.icon_quanzhang.MouseLeave += hideSkillInfo;

            //this.icon_bili.MouseLeave += hideSkillInfo;
            //this.icon_shenfa.MouseLeave += hideSkillInfo;
            //this.icon_gengu.MouseLeave += hideSkillInfo;
            //this.icon_fuyuan.MouseLeave += hideSkillInfo;
            //this.icon_dingli.MouseLeave += hideSkillInfo;
            //this.icon_wuxing.MouseLeave += hideSkillInfo;
            
            ToolTipService.SetToolTip(this.icon_daofa, CommonSettings.AttributeDesc("daofa"));
            ToolTipService.SetToolTip(this.icon_jianfa, CommonSettings.AttributeDesc("jianfa"));
            ToolTipService.SetToolTip(this.icon_qimen, CommonSettings.AttributeDesc("qimen"));
            ToolTipService.SetToolTip(this.icon_quanzhang, CommonSettings.AttributeDesc("quanzhang"));

            ToolTipService.SetToolTip(this.icon_bili,CommonSettings.AttributeDesc(this.icon_bili.Text));
            ToolTipService.SetToolTip(this.icon_shenfa, CommonSettings.AttributeDesc(this.icon_shenfa.Text));
            ToolTipService.SetToolTip(this.icon_gengu, CommonSettings.AttributeDesc(this.icon_gengu.Text));
            ToolTipService.SetToolTip(this.icon_fuyuan, CommonSettings.AttributeDesc(this.icon_fuyuan.Text));
            ToolTipService.SetToolTip(this.icon_dingli, CommonSettings.AttributeDesc(this.icon_dingli.Text));
            ToolTipService.SetToolTip(this.icon_wuxing, CommonSettings.AttributeDesc(this.icon_wuxing.Text));
            


            AddButtons = new List<Button>()
            {
                addbili,addwuxing,addgengu,adddingli,addshenfa,addfuyuan
            };
        }

        private bool EquipChangable = false;

        /// <summary>
        /// 隐藏框以及关闭按钮等
        /// </summary>
        public void HideUI()
        {
            //if(!_hidedUi)
            //    this.dragElement.Detach();

            this.layoutRoot.Background.Opacity = 0;
            this.closeButton.Visibility = System.Windows.Visibility.Collapsed;
            _hidedUi = true;
        }
        private bool _hidedUi = false;

        public void Show(Role role,bool isList=false)
        {
            lock (this)
            {
                _isList = isList;
                if (isList == false) //只显示一个,战斗中
                {
                    this.EquipChangable = false;
                    SetAddPointButtonVisiable(false);
                }
                else
                {
                    this.EquipChangable = true;
                    SetAddPointButtonVisiable(role.LeftPoint > 0);
                }
                this.currentRole = role;

                this.bili.Text = role.GetAttributeString("bili");
                this.wuxing.Text = role.GetAttributeString("wuxing");
                this.fuyuan.Text = role.GetAttributeString("fuyuan");
                this.gengu.Text = role.GetAttributeString("gengu");
                this.dingli.Text = role.GetAttributeString("dingli");
                this.shenfa.Text = role.GetAttributeString("shenfa");
                this.detailPanel.Show(role);

                this.leftpoint.Text = role.LeftPoint.ToString();

                this.quanzhang.Width = 143 * (role.AttributesFinal["quanzhang"] / (float)CommonSettings.MAX_ATTRIBUTE);
                this.jian.Width = 143 * (role.AttributesFinal["jianfa"] / (float)CommonSettings.MAX_ATTRIBUTE);
                this.dao.Width = 143 * (role.AttributesFinal["daofa"] / (float)CommonSettings.MAX_ATTRIBUTE);
                this.qimen.Width = 143 * (role.AttributesFinal["qimen"] / (float)CommonSettings.MAX_ATTRIBUTE);

                this.quanzhangText.Text =
                    role.GetAttributeString("quanzhang");
                this.jianfaText.Text =
                    role.GetAttributeString("jianfa");
                this.daofaText.Text =
                    role.GetAttributeString("daofa");
                this.qimenText.Text =
                    role.GetAttributeString("qimen");

                this.attackText.Text = string.Format("{0}", (int)(role.Attack * 10));
                this.defenceText.Text = string.Format("{0}", (int)(role.Defence * 10));
                this.criticalText.Text = string.Format("{0}%", (int)(role.Critical * 100));

                this.skills.Children.Clear();
                foreach (var s in role.SpecialSkills)
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = string.Format("{0}", s.Skill.Name),
                        Foreground = new SolidColorBrush() { Color = Colors.Cyan },
                        Tag = s.DetailInfo, 
                        FontFamily = new FontFamily("SimHei")
                    };
                    ToolTipService.SetToolTip(textBlock, s.GenerateToolTip());
                    this.skills.Children.Add(textBlock);
                }

                foreach (var s in role.Skills)
                {
                    TextBlock textBlock = new TextBlock() 
                    {
                        Text = string.Format("{0} {1} {2}", s.Skill.Name, s.Level, s.IsUsed ? "√" : ""), 
                        Foreground = new SolidColorBrush() { Color = Colors.White }, 
                        Tag = s.DetailInfo ,
                        FontFamily = new FontFamily("SimHei")
                    };

                    RichTextBox rt = s.GenerateToolTip();
                    ToolTipService.SetToolTip(textBlock, rt);

                    if (isList)
                    {
                        (rt.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                        (rt.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                        (rt.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = "(鼠标左键点击启用/禁用该武功)", FontFamily = new FontFamily("SimHei") });
                        SkillInstance skillInstance = s;
                        textBlock.MouseLeftButtonUp += (ss, e) =>
                        {
                            skillInstance.IsUsed = !skillInstance.IsUsed;
                            AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                            this.Dispatcher.BeginInvoke(() => this.Refresh());
                        };
                    }
                    //textBlock.MouseEnter += showSkillInfo;
                    //textBlock.MouseLeave += hideSkillInfo;
                    this.skills.Children.Add(textBlock);

                    foreach (var us in s.UniqueSkillInstances)
                    {
                        if (s.Level < us.Skill.RequireLevel) continue;
                        TextBlock textBlockTmp = new TextBlock()
                        {
                            Text = string.Format("绝招 {0}", us.Skill.Name),
                            Foreground = new SolidColorBrush() { Color = Colors.Red },
                            Tag = us.DetailInfo,
                            FontFamily = new FontFamily("SimHei")
                        };
                        ToolTipService.SetToolTip(textBlockTmp, us.GenerateToolTip());
                        //textBlockTmp.MouseEnter += showSkillInfo;
                        //textBlockTmp.MouseLeave += hideSkillInfo;
                        this.skills.Children.Add(textBlockTmp);
                    }
                }

                foreach (var s in role.InternalSkills)
                {
                    TextBlock textBlock = new TextBlock() { 
                        Text = string.Format("{0} {1} {2}", s.Skill.Name, s.Level, s.Equipped ? "√" : ""),
                        Foreground = new SolidColorBrush() { Color = Colors.Yellow }, 
                        Tag = s.GenerateToolTip(),
                        FontFamily = new FontFamily("SimHei")
                    };
                    
                    if (isList)
                    {
                        if (s != role.GetEquippedInternalSkill())
                        {
                            RichTextBox rt = textBlock.Tag as RichTextBox;
                            (rt.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                            (rt.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                            (rt.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = "(鼠标左键点击切换到该内功)", FontFamily = new FontFamily("SimHei") });
                        }
                        InternalSkillInstance instance = s;
                        textBlock.MouseLeftButtonUp += (ss, e) =>
                        {
                            role.EquipInternalSkill(instance);
                            AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                            this.Dispatcher.BeginInvoke(() => this.Refresh());
                        };
                    }
                    else
                    {
                        //if (s != role.GetEquippedInternalSkill())
                        //    textBlock.Tag += "\n\n战斗时不允许切换内功";
                    }
                    ToolTipService.SetToolTip(textBlock, textBlock.Tag);
                    //textBlock.MouseEnter += showSkillInfo;
                    //textBlock.MouseLeave += hideSkillInfo;
                    this.skills.Children.Add(textBlock);

                    foreach (var us in s.UniqueSkillInstances)
                    {
                        if (s.Level < us.Skill.RequireLevel) continue;
                        TextBlock textBlockTmp = new TextBlock()
                        {
                            Text = string.Format("绝招 {0}", us.Skill.Name),
                            Foreground = new SolidColorBrush() { Color = Colors.Red },
                            Tag = us.GenerateToolTip(),
                            FontFamily = new FontFamily("SimHei")
                        };
                        //textBlockTmp.MouseEnter += showSkillInfo;
                        //textBlockTmp.MouseLeave += hideSkillInfo;
                        this.skills.Children.Add(textBlockTmp);
                        ToolTipService.SetToolTip(textBlockTmp, textBlockTmp.Tag);
                    }
                }

                this.wuqi.Reset();
                this.fangju.Reset();
                this.teshu.Reset();
                this.jingshu.Reset();

                if (role.Equipment[(int)ItemType.Weapon] != null)
                {
                    this.wuqi.BindItem(role.Equipment[(int)ItemType.Weapon]);
                }
                if (role.Equipment[(int)ItemType.Armor] != null)
                {
                    this.fangju.BindItem(role.Equipment[(int)ItemType.Armor]);
                }
                if (role.Equipment[(int)ItemType.Accessories] != null)
                {
                    this.teshu.BindItem(role.Equipment[(int)ItemType.Accessories]);
                }
                if (role.Equipment[(int)ItemType.Book] != null)
                {
                    this.jingshu.BindItem(role.Equipment[(int)ItemType.Book]);
                }

                this.exp.Text = string.Format("{0}/{1}", role.Exp, role.LevelupExp);
                if (role.Level >= CommonSettings.MAX_LEVEL)
                    this.exp.Text = "-/-";
                this.level.Text = role.Level.ToString();

                //天赋栏
                talentPanel.Children.Clear();

                //天赋
                foreach (var t in role.Talents)
                {
                    TextBlock tb = new TextBlock() { Text = t, Foreground = new SolidColorBrush(Colors.Yellow), FontFamily = new FontFamily("SimHei") };
                    string tmp = Talent.GetTalentInfo(t);
                    ToolTipService.SetToolTip(tb, tmp);
                    talentPanel.Children.Add(tb);
                }

                //装备天赋
                foreach (var t in role.EquipmentTalents)
                {
                    TextBlock tb = new TextBlock() { Text = t, Foreground = new SolidColorBrush(Colors.Red), FontFamily = new FontFamily("SimHei") };
                    string tmp = Talent.GetTalentInfo(t, false);
                    ToolTipService.SetToolTip(tb, tmp);
                    talentPanel.Children.Add(tb);
                }

                //内功天赋
                foreach (var t in role.InternalTalents)
                {
                    TextBlock tb = new TextBlock() { Text = t, Foreground = new SolidColorBrush(Colors.Purple), FontFamily = new FontFamily("SimHei") };
                    string tmp = Talent.GetTalentInfo(t , false);
                    ToolTipService.SetToolTip(tb, tmp);
                    talentPanel.Children.Add(tb);
                }

                //人物介绍
                if (ResourceManager.ResourceMap.ContainsKey("人物." + role.Name))
                {
                    string roleInfoText = ResourceManager.Get("人物." + role.Name);
                    roleInfoTextBox.Text = roleInfoText;
                    roleInfo.Visibility = System.Windows.Visibility.Visible;
                    roleInfo.Text = "人物百科:" + role.Name;
                }
                else
                {
                    roleInfo.Visibility = System.Windows.Visibility.Collapsed;
                }
                roleInfoCanvas.Visibility = System.Windows.Visibility.Collapsed;

                //this.spiritImage.Source = RoleManager.GetAnimationTempalte(role.Animation).Images[Spirit.SpiritStatus.Standing][0];
                this.spiritImage.Source = role.GetAnimation("stand")[0].Image;

                //武学常识
                int totalWuxueCost = role.GetTotalWuxueCost();
                string wuxueInfo = string.Format("{0}/{1}", totalWuxueCost, role.Attributes["wuxue"]);
                this.WuxueTextBlock.Text = wuxueInfo;

                this.Visibility = System.Windows.Visibility.Visible;
            }
        }

        //人物介绍

        private void roleInfoCloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            roleInfoCanvas.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void Refresh()
        {
            this.Show(this.currentRole, _isList);
        }

        private Role currentRole;
        private bool _isList;

        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        #region 装备显示及拆卸
        private void wuqi_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InfoEquipment(ItemType.Weapon);
        }

        private void fangju_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InfoEquipment(ItemType.Armor);
        }

        private void teshu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InfoEquipment(ItemType.Accessories);
        }

        private void jingshu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InfoEquipment(ItemType.Book);
        }

        private void wuqi_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TakeoffEquipment(ItemType.Weapon);
        }

        private void fangju_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TakeoffEquipment(ItemType.Armor);
        }

        private void teshu_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TakeoffEquipment(ItemType.Accessories);
        }

        private void jingshu_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TakeoffEquipment(ItemType.Book);
        }

        private void InfoEquipment(ItemType t)
        {
            //if (this.currentRole.Equipment[(int)t] != null)
            //{
            //    //description.Text = this.currentRole.Equipment[(int)t].ToString();
            //    //if (this.EquipChangable)
            //    //    description.Text += "(点击卸下)";
            //    //info.Visibility = System.Windows.Visibility.Visible;
            //    string text = this.currentRole.Equipment[(int)t].ToString();
            //    if (this.EquipChangable)
            //        text += "(点击卸下)";
            //    string tmp = (string)ToolTipService.GetToolTip(sender as DependencyObject);
            //    if(tmp==null || tmp==string.Empty || tmp != text)
            //        ToolTipService.SetToolTip(sender as DependencyObject, text);
            //}
        }

        private void TakeoffEquipment(ItemType t)
        {
            if (this.currentRole.Equipment[(int)t] != null && this.EquipChangable)
            {
                Item tmp = this.currentRole.Equipment[(int)t];
                RuntimeData.Instance.Items.Add(tmp);
                this.currentRole.Equipment[(int)t] = null;
                description.Text = "";
                AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                this.Dispatcher.BeginInvoke(() => 
                {
                    this.Refresh();
                    uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);
                });
            }
        }
        #endregion

        #region 角色分配点数
        private List<Button> AddButtons;
        public void SetAddPointButtonVisiable(bool can)
        {
            if (!can)
            {
                foreach (var btn in AddButtons) { btn.Visibility = System.Windows.Visibility.Collapsed; }
            }
            else
            {
                foreach (var btn in AddButtons) { btn.Visibility = System.Windows.Visibility.Visible; }
            }
        }
        private void addbili_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("bili");
        }

        private void addwuxing_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("wuxing");
        }

        private void addshenfa_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("shenfa");
        }

        private void addfuyuan_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("fuyuan");
        }

        private void addgengu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("gengu");
        }

        private void adddingli_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAttribute("dingli");
        }

        private void AddAttribute(string attr)
        {
            if (this.currentRole.Attributes[attr] < 150)
            {
                this.currentRole.Attributes[attr]++;
                this.currentRole.LeftPoint--;
                AudioManager.PlayEffect(ResourceManager.Get("音效.加点"));
                this.Refresh();
            }
        }

        private void TextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            string content = ((TextBlock)sender).Text;
            description.Text = CommonSettings.AttributeDesc(content);
            info.Visibility = System.Windows.Visibility.Visible;
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = ((Image)(sender));
            if (img == this.icon_jianfa)
            {
                description.Text = CommonSettings.AttributeDesc("jianfa");
            }
            if (img == this.icon_daofa)
            {
                description.Text = CommonSettings.AttributeDesc("daofa");
            }
            if (img == this.icon_quanzhang)
            {
                description.Text = CommonSettings.AttributeDesc("quanzhang");
            }
            if (img == this.icon_qimen)
            {
                description.Text = CommonSettings.AttributeDesc("qimen");
            }
            info.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(currentRole.GenerateRoleXml().ToString());
        }
        #endregion

        #region 天赋

        private void ShowTalentsInfo()
        {
            description.Text = currentRole.TalentsString;
        }

        private void roleInfo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            roleInfoCanvas.Visibility = System.Windows.Visibility.Visible;
        }


        #endregion
    }
}
