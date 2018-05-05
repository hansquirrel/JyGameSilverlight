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
using System.Windows.Media.Imaging;

using System.Xml.Linq;
using System.Linq;
using JyGame.UserControls;
using JyGame.Logic;
using System.IO;
using System.Xml.Serialization;

namespace JyGame.GameData
{
    public class SkillLevelInfo
    {
        public int Level;
        public int CoverType;
        public int CoverSize;
        public float Power;
        public string Animation;
        public int Cd;

        public static SkillLevelInfo Parse(XElement node)
        {
            SkillLevelInfo rst = new SkillLevelInfo();
            rst.Level = Tools.GetXmlAttributeInt(node, "level");
            rst.CoverType = Tools.GetXmlAttributeInt(node, "covertype");
            rst.CoverSize = Tools.GetXmlAttributeInt(node , "coversize");
            rst.Power = Tools.GetXmlAttributeFloat(node, "power");
            rst.Animation = Tools.GetXmlAttribute(node, "animation");
            rst.Cd = Tools.GetXmlAttributeInt(node, "cd");
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("level");
            rst.SetAttributeValue("level", Level);
            rst.SetAttributeValue("covertype", CoverType);
            rst.SetAttributeValue("coversize", CoverSize);
            rst.SetAttributeValue("power", Power);
            rst.SetAttributeValue("animation", Animation);
            rst.SetAttributeValue("cd", Cd);
            return rst;
        }
    }

    /// <summary>
    /// 技能
    /// </summary>
    public class Skill
    {
        public string Name;
        public string IconPic;
        public bool Tiaohe = false;
        public float Suit;
        public float Hard;
        public string Info; //简介
        public string Audio;
        public int Type;
        public int CoverType;
        public float BasePower;
        public float Step;
        public string Animation;
        public int Cd;
        public int BaseCoverSize;
        public List<Buff> Buffs = new List<Buff>();
        public List<SkillLevelInfo> Levels = new List<SkillLevelInfo>();
        public List<ItemTrigger> Triggers = new List<ItemTrigger>();

        private SkillLevelInfo GetSkillLevelInfo(int level)
        {
            foreach(var l in Levels)
            {
                if (l.Level == level)
                    return l;
            }
            return null;
        }

        public float GetPower(int level)
        {
            SkillLevelInfo levelInfo = GetSkillLevelInfo(level);
            if(levelInfo!=null)
            {
                return levelInfo.Power;
            }
            return BasePower + (level - 1) * Step;
        }

        public int GetCoverSize(int level)
        {
            SkillLevelInfo levelInfo = GetSkillLevelInfo(level);
            if (levelInfo != null)
            {
                return levelInfo.CoverSize;
            }
            float dSize = new SkillCoverTypeHelper((SkillCoverType)CoverType).dSize;
            int size = level <= 10 ? (int)(1 + dSize * level) : (int)(1 + dSize * 10);
            return size;
        }

        public SkillCoverType GetCoverType(int level)
        {
            SkillLevelInfo levelInfo = GetSkillLevelInfo(level);
            if (levelInfo != null)
            {
                return (SkillCoverType)levelInfo.CoverType;
            }

            return (SkillCoverType)this.CoverType;
        }

        public int GetCastSize(int level)
        {
            SkillCoverTypeHelper helper = new SkillCoverTypeHelper((SkillCoverType)GetCoverType(level));
            int baseCastSize = helper.baseCastSize;
            float dCastSize = helper.dCastSize;
            return level <= 10 ? (int)(baseCastSize + dCastSize * level) : (int)(baseCastSize + dCastSize * 10);
        }

        public string GetAnimationName(int level)
        {
            SkillLevelInfo levelInfo = GetSkillLevelInfo(level);
            if (levelInfo != null)
            {
                return levelInfo.Animation;
            }
            return Animation;
        }

        public AnimationGroup GetAnimations(int level)
        {
            return SkillManager.GetSkillAnimation(this.GetAnimationName(level));
        }

        public int GetCooldown(int level)
        {
            SkillLevelInfo levelInfo = GetSkillLevelInfo(level);
            if (levelInfo != null)
            {
                return levelInfo.Cd;
            }
            return Cd;
        }

        public string BuffInfo { get { return CommonSettings.BuffInfo(Buffs); } }

        public List<UniqueSkill> UniqueSkills = new List<UniqueSkill>();
        
        public XElement GenerateXml()
        {
            XElement rst = new XElement("skill");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("tiaohe", Tiaohe ? 1:0);
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("suit", Suit);
            rst.SetAttributeValue("hard", Hard);
            rst.SetAttributeValue("info", Info);
            rst.SetAttributeValue("audio", Audio);
            rst.SetAttributeValue("basepower", BasePower);
            rst.SetAttributeValue("step", Step);
            rst.SetAttributeValue("animation", Animation);
            rst.SetAttributeValue("cd", Cd);
            if(Buffs.Count>0)
            {
                rst.SetAttributeValue("buff", Buff.BuffsToString(Buffs));
            }
            foreach(var levelInfo in Levels)
            {
                rst.Add(levelInfo.GenerateXml());
            }
            foreach(var uniqueSkill in UniqueSkills)
            {
                rst.Add(uniqueSkill.GenerateXml());
            }
            foreach (var trigger in Triggers)
            {
                rst.Add(trigger.GenerateXml());
            }
            return rst;
        }

        /// <summary>
        /// 获取升级经验
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetLevelupExp(int currentLevel)
        {
            return (int)((float)currentLevel/4 * (Hard + 1)/4 * 15 * 8);
        }

        public string SuitInfo
        {
            get
            {
                if (this.Tiaohe) { return "阴阳调和"; }
                if (this.Suit == 0) { return "无适性"; }
                if (this.Suit > 0) { return string.Format("阳{0}%" , this.Suit * 100); }
                if (this.Suit < 0) { return string.Format("阴{0}%", -this.Suit * 100); }
                return "错误适性";
            }
        }

        static public Skill Parse(XElement node)
        {
            Skill skill = new Skill();
            skill.Name = Tools.GetXmlAttribute(node, "name");
            skill.Tiaohe = Tools.GetXmlAttributeInt(node, "tiaohe") == 1;
            skill.Suit = Tools.GetXmlAttributeFloat(node, "suit");
            skill.Hard = Tools.GetXmlAttributeFloat(node, "hard");
            skill.Info = Tools.GetXmlAttribute(node, "info");
            skill.Audio = Tools.GetXmlAttribute(node, "audio");
            skill.Type = Tools.GetXmlAttributeInt(node, "type");

            //在未指定的情况下，各系武功与攻击类型一致
            int coverTypeCode = skill.Type;
            if (node.Attribute("covertype") != null)
            {
                coverTypeCode = Tools.GetXmlAttributeInt(node, "covertype");
            }
            skill.CoverType = coverTypeCode;

            float dSize = new SkillCoverTypeHelper((SkillCoverType)coverTypeCode).dSize;

            int baseCastSize = 0;
            float dCastSize = 0;//技能施展的增长范围
            if (node.Attribute("castsize") != null)
            {
                baseCastSize = Tools.GetXmlAttributeInt(node, "castsize") - 1;
            }
            else //默认情况
            {
                SkillCoverTypeHelper helper = new SkillCoverTypeHelper((SkillCoverType)coverTypeCode);
                baseCastSize = helper.baseCastSize;
                dCastSize = helper.dCastSize;
            }

            string animationKey = Tools.GetXmlAttribute(node, "animation");
            skill.Animation = animationKey;
            int cd = Tools.GetXmlAttributeInt(node, "cd");
            skill.Cd = cd;
            float basepower = Tools.GetXmlAttributeFloat(node, "basepower");
            skill.BasePower = basepower;
            float step = Tools.GetXmlAttributeFloat(node, "step");
            skill.Step = step;

            //具有特殊指定的等级
            if (Tools.GetXmlElements(node, "level") != null)
            {
                foreach (XElement level in Tools.GetXmlElements(node, "level"))
                {
                    skill.Levels.Add(SkillLevelInfo.Parse(level));
                }
            }

            if (Tools.GetXmlElements(node, "unique") != null)
            {
                foreach (XElement unique in Tools.GetXmlElements(node, "unique"))
                {
                    UniqueSkill us = UniqueSkill.Parse(unique);
                    us.skill = skill;
                    skill.UniqueSkills.Add(us);
                }
            }

            if(Tools.GetXmlElements(node,"trigger") != null)
            {
                foreach(var trigger in Tools.GetXmlElements(node,"trigger"))
                {
                    ItemTrigger t = ItemTrigger.Parse(trigger);
                    skill.Triggers.Add(t);
                }
            }

            if (node.Attribute("buff") != null)
            {
                skill.Buffs = Buff.Parse(node.Attribute("buff").Value);
            }
            return skill;
        }
    }

    /// <summary>
    /// 技能实例
    /// </summary>
    public class SkillInstance
    {
        public SkillInstance()
        {
            Exp = 0;
            Level = 1;
        }

        public UIHost uihost = null;
        public Role Owner;
        public Skill Skill
        {
            set
            {
                _skill = value;
                foreach (var us in _skill.UniqueSkills)
                {
                    UniqueSkillInstances.Add(new UniqueSkillInstance() { Cd = 0, Skill = us, Instance = this });
                }
            }
            get { return _skill; }
        }
        private Skill _skill;

        public List<UniqueSkillInstance> UniqueSkillInstances = new List<UniqueSkillInstance>();

        /// <summary>
        /// 是否战斗时启用
        /// </summary>
        public bool IsUsed = true;

        public int Level
        {
            set { _level = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_level); }
        }
        private int _level;

        public int MaxLevel
        {
            set { _maxLevel = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_maxLevel); }
        }
        private int _maxLevel;

        public int Exp  //经验值
        {
            set { _exp = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_exp); }
        }
        private int _exp;

        public int LevelupExp { get { return Skill.GetLevelupExp(Level); } }
        public int PreLevelupExp { get { return Skill.GetLevelupExp(Level - 1); } }
        
        public bool TryAddExp(int exp) //尝试增加经验
        {
            exp += Owner.AttributesFinal["wuxing"] / 30;
            if(Owner.HasTalent("武学奇才"))
                Exp+= exp * 2;
            else
                Exp+= exp;
            bool isLevelUp = false;
            while (Exp >= LevelupExp)
            {
                if (Level < MaxLevel)
                {
                    Exp = Exp - LevelupExp;
                    Level++;
                    isLevelUp = true;
                }
                else
                {
                    Exp = LevelupExp;
                    break;
                }
            }
            return isLevelUp;
        }

        public int CurrentCd = 0;

        /// <summary>
        /// 施放技能，记录CD
        /// </summary>
        public void CastCd()
        {
            CurrentCd += this.Cooldown;
        }

        public bool IsEnable
        {
            get
            {
                return CurrentCd <= 0;
            }
        }

        public SkillCoverType CoverType {get { return Skill.GetCoverType(Level); }}
        public int Size { get { return Skill.GetCoverSize(Level); } }
        public int CastSize { get { return Skill.GetCastSize(Level); } }
        public int Cooldown { get { return Skill.GetCooldown(Level); } }
        public float Power { get { return Skill.GetPower(Level); } }
        public AnimationGroup Animation { get { return Skill.GetAnimations(Level); } }
        public string AnimationName { get { return Skill.GetAnimationName(Level); } }

        public int CostMp
        {
            get
            {
                return new SkillCoverTypeHelper(CoverType).CostMp(this.Power, Size);
            }
        }

        public RichTextBox GenerateToolTip(bool showBeidong = true)
        {
            RichTextBox rst = new RichTextBox();
            rst.BorderThickness = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            rst.Background = new SolidColorBrush(Colors.Transparent);
            rst.IsReadOnly = true;
            Paragraph ph = new Paragraph();
            rst.Blocks.Add(ph);
            ph.Inlines.Add(new Run() { Text = this.Skill.Name, Foreground = new SolidColorBrush(Colors.Black), FontSize = 14, FontWeight = FontWeights.ExtraBold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            if (this.Animation.Images.Count > 0)
            {
                InlineUIContainer container = new InlineUIContainer();
                container.Child = new Image() { Source = this.Animation.Images[this.Animation.Images.Count / 2].Image, Width = 80, Height = 80 };
                ph.Inlines.Add(container);
                ph.Inlines.Add(new LineBreak());
            }
            ph.Inlines.Add(new Run() { Text = this.Skill.Info, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new LineBreak());
            string tmp = string.Format("等级 {0}/{1}", Level, MaxLevel);
            ph.Inlines.Add(new Run() { Text = tmp, FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("经验 {0}/{1}", Exp, LevelupExp);
            ph.Inlines.Add(new Run() { Text = tmp, FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("威力 {0}", Power);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("覆盖类型/覆盖范围/施展范围 {0}/{1}/{2}", CommonSettings.GetCoverTypeInfo(CoverType), Size, CastSize);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("消耗内力 {0}", CostMp);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Blue), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("技能CD {0}/{1}", CurrentCd, Cooldown);
            if (CurrentCd == 0)
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Green), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            else
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            ph.Inlines.Add(new LineBreak());

            Color c = Colors.Red;
            if (this.Skill.Tiaohe) 
            {
                tmp = "阴阳调和"; c = Colors.Green; 
            }
            else if (this.Skill.Suit == 0) 
            {
                tmp= "无适性";  c = Colors.Black; 
            }
            else if (this.Skill.Suit > 0) 
            { 
                tmp = string.Format("阳{0}%", this.Skill.Suit * 100); c = Colors.Yellow; 
            }
            else if (this.Skill.Suit < 0)
            { 
                tmp = string.Format("阴{0}%", -this.Skill.Suit * 100); c = Colors.Blue; 
            }

            tmp = string.Format("\n适性: {0}   ", tmp);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(c), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            if (Skill.BuffInfo != string.Empty)
            {
                ph.Inlines.Add(new LineBreak());
                tmp = string.Format("特效:", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, FontFamily = new FontFamily("SimHei") });
                tmp = string.Format("{0}", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Purple), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
                ph.Inlines.Add(new LineBreak());
            }

            if (Skill.Triggers.Count > 0 && showBeidong)
            {
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { Text = "被动增益:", FontFamily = new FontFamily("SimHei"), Foreground = new SolidColorBrush(Colors.Cyan) });
                ph.Inlines.Add(new LineBreak());
                foreach (var trigger in Skill.Triggers)
                {
                    Color color = Colors.Red;
                    if (this.Level >= trigger.Level)
                        color = Colors.Green;

                    tmp = "";
                    if (!(this.Level >= trigger.Level))
                    {
                        tmp = string.Format("(×)({0}级解锁)???", trigger.Level);
                    }
                    else
                    {
                        tmp = string.Format("(√)({1}级解锁){0}", trigger.ToString(), trigger.Level);
                    }
                    ph.Inlines.Add(new Run()
                    {
                        Text = tmp,
                        Foreground = new SolidColorBrush(color),
                        FontSize = 12,
                    });
                    ph.Inlines.Add(new LineBreak());
                }
            }
            return rst;
        }

        public string DetailInfo
        {
            get
            {
                string skillinfo = string.Format("{0}\n\n{1}\n等级 {2}/{3}\n经验 {4}/{5}\n威力 {6}\n覆盖范围 {7}/{8}/{9}\n消耗内力 {10}\n技能CD {11}/{12}\n特效 {13}\n适性 {14}",
                        Skill.Name,
                        Skill.Info,
                        Level, MaxLevel,
                        Exp, LevelupExp,
                        Power,
                        CommonSettings.GetCoverTypeInfo(CoverType), Size, CastSize,
                        CostMp,
                        this.CurrentCd,
                        this.Cooldown,
                        this.Skill.BuffInfo,
                        this.Skill.SuitInfo
                        );
                return skillinfo;
            }
        }
    }

    /// <summary>
    /// 内功
    /// </summary>
    public class InternalSkill
    {
        public string Name;
        public string Info;
        public int Yin;
        public int Yang;
        public float Attack;
        public float Critical;
        public float Defence;
        public float Hard;
        public List<ItemTrigger> Triggers = new List<ItemTrigger>();

        internal List<string> Talent = new List<string>();

        public List<UniqueSkill> UniqueSkills = new List<UniqueSkill>();

        public XElement GenerateXml()
        {
            XElement rst = new XElement("internal_skill");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("info", Info);
            rst.SetAttributeValue("yin", Yin);
            rst.SetAttributeValue("yang", Yang);
            rst.SetAttributeValue("attack", Attack);
            rst.SetAttributeValue("defence", Defence);
            rst.SetAttributeValue("critical", Critical);
            rst.SetAttributeValue("hard", Hard);
            if(Talent.Count>0)
            {
                string talentStr = "";
                foreach (var t in Talent) { talentStr += "#" + t; }
                rst.SetAttributeValue("talent", talentStr.TrimStart(new char[] { '#' }));
            }
            foreach(var t in Triggers)
            {
                rst.Add(t.GenerateXml());
            }
            foreach (var uniqueSkill in UniqueSkills)
            {
                rst.Add(uniqueSkill.GenerateXml());
            }
            return rst;
        }

        static public InternalSkill Parse(XElement node)
        {
            InternalSkill skill = new InternalSkill();
            skill.Name = Tools.GetXmlAttribute(node, "name");
            skill.Info = Tools.GetXmlAttribute(node, "info");
            skill.Yin = Tools.GetXmlAttributeInt(node, "yin");
            skill.Yang = Tools.GetXmlAttributeInt(node, "yang");
            skill.Attack = Tools.GetXmlAttributeFloat(node, "attack");
            skill.Critical = Tools.GetXmlAttributeFloat(node, "critical");
            skill.Defence = Tools.GetXmlAttributeFloat(node, "defence");
            skill.Hard = Tools.GetXmlAttributeFloat(node, "hard");

            if (node.Attribute("talent") != null)
            {
                foreach (var t in Tools.GetXmlAttribute(node, "talent").Split(new char[] { '#' }))
                {
                    skill.Talent.Add(t);
                }
            }

            if (Tools.GetXmlElements(node, "unique") != null)
            {
                foreach (XElement unique in Tools.GetXmlElements(node, "unique"))
                {
                    UniqueSkill us = UniqueSkill.Parse(unique);
                    us.internalSkill = skill;
                    skill.UniqueSkills.Add(us);
                }
            }

            if (Tools.GetXmlElements(node, "trigger") != null)
            {
                foreach (var trigger in Tools.GetXmlElements(node, "trigger"))
                {
                    ItemTrigger t = ItemTrigger.Parse(trigger);
                    skill.Triggers.Add(t);
                }
            }
            return skill;
        }
    }

    /// <summary>
    /// 内功实例
    /// </summary>
    public class InternalSkillInstance
    {
        public Role Owner;
        public InternalSkill Skill
        {
            set
            {
                _skill = value;
                foreach (var us in _skill.UniqueSkills)
                {
                    UniqueSkillInstances.Add(new UniqueSkillInstance() { Cd = 0, Skill = us, InternalInstance = this });
                }
            }
            get { return _skill; }
        }
        private InternalSkill _skill;

        public List<UniqueSkillInstance> UniqueSkillInstances = new List<UniqueSkillInstance>();
        public int Level;
        public int MaxLevel;

        public bool HasTalent(string talent)
        {
            return Talent.Contains(talent);
        }
        public List<string> Talent
        {
            get
            {
                List<string> rst = new List<string>();
                foreach(var t in Skill.Triggers)
                {
                    if (t.Name == "eq_talent" && this.Level >= t.Level)
                    {
                        rst.Add(t.Argvs[0]);
                    }
                }
                if (this.Level < 10)
                    return rst;
                else
                {
                    foreach(var t in Skill.Talent)
                    {
                        rst.Add(t);
                    }
                    return rst;
                }
            }
        }

        public bool Equipped = false;

        public int Yin { get { return Skill.Yin * Level / 10; } }
        public int Yang 
        {
            get 
            {
                int yang = Skill.Yang * Level / 10; 
                if (Owner.HasTalent("至刚至阳"))
                {
                    yang = (int)(yang * 1.3);
                }
                return yang;
            } 
        }
        public float Attack 
        {
            get 
            {
                List<ItemTrigger> trigger = Owner.GetItemTriggers("powerup_internalskill");
                float rate = 1;
                if(trigger.Count > 0)
                {
                    foreach(var t in trigger)
                    {
                        if(t.Argvs[0] == this.Skill.Name)
                        {
                            rate += (float)(int.Parse(t.Argvs[1]) / 100.0);
                        }
                    }
                }
                return (float)Level / 10 * Skill.Attack * rate; 
            }
        }
        public float Critical 
        {
            get 
            {
                if (Level < 10)
                    return (float)Level / 10 * Skill.Critical;
                else
                    return Skill.Critical;
            }
        }
        public float Defence
        {
            get
            {
                List<ItemTrigger> trigger = Owner.GetItemTriggers("powerup_internalskill");
                float rate = 1;
                if (trigger.Count > 0)
                {
                    foreach (var t in trigger)
                    {
                        if (t.Argvs[0] == this.Skill.Name)
                        {
                            rate += (float)(int.Parse(t.Argvs[1]) / 100.0);
                        }
                    }
                }
                return (float)Level / 10 * Skill.Defence * rate;
            }
        }
        public float Hard { get { return (float)Skill.Hard; } }

        public int CostMp
        {
            get
            {
                int rst = (int)Hard * Level * 4;
                return rst;
            }
        }
        public float Power { get { return Attack * 13; } }

        public int Exp;
        public int LevelupExp { get { return GetLevelupExp(Level); } }
        public int GetLevelupExp(int currentLevel)
        {
            return (int)(((float)currentLevel + 4) / 4 * (Hard + 4) / 4 * 40);
        }

        public bool TryAddExp(int exp) //尝试增加经验
        {
            exp += Owner.AttributesFinal["wuxing"] / 30;
            if (Owner.HasTalent("武学奇才"))
                Exp += exp * 2;
            else
                Exp += exp;

            bool isLevelUp = false;
            while(Exp >= LevelupExp)
            {
                if (Level < MaxLevel)
                {
                    Exp = Exp - LevelupExp;
                    Level++;
                    isLevelUp = true;
                }
                else
                {
                    Exp = LevelupExp;
                    break;
                }
            }
            return isLevelUp;
        }

        public RichTextBox GenerateToolTip(bool showBeidong = true)
        {
            RichTextBox rst = new RichTextBox();
            rst.BorderThickness = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            rst.Background = new SolidColorBrush(Colors.Transparent);
            rst.IsReadOnly = true;
            Paragraph ph = new Paragraph();
            rst.Blocks.Add(ph);

            ph.Inlines.Add(new Run() { Text = this.Skill.Name, Foreground = new SolidColorBrush(Colors.Black), FontWeight = FontWeights.ExtraBold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new Run() { Text = this.Skill.Info, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new LineBreak());

            string tmp = string.Format("等级 {0}/{1}",Level, MaxLevel);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("经验 {0}/{1}", Exp, LevelupExp);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("+攻击 {0}%", Attack * 100);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("+防御 {0}%", Defence * 100);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Green), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("+爆发 {0}%", Critical * 100);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Yellow), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            ph.Inlines.Add(new LineBreak());
            tmp = string.Format("阴适性 {0}   ", Yin);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Blue), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            

            tmp = string.Format("阳适性 {0}", Yang);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Yellow), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            //if (this.Talent.Count > 0)
            //{
            //    ph.Inlines.Add(new LineBreak());
            //    ph.Inlines.Add(new Run() { Text = "附加天赋:", FontFamily = new FontFamily("SimHei") });
            //    ph.Inlines.Add(new LineBreak());
            //    foreach (var t in this.Talent)
            //    {
            //        ph.Inlines.Add(new Run() { Text = JyGame.GameData.Talent.GetTalentInfo(t, false) + "\n", Foreground = new SolidColorBrush(Colors.Purple), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            //    }
            //}

            if (Skill.Triggers.Count > 0 && showBeidong)
            {
                ph.Inlines.Add(new LineBreak());
                ph.Inlines.Add(new Run() { Text = "被动增益:", FontFamily = new FontFamily("SimHei"), Foreground = new SolidColorBrush(Colors.Cyan) });
                ph.Inlines.Add(new LineBreak());
                foreach (var trigger in Skill.Triggers)
                {
                    Color color = Colors.Red;
                    if (this.Level >= trigger.Level)
                        color = Colors.Green;

                    tmp = "";
                    if (!(this.Level >= trigger.Level))
                    {
                        tmp = string.Format("(×)({0}级解锁)???", trigger.Level);
                    }
                    else
                    {
                        tmp = string.Format("(√)({1}级解锁){0}", trigger.ToString(), trigger.Level);
                    }
                    ph.Inlines.Add(new Run()
                    {
                        Text = tmp,
                        Foreground = new SolidColorBrush(color),
                        FontSize = 12,
                    });
                    ph.Inlines.Add(new LineBreak());
                }
            }
            return rst;
        }

        public string DetailInfo
        {
            get
            {
                string skillinfo = string.Format("{0}\n\n{1}\n等级 {2}/{3}\n经验 {4}/{5}\n+攻击 {6}%\n+防御 {7}%\n+爆发 {8}%\n阴/阳 {9}/{10}",
                        Skill.Name,
                        Skill.Info,
                        Level, MaxLevel,
                        Exp, LevelupExp,
                        Attack * 100, Defence * 100, Critical * 100,
                        Yin, Yang);
                if (this.Talent.Count>0)
                {
                    skillinfo += "\n天赋 ";
                    foreach (var t in Skill.Talent)
                    {
                        skillinfo += t + " ";
                    }
                    skillinfo.TrimEnd();
                }
                return skillinfo;
            }
        }
    }

    public class UniqueSkill
    {
        public string Name;
        public string Info;

        public SkillCoverType CoverType;
        public int CoverSize;
        public int CastSize = 0;
        public float PowerAdd;

        public int RequireLevel;
        public AnimationGroup Animation { get { return SkillManager.GetSkillAnimation(AnimationName); } }
        public string AnimationName;
        public int CastCd;
        public int CostBall;
        public string Audio;

        public XElement xml;
        public Skill skill;
        public InternalSkill internalSkill;

        public List<Buff> Buffs = new List<Buff>();
        public string BuffInfo { get { return CommonSettings.BuffInfo(Buffs); } }
        public static UniqueSkill Parse(XElement node)
        {
            UniqueSkill skill = new UniqueSkill();
            skill.xml = node;
            skill.Name = Tools.GetXmlAttribute(node, "name");
            skill.Info = Tools.GetXmlAttribute(node, "info");
            skill.CoverType = (SkillCoverType)Tools.GetXmlAttributeInt(node, "covertype");
            skill.CoverSize = Tools.GetXmlAttributeInt(node, "coversize");
            if (node.Attribute("castsize") != null)
            {
                skill.CastSize = Tools.GetXmlAttributeInt(node, "castsize");
            }
            skill.PowerAdd = Tools.GetXmlAttributeFloat(node, "poweradd");
            skill.CastCd = Tools.GetXmlAttributeInt(node, "cd");
            skill.CostBall = Tools.GetXmlAttributeInt(node, "costball");
            skill.AnimationName = Tools.GetXmlAttribute(node, "animation");
            skill.Audio = Tools.GetXmlAttribute(node, "audio");
            skill.RequireLevel = Tools.GetXmlAttributeInt(node, "requirelv");

            if (node.Attribute("buff") != null)
            {
                skill.Buffs = Buff.Parse(node.Attribute("buff").Value);
            }
            return skill;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("unique");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("info", Info);
            rst.SetAttributeValue("covertype", (int)CoverType);
            rst.SetAttributeValue("castsize", CastSize);
            rst.SetAttributeValue("coversize", CoverSize);
            rst.SetAttributeValue("poweradd", PowerAdd);
            rst.SetAttributeValue("requirelv", RequireLevel);
            rst.SetAttributeValue("animation", AnimationName);
            rst.SetAttributeValue("cd", CastCd);
            rst.SetAttributeValue("costball", CostBall);
            rst.SetAttributeValue("audio", Audio);
            if (Buffs.Count > 0)
            {
                rst.SetAttributeValue("buff", Buff.BuffsToString(Buffs));
            }
            
            return rst;
        }
    }

    public class UniqueSkillInstance
    {
        public int Cd = 0;
        public UniqueSkill Skill;
        public SkillInstance Instance;
        public InternalSkillInstance InternalInstance;

        public RichTextBox GenerateToolTip(bool showBeidong = true)
        {
            RichTextBox rst = new RichTextBox();
            rst.BorderThickness = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            rst.Background = new SolidColorBrush(Colors.Transparent);
            rst.IsReadOnly = true;
            Paragraph ph = new Paragraph();
            rst.Blocks.Add(ph);
            ph.Inlines.Add(new Run() { Text = this.Skill.Name, Foreground = new SolidColorBrush(Colors.Black), FontSize = 14, FontWeight = FontWeights.ExtraBold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            if (this.Skill.Animation.Images.Count > 0)
            {
                InlineUIContainer container = new InlineUIContainer();
                container.Child = new Image() { Source = this.Skill.Animation.Images[this.Skill.Animation.Images.Count / 2].Image, Width = 80, Height = 80 };
                ph.Inlines.Add(container);
                ph.Inlines.Add(new LineBreak());
            }
            ph.Inlines.Add(new Run() { Text = this.Skill.Info, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new LineBreak());

            float power = Skill.PowerAdd + (Instance != null ? Instance.Power : InternalInstance.Power);
            string tmp = string.Format("威力 {0}", power);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("覆盖类型/覆盖范围/施展范围 {0}/{1}/{2}", CommonSettings.GetCoverTypeInfo(Skill.CoverType), Skill.CoverSize, Skill.CastSize);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("消耗内力 {0}", Instance != null ? Instance.CostMp : InternalInstance.CostMp );
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Blue), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("消耗集气 {0}/{1}", Instance != null ? Instance.Owner.Balls: InternalInstance.Owner.Balls , Skill.CostBall);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Yellow), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("技能CD {0}/{1}", Cd, Skill.CastCd);
            if (Cd == 0)
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Green), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            else
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            ph.Inlines.Add(new LineBreak());

            if (Skill.BuffInfo != string.Empty)
            {
                tmp = string.Format("特效:", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, FontFamily = new FontFamily("SimHei") });
                tmp = string.Format("{0}", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Purple), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            return rst;
        }

        public string DetailInfo
        {
            get
            {
                if (Instance != null) //外功绝技
                {
                    string skillinfo = string.Format(
                        "{0}\n\n{1}\n所属武功 {2}\n所属武功 等级 {3}/{4}\n所属武功 经验 {5}/{6}\n威力 {7}\n覆盖类型/覆盖范围/施展范围 {8}/{9}/{10}\n消耗内力 {11}\n消耗集气 {12}/{13}\n技能CD {14}/{15}\n特效 {16}",
                            Skill.Name,
                            Skill.Info,
                            Instance.Skill.Name,
                            Instance.Level, Instance.MaxLevel,
                            Instance.Exp, Instance.LevelupExp,
                            Instance.Power + Skill.PowerAdd,
                            CommonSettings.GetCoverTypeInfo(Skill.CoverType), Skill.CoverSize, Skill.CastSize,
                            Instance.CostMp,
                            Instance.Owner.Balls, Skill.CostBall,
                            Cd,
                            Skill.CastCd,
                            Skill.BuffInfo
                            );
                    return skillinfo;
                }
                else //内功绝技
                {
                    string skillinfo = string.Format(
                        "{0}\n\n{1}\n所属武功 {2}\n所属武功 等级 {3}/{4}\n所属武功 经验 {5}/{6}\n威力 {7}\n覆盖类型/覆盖范围/施展范围 {8}/{9}/{10}\n消耗内力 {11}\n消耗集气 {12}/{13}\n技能CD {14}/{15}\n特效 {16}",
                            Skill.Name,
                            Skill.Info,
                            InternalInstance.Skill.Name,
                            InternalInstance.Level, InternalInstance.MaxLevel,
                            InternalInstance.Exp, InternalInstance.LevelupExp,
                            InternalInstance.Power + Skill.PowerAdd,
                            CommonSettings.GetCoverTypeInfo(Skill.CoverType), Skill.CoverSize, Skill.CastSize,
                            InternalInstance.CostMp,
                            InternalInstance.Owner.Balls, Skill.CostBall,
                            Cd,
                            Skill.CastCd,
                            Skill.BuffInfo
                            );
                    return skillinfo;
                }
            }
        }
    }

    //特殊武功，（加血、加BUFF、特殊形式的攻击等个性化手段）
    public class SpecialSkill
    {
        public string Name;
        public string Info;

        public SkillCoverType CoverType;
        public int Size;
        public int CastSize = 0;

        public AnimationGroup Animation;
        public string AnimationName;
        public int costMp;
        public int CastCd;
        public int CostBall;
        public string Audio;
        public bool HitSelf;

        public XElement xml;

        public List<Buff> Buffs = new List<Buff>();
        public string BuffInfo { get { return CommonSettings.BuffInfo(Buffs); } }

        public static SpecialSkill Parse(XElement node)
        {
            SpecialSkill skill = new SpecialSkill();
            skill.xml = node;
            skill.Name = Tools.GetXmlAttribute(node, "name");
            skill.Info = Tools.GetXmlAttribute(node, "info");
            skill.CoverType = (SkillCoverType)Tools.GetXmlAttributeInt(node, "type");
            skill.Size = Tools.GetXmlAttributeInt(node, "coversize");
            skill.CastCd = Tools.GetXmlAttributeInt(node, "cd");
            if (node.Attribute("castsize") != null)
            {
                skill.CastSize = Tools.GetXmlAttributeInt(node, "castsize");
            }
            skill.costMp = Tools.GetXmlAttributeInt(node, "costMp");
            skill.CostBall = Tools.GetXmlAttributeInt(node, "costball");
            skill.Animation = SkillManager.GetSkillAnimation(Tools.GetXmlAttribute(node, "animation"));
            skill.AnimationName = Tools.GetXmlAttribute(node, "animation");
            skill.Audio = Tools.GetXmlAttribute(node, "audio");
            skill.HitSelf = Tools.GetXmlAttributeInt(node, "hitself") == 1 ? true : false;

            if (node.Attribute("buff") != null)
            {
                skill.Buffs = Buff.Parse(node.Attribute("buff").Value);
            }
            return skill;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("skill");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("info", Info);
            rst.SetAttributeValue("type", (int)CoverType);
            rst.SetAttributeValue("castsize", CastSize);
            rst.SetAttributeValue("coversize", Size);
            rst.SetAttributeValue("audio", Audio);
            rst.SetAttributeValue("animation", AnimationName);
            rst.SetAttributeValue("costMp", costMp);
            rst.SetAttributeValue("cd", CastCd);
            rst.SetAttributeValue("costball", CostBall);
            rst.SetAttributeValue("hitself", HitSelf ? 1:0);
            if (Buffs.Count > 0)
            {
                rst.SetAttributeValue("buff", Buff.BuffsToString(Buffs));
            }
            return rst;
        }
    }
    public class SpecialSkillInstance
    {
        public int Cd = 0;
        public SpecialSkill Skill;

        public Role Owner;

        public RichTextBox GenerateToolTip(bool showBeidong = true)
        {
            RichTextBox rst = new RichTextBox();
            rst.BorderThickness = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
            rst.Background = new SolidColorBrush(Colors.Transparent);
            rst.IsReadOnly = true;
            Paragraph ph = new Paragraph();
            rst.Blocks.Add(ph);
            ph.Inlines.Add(new Run() { Text = this.Skill.Name, Foreground = new SolidColorBrush(Colors.Black), FontSize = 14, FontWeight = FontWeights.ExtraBold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            if (Skill.Animation.Images.Count > 0)
            {
                InlineUIContainer container = new InlineUIContainer();
                container.Child = new Image() { Source = this.Skill.Animation.Images[Skill.Animation.Images.Count/2].Image, Width = 80, Height = 80 };
                ph.Inlines.Add(container);
                ph.Inlines.Add(new LineBreak());
            }
            ph.Inlines.Add(new Run() { Text = this.Skill.Info, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());
            ph.Inlines.Add(new LineBreak());

            string tmp = string.Format("覆盖类型/覆盖范围/施展范围 {0}/{1}/{2}", CommonSettings.GetCoverTypeInfo(Skill.CoverType), Skill.Size, Skill.CastSize);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Black), FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("消耗内力 {0}", Skill.costMp);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Blue), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("消耗集气 {0}/{1}", Owner != null?Owner.Balls:0, Skill.CostBall);
            ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Yellow), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            ph.Inlines.Add(new LineBreak());

            tmp = string.Format("技能CD {0}/{1}", Cd, Skill.CastCd);
            if (Cd == 0)
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Green), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            else
            {
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            ph.Inlines.Add(new LineBreak());

            if (Skill.BuffInfo != string.Empty)
            {
                tmp = string.Format("特效:", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, FontFamily = new FontFamily("SimHei") });
                tmp = string.Format("{0}", Skill.BuffInfo);
                ph.Inlines.Add(new Run() { Text = tmp, Foreground = new SolidColorBrush(Colors.Purple), FontWeight = FontWeights.Bold, FontFamily = new FontFamily("SimHei") });
            }
            return rst;
        }
        public string DetailInfo
        {
            get
            {
            string skillinfo = string.Format(
                "{0}\n\n{1}\n覆盖类型/覆盖范围/施展范围 {2}/{3}/{4}\n消耗内力 {5}\n消耗集气 {6}/{7}\n技能CD {8}/{9}\n特效 {10}",
                    Skill.Name,
                    Skill.Info,
                    CommonSettings.GetCoverTypeInfo(Skill.CoverType), Skill.Size, Skill.CastSize,
                    Skill.costMp,
                    Owner != null?Owner.Balls:0, Skill.CostBall,
                    Cd, Skill.CastCd,
                    Skill.BuffInfo
                    );
            return skillinfo;
            }
        }
    }

    /// <summary>
    /// 奥义前置条件
    /// </summary>
    public class AoyiCondition
    {
        public string type;
        public string value;
        public int level;

        public XElement GenerateXml()
        {
            XElement rst = new XElement("condition");
            rst.SetAttributeValue("type", type);
            rst.SetAttributeValue("value", value);
            rst.SetAttributeValue("level", level);
            return rst;
        }
    }

    /// <summary>
    /// 奥义
    /// </summary>
    public class Aoyi
    {
        public string Name;
        public string Start;
        public bool Internal = false;
        public int StartLevel;
        public float Probability;
        public List<AoyiCondition> conditions = new List<AoyiCondition>();
        //public string IconPic;

        public bool Tiaohe = false;
        public float Suit;
        //public float Hard;
        //public string Info; //简介
        public string Audio;
        public int Type;
        //public SkillAnimation Animations = null;
        public List<Buff> Buffs = new List<Buff>();
 
        public float AddPower = 3;
        public int AddSize = 0;
        public int AddCastSize = 0;
        public SkillCoverType CoverType = SkillCoverType.FACE;
        public AnimationGroup Animations
        {
            get
            {
                return SkillManager.GetSkillAnimation(AnimationName);
            }
        }
        public string AnimationName = "";
        //public int Cooldown = 0;
     
        public XElement GenerateXml()
        {
            XElement rst = new XElement("aoyi");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("start", Start);
            rst.SetAttributeValue("level", StartLevel);
            rst.SetAttributeValue("probability", Probability);
            rst.SetAttributeValue("animation", AnimationName);
            rst.SetAttributeValue("addPower", AddPower);
            rst.SetAttributeValue("buff", Buff.BuffsToString(Buffs));
            foreach(var c in conditions)
            {
                rst.Add(c.GenerateXml());
            }
            return rst;
        }

        static public Aoyi Parse(XElement node)
        {
            Aoyi aoyi = new Aoyi();
            aoyi.Name = Tools.GetXmlAttribute(node, "name");
            aoyi.Start = Tools.GetXmlAttribute(node, "start");
            //MessageBox.Show(aoyi.Name);
            aoyi.StartLevel = Tools.GetXmlAttributeInt(node, "level");
            aoyi.Probability = Tools.GetXmlAttributeFloat(node, "probability");
            aoyi.conditions.Clear();
            foreach (XElement condition in node.Elements("condition"))
            {
                AoyiCondition cond = new AoyiCondition();
                cond.type = Tools.GetXmlAttribute(condition, "type");
                cond.value = Tools.GetXmlAttribute(condition, "value");
                if (condition.Attribute("level") != null)
                    cond.level = Tools.GetXmlAttributeInt(condition, "level");
                else
                    cond.level = 0;
                aoyi.conditions.Add(cond);
            }

            if (node.Attribute("internal") != null)
            {
                aoyi.Internal = Tools.GetXmlAttributeInt(node, "internal") == 1;
            }
            else
            {
                Skill s = SkillManager.GetSkill(aoyi.Start);
                if (s == null) aoyi.Internal = true;
            }

            if (node.Attribute("tiaohe") != null)
            {
                aoyi.Tiaohe = (Tools.GetXmlAttributeInt(node, "tiaohe") == 1);
            }
            else
            {
                if (!aoyi.Internal)
                {
                    Skill s = SkillManager.GetSkill(aoyi.Start);
                    aoyi.Tiaohe = s == null ? false : s.Tiaohe;
                }
                else
                {
                    aoyi.Tiaohe = true;
                }
            }

            if (node.Attribute("suit") != null)
            {
                aoyi.Suit = Tools.GetXmlAttributeFloat(node, "suit");
            }
            else
            {
                if (!aoyi.Internal)
                {
                    Skill s = SkillManager.GetSkill(aoyi.Start);
                    aoyi.Suit = s == null ? 0 : s.Suit;
                }
                else
                {
                    aoyi.Suit = 0;
                }
            }

            if (node.Attribute("audio") != null)
            {
                aoyi.Audio = Tools.GetXmlAttribute(node, "audio");
            }
            else
            {
                if (!aoyi.Internal)
                {
                    aoyi.Audio = SkillManager.GetSkill(aoyi.Start).Audio;
                }
                else
                {
                    aoyi.Audio = SkillManager.GetInternalUniqueSkill(aoyi.Start).Audio;
                }
            }

            if (node.Attribute("type") != null)
            {
                aoyi.Type = Tools.GetXmlAttributeInt(node, "type");
            }
            else
            {
                if (!aoyi.Internal)
                {
                    Skill s = SkillManager.GetSkill(aoyi.Start);
                    aoyi.Type = s == null ? 4 : s.Type;
                }
                else
                {
                    aoyi.Type = 4;
                }
            }

            if (node.Attribute("animation") != null)
            {
                aoyi.AnimationName = Tools.GetXmlAttribute(node, "animation");
            }
            else
            {
                aoyi.AnimationName = "aoyi1";
            }

            if (node.Attribute("buff") != null)
            {
                aoyi.Buffs = Buff.Parse(node.Attribute("buff").Value);
                if (!aoyi.Internal)
                {
                    foreach (var buff in SkillManager.GetSkill(aoyi.Start).Buffs)
                    {
                        aoyi.Buffs.Add(buff);
                    }
                }
                else
                {
                    foreach (var buff in SkillManager.GetInternalUniqueSkill(aoyi.Start).Buffs)
                    {
                        aoyi.Buffs.Add(buff);
                    }
                }
            }
            else
            {
                if (!aoyi.Internal)
                {
                    aoyi.Buffs = SkillManager.GetSkill(aoyi.Start).Buffs;
                }
                else
                {
                    aoyi.Buffs = SkillManager.GetInternalUniqueSkill(aoyi.Start).Buffs;
                }
            }

            if (node.Attribute("addPower") != null)
            {
                aoyi.AddPower = Tools.GetXmlAttributeInt(node, "addPower");
            }
            if (node.Attribute("addSize") != null)
            {
                aoyi.AddSize = Tools.GetXmlAttributeInt(node, "addSize");
            }
            if (node.Attribute("addCastSize") != null)
            {
                aoyi.AddCastSize = Tools.GetXmlAttributeInt(node, "addCastSize");
            }
            if (node.Attribute("covertype") != null)
            {
                aoyi.CoverType = (SkillCoverType)Tools.GetXmlAttributeInt(node, "covertype");
            }
            return aoyi;
        }
    }

    /// <summary>
    /// 技能实例
    /// </summary>
    public class AoyiInstance
    {
        public UIHost uihost = null;
        public Role Owner;
        public SkillInstance skill;

        public Aoyi Aoyi
        {
            set
            {
                _aoyi = value;
            }
            get { return _aoyi; }
        }
        private Aoyi _aoyi;

        //public int CurrentCd = 0;

        /// <summary>
        /// 施放技能，记录CD
        /// </summary>
        /*public void CastCd()
        {
            CurrentCd += this.Cooldown;
        }*/

        public bool IsEnable
        {
            get
            {
                //return CurrentCd <= 0;
                return true;
            }
        }

        public SkillCoverType CoverType { get { 
            //return Aoyi.CoverType; 
            if (skill.CoverType == SkillCoverType.NORMAL)
                return SkillCoverType.FACE;
            else
                return skill.CoverType;
        } }
        public int Size { get {
            return skill.Size + Aoyi.AddSize;
        } }
        public int CastSize { get { return skill.CastSize + Aoyi.AddCastSize; } }
        //public int Cooldown { get { return Aoyi.Cooldown; } }
        public float Power
        {
            get
            {
                float addRate = 1;

                //计算奥义加成
                List<ItemTrigger> triggers = Owner.GetItemTriggers("powerup_aoyi");
                if (triggers.Count > 0)
                {
                    foreach (var tr in triggers)
                    {
                        if (tr.Argvs[0] == this.Aoyi.Name)
                        {
                            addRate *= (1 + int.Parse(tr.Argvs[1]) / 100f);
                        }
                    }
                }
                return (skill.Power + Aoyi.AddPower) * addRate;
            }
        }
        public AnimationGroup Animation { get { return skill.Animation; } }
        public string AnimationName {get {return skill.AnimationName; } }

        public int CostMp
        {
            get
            {
                return skill.CostMp;
            }
        }
    }


    /// <summary>
    /// 技能封装盒
    /// </summary>
    public class SkillBox
    {
        public bool XilianTag = false; //洗练

        public bool IsSwitchInternalSkill = false; //是否是切换内功
        public InternalSkillInstance SwitchInternalSkill = null; //切换内功的名字

        public SkillInstance Instance;
        public AoyiInstance AoyiInstance;
        public UniqueSkillInstance UniqueSkill = null;
        public InternalSkillInstance InternalInstance { get { return UniqueSkill.InternalInstance; } }
        public SpecialSkillInstance SpecialSkill = null;

        public bool IsUnique { get { return UniqueSkill != null; } } //是否是绝招
        public bool IsInternalUnique //是否是内功绝招
        { 
            get {
                if (!IsUnique) return false;
                return UniqueSkill.InternalInstance != null; 
            } 
        }
        public bool IsSpecial { get { return SpecialSkill != null; } }//是否为特殊招式
        public bool IsAoyi { get { return AoyiInstance != null; } } //是否为奥义

        public bool HitSelf
        {
            get
            {
                if (IsSpecial && SpecialSkill.Skill.HitSelf) return true;
                return false;
            }
        }

        public int Cd
        {
            get
            {
                if (IsUnique) return UniqueSkill.Cd;
                if (IsSpecial) return SpecialSkill.Cd;
                //if (IsAoyi) return AoyiInstance.CurrentCd;
                return Instance.CurrentCd;
            }
        }

        public int CastCd
        {
            get
            {
                if (IsUnique) return UniqueSkill.Skill.CastCd;
                if (IsSpecial) return SpecialSkill.Skill.CastCd;
                return Instance.Cooldown;
            }
        }

        public string Name
        {
            get
            {
                if (IsSwitchInternalSkill && !XilianTag) return "切换内功:" + SwitchInternalSkill.Skill.Name;
                if (IsSwitchInternalSkill && XilianTag) return SwitchInternalSkill.Skill.Name;
                if (IsUnique) return UniqueSkill.Skill.Name;
                if (IsSpecial) return SpecialSkill.Skill.Name;
                return Instance.Skill.Name;
            }
        }

        public string Info
        {
            get
            {
                if (IsUnique) return UniqueSkill.Skill.Info;
                if (IsSpecial) return SpecialSkill.Skill.Info;
                return Instance.Skill.Info;
            }
        }

        public int Level
        {
            get
            {
                if (IsInternalUnique) return InternalInstance.Level;
                if (IsSpecial) return 1;
                return Instance.Level;
            }
        }

        public int CostMp { 
            get 
            {
                int rst = 0;
                if (IsSpecial)
                    rst = SpecialSkill.Skill.costMp;
                else if (IsInternalUnique)
                {
                    rst = InternalInstance.CostMp;
                }
                else if (IsAoyi)
                {
                    rst = AoyiInstance.CostMp;
                }
                else
                {
                    rst = Instance.CostMp;
                }
                //增加内力消耗
                BuffInstance buffInstance = this.Owner.GetBuff("内伤");
                if (buffInstance != null)
                {
                    rst = (int)(rst * (1 + 0.3 * buffInstance.Level));
                }
                if (Owner.HasTalent("阉人"))
                    rst *= 2;
                return rst;
            } 
        }

        public Role Owner { get {
            if (IsInternalUnique) return InternalInstance.Owner;
            if (IsSpecial) return SpecialSkill.Owner;
            if (IsAoyi) return AoyiInstance.Owner;
            return Instance.Owner;
            //return IsInternalUnique ? InternalInstance.Owner : Instance.Owner; 
        } }
        public List<Buff> Buffs { get {
            if (IsUnique) return UniqueSkill.Skill.Buffs;
            if (IsSpecial) return SpecialSkill.Skill.Buffs;
            if (IsAoyi) return AoyiInstance.Aoyi.Buffs;
            return Instance.Skill.Buffs;
            //return IsUnique ? UniqueSkill.Skill.Buffs : Instance.Skill.Buffs; 
        } }

        public float Power { 
            get 
            {
                if (IsUnique)
                {
                    return IsInternalUnique ? (InternalInstance.Power + UniqueSkill.Skill.PowerAdd):(Instance.Power + UniqueSkill.Skill.PowerAdd);
                }
                if (IsSpecial) return 0;
                if (IsAoyi) return AoyiInstance.Power;
                return Instance.Power; 
            } 
        }

        public string Audio
        {
            get
            {
                return ResourceManager.Get(AudioResource);
            }
        }

        private string AudioResource { get {
            if (IsUnique) return UniqueSkill.Skill.Audio;
            if (IsSpecial) return SpecialSkill.Skill.Audio;
            if (IsAoyi) return AoyiInstance.Aoyi.Audio;
            return Instance.Skill.Audio;
            //return IsUnique ? UniqueSkill.Skill.Audio : Instance.Skill.Audio; 
        } }

        public SkillCoverType CoverType { get {
            if (IsUnique) return UniqueSkill.Skill.CoverType;
            if (IsSpecial) return SpecialSkill.Skill.CoverType;
            if (IsAoyi) return AoyiInstance.CoverType;
            return Instance.CoverType;
            //return IsUnique ? UniqueSkill.Skill.CoverType : Instance.CoverType; 
        }}
        public AnimationGroup Animation
        { 
            get {
                if (IsUnique) return UniqueSkill.Skill.Animation;
                if (IsSpecial) return SpecialSkill.Skill.Animation;
                if (IsAoyi) return AoyiInstance.Animation;
                return Instance.Animation;
                //return IsUnique ? UniqueSkill.Skill.Animation : Instance.Animation; 
            } 
        }
        public string AnimationName
        {
            get {
                if (IsUnique) return UniqueSkill.Skill.AnimationName;
                if (IsSpecial) return SpecialSkill.Skill.AnimationName;
                if (IsAoyi) return AoyiInstance.AnimationName;
                return Instance.AnimationName;
                //return IsUnique ? UniqueSkill.Skill.Animation : Instance.Animation; 
            } 
        }
        public int Size
        {
            get
            {
                int size = 0;
                if (IsUnique) size = UniqueSkill.Skill.CoverSize;
                else if (IsSpecial) size = SpecialSkill.Skill.Size;
                else if (IsAoyi) size = AoyiInstance.Size;
                else size = Instance.Size;
                //int size = IsUnique ? UniqueSkill.Skill.Size : Instance.Size;
                BuffInstance buffInstance = this.Owner.GetBuff("致盲"); //致盲buff
                if (buffInstance != null && !this.Owner.HasTalent("心眼通明"))
                {
                    int originalSize = size;
                    size -= (int)(buffInstance.Level * 1.5);
                    if (size <= 0)
                    {
                        if (originalSize <= 0)
                            size = 0;
                        else
                            size = 1;
                    }
                }
                return size;
            }
        }
        public int CastSize
        {
            get
            {
                int castsize = 0;
                if (IsUnique) castsize = UniqueSkill.Skill.CastSize;
                else if (IsSpecial) castsize = SpecialSkill.Skill.CastSize;
                else if (IsAoyi) castsize = AoyiInstance.CastSize;
                else castsize = Instance.CastSize;
                if (this.Owner.HasTalent("寸长寸强"))
                {
                    castsize += 1;
                }
                if(this.Owner.HasTalent("吴钩霜雪") && this.Name == "太玄神功")
                {
                    castsize += 3;
                }
                if (castsize != 0)
                {
                    BuffInstance buffInstance = this.Owner.GetBuff("致盲"); //致盲buff
                    if (buffInstance != null && !this.Owner.HasTalent("心眼通明"))
                    {
                        castsize -= (int)(buffInstance.Level * 1.5);
                        if (castsize <= 0) castsize = 1;
                    }
                }
                return castsize;
            }
        }

        public float Suit { get {
            if (IsInternalUnique) return 0;
            if (IsAoyi) return AoyiInstance.Aoyi.Suit;
            return Instance.Skill.Suit;
        } }
        public bool Tiaohe { get {
            if (IsInternalUnique) return true;
            if (IsAoyi) return AoyiInstance.Aoyi.Tiaohe;
            return Instance.Skill.Tiaohe;
        } }
        public int Type { get {
            if (IsInternalUnique) return CommonSettings.SKILLTYPE_NEIGONG;
            if (IsAoyi) return AoyiInstance.Aoyi.Type;
            return Instance.Skill.Type;
        } }
        public int BallCost { get {
            if (IsUnique) return UniqueSkill.Skill.CostBall;
            if (IsSpecial) return SpecialSkill.Skill.CostBall;
            return 0;
            //return IsUnique ? UniqueSkill.Skill.CostBall : 0; 
        } }
        public float HardLevel { get {
            if (IsInternalUnique) return InternalInstance.Hard;
            if (IsAoyi) return 10;
            return Instance.Skill.Hard;
        } }

        public bool TryAddExp(int exp)
        {
            if (IsSpecial) return false;
            if (IsInternalUnique)
            {
                return InternalInstance.TryAddExp(exp);
            }
            //if (IsAoyi)
            //    return false;
             
            return Instance.TryAddExp(exp);
        }
        public SkillStatus Status
        {
            get
            {
                //Role owner = IsInternalUnique ? InternalInstance.Owner : Instance.Owner;
                if (IsSwitchInternalSkill) return SkillStatus.Ok;
                Role owner = null;
                if (IsInternalUnique) owner = InternalInstance.Owner;
                else if (IsSpecial) owner = SpecialSkill.Owner;
                else owner = Instance.Owner;
                if (IsUnique) //可以发绝招
                {
                    if (owner.Balls < UniqueSkill.Skill.CostBall) return SkillStatus.NoBalls;
                }
                if (IsSpecial)
                {
                    if (owner.Balls < SpecialSkill.Skill.CostBall) return SkillStatus.NoBalls;
                }
                if (this.Cd > 0) //CD
                    return SkillStatus.NoCd;
                if (CostMp > owner.Attributes["mp"])
                    return SkillStatus.NoMp;
                //if (!IsUnique && !IsSpecial && !IsInternalUnique && ( owner.GetBuff("诸般封印") != null || (owner.GetBuff("拳掌封印") != null && this.Type == 0) || (owner.GetBuff("剑封印") != null && this.Type == 1) || (owner.GetBuff("刀封印") != null && this.Type == 2) || ((owner.GetBuff("奇门封印") != null && this.Type == 3))) )
                if (!IsSpecial && !IsInternalUnique && (owner.GetBuff("诸般封印") != null || (owner.GetBuff("拳掌封印") != null && this.Type == 0) || (owner.GetBuff("剑封印") != null && this.Type == 1) || (owner.GetBuff("刀封印") != null && this.Type == 2) || ((owner.GetBuff("奇门封印") != null && this.Type == 3))))
                    return SkillStatus.Seal;
                return SkillStatus.Ok;
            }
        }

        public AttackResult Attack(Spirit source, Spirit target)
        {
            return CommonSettings.GetAttackResult(
                source, target, this );
        }

        public void AddCastCd()
        {
            if (IsUnique)
            {
                UniqueSkill.Cd += UniqueSkill.Skill.CastCd;
            }
            else if (IsSpecial)
            {
                SpecialSkill.Cd += SpecialSkill.Skill.CastCd;
            }
            else if(!IsInternalUnique)
                Instance.CastCd();
        }

        public RichTextBox GenerateToolTip(bool showBeidong = true)
        {
            RichTextBox rst = null;
            if (IsSwitchInternalSkill)
            {
                rst = SwitchInternalSkill.GenerateToolTip(showBeidong);
            }
            else if (IsUnique)
            {
                rst = UniqueSkill.GenerateToolTip(showBeidong);
            }
            else if (IsSpecial)
            {
                rst = SpecialSkill.GenerateToolTip(showBeidong);
            }
            else
            {
                rst = Instance.GenerateToolTip(showBeidong);
            }
            (rst.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
            (rst.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = StatusInfo, FontFamily = new FontFamily("SimHei") });
            return rst;
        }

        public string DetailInfo
        {
            get
            {
                string rst = "";
                if (IsSwitchInternalSkill)
                {
                    rst += SwitchInternalSkill.DetailInfo;
                }
                else if (IsUnique)
                {
                    rst += UniqueSkill.DetailInfo;
                }
                else if (IsSpecial)
                {
                    rst += SpecialSkill.DetailInfo;
                }
                else
                {
                    rst += Instance.DetailInfo;
                }
                rst += "\n" + StatusInfo;
                
                return rst;
            }
        }

        public string StatusInfo
        {
            get
            {
                string rst = "";
                switch (this.Status)
                {
                    case SkillStatus.Ok:
                        break;
                    case SkillStatus.NoCd:
                        rst += string.Format("(cd中,{0})", Cd);
                        break;
                    case SkillStatus.NoBalls:
                        rst += string.Format("(蓄力值不足)");
                        break;
                    case SkillStatus.NoMp:
                        rst += string.Format("(内力不足)");
                        break;
                    case SkillStatus.Seal:
                        rst += string.Format("(封印，无法施展）");
                        break;
                }
                return rst;
            }
        }

        public List<LocationBlock> GetHitRange(int x, int y)
        {
            List<LocationBlock> rst = new List<LocationBlock>();
            List<LocationBlock> castBlocks = this.GetSkillCastBlocks(x, y);
            foreach (var block in castBlocks)
            {
                if(rst.Count(g=>g.X == block.X && g.Y == block.Y) == 0)
                    rst.AddRange(this.GetSkillCoverBlocks(block.X, block.Y, x, y));
            }
            return rst;
        }

        public List<LocationBlock> GetSkillCastBlocks(int x, int y)
        {
            List<LocationBlock> rst = new List<LocationBlock>();

            int size = this.CastSize;
            for (int i = -size; i <= size; ++i)
                for (int j = -size; j <= size; ++j )
                {
                    if(Math.Abs(i) + Math.Abs(j) <=size)
                    {
                        rst.Add(new LocationBlock() { X = x + i, Y = y + j });
                    }
                }
            return rst;
        }

        public List<LocationBlock> GetSkillCoverBlocks(int x, int y,int spx,int spy)
        {
            return new SkillCoverTypeHelper(this.CoverType).GetSkillCoverBlocks(x, y, spx, spy, this.Size);
        }

        static public List<LocationBlock> GetSkillCoverBlocks(int x, int y, int spx, int spy, int size, SkillCoverType covertype)
        {
            return new SkillCoverTypeHelper(covertype).GetSkillCoverBlocks(x, y, spx, spy, size);
        }
    }

    public enum SkillStatus //技能状态
    {
        Ok = 0, //可以施展
        NoBalls = 1, //没有集气点
        NoCd = 2, //CD中
        NoMp = 3, //缺魔
        Seal = 4 //被封印
    }

    //攻击结果
    public class AttackResult
    {
        public int Hp=0;
        public int Mp=0;
        public int costBall=0;
        public bool Critical = false; //致命一击

        public List<Buff> Debuff = new List<Buff>();
        public List<Buff> Buff = new List<Buff>();

        //public bool IsMiss;

        //讲的话
        public string sourceCastInfo = null;
        public double sourceCastProperty = 1;
        public string targetCastInfo = null;
        public double targetCastProperty = 1;
    }
    
    /// <summary>
    /// 技能管理器
    /// </summary>
    public class SkillManager
    {
        static public List<Skill> Skills = new List<Skill>();
        static public List<InternalSkill> InternalSkills = new List<InternalSkill>();
        static public List<SpecialSkill> SpecialSkills = new List<SpecialSkill>();
        static public List<Aoyi> Aoyis = new List<Aoyi>();

        static Dictionary<string, AnimationGroup> SkillAnimations = new Dictionary<string, AnimationGroup>();

        static public void Init()
        {
            Skills.Clear();
            InternalSkills.Clear();
            SpecialSkills.Clear();
            Aoyis.Clear();
            foreach (var skillXmlFile in GameProject.GetFiles("skill"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + skillXmlFile);
                //load skill data
                if (xmlRoot.Element("skills") != null)
                {
                    foreach (XElement node in Tools.GetXmlElements(Tools.GetXmlElement(xmlRoot, "skills"), "skill"))
                    {
                        Skills.Add(Skill.Parse(node));
                    }
                }

                //load internal skill data
                if (xmlRoot.Element("internal_skills") != null)
                {
                    foreach (var node in Tools.GetXmlElements(Tools.GetXmlElement(xmlRoot, "internal_skills"), "internal_skill"))
                    {
                        InternalSkills.Add(InternalSkill.Parse(node));
                    }
                }

                if (xmlRoot.Element("special_skills") != null)
                {
                    if (Tools.GetXmlElement(xmlRoot, "special_skills") != null)
                    {
                        foreach (XElement node in Tools.GetXmlElements(Tools.GetXmlElement(xmlRoot, "special_skills"), "skill"))
                        {
                            SpecialSkills.Add(SpecialSkill.Parse(node));
                        }
                    }
                }
            }

            foreach (var skillXmlFile in GameProject.GetFiles("skill"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + skillXmlFile);
                if (xmlRoot.Element("aoyis") != null)
                {
                    if (Tools.GetXmlElement(xmlRoot, "aoyis") != null)
                    {
                        foreach (XElement node in Tools.GetXmlElements(Tools.GetXmlElement(xmlRoot, "aoyis"), "aoyi"))
                        {
                            Aoyis.Add(Aoyi.Parse(node));
                        }
                    }
                }
            }

            SkillAnimations.Clear();
        }

        static public void Export(string dir)
        {
            XElement rootNode = new XElement("root");
            XElement skillsNode = new XElement("skills");
            rootNode.Add(skillsNode);
            foreach(var s in Skills)
            {
                skillsNode.Add(s.GenerateXml());
            }

            //TODO cg,2014-06-25添加内功、特殊攻击、奥义
            XElement internalSkillsNode = new XElement("internal_skills");
            rootNode.Add(internalSkillsNode);
            foreach(var s in InternalSkills)
            {
                internalSkillsNode.Add(s.GenerateXml());
            }

            XElement spSkillsNode = new XElement("special_skills");
            rootNode.Add(spSkillsNode);
            foreach(var s in SpecialSkills)
            {
                spSkillsNode.Add(s.GenerateXml());
            }

            XElement aoyiNode = new XElement("aoyis");
            rootNode.Add(aoyiNode);
            foreach(var s in Aoyis)
            {
                aoyiNode.Add(s.GenerateXml());
            }

            using (StreamWriter sw = new StreamWriter(dir + "/skills.xml"))
            {
                sw.Write(rootNode.ToString());
            }
        }

        static public Skill GetSkill(string Name)
        {
            foreach (var s in Skills)
            {
                if (s.Name.Equals(Name)) return s;
            }
            
            //MessageBox.Show("错误，调用了未定义的skill:" + Name);
            return null;
        }

        static public List<Skill> GetSkills()
        {
            return Skills;
        }

        static public Skill GetRandomSkill()
        {
            return Skills[Tools.GetRandomInt(0, Skills.Count - 1)];
        }

        static public InternalSkill GetInternalSkill(string Name)
        {
            foreach (var s in InternalSkills)
            {
                if (s.Name.Equals(Name)) return s;
            }
            //MessageBox.Show("错误，调用了未定义的内功:" + Name);
            return null;
        }

        static public InternalSkill GetRandomInternalSkill()
        {
            return InternalSkills[Tools.GetRandomInt(0, InternalSkills.Count - 1)];
        }

        static public UniqueSkill GetInternalUniqueSkill(string Name)
        {
            foreach (var s in InternalSkills)
            {
                foreach (var us in s.UniqueSkills)
                {
                    if (us.Name.Equals(Name)) return us;
                }
            }
            //MessageBox.Show("错误，调用了未定义的内功绝招:" + Name);
            return null;
        }

        static public SpecialSkill GetSpecialSkill(string Name)
        {
            foreach (var s in SpecialSkills)
            {
                if (s.Name.Equals(Name)) return s;
            }
            //MessageBox.Show("错误，调用了未定义的特殊攻击:" + Name);
            return null;
        }

        static public List<Aoyi> GetAoyis()
        {
            return Aoyis;
        }

        static public Aoyi GetAoyi(string Name)
        {
            foreach (var s in Aoyis)
            {
                if (s.Name.Equals(Name)) return s;
            }
            //MessageBox.Show("错误，调用了未定义的奥义:" + Name);
            return null;
        }

        static public AnimationGroup GetSkillAnimation(string animationName)
        {
            return AnimationManager.GetAnimation(animationName, "skill");
        }
    }
}
