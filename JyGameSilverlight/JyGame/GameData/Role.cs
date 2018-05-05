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
using System.IO;

namespace JyGame.GameData
{

    /// <summary>
    /// 角色成长模板
    /// </summary>
    public class RoleGrowTempalte
    {
        public string Name;
        public Dictionary<string, int> Attributes = new Dictionary<string, int>();

        public static string[] attrs = new string[] { "hp","mp","wuxing","shenfa","bili","gengu","fuyuan","dingli","quanzhang","jianfa","daofa","qimen","wuxue"};
        public static RoleGrowTempalte Parse(XElement node)
        {
            RoleGrowTempalte rst = new RoleGrowTempalte();
            rst.Name = Tools.GetXmlAttribute(node, "name");
            foreach (var a in attrs)
            {
                rst.Attributes[a] = int.Parse(Tools.GetXmlAttribute(node, a));
            }
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("grow_template");
            rst.SetAttributeValue("name", Name);
            foreach(var att in attrs)
            {
                rst.SetAttributeValue(att, Attributes[att]);
            }
            return rst;
        }
    }

    /// <summary>
    /// 角色动画模板
    /// </summary>
    public class RoleAnimationTemplate
    {
        public Dictionary<JyGame.UserControls.Spirit.SpiritStatus, List<ImageSource>> Images =
            new Dictionary<JyGame.UserControls.Spirit.SpiritStatus, List<ImageSource>>();

        public Dictionary<JyGame.UserControls.Spirit.SpiritStatus, int> imageWidths =
            new Dictionary<JyGame.UserControls.Spirit.SpiritStatus, int>();
        public Dictionary<JyGame.UserControls.Spirit.SpiritStatus, int> imageHeights =
            new Dictionary<JyGame.UserControls.Spirit.SpiritStatus, int>();

        /// <summary>
        /// s站立
        /// m移动
        /// a攻击
        /// </summary>
        static string[] animationPrefix = new string[] { "s", "m", "a" };

        public static RoleAnimationTemplate Parse(string key)
        {
            string value = ResourceManager.Get(key);

            RoleAnimationTemplate template = new RoleAnimationTemplate();

            string prefix = value.Split(new char[] { '#' })[0].Trim();
            string counts = value.Split(new char[] { '#' })[1].Trim();
            int width = int.Parse(value.Split(new char[] { '#' })[2].Trim());
            int height = int.Parse(value.Split(new char[] { '#' })[3].Trim());
            
            int type = 0;
            foreach (var p in counts.Split(new char[] { '|' }))
            {
                List<ImageSource> imgs = new List<ImageSource>();
                int c = int.Parse(p);
                for (int j = 1; j <= c; ++j)
                {
                    string path = string.Format("{0}{1}{2}.png", prefix, animationPrefix[type], j);
                    BitmapSource imgSource = Tools.GetImage(path);
                    imgs.Add(imgSource);
                }
                template.imageWidths[(JyGame.UserControls.Spirit.SpiritStatus)type] = width;
                template.imageHeights[(JyGame.UserControls.Spirit.SpiritStatus)type] = height;
                template.Images[(JyGame.UserControls.Spirit.SpiritStatus)type] = imgs;
                type++;
            }
            return template;
        }
    }

    /// <summary>
    /// 角色管理器
    /// </summary>
    public class RoleManager
    {
        static private List<Role> Roles = new List<Role>();
        static public List<RoleGrowTempalte> RoleGrowTemplates = new List<RoleGrowTempalte>();

        public static void Init()
        {
            Roles.Clear();
            RoleGrowTemplates.Clear();
            //角色成长模板
            foreach (var roleXmlFile in GameProject.GetFiles("role"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + roleXmlFile);
                if (xmlRoot.Element("grow_templates") != null)
                {
                    foreach (XElement node in xmlRoot.Element("grow_templates").Elements("grow_template"))
                    {
                        RoleGrowTempalte template = RoleGrowTempalte.Parse(node);
                        RoleGrowTemplates.Add(template);
                    }
                }
            }

            //角色
            foreach (var roleXmlFile in GameProject.GetFiles("role"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + roleXmlFile);
                if (xmlRoot.Element("roles") != null)
                {
                    foreach (XElement node in xmlRoot.Element("roles").Elements("role"))
                    {
                        Role role = Role.Parse(node);
                        Roles.Add(role);
                    }
                }
            }
        }

        public static void Export(string dir)
        {
            XElement rootNode = new XElement("root");
            XElement rolesNode = new XElement("roles");
            rootNode.Add(rolesNode);

            foreach(var r in Roles)
            {
                rolesNode.Add(r.GenerateRoleXml());
            }

            XElement growTempalteNode = new XElement("grow_templates");
            rootNode.Add(growTempalteNode);
            foreach(var t in RoleGrowTemplates)
            {
                growTempalteNode.Add(t.GenerateXml());
            }

            string file = dir + "/roles.xml";
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(rootNode.ToString());
            }
        }

        public static RoleGrowTempalte GetRoleGrowTemplate(string name)
        {
            foreach (var t in RoleGrowTemplates)
                if (t.Name == name) return t;
            return null;
        }

        private static Dictionary<string, RoleAnimationTemplate> animationTemplateCache = new Dictionary<string, RoleAnimationTemplate>();
        public static RoleAnimationTemplate GetAnimationTempalte(string key)
        {
            if (animationTemplateCache.ContainsKey(key))
                return animationTemplateCache[key];
            RoleAnimationTemplate rst =  RoleAnimationTemplate.Parse(key);
            animationTemplateCache.Add(key, rst);
            return rst;
        }

        public static Role GetRole(string key)
        {
            foreach (var r in Roles)
            {
                if (r.Key == key)
                    return r;
            }
            return null;
        }

        public static List<Role> GetRoles()
        {
            return Roles;
        }
    }

    /// <summary>
    /// 角色
    /// </summary>
    public class Role
    {
        public Role()
        {
            LeftPoint = 0;
            Level = 1;
            Exp = 0;
            this.AttributesFinal = new AttributeHelper(this);
        }

        private XElement node;
        public int Balls
        {
            get { return _balls; }
            set
            {
                _balls = value;
                if (_balls > 6) _balls = 6;
            }
        }
        private int _balls = 0;

        public string Key;

        private string _animation;
        public string Animation
        {
            get 
            {
                return _animation; 
            }
            set
            {
                _animation = value;
            }
        }

        private string GetItemAnimation()
        {
            foreach(var trigger in this.GetItemTriggers("animation"))
            {
                return trigger.Argvs[1];
            }
            return null;
        }

        private string GetBuffAnimation()
        {
            foreach(var buff in this.Buffs)
            {
                if(buff.buff.Name == "魔神降临")
                {
                    return "moshen";
                }
            }
            return null;
        }

        public List<AnimationImage> GetAnimation(string groupName)
        {
            string animation = this.Animation;
            string tmp = GetItemAnimation();
            if (tmp != null) animation = tmp;

            tmp = GetBuffAnimation();
            if(tmp != null) animation = tmp;
            AnimationGroup gop = AnimationManager.GetAnimation(animation, groupName);
            if (gop == null)
            {
                return null;
            }
            return gop.Images;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        private string _name;
        public SecureDictionary Attributes = new SecureDictionary();
        public AttributeHelper AttributesFinal = null;

        public double SpAddSpeed
        {
            get
            {
                double rst =
                    this.AttributesFinal["shenfa"] / 130.0 + this.AttributesFinal["gengu"] / 130.0;
                if (rst > 2) rst = 2; //设置一个上限
                if (rst < 1.3) rst = 1.3; //下限

                List<ItemTrigger> triggers = this.GetItemTriggers("sp");
                foreach(var t in triggers)
                {
                    rst += double.Parse(t.Argvs[0]);
                }

                BuffInstance mabiBuff = this.GetBuff("麻痹");
                if(mabiBuff!=null)
                {
                    if (mabiBuff.Level > 0)
                    {
                        rst -= 0.2 * mabiBuff.Level;
                    }
                }
                BuffInstance shenxingBuff = this.GetBuff("神行");
                if (shenxingBuff != null)
                {
                    rst += shenxingBuff.Level * 0.2;
                }
                if (rst > 2.5) rst = 2.5; //最快集气速度
                if (rst < 0.8) rst = 0.8; //最慢集气速度
                BuffInstance faintBuff = this.GetBuff("晕眩");
                if(faintBuff!=null)
                {
                    rst = 0;
                }
                return rst;
            }
        }

        public int GetAdditionAttribute(string attribute)
        {
            string chineseName = CommonSettings.AttributeToChinese(attribute);
            List<ItemTrigger> triggers = this.GetItemTriggers("attribute");
            int rst = 0;
            foreach(var t in triggers)
            {
                if(t.Argvs[0] == chineseName)
                {
                    rst += int.Parse(t.Argvs[1]);
                }
            }
            if (this.HasTalent("武学奇才"))
                rst *= 2;
            return rst;
        }

        //public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public Item[] Equipment = new Item[6];

        private void AddCache(ItemTrigger trigger)
        {
            string key = trigger.Name;
            if (!ItemTriggerCache.ContainsKey(key))
                ItemTriggerCache[key] = new List<ItemTrigger>();
            if (!ItemTriggerCache.ContainsKey("ALL"))
                ItemTriggerCache["ALL"] = new List<ItemTrigger>();

            ItemTriggerCache[key].Add(trigger);
            ItemTriggerCache["ALL"].Add(trigger);
        }

        /// <summary>
        /// 刷新trigger cache
        /// </summary>
        private void RefreshItemTriggerCache()
        {
            ItemTriggerCache.Clear();
            List<ItemTrigger> rst = new List<ItemTrigger>();
            //来自物品
            foreach (var item in Equipment)
            {
                if (item != null)
                {
                    if (item.Triggers != null && item.Triggers.Count > 0)
                    {
                        foreach (var tr in item.Triggers)
                        {
                            AddCache(tr);
                        }
                    }
                    if (item.AdditionTriggers != null && item.AdditionTriggers.Count > 0)
                    {
                        foreach (var tr in item.AdditionTriggers)
                        {
                            AddCache(tr);
                        }
                    }
                }
            }
            //来自技能
            foreach (var skill in this.Skills)
            {
                foreach (var tr in skill.Skill.Triggers)
                {
                    if (skill.Level >= tr.Level)
                    {
                        AddCache(tr);
                    }
                }
            }
            //来自内功
            foreach (var skill in this.InternalSkills)
            {
                foreach (var tr in skill.Skill.Triggers)
                {
                    if (skill.Level >= tr.Level)
                    {
                        AddCache(tr);
                    }
                }
            }
        }

        public Dictionary<string, List<ItemTrigger>> ItemTriggerCache = new Dictionary<string, List<ItemTrigger>>();

        /// <summary>
        /// 获取所有的物品trigger
        /// </summary>
        /// <param name="name">物品名，如果为空，则取所有的</param>
        /// <returns></returns>
        public List<ItemTrigger> GetItemTriggers(string name, bool isRefreshCache = true)
        {
            if(isRefreshCache)
                this.RefreshItemTriggerCache();
            if (name == "" && ItemTriggerCache.ContainsKey("ALL"))
                return ItemTriggerCache["ALL"];
            if (ItemTriggerCache.ContainsKey(name))
                return ItemTriggerCache[name];
            else
                return new List<ItemTrigger>();
        }
        public List<ItemTrigger> GetAllItemTriggers()
        {
            return this.GetItemTriggers("");
        }

        public int LeftPoint
        {
            set { _leftPoint = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_leftPoint); }
        }
        private int _leftPoint;

        public int Level
        {
            set { _level = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_level); }
        }
        private int _level;

        public int Exp
        {
            set { _exp = DEncryptHelper.EncryptInt(value); }
            get { return DEncryptHelper.DecryptInt(_exp); }
        }
        private int _exp;

        public string Talent;
        public RoleGrowTempalte GrowTemplate = null;

        #region 评估 

        public string GetAttributeString(string attr)
        {
            return string.Format("{0,3}+({1})", this.Attributes[attr], this.GetAdditionAttribute(attr));
        }
        public int GetSkillAddition(int type)
        {
            int result = 0;
            //foreach (var s in this.Skills)
            //{
            //    if (s.Skill.Type == type)
            //    {
            //        result += (int)(s.Power);
            //    }
            //}
            if (type == CommonSettings.SKILLTYPE_QUAN)
            {
                return this.GetAdditionAttribute("quanzhang");
            }else if(type==CommonSettings.SKILLTYPE_JIAN)
            {
                return this.GetAdditionAttribute("jianfa");
            }
            else if (type == CommonSettings.SKILLTYPE_DAO)
            {
                return this.GetAdditionAttribute("daofa");
            }
            else if (type == CommonSettings.SKILLTYPE_QIMEN)
            {
                return this.GetAdditionAttribute("qimen");
            }
            return 0;
        }

        public double Defence
        {
            get
            {
                double result = 1;
                result = (10 + AttributesFinal["dingli"] / 40.0 + AttributesFinal["gengu"] / 70.0);
                result *= (1 + this.GetEquippedInternalSkill().Defence);
                foreach (var item in Equipment)
                {
                    if (item == null || item.Triggers == null) continue;
                    foreach (var trigger in item.AllTriggers)
                    {
                        if (trigger.Name == "defence")
                        {
                            result += double.Parse(trigger.Argvs[0]) / 20;
                        }
                    }
                }
                result += this.Attributes["maxhp"] / 250;
                return result;
            }
        }

        public double Attack
        {
            get
            {
                //最大的武器系数
                //臂力等属性
                //内功加成
                //装备加成
                double result = 1;
                result *= (4.0 + Attributes["bili"] / 120.0);
                int max = 0;
                if (AttributesFinal["quanzhang"] > max) max = AttributesFinal["quanzhang"];
                if (AttributesFinal["jianfa"] > max) max = AttributesFinal["jianfa"];
                if (AttributesFinal["daofa"] > max) max = AttributesFinal["daofa"];
                if (AttributesFinal["qimen"] > max) max = AttributesFinal["qimen"];
                result *= (2.0 + max / 120.0);
                result *= (1 + this.GetEquippedInternalSkill().Attack);
                foreach (var item in Equipment)//攻击增益
                {
                    if (item == null || item.Triggers == null) continue;

                    foreach(var trigger in item.AllTriggers)
                    {
                        if(trigger.Name == "attack")
                        {
                            result += double.Parse(trigger.Argvs[0]) / 35;
                        }
                    }
                }
                return result;
            }
        }

        public double Critical
        {
            get
            {
                double result = 1;
                result = (AttributesFinal["fuyuan"] / 50.0) / 20.0 * (1 + this.GetEquippedInternalSkill().Critical);

                foreach (var trigger in this.GetAllItemTriggers())
                {
                    if (trigger.Name == "attack")
                    {
                        result += double.Parse(trigger.Argvs[1]) / 100.0;
                    }
                }
                
                return result;
            }
        }

        #endregion

        /// <summary>
        /// 是否是女性
        /// </summary>
        public bool Female
        {
            get
            {
                return this.Attributes["female"] == 1;
            }
        }

        /// <summary>
        /// 是否是动物
        /// </summary>
        public bool Animal
        {
            get
            {
                return this.Attributes["female"] == -1;
            }
        }

        #region 天赋

        /// <summary>
        /// 获取天赋已经消耗的武学点数
        /// </summary>
        /// <returns></returns>
        public int GetTotalWuxueCost()
        {
            int rst = 0;
            foreach (var t in Talents)
            {
                rst += JyGame.GameData.Talent.GetTalentCost(t);
            }
            return rst;
        }

        /// <summary>
        /// 判断一个角色是否能学某个天赋
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool CanLearnTalent(string t, ref int need)
        {
            int cost = int.Parse(ResourceManager.Get("天赋." + t).Split(new char[] { '#' })[0]);
            int wuxue = this.Attributes["wuxue"];
            int costed = this.GetTotalWuxueCost();
            need = cost;
            return !(cost + costed > wuxue);
        }

        public List<string> Talents
        {
            get
            {
                List<string> rst = new List<string>();
                if (this.Talent == null || this.Talent.Equals(string.Empty))
                {
                    return rst;
                }
                string[] talents = this.Talent.Split(new char[] { '#' });
                foreach (var t in talents) { rst.Add(t); }
                return rst;
            }
        }

        public void RemoveTalent(string talent)
        {
            //移除天赋
            List<string> rst = new List<string>();
            if (Talent != null && (!Talent.Equals(string.Empty)))
            {
                string[] talents = Talent.Split(new char[] { '#' });
                foreach (var t in talents)
                {
                    if (t != talent)
                        rst.Add(t);
                }
            }

            if (rst.Count == 0)
                Talent = "";
            else
            {
                Talent = rst[0];
                for (int i = 1; i < rst.Count; i++)
                    Talent += "#" + rst[i];
            }
        }
        
        public List<string> EquipmentTalents
        {
            get
            {
                List<string> rst = new List<string>();
                List<string> myTalent = this.Talents;

                foreach(var t in this.GetItemTriggers("talent"))
                {
                    if (t.Name == "talent" && !myTalent.Contains(t.Argvs[0]))
                        rst.Add(t.Argvs[0]);
                }
                foreach (var t in this.Equipment)
                {
                    if (t != null)
                    {
                        foreach (var c in t.Talent)
                        {
                            if (!rst.Contains(c) && !myTalent.Contains(c))
                                rst.Add(c);
                        }
                    }
                }
                return rst;
            }
        }

        public List<string> InternalTalents
        {
            get
            {
                List<string> myTalent = this.Talents;
                List<string> equipmengTalents = this.EquipmentTalents;
                List<string> rst = new List<string>();
                InternalSkillInstance internalSkill = this.GetEquippedInternalSkill();
                if (internalSkill == null) return rst;
                foreach (var c in internalSkill.Talent)
                {
                    if (!rst.Contains(c) && !myTalent.Contains(c) && !equipmengTalents.Contains(c))
                        rst.Add(c);
                }
                return rst;
            }
        }

        /// <summary>
        /// 是否有天赋
        /// </summary>
        /// <param name="talentName"></param>
        /// <returns></returns>
        public bool HasTalent(string talentName)
        {
            //角色天赋
            foreach (var t in this.Talents)
            {
                if (talentName.Equals(t))
                    return true;
            }
            //装备天赋
            foreach (var t in this.EquipmentTalents)
            {
                if (talentName.Equals(t)) 
                    return true;
            }
            //内功天赋
            InternalSkillInstance internalSkill = this.GetEquippedInternalSkill();
            if (internalSkill != null)
            {
                if (internalSkill.HasTalent(talentName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取天赋说明
        /// </summary>
        public string TalentsString
        {
            get
            {
                string rst = "";
                foreach (var t in Talents)
                {
                    rst += string.Format("{0}:(消耗武学常识:{1}){2}\n\n", 
                        t,
                        JyGame.GameData.Talent.GetTalentCost(t),
                        ResourceManager.Get("天赋." + t).Split(new char[] { '#' })[1]);
                }
                return rst;
            }
        }

        private void addRandomTalent()
        {
            string talent = "";

            //根据难度增加随机天赋
            int talentCount = 0;
            if (RuntimeData.Instance.GameMode == "hard") talentCount = 1;
            //if (RuntimeData.Instance.GameMode == "crazy") talentCount = 3;

            //疯狂模式
            if (RuntimeData.Instance.GameMode == "crazy")
            {
                string attackTalent = CommonSettings.GetEnemyRandomTalentListCrazyAttack();
                string defenceTalent = CommonSettings.GetEnemyRandomTalentListCrazyDefence();
                string otherTalent = CommonSettings.GetEnemyRandomTalentListCrazyOther();
                talent = attackTalent + "#" + defenceTalent + "#" + otherTalent;

                if (this.Talent == null || this.Talent == "")
                    this.Talent = talent;
                else
                    this.Talent += "#" + talent;
            }
            else
            {
                for (int i = 0; i < talentCount; ++i)
                {
                    while (true)
                    {
                        talent = CommonSettings.GetEnemyRandomTalent(this.Female);
                        if (!this.HasTalent(talent)) break;
                    }
                    if (this.Talent == null || this.Talent == "")
                        this.Talent = talent;
                    else
                        this.Talent += "#" + talent;
                }
            }
        }

        private void addRandomEquippments()
        {
            string[] weaponPool = new string[] { "", "" };
            double property = 0;
            if(RuntimeData.Instance.GameMode == "hard")
            {
                property = 0.2;
            }
            if (RuntimeData.Instance.GameMode == "crazy")
            {
                property = 0.5;
            }

            //为角色增加随机装备
            if (this.Equipment[(int)ItemType.Weapon] == null && Tools.ProbabilityTest(property))
            {
                string weaponName = CommonSettings.GetEnemyRandomWeaponItem();
                this.Equipment[(int)ItemType.Weapon] = ItemManager.GetItem(weaponName).Clone(true);
            }

            if (this.Equipment[(int)ItemType.Armor] == null && Tools.ProbabilityTest(property))
            {
                string defenceName = CommonSettings.GetEnemyRandomDefenceItem();
                this.Equipment[(int)ItemType.Armor] = ItemManager.GetItem(defenceName).Clone(true);
            }

            if (this.Equipment[(int)ItemType.Accessories] == null && Tools.ProbabilityTest(property))
            {
                string specialItemName = CommonSettings.GetEnemyRandomSpecialItem();
                this.Equipment[(int)ItemType.Accessories] = ItemManager.GetItem(specialItemName).Clone(true);
            }
        }

        //增加随机天赋
        public void addRandomTalentAndWeapons()
        {
            this.addRandomTalent();
            this.addRandomEquippments();
        }

        //增加技能等级
        public void addSkillLevel()
        {
            int round = RuntimeData.Instance.Round;
            int levelAdd = round / 2; //每两个周目成长1级
            if (levelAdd > 0)
            {
                foreach(var s in this.Skills)
                {
                    s.Level += levelAdd;
                    if (s.Level > 20) s.Level = 20;
                }
                foreach(var s in this.InternalSkills)
                {
                    s.Level += levelAdd;
                    if (s.Level > 20) s.Level = 20;
                }
            }
        }

        #endregion

        /// <summary>
        /// 技能
        /// skill - level
        /// </summary>
        public List<SkillInstance> Skills = new List<SkillInstance>();
        public List<InternalSkillInstance> InternalSkills = new List<InternalSkillInstance>();
        public List<SpecialSkillInstance> SpecialSkills = new List<SpecialSkillInstance>();
        public string HeadPicPath
        {
            set
            {
                _headpicpath = value;
            }
            get { return _headpicpath; }
        }
        private string _headpicpath;

        /// <summary>
        ///  头像
        /// </summary>
        public ImageSource Head { get { return ResourceManager.GetImage(_headpicpath); } }
        public int LevelupExp { get { return CommonSettings.LevelupExp(this.Level); } }
        public int PrevLevelupExp { get { return CommonSettings.LevelupExp(this.Level - 1); } }
        /// <summary>
        /// 增加经验值
        /// </summary>
        /// <param name="add">经验值</param>
        /// <returns>是否升级</returns>
        public bool AddExp(int add)
        {
            Exp += add;

            //经书
            Item book = this.Equipment[(int)ItemType.Book];
            if (book != null)
            {
                ItemSkill itemSkill = book.GetItemSkill();
                if (itemSkill.IsInternal) //内功
                {
                    bool learned = false;
                    foreach (var s in this.InternalSkills)
                    {
                        if (s.Skill.Name.Equals(itemSkill.SkillName))
                        {
                            s.MaxLevel = Math.Max(s.MaxLevel,itemSkill.MaxLevel);
                            s.TryAddExp(add);
                            learned = true;
                        }
                    }
                    if (!learned && InternalSkills.Count < CommonSettings.MAX_INTERNALSKILL_COUNT)
                    {
                        //MessageBox.Show(string.Format("【{0}】学会内功【{1}】", this.Name, itemSkill.SkillName));

                        UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
                        Dialog dialog = new Dialog();
                        dialog.role = this.Key;
                        dialog.type = "DIALOG";
                        dialog.info = string.Format("【{0}】学会内功【{1}】", this.Name, itemSkill.SkillName);
                        uiHost.dialogPanel.ShowDialog(dialog);

                        InternalSkillInstance internalSkillInstance = new InternalSkillInstance()
                        {
                            Level = 1,
                            MaxLevel = itemSkill.MaxLevel,
                            Owner = this,
                            Equipped = false,
                        };
                        internalSkillInstance.Skill = SkillManager.GetInternalSkill(itemSkill.SkillName);
                        this.InternalSkills.Add(internalSkillInstance);
                        internalSkillInstance.TryAddExp(add);
                    }
                }
                else //外功
                {
                    bool learned = false;
                    foreach (var s in this.Skills)
                    {
                        if (s.Skill.Name.Equals(itemSkill.SkillName))
                        {
                            s.MaxLevel = Math.Max(s.MaxLevel, itemSkill.MaxLevel);
                            s.TryAddExp(add);
                            learned = true;
                        }
                    }
                    if (!learned && this.Skills.Count<CommonSettings.MAX_SKILL_COUNT)
                    {
                        //MessageBox.Show(string.Format("【{0}】学会武功【{1}】", this.Name, itemSkill.SkillName));
                        UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
                        Dialog dialog = new Dialog();
                        dialog.role = this.Key;
                        dialog.type = "DIALOG";
                        dialog.info = string.Format("【{0}】学会武功【{1}】", this.Name, itemSkill.SkillName);
                        uiHost.dialogPanel.ShowDialog(dialog);

                        SkillInstance skillInstance = new SkillInstance()
                        {
                            Level = 1,
                            MaxLevel = itemSkill.MaxLevel,
                            Owner = this,
                        };
                        skillInstance.Skill = SkillManager.GetSkill(itemSkill.SkillName);
                        this.Skills.Add(skillInstance);
                        skillInstance.TryAddExp(add);
                    }
                }
            }

            bool levelupFlag = false;
            //30级为顶级
            if (Level >= CommonSettings.MAX_LEVEL)
            {
                Exp = 0;
            }
            while (Exp > LevelupExp && Level < CommonSettings.MAX_LEVEL)
            {
                Level++;
                
                //升级奖励
                LeftPoint += 2;

                Attributes["maxhp"] += this.GrowTemplate.Attributes["hp"];
                Attributes["maxmp"] += this.GrowTemplate.Attributes["mp"];

                if (Attributes["bili"] < CommonSettings.MAX_ATTRIBUTE) Attributes["bili"] += this.GrowTemplate.Attributes["bili"];
                if (Attributes["fuyuan"] < CommonSettings.MAX_ATTRIBUTE) Attributes["fuyuan"] += this.GrowTemplate.Attributes["fuyuan"];
                if (Attributes["gengu"] < CommonSettings.MAX_ATTRIBUTE) Attributes["gengu"] += this.GrowTemplate.Attributes["gengu"];
                if (Attributes["dingli"] < CommonSettings.MAX_ATTRIBUTE) Attributes["dingli"] += this.GrowTemplate.Attributes["dingli"];
                if (Attributes["shenfa"] < CommonSettings.MAX_ATTRIBUTE) Attributes["shenfa"] += this.GrowTemplate.Attributes["shenfa"];

                if (Attributes["quanzhang"] < CommonSettings.MAX_ATTRIBUTE) Attributes["quanzhang"] += this.GrowTemplate.Attributes["quanzhang"];
                if (Attributes["jianfa"] < CommonSettings.MAX_ATTRIBUTE) Attributes["jianfa"] += this.GrowTemplate.Attributes["jianfa"];
                if (Attributes["daofa"] < CommonSettings.MAX_ATTRIBUTE) Attributes["daofa"] += this.GrowTemplate.Attributes["daofa"];
                if (Attributes["qimen"] < CommonSettings.MAX_ATTRIBUTE) Attributes["qimen"] += this.GrowTemplate.Attributes["qimen"];
                
                
                Attributes["wuxue"] += this.GrowTemplate.Attributes["wuxue"];

                levelupFlag = true;
            }

            return levelupFlag;
        }

        #region 内功
        public void EquipInternalSkill(InternalSkillInstance internalskill)
        {
            if (!InternalSkills.Contains(internalskill))
                throw new Exception("装备了一个不属于本角色的内功");

            foreach (var s in InternalSkills) { s.Equipped = false; }
            internalskill.Equipped = true;
        }

        public InternalSkillInstance GetEquippedInternalSkill()
        {
            foreach (var s in InternalSkills)
            {
                if (s.Equipped) return s;
            }
            return null;
        }

        public void SkillCdRecover()
        {
            foreach (var s in Skills)
            {
                s.CurrentCd = 0;
                foreach (var us in s.UniqueSkillInstances)
                {
                    us.Cd = 0;
                }
            }
            foreach (var s in InternalSkills)
            {
                foreach (var us in s.UniqueSkillInstances)
                {
                    us.Cd = 0;
                }
            }
            foreach (var s in SpecialSkills)
            {
                s.Cd = 0;
            }
        }
        #endregion

        public List<SkillBox> GetAvaliableSkills()
        {
            List<SkillBox> rst = new List<SkillBox>();
            foreach (var s in this.SpecialSkills)
            {
                rst.Add(new SkillBox() { SpecialSkill = s });
            }
            foreach (var s in this.Skills)
            {
                if (!s.IsUsed) continue;
                rst.Add(new SkillBox() { Instance = s });
                foreach (var us in s.UniqueSkillInstances )
                {
                    if(s.Level >= us.Skill.RequireLevel)
                        rst.Add(new SkillBox() { Instance = s, UniqueSkill = us });
                }
            }
            InternalSkillInstance EquippedInternalSkill = this.GetEquippedInternalSkill();
            if (EquippedInternalSkill != null)
            {
                foreach (var us in EquippedInternalSkill.UniqueSkillInstances)
                {
                    if (EquippedInternalSkill.Level >= us.Skill.RequireLevel)
                        rst.Add(new SkillBox() { Instance = null, UniqueSkill = us });
                }
            }
            return rst;
        }

        public void GetAttribute(XElement node, string attribute)
        {
            this.Attributes[attribute] = int.Parse(Tools.GetXmlAttribute(node, attribute));
        }

        public void Reset(bool recover=true)
        {
            if (recover)
            {
                this.Attributes["hp"] = this.Attributes["maxhp"];
                this.Attributes["mp"] = this.Attributes["maxmp"];
            }
            else
            {
                if (this.Attributes["hp"] <= 0) this.Attributes["hp"] = 1;
                if (this.Attributes["mp"] <= 0) this.Attributes["mp"] = 0;
            }
            this.Balls = 0;
            this.SkillCdRecover();
            this.ClearBuffs();
        }

        #region BUFF&DEBUFF
        public List<BuffInstance> Buffs = new List<BuffInstance>();
        public void ClearBuffs()
        {
            Buffs.Clear();
        }
        public List<RoundBuffResult> RunBuffs()
        {
            List<RoundBuffResult> rst = new List<RoundBuffResult>();
            List<BuffInstance> removeList = new List<BuffInstance>();
            
            //bool equipeNecklaceOfDream = false;
            //foreach (var equip in this.Equipment)
            //{
            //    if (equip != null && equip.Name == "仙丽雅的项链")
            //    {
            //        equipeNecklaceOfDream = true;
            //        break;
            //    }
            //}

            foreach (var b in Buffs) 
            {
                b.TimeStamp++;
                if (b.TimeStamp < CommonSettings.BUFF_RUN_CYCLE) //还没到计时
                {
                    continue;
                }
                b.TimeStamp = 0; //清空计时
                rst.Add(b.RoundEffect());
                b.LeftRound--;

                //if ( (this.HasTalent("清心") || equipeNecklaceOfDream) && b.IsDebuff)
                if (this.HasTalent("清心") && b.IsDebuff)
                {
                    if( Tools.ProbabilityTest(0.5))
                        b.LeftRound = 0;
                }
                if (this.HasTalent("清风") && b.IsDebuff)
                {
                    if (Tools.ProbabilityTest(0.015 * this.Level))
                    {
                        b.LeftRound = 0;
                    }
                }
                if (b.LeftRound <= 0)
                {
                    removeList.Add(b);
                }
            }
            foreach (var b in removeList)
            {
                Buffs.Remove(b);
            }
            return rst;
        }
        public void AddBuff(BuffInstance buffAdd)
        {
            BuffInstance buff = GetBuff(buffAdd.buff.Name);
            buffAdd.Owner = this;
            if (buff == null)
            {
                Buffs.Add(buffAdd);
            }
            else
            {
                if (buffAdd.Level >= buff.Level) //覆盖刷新buff
                {
                    buff.TimeStamp = 0;
                    buff.Level = buffAdd.Level;
                    if (buffAdd.LeftRound > buff.LeftRound)
                        buff.LeftRound = buffAdd.LeftRound;
                }
               
            }
        }
        public BuffInstance GetBuff(string name)
        {
            foreach (var b in Buffs)
            {
                if (b.buff.Name.Equals(name))
                    return b;
            }
            return null;
        }
        public bool DeleteBuff(string name)
        {
            BuffInstance tag = null;
            foreach (var b in Buffs)
            {
                if (b.buff.Name.Equals(name))
                {
                    tag = b;
                    break;
                }
            }
            if (tag != null) Buffs.Remove(tag);
            return tag != null;
        }
        //public string BuffInfo { get { return CommonSettings.BuffInfo(Buffs); } }
        #endregion

        static public Role Parse(XElement node)
        {
            Role role = new Role();
            role.node = node;
            role.Key = Tools.GetXmlAttribute(node, "key");
            role.Animation = Tools.GetXmlAttribute(node, "animation");
            role.Name = Tools.GetXmlAttribute(node, "name");
            role.HeadPicPath = Tools.GetXmlAttribute(node, "head");
            role.Level = Tools.GetXmlAttributeInt(node, "level");
            role.Exp = role.PrevLevelupExp;

            if(node.Attribute("balls") != null)
                role.Balls = Tools.GetXmlAttributeInt(node, "balls");

            if (node.Element("buffs") != null)
                role.Buffs = BuffInstance.parseOLData(Tools.GetXmlElement(node, "buffs"), role);

            //必填项属性，会影响战斗判定
            foreach (var s in CommonSettings.RoleAttributeList)
            {
                role.GetAttribute(node, s);
            }

            //经验
            if (node.Attribute("exp") != null)
            {
                role.Exp = Tools.GetXmlAttributeInt(node, "exp");
            }

            if (node.Attribute("talent") != null)
            {
                role.Talent = Tools.GetXmlAttribute(node, "talent");
            }

            if (node.Attribute("leftpoint") != null)
            {
                role.LeftPoint = Tools.GetXmlAttributeInt(node, "leftpoint");
            }

            //竞技场相关
            if (node.Attribute("arena") != null && Tools.GetXmlAttribute(node, "arena") == "no")
            {
                role.Attributes["arena"] = 0;
            }
            else
            {
                role.Attributes["arena"] = 1;
            }

            if (node.Attribute("arenaJoinProb") != null)
            {
                role.Attributes["arenaJoinProb"] = Tools.GetXmlAttributeInt(node, "arenaJoinProb");
            }
            else
            {
                role.Attributes["arenaJoinProb"] = 0;
            }

            if (node.Attribute("grow_template") != null) //角色成长模板
            {
                string templateName = Tools.GetXmlAttribute(node, "grow_template");
                role.GrowTemplate = RoleManager.GetRoleGrowTemplate(templateName);
            }
            else
            {
                role.GrowTemplate = RoleManager.GetRoleGrowTemplate("default");
            }

            if (node.Attribute("wuxue") != null)
            {
                role.Attributes["wuxue"] = Tools.GetXmlAttributeInt(node, "wuxue");
            }
            else
            {
                role.Attributes["wuxue"] = 20 + role.Level * role.GrowTemplate.Attributes["wuxue"]; //如果没有指定，则按照成长模板来
            }

            //技能
            foreach(var skill in node.Element("skills").Elements("skill"))
            {
                SkillInstance sintance = new SkillInstance()
                {
                    Skill = SkillManager.GetSkill(Tools.GetXmlAttribute(skill, "name")),
                    Level = Tools.GetXmlAttributeInt(skill, "level"),
                    MaxLevel = Tools.GetXmlAttributeInt(skill, "maxlevel"),
                    Owner = role,
                };
                if (skill.Attribute("exp") != null)
                {
                    sintance.Exp = Tools.GetXmlAttributeInt(skill, "exp");
                }
                if (skill.Attribute("cd") != null)
                {
                    sintance.CurrentCd = Tools.GetXmlAttributeInt(skill, "cd");
                }
                if (skill.Attribute("use") != null)
                {
                    sintance.IsUsed = Tools.GetXmlAttributeBool(skill, "use");
                }
                if (skill.Element("uniqueSkillCd") != null)
                {
                    foreach (XElement cdXML in Tools.GetXmlElements(skill, "uniqueSkillCd"))
                    {
                        string skillKey = Tools.GetXmlAttribute(cdXML, "skill");
                        int cd = Tools.GetXmlAttributeInt(cdXML, "cd");
                        foreach (var instance in sintance.UniqueSkillInstances)
                        {
                            if (instance.Skill.Name == skillKey)
                                instance.Cd = cd;
                        }
                    }
                }

                role.Skills.Add(sintance);
            }

            foreach (var s in node.Element("internal_skills").Elements("internal_skill"))
            {
                InternalSkillInstance sintance = new InternalSkillInstance()
                {
                    Skill = SkillManager.GetInternalSkill(Tools.GetXmlAttribute(s, "name")),
                    Level = Tools.GetXmlAttributeInt(s, "level"),
                    Equipped = Tools.GetXmlAttributeInt(s, "equipped") == 1,
                    MaxLevel = Tools.GetXmlAttributeInt(s, "maxlevel"),
                    Owner = role,
                };
                if (s.Attribute("exp") != null)
                {
                    sintance.Exp = Tools.GetXmlAttributeInt(s, "exp");
                }

                if (s.Element("uniqueSkillCd") != null)
                {
                    foreach (XElement cdXML in Tools.GetXmlElements(s, "uniqueSkillCd"))
                    {
                        string skillKey = Tools.GetXmlAttribute(cdXML, "skill");
                        int cd = Tools.GetXmlAttributeInt(cdXML, "cd");
                        foreach (var instance in sintance.UniqueSkillInstances)
                        {
                            if (instance.Skill.Name == skillKey)
                                instance.Cd = cd;
                        }
                    }
                }

                role.InternalSkills.Add(sintance);
            }

            if (node.Element("special_skills") != null)
            {
                foreach (var s in node.Element("special_skills").Elements("skill"))
                {
                    SpecialSkillInstance skill = new SpecialSkillInstance()
                    {
                        Skill = SkillManager.GetSpecialSkill(Tools.GetXmlAttribute(s, "name")),
                        Cd = 0,
                        Owner = role
                    };
                    if (s.Attribute("cd") != null)
                    {
                        skill.Cd = Tools.GetXmlAttributeInt(s, "cd");
                    }
                    role.SpecialSkills.Add(skill);
                }

            }

            //物品
            if (node.Element("items") != null)
            {
                foreach (var item in node.Element("items").Elements("item"))
                {
                    string itemName = Tools.GetXmlAttribute(item,"name");
                    Item myItem = ItemManager.GetItem(itemName).Clone();
                    role.Equipment[Tools.GetXmlAttributeInt(item, "type")] = myItem;
                    if(item.Element("addition_triggers") != null)
                    {
                        myItem.SetAdditionTriggers(item.Element("addition_triggers"));
                    }
                }
            }
            return role;
        }

        public Role Clone()
        {
            Role r = Role.Parse(this.node);
            r.Name = this.Name;
            return r;
        }

        #region 生成XML
        public XElement GenerateRoleXml()
        {
            XElement rootNode = new XElement("role");
            //基础属性
            rootNode.SetAttributeValue("name", this.Name);
            rootNode.SetAttributeValue("level", this.Level);
            rootNode.SetAttributeValue("key", this.Key);
            rootNode.SetAttributeValue("head", this.HeadPicPath);
            rootNode.SetAttributeValue("leftpoint", this.LeftPoint);
            rootNode.SetAttributeValue("animation", this.Animation);
            rootNode.SetAttributeValue("exp", this.Exp);
            rootNode.SetAttributeValue("talent", this.Talent);
            rootNode.SetAttributeValue("balls", this.Balls);

            //buff
            XElement buffsXML = new XElement("buffs");
            foreach (BuffInstance buff in this.Buffs)
            {
                buffsXML.Add(buff.toOLDataXML());
            }
            rootNode.Add(buffsXML);

            if (this.GrowTemplate != null)
            {
                rootNode.SetAttributeValue("grow_template", this.GrowTemplate.Name);
            }
            //Attributes
            foreach (var key in this.Attributes.Keys)
            {
                rootNode.SetAttributeValue(key, Attributes[key]);
            }
            //技能树
            rootNode.Add(this.GenerateSkillXml());
            rootNode.Add(this.GenerateInternalSkillXml());
            rootNode.Add(this.GenerateSpecialSkillXml());

            //装备物品
            rootNode.Add(this.GenerateItemXml());

            return rootNode;
        }

        private XElement GenerateSkillXml()
        {
            XElement rootNode = new XElement("skills");
            foreach (var s in this.Skills)
            {
                XElement skillNode = new XElement("skill");
                skillNode.SetAttributeValue("level", s.Level);
                skillNode.SetAttributeValue("maxlevel", s.MaxLevel);
                skillNode.SetAttributeValue("exp", s.Exp);
                skillNode.SetAttributeValue("name", s.Skill.Name);
                skillNode.SetAttributeValue("cd", s.CurrentCd);
                skillNode.SetAttributeValue("use", s.IsUsed);

                foreach (var skill in s.UniqueSkillInstances)
                {
                    XElement cdXML = new XElement("uniqueSkillCd");
                    cdXML.SetAttributeValue("skill", skill.Skill.Name);
                    cdXML.SetAttributeValue("cd", skill.Cd);
                    skillNode.Add(cdXML);
                }

                rootNode.Add(skillNode);
            }
            return rootNode;
        }

        private XElement GenerateInternalSkillXml()
        {
            XElement rootNode = new XElement("internal_skills");
            foreach (var s in this.InternalSkills)
            {
                XElement skillNode = new XElement("internal_skill");
                skillNode.SetAttributeValue("level", s.Level);
                skillNode.SetAttributeValue("maxlevel", s.MaxLevel);
                skillNode.SetAttributeValue("exp", s.Exp);
                skillNode.SetAttributeValue("name", s.Skill.Name);
                skillNode.SetAttributeValue("equipped", s.Equipped ? 1:0);

                foreach (var skill in s.UniqueSkillInstances)
                {
                    XElement cdXML = new XElement("uniqueSkillCd");
                    cdXML.SetAttributeValue("skill", skill.Skill.Name);
                    cdXML.SetAttributeValue("cd", skill.Cd);
                    skillNode.Add(cdXML);
                }

                rootNode.Add(skillNode);
            }
            return rootNode;
        }

        private XElement GenerateSpecialSkillXml()
        {
            XElement rootNode = new XElement("special_skills");
            foreach (var s in this.SpecialSkills)
            {
                XElement skillNode = new XElement("skill");
                skillNode.SetAttributeValue("name", s.Skill.Name);
                skillNode.SetAttributeValue("cd", s.Cd);
                rootNode.Add(skillNode);
            }
            return rootNode;
        }

        private XElement GenerateItemXml()
        {
            XElement rootNode = new XElement("items");

            for (int i = 0; i < this.Equipment.Length; ++i)
            {
                if (this.Equipment[i] != null)
                {
                    Item item = this.Equipment[i];
                    rootNode.Add(item.ToXml());
                }
            }
            return rootNode;
        }

        #endregion
    }

    public class AttributeHelper
    {
        public AttributeHelper(Role Owner) { owner = Owner; }
        private Role owner = null;
        public int this[string key]
        {
            get
            {
                return owner.Attributes[key] + owner.GetAdditionAttribute(key);
            }
        }
    }

}
