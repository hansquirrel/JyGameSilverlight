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
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace JyGame.GameData
{

    public enum ItemType
    {
        Costa = 0,
        Weapon = 1, //武器
        Armor = 2, //防具
        Accessories = 3, //饰品
        Book = 4,
        Mission = 5,
        SpeicalSkillBook = 6,
        TalentBook = 7,
        Upgrade = 8,
        Special = 9, //特殊物品
        Canzhang = 10,
    }

    /// <summary>
    /// 物品
    /// </summary>
    public class Item
    {
        public Item() { }

        public bool Used
        {
            get
            {
                lock(this)
                {
                    return _used;
                }
            }
            set
            {
                lock (this)
                {
                    _used = value;
                }
            }
        }
        private bool _used = false;

        public bool IsCheat
        {
            get
            {
                if (AdditionTriggers.Count > 5) //个数过多
                { 
                    return true; 
                }
                foreach(var t in AdditionTriggers)
                {
                    if (t.IsCheat(this.level)) //超范围
                        return true;
                }
                return false;
            }
        }

        public string Name { get; set; }
        public string Info { get; set; }
        public string PicInfo;
        public ImageSource Pic { get { return ResourceManager.GetImage(PicInfo); } }
        public List<ItemTrigger> Triggers = new List<ItemTrigger>();
        public ItemRequire Require { get; set; }
        public int Type;
        public int level = 1;
        public int price;
        public string CanzhangSkill = string.Empty;


        public Color GetColor()
        {
            if (AdditionTriggers.Count >= 4)
            {
                return Colors.Magenta;
            }
            else if (AdditionTriggers.Count == 3)
            {
                return (Colors.Yellow);
            }
            else if (AdditionTriggers.Count == 2)
            {
                return (Colors.Green);
            }
            else if (AdditionTriggers.Count == 1)
            {
                return (Colors.Blue);
            }

            if (Name.EndsWith("残章"))
            {
                return (Colors.Orange);
            }
            if (Type == (int)ItemType.Upgrade)
            {
                return (Colors.Red);
            }
            if (Type == (int)ItemType.TalentBook)
            {
                return (Colors.Brown);
            }
            return Colors.White;
        }
        
        /// <summary>
        /// 是否随机掉落
        /// </summary>
        public bool IsDrop = false; 

        public List<ItemTrigger> AllTriggers
        {
            get
            {
                List<ItemTrigger> rst = new List<ItemTrigger>();
                rst.AddRange(Triggers);
                rst.AddRange(AdditionTriggers);
                return rst;
            }
        }

        /// <summary>
        /// 附加trigger
        /// </summary>
        //public List<ItemTrigger> AdditionTriggers = new List<ItemTrigger>();

        public List<ItemTrigger> AdditionTriggers = new List<ItemTrigger>();
        public XElement GenerateAdditionTriggersXml()
        {
            XElement rst = new XElement("addition_triggers");
            foreach(var t in this.AdditionTriggers)
            {
                rst.Add(t.GenerateXml());
            }
            return rst;
        }
        public void SetAdditionTriggers(XElement xml)
        {
            this.AdditionTriggers.Clear();
            foreach(var trigger in xml.Elements("trigger"))
            {
                ItemTrigger t = new ItemTrigger();
                t.Name = Tools.GetXmlAttribute(trigger, "name");
                t.ArgvsString = Tools.GetXmlAttribute(trigger, "argvs");
                this.AdditionTriggers.Add(t);
            }
        }

        //public int yuanbao;
        public int Cooldown = 0; //冷却时间

        public List<string> Talent = new List<string>();
        public bool HasTalent(string talent)
        {
            if(Talent.Contains(talent)) return true;
            foreach(var t in this.Triggers)
            {
                if (t.Name == "talent" && t.Argvs[0] == talent) return true;
            }
            foreach(var t in this.AdditionTriggers)
            {
                if (t.Name == "talent" && t.Argvs[0] == talent) return true;
            }
            return false;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("item");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("desc", Info);
            rst.SetAttributeValue("pic", PicInfo);
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("level", level);
            rst.SetAttributeValue("price", price);
            rst.SetAttributeValue("drop", IsDrop);
            if (Talent.Count > 0)
            {
                rst.SetAttributeValue("talent", string.Join("#", Talent.ToArray()));
            }
            foreach(var t in Triggers)
            {
                rst.Add(t.GenerateXml());
            }
            return rst;
        }


        public XElement ToXml()
        {
            XElement itemNode = new XElement("item");
            itemNode.SetAttributeValue("type", this.Type);
            itemNode.SetAttributeValue("name", this.Name);
            if (AdditionTriggers.Count > 0)
            {
                itemNode.Add(this.GenerateAdditionTriggersXml());
            }
            return itemNode;
        }

        XElement myNode;
        static public Item Parse(XElement node)
        {
            Item rst = new Item();
            rst.myNode = node;
            rst.Name = Tools.GetXmlAttribute(node, "name");
            rst.Info = Tools.GetXmlAttribute(node, "desc");
            rst.PicInfo = Tools.GetXmlAttribute(node, "pic");
            rst.Type = Tools.GetXmlAttributeInt(node, "type");
            rst.level = Tools.GetXmlAttributeInt(node, "level");
            if (node.Attribute("price") != null)
            {
                rst.price = Tools.GetXmlAttributeInt(node, "price");
            }
            else
            {
                rst.price = 0;
            }

            /*if (node.Attribute("yuanbao") != null)
            {
                rst.yuanbao = Tools.GetXmlAttributeInt(node, "yuanbao");
            }
            else
            {
                rst.yuanbao = 0;
            }*/

            if (node.Attribute("talent") != null)
            {
                foreach (var t in Tools.GetXmlAttribute(node,"talent").Split(new char[] { '#' }))
                {
                    rst.Talent.Add(t);
                }
            }

            if (node.Attribute("cd") != null)
            {
                rst.Cooldown = Tools.GetXmlAttributeInt(node, "cd");
            }

            if (Tools.GetXmlElements(node, "require") != null)
            {
                rst.Require = ItemRequire.Parse(Tools.GetXmlElements(node, "require"));
            }

            if (Tools.GetXmlElements(node, "trigger") != null)
            {
                foreach (var trigger in Tools.GetXmlElements(node, "trigger"))
                {
                    rst.Triggers.Add(ItemTrigger.Parse(trigger));
                }
            }

            if(node.Attribute("drop") != null)
            {
                rst.IsDrop = bool.Parse(Tools.GetXmlAttribute(node, "drop"));
            }

            //rst.Trigger = Tools.GetXmlAttribute(node, "trigger");
            //rst.Args = Tools.GetXmlAttribute(node, "args");
            return rst;
        }

        public RichTextBox GenerateTooltip()
        {
            RichTextBox rst = new RichTextBox();
            rst.BorderThickness = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            rst.Background = new SolidColorBrush(Colors.Transparent);
            rst.IsReadOnly = true;
            Paragraph ph = new Paragraph();
            
            rst.Blocks.Add(ph);
            if(this.GetColor() != Colors.White)
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "★", Foreground = new SolidColorBrush(this.GetColor()), FontSize = 14, FontWeight = FontWeights.ExtraBold });
            ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = this.Name, Foreground = new SolidColorBrush(Colors.White), FontSize = 14, FontWeight = FontWeights.ExtraBold });
            ph.Inlines.Add(new LineBreak());
            InlineUIContainer container = new InlineUIContainer();
            container.Child = new Image() { Source = this.Pic, Width = 50, Height = 50 };
            ph.Inlines.Add(container);
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new LineBreak());

            ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = this.Info, Foreground = new SolidColorBrush(Colors.Black) });
            ph.Inlines.Add(new LineBreak());
            string equipCase = this.EquipCase;
            if (!equipCase.Equals(""))
            {
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "装备要求:", Foreground = new SolidColorBrush(Colors.Red) });
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = equipCase, Foreground = new SolidColorBrush(Colors.Red) });
                ph.Inlines.Add(new LineBreak());
            }
            if (this.Triggers.Count > 0)
            {
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "物品效果:", Foreground = new SolidColorBrush(Colors.Cyan) });
                ph.Inlines.Add(new LineBreak());
                foreach (var tt in this.Triggers)
                {
                    ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = tt.ToString(), Foreground = new SolidColorBrush(Colors.Green) });
                    ph.Inlines.Add(new LineBreak());
                }
            }
            if (this.Talent.Count > 0)
            {
                //ph.Inlines.Add(new LineBreak());
                //ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "附加天赋:", Foreground = new SolidColorBrush(Colors.Cyan) });
                //ph.Inlines.Add(new LineBreak());
                foreach (var tt in this.Talent)
                {
                    string talentInfo = JyGame.GameData.Talent.GetTalentInfo(tt, false);
                    ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "天赋:" + talentInfo.ToString(), Foreground = new SolidColorBrush(Colors.Blue) });
                    ph.Inlines.Add(new LineBreak());
                }
            }
            if(this.AdditionTriggers.Count>0)
            {
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "附加效果:", Foreground = new SolidColorBrush(Colors.Cyan) });
                ph.Inlines.Add(new LineBreak());
                foreach (var tt in this.AdditionTriggers)
                {
                    if (tt.Name == "talent")
                    {
                        string talentInfo = JyGame.GameData.Talent.GetTalentInfo(tt.Argvs[0], false);
                        ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "天赋:" + talentInfo.ToString(), Foreground = new SolidColorBrush(Colors.Blue) });
                        ph.Inlines.Add(new LineBreak());
                    }
                    else
                    {
                        ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = tt.ToString(), Foreground = new SolidColorBrush(Colors.Green) });
                        ph.Inlines.Add(new LineBreak());
                    }
                }
            }
            if (this.Cooldown > 0 && RuntimeData.Instance.GameMode != "normal")
            {
                ph.Inlines.Add(new Run() { FontFamily = new FontFamily("SimHei"), Text = "冷却回合数:" + this.Cooldown.ToString(), Foreground = new SolidColorBrush(Colors.Yellow) });
                ph.Inlines.Add(new LineBreak());
            }
            return rst;
        }

        public override string ToString()
        {
            string equipCase = this.EquipCase;
            string rst = Name + "\n" + Info + "\n";

            if (!equipCase.Equals(""))
                rst += " \n装备条件 " + equipCase ;

            if (this.Triggers.Count > 0)
            {
                rst += "\n";
                foreach (var t in this.Triggers)
                {
                    rst += t + " ";
                }
                rst = rst.TrimEnd();
            }
            if (this.Talent.Count>0)
            {
                rst += "\n天赋 ";
                foreach (var t in this.Talent)
                {
                    string talentInfo = JyGame.GameData.Talent.GetTalentInfo(t);
                    rst += talentInfo + "\n";
                }
                rst = rst.TrimEnd();
            }
            if (this.Cooldown > 0 && RuntimeData.Instance.GameMode != "normal")
            {
                rst += "\n冷却回合数 " + this.Cooldown.ToString();
            }
            return rst;
        }

        public void AddRandomTriggers(int number)
        {
            if (!(this.Type == (int)ItemType.Weapon ||
                this.Type == (int)ItemType.Armor ||
                this.Type == (int)ItemType.Accessories))
                return; //只有装备可以添加随机trigger

            for(int i=0;i<number;++i)
            {
                AddRandomTrigger();
            }
        }

        /// <summary>
        /// 给物品增加随机属性
        /// </summary>
        public void AddRandomTrigger()
        {
            if (!(this.Type == (int)ItemType.Weapon ||
                this.Type == (int)ItemType.Armor ||
                this.Type == (int)ItemType.Accessories))
                return; //只有装备可以添加随机trigger

            //决定物品字样的因素：
            // 物品等级、物品类型
            //字样：
            // 数值范围、取值范围
            List<ITTrigger> triggers = new List<ITTrigger>();
            int totalWeight = 0;
            //寻找满足过滤条件的trigger
            foreach(var triggerDb in ItemManager.ItemTriggerDb)
            {
                if(this.level>=triggerDb.MinLevel && this.level <= triggerDb.MaxLevel || this.Name.Equals(triggerDb.Name))
                {
                    foreach(var t in triggerDb.Triggers)
                    {
                        totalWeight += t.Weight;
                        triggers.Add(t);
                    }
                }
            }
            while (true)
            {
                ItemTrigger t = null;
                ITTrigger selectTrigger = null;
                foreach(var trigger in triggers) //遍历所有的trigger池
                {
                    double p = (double)trigger.Weight / (double)totalWeight;  //计算这个trigger加到物品上的概率
                    if(Tools.ProbabilityTest(p)) //如果满足概率
                    {
                        t = trigger.GenerateItemTrigger(); //生成一个属性
                        selectTrigger = trigger;
                        break;
                    }
                }
                if (t == null) continue;
                bool finish = true;
                foreach(var myTrigger in this.AdditionTriggers)
                {
                    if(t.Name == myTrigger.Name && !selectTrigger.HasPool) //主键重了
                    {
                        finish = false;
                        break;
                    }else if(t.Name == myTrigger.Name && selectTrigger.HasPool && t.Argvs[0] == myTrigger.Argvs[0])
                    {
                        finish = false;
                        break;
                    }
                }
                if (finish) { this.AdditionTriggers.Add(t); break; }
            }
        }

        public Item Clone(bool setRandomTrigger = false)
        {
            Item rst = null;
            if (myNode != null)
                rst = Item.Parse(myNode);
            else
                rst = (Item)(this.MemberwiseClone());

            if(setRandomTrigger)
            {
                if(Tools.ProbabilityTest(0.05))
                {
                    rst.AddRandomTriggers(4);
                }
                else if (Tools.ProbabilityTest(0.1))
                {
                    rst.AddRandomTriggers(3);
                }
                else if(Tools.ProbabilityTest(0.4))
                {
                    rst.AddRandomTriggers(2);
                }
                else rst.AddRandomTriggers(1);
            }

            return rst;
        }

        #region 装备相关
        public bool CanEquip(Role r)
        {
            if (Require == null)
                return false;
            foreach (var s in CommonSettings.RoleAttributeList)
            {
                if (r.AttributesFinal[s] < Require.Attributes[s])
                    return false;
            }
            foreach (var s in Require.TalentAttributes)
            {
                if (!r.HasTalent(s))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取装备条件说明
        /// </summary>
        public string EquipCase
        {
            get
            {
                if (Require == null) return ""; //没有装备要求
                string rst = string.Empty;
                foreach (var s in Require.Attributes)
                {
                    if (s.Value > 0)
                    {
                        string attrCn = CommonSettings.AttributeToChinese(s.Key);
                        rst += string.Format("{0}>{1} ", attrCn, s.Value);
                    }
                }
                foreach (var s in Require.TalentAttributes)
                {
                    rst += string.Format("具有天赋【" + s + "】 ");
                }
            return rst.TrimEnd();
            }
        }

        
        /// <summary>
        /// 装备物品
        /// </summary>
        /// <param name="r"></param>
        /// <returns>褪下来的装备</returns>
        public Item EquipToRole(Role r)
        {
            AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
            Item rst = r.Equipment[this.Type];
            r.Equipment[this.Type] = this;
            return rst;
        }

        /// <summary>
        /// 装备过滤，影响到战斗计算
        /// </summary>
        /// <param name="isAttacker">攻击者/防御者</param>
        /// <param name="skillType">技能类型</param>
        /// <param name="attackLow">攻击下限评估</param>
        /// <param name="attackUp">攻击上限评估</param>
        /// <param name="criticalHit">致命一击概率</param>
        /// <param name="defence">防御评估</param>
        public void EquipFilter(bool isAttacker,SkillBox skill, ref double attackLow,
            ref double attackUp, ref double criticalHit, ref double criticalPower, ref double defence)
        {
            int skillType = skill.Type;
            foreach (var trigger in this.AllTriggers)
            {
                switch (trigger.Name)
                {
                    case "defence":
                        if (!isAttacker)
                        {
                            defence += double.Parse(trigger.Argvs[0]);
                            criticalHit -= double.Parse(trigger.Argvs[1]) / 100;
                            if (criticalHit < 0) criticalHit = 0;
                        }
                        break;
                    case "attack":
                        if (isAttacker)
                        {
                            if (this.Type == (int)ItemType.Weapon)
                            {
                                if (this.HasTalent("拳系装备") && skillType != CommonSettings.SKILLTYPE_QUAN) break;
                                if (this.HasTalent("剑系装备") && skillType != CommonSettings.SKILLTYPE_JIAN) break;
                                if (this.HasTalent("刀系装备") && skillType != CommonSettings.SKILLTYPE_DAO) break;
                                if (this.HasTalent("奇门装备") && skillType != CommonSettings.SKILLTYPE_QIMEN) break;
                            }
                            attackLow += double.Parse(trigger.Argvs[0])/2;
                            attackUp += double.Parse(trigger.Argvs[0]);
                            criticalHit += double.Parse(trigger.Argvs[1]) / 100;
                        }
                        break;
                    case "powerup_skill":
                        if(isAttacker)
                        {
                            if(skill.Name.Contains(trigger.Argvs[0]))
                            {
                                double rate = (1 + (int.Parse(trigger.Argvs[1])) / 100.0); 
                                attackLow *= rate;
                                attackUp *= rate;
                            }
                        }
                        break;
                    case "powerup_quanzhang":
                        if(isAttacker)
                        {
                            if(skill.Type == CommonSettings.SKILLTYPE_QUAN)
                            {
                                double rate = (1 + (int.Parse(trigger.Argvs[0])) / 100.0);
                                attackLow *= rate;
                                attackUp *= rate;
                            }
                        }
                        break;
                    case "powerup_jianfa":
                        if (isAttacker)
                        {
                            if (skill.Type == CommonSettings.SKILLTYPE_JIAN)
                            {
                                double rate = (1 + (int.Parse(trigger.Argvs[0])) / 100.0);
                                attackLow *= rate;
                                attackUp *= rate;
                            }
                        }
                        break;
                    case "powerup_daofa":
                        if (isAttacker)
                        {
                            if (skill.Type == CommonSettings.SKILLTYPE_DAO)
                            {
                                double rate = (1 + (int.Parse(trigger.Argvs[0])) / 100.0);
                                attackLow *= rate;
                                attackUp *= rate;
                            }
                        }
                        break;
                    case "powerup_qimen":
                        if (isAttacker)
                        {
                            if (skill.Type == CommonSettings.SKILLTYPE_QIMEN)
                            {
                                double rate = (1 + (int.Parse(trigger.Argvs[0])) / 100.0);
                                attackLow *= rate;
                                attackUp *= rate;
                            }
                        }
                        break;
                    case "critical":
                        if(isAttacker)
                        {
                            criticalPower += int.Parse(trigger.Argvs[0]) / 100.0;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public ItemSkill GetItemSkill()
        {
            foreach (var trigger in this.Triggers)
            {
                if (trigger.Name.Equals("skill")||trigger.Name.Equals("internalskill") ||trigger.Name.Equals("specialskill")||trigger.Name.Equals("talent"))
                {
                    return new ItemSkill()
                    {
                        IsInternal = trigger.Name.Equals("internalskill"),
                        SkillName = trigger.Argvs[0],
                        MaxLevel = (trigger.Argvs.Count > 1) ? int.Parse(trigger.Argvs[1]) : 1
                    };
                }
            }
            return null;
        }

        #endregion
    }

    public class ItemSkill
    {
        public bool IsInternal = false; //是否是内功
        public string SkillName;
        public int MaxLevel;
    }

    /// <summary>
    /// 物品使用结果
    /// </summary>
    public class ItemResult
    {
        public int Hp = 0;
        public int Mp = 0;
        public int DescPoisonLevel = 0;
        public int DescPoisonDuration = 0;
        public int Balls = 0;
        public int MaxHp = 0;
        public int MaxMp = 0;
        public string UpgradeSkill = "";
        public string UpgradeInternalSkill = "";
        public List<Buff> Buffs = new List<Buff>();
    }

    public class ItemTrigger
    {
        public bool IsCheat(int itemLevel)
        {
            ITTriggerDb db = ItemManager.GetTriggerDb(itemLevel);
            bool findMatch = false;
            foreach (var t in db.Triggers)
            {
                if (t.Name == this.Name)
                {
                    for (int i = 0; i < Argvs.Count; ++i)
                    {
                        string argv = Argvs[i];
                        ITParam p = t.Params[i];
                        if (p.Min != -1 && (int.Parse(argv) >= p.Min && int.Parse(argv) <= p.Max))
                        {
                            findMatch = true;
                        }
                        else if (p.Pool != string.Empty)
                        {
                            bool find = false;
                            foreach (var pp in p.PoolList)
                            {
                                if (pp == argv) find = true;
                            }
                            if (find)
                            {
                                findMatch = true;
                            }
                        }
                    }
                }
            }
            return !findMatch;
        }

        public string Name;
        public List<string> Argvs = new List<string>();
        public int Level = -1;
        public string ArgvsString
        {
            get
            {
                string rst = "";
                foreach (var s in Argvs)
                {
                    rst += s + ",";
                }
                return rst.TrimEnd(new char[] { ',' });
            }
            set
            {
                Argvs.Clear();
                foreach (var s in value.Split(new char[] { ',' }))
                {
                    Argvs.Add(s);
                }
            }
        }

        public override string ToString()
        {
            if (Name == "AddBuff")
                return "";

            if (Name == "talent")
            {
                string talent = Argvs[0];
                string rst = string.Format("天赋(被动生效) {0}", Talent.GetTalentInfo(talent, false));
                return rst;
            }

            else if(Name == "eq_talent")
            {
                string talent = Argvs[0];
                string rst = string.Format("天赋(装备生效) {0}", Talent.GetTalentInfo(talent, false));
                return rst;
            }

            string format = ResourceManager.Get("ItemTrigger." + Name);
            return string.Format(format, Argvs.ToArray());
        }

        public static ItemTrigger Parse(XElement node)
        {
            ItemTrigger rst = new ItemTrigger();
            rst.Name = Tools.GetXmlAttribute(node, "name");
            if (rst.Name != "AddBuff")
            {
                foreach (var s in Tools.GetXmlAttribute(node, "argvs").Split(new char[] { ',' }))
                {
                    rst.Argvs.Add(s);
                }
            }
            else
                rst.Argvs.Add(Tools.GetXmlAttribute(node, "argvs"));

            if(node.Attribute("lv") != null)
            {
                rst.Level = Tools.GetXmlAttributeInt(node,"lv");
            }

            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("trigger");
            rst.SetAttributeValue("name", this.Name);
            rst.SetAttributeValue("argvs", this.ArgvsString);
            if(this.Level != -1)
            {
                rst.SetAttributeValue("lv", this.Level);
            }
            return rst;
        }
    }

    public class ItemRequire
    {
        public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public List<string> TalentAttributes = new List<string>();
        public ItemRequire()
        {
            foreach (var s in CommonSettings.RoleAttributeList)
            {
                Attributes[s] = int.MinValue;
            }
            TalentAttributes.Clear();
        }

        public static ItemRequire Parse(IEnumerable<XElement> nodes)
        {
            ItemRequire rst = new ItemRequire();
            try
            {
                foreach (var trigger in nodes)
                {
                    string name = Tools.GetXmlAttribute(trigger, "name");
                    string argvs = Tools.GetXmlAttribute(trigger, "argvs");
                    if (name == "talent")
                        rst.TalentAttributes.Add(argvs);
                    else
                        rst.Attributes[name] = int.Parse(argvs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("invalid item require type!"+e.ToString());
                return null;
            }
            return rst;
        }
    }

    /// <summary>
    /// 物品管理器
    /// </summary>
    public class ItemManager
    {
        static public List<Item> Items = new List<Item>();
        static public List<ITTriggerDb> ItemTriggerDb = new List<ITTriggerDb>();

        static public ITTriggerDb GetTriggerDb(int level)
        {
            ITTriggerDb rst = new ITTriggerDb();
            foreach(var t in ItemTriggerDb)
            {
                if(t.MinLevel<=level && t.MaxLevel>=level)
                {
                    rst.Triggers.AddRange(t.Triggers);
                }
            }
            return rst;
        }

        static public void Export(string dir)
        {
            XElement rootNode = new XElement("items");

            foreach (var item in Items)
            {
                XElement node = item.GenerateXml();
                rootNode.Add(node);
            }

            string file = dir + "/items.xml";
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(rootNode.ToString());
            }

            XElement triggerRootNode = new XElement("item_triggers");
            foreach(var itemTrigger in ItemTriggerDb)
            {
                XElement node = itemTrigger.GenerateXml();
                triggerRootNode.Add(node);
            }
            string fileTrigger = dir + "/item_triggers.xml";
            using (StreamWriter sw = new StreamWriter(fileTrigger))
            {
                sw.Write(triggerRootNode.ToString());
            }
        }

        static public void Init()
        {
            Items.Clear();
            ItemTriggerDb.Clear();
            foreach (var itemXmlFile in GameProject.GetFiles("item"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + itemXmlFile);
                if (xmlRoot.Name == "items")
                {
                    foreach (XElement node in Tools.GetXmlElements(xmlRoot, "item"))
                    {
                        Items.Add(Item.Parse(node));
                    }
                }else if(xmlRoot.Name == "item_triggers")
                {
                    foreach (XElement node in Tools.GetXmlElements(xmlRoot, "item_trigger"))
                    {
                        ItemTriggerDb.Add(ITTriggerDb.Parse(node));
                    }
                }
            }
        }

        static public Item GetItem(string name)
        {
            if(name.EndsWith("残章"))
            {
                string skillName = name.Replace("残章", "");
                Item it = new Item() { 
                    Name = name, 
                    Type = (int)ItemType.Canzhang,
                    PicInfo = "物品.剑谱",
                    CanzhangSkill = skillName ,
                    Info = "【稀有】神秘的武学残章，能够提高" + skillName + "的等级上限1级",
                    price = 200,
                };
                return it;
            }

            foreach (var s in Items)
            {
                if (s.Name.Equals(name)) return s.Clone();
            }
            MessageBox.Show("错误，获取了未定义的物品:" + name);
            return null;
        }

        static public ItemResult TryUseItem(Role source, Role target, Item item)
        {
            ItemResult result = new ItemResult();
            result.Buffs.Clear();
            foreach (var trigger in item.Triggers)
            {
                switch (trigger.Name)
                {
                    case "AddHp":
                        int hp = int.Parse(trigger.Argvs[0]);
                        result.Hp = hp;
                        break;
                    case "AddMp":
                        int mp = int.Parse(trigger.Argvs[0]);
                        result.Mp = mp;
                        break;
                    case "解毒":
                        result.DescPoisonLevel = int.Parse(trigger.Argvs[0]);
                        result.DescPoisonDuration = int.Parse(trigger.Argvs[1]);
                        break;
                    case "Balls":
                        result.Balls = int.Parse(trigger.Argvs[0]);
                        break;
                    case "AddMaxHp":
                        result.MaxHp = int.Parse(trigger.Argvs[0]);
                        break;
                    case "AddMaxMp":
                        result.MaxMp = int.Parse(trigger.Argvs[0]);
                        break;
                    case "AddBuff":
                        result.Buffs = Buff.Parse(trigger.Argvs[0]);
                        break;
                    default:
                        MessageBox.Show("error item trigger " + trigger.Name);
                        break;
                }
            }
            return result;
        }
    }
}
