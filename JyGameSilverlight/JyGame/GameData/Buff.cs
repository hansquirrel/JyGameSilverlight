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
using System.Xml;
using System.Xml.Linq;

namespace JyGame.GameData
{
    public class Buff
    {
        public static string BuffsToString(List<Buff> buffs)
        {
            if (buffs.Count > 0)
            {
                string buffStr = "";
                foreach (var buff in buffs)
                {
                    buffStr += string.Format("#{0}.{1}.{2}.{3}", buff.Name, buff.Level, buff.Round, buff.Property);
                }
                buffStr = buffStr.TrimStart(new char[] { '#' });
                return buffStr;
            }
            return "";
        }

        public static string[] BuffNames = { "恢复", "集气", "攻击强化", "飘渺", "左右互搏", "神速攻击", "醉酒", "溜须拍马", "易容", 
                                               "狂战", "坚守", "沾衣十八跌", "圣战", "轻身", "防御强化", "魔神降临", "神行" };
        public static string[] DebuffNames = { "中毒", "内伤", "致盲", "缓速", "晕眩", "攻击弱化", "诸般封印", "剑封印", "刀封印", 
                                                 "拳掌封印", "奇门封印", "伤害加深", "重伤", "定身", "封穴", "点穴" };

        public string Name;
        public int Level;
        public int Round;
        public int Property = -1; //概率
        public bool IsDebuff
        {
            get
            {
                foreach (var s in BuffNames)
                {
                    if (Name.Equals(s))
                        return false;
                }
                return true;
            }
        }

        static public List<Buff> Parse(string content)
        {
            List<Buff> rst = new List<Buff>();
            foreach(var s in content.Split(new char[] { '#' }))
            {
                string name = s.Split(new char[] { '.' })[0];
                int level = 1;
                int round = 3;
                int property = -1;
                if (s.Split(new char[] { '.' }).Length > 1)
                {
                    level = int.Parse(s.Split(new char[] { '.' })[1]);
                }
                if (s.Split(new char[] { '.' }).Length > 2)
                {
                    round = int.Parse(s.Split(new char[] { '.' })[2]);
                }
                if (s.Split(new char[] { '.' }).Length > 3)
                {
                    property = int.Parse(s.Split(new char[] { '.' })[3]);
                }
                rst.Add(new Buff() { Name = name, Level = level, Round = round, Property = property });
            }
            return rst;
        }
    }

    public class BuffInstance
    {
        public Role Owner;
        public Buff buff;
        public int Level //若没有set，则默认使用buff的等级
        {
            get { return _level == -1 ? buff.Level : _level; }
            set { _level = value; }
        }
        private int _level = -1;

        public int LeftRound;
        public bool IsDebuff { get { return buff.IsDebuff; } }

        public int TimeStamp = 0;

        public XElement toOLDataXML()
        {
            XElement buffXML = new XElement("buffInstance");

            //联机模式下，不需要保存Owner，因为buff都是role的附属品
            buffXML.SetAttributeValue("buffName", buff.Name);
            buffXML.SetAttributeValue("buffRound", buff.Round);
            buffXML.SetAttributeValue("buffLevel", buff.Level);
            buffXML.SetAttributeValue("buffProperty", buff.Property);
            buffXML.SetAttributeValue("buffInstanceLevel", this.Level);
            buffXML.SetAttributeValue("buffInstanceLeftRound", this.LeftRound);

            return buffXML;
        }

        public static List<BuffInstance> parseOLData(XElement buffsXML, Role owner)
        {
            List<BuffInstance> buffs = new List<BuffInstance>(); buffs.Clear();
            foreach (XElement buffXML in Tools.GetXmlElements(buffsXML, "buffInstance"))
            {
                BuffInstance instance = new BuffInstance();
                Buff buff = new Buff();

                buff.Name = Tools.GetXmlAttribute(buffXML, "buffName");
                buff.Round = Tools.GetXmlAttributeInt(buffXML, "buffRound");
                buff.Level = Tools.GetXmlAttributeInt(buffXML, "buffLevel");
                buff.Property = Tools.GetXmlAttributeInt(buffXML, "buffProperty");

                instance.buff = buff;
                instance.Level = Tools.GetXmlAttributeInt(buffXML, "buffInstanceLevel");
                instance.LeftRound = Tools.GetXmlAttributeInt(buffXML, "buffInstanceLeftRound");

                instance.Owner = owner;

                buffs.Add(instance);
            }
            return buffs;
        }

        public override string ToString()
        {
            //if (buff.Name == "醉酒" || buff.Name == "溜须拍马" || buff.Name == "易容" || buff.Name == "晕眩" || buff.Name == "诸般封印" || buff.Name == "剑封印" || buff.Name == "刀封印" || buff.Name == "拳掌封印" || buff.Name == "奇门封印")
            //    return string.Format("{0}{1} ", buff.Name, LeftRound);
            //else
            //    return string.Format("{0}{1} ", buff.Name, Level);
            return buff.Name + " ";
        }

        public string Info()
        {
            string info = "";
            
            if (buff.Level > 0)
                info += "程度:" + this.Level + "\n";
            else
                info += "程度:\n";

            info += "持续时间:" + (LeftRound).ToString() + "回合";

            return info;
        }

        public RoundBuffResult RoundEffect()
        {
            RoundBuffResult rst = new RoundBuffResult();
            rst.buff = this;
            switch (this.buff.Name)
            {
                case "中毒":
                    int hpDesc = (int)((35 * this.Level) * (1 - Owner.Attributes["dingli"] / 200) * Tools.GetRandom(0.5, 1));
                    if (hpDesc <= 0) hpDesc = 1;
                    if (Owner.Attributes["hp"] - hpDesc < 0)
                    {
                        hpDesc = Owner.Attributes["hp"] - 1;
                        Owner.Attributes["hp"] = 1;
                    }
                    rst.AddHp = -hpDesc;
                    break;
                case "恢复":
                    int hpAdd =  (int)(Owner.Attributes["gengu"] / 3 * (this.Level) * ( 1 + Tools.GetRandom(0,0.5)));
                    if(Owner.Attributes["hp"]+hpAdd>Owner.Attributes["maxhp"])
                    {
                        hpAdd = Owner.Attributes["maxhp"] - Owner.Attributes["hp"];
                    }
                    Owner.Attributes["hp"] += hpAdd;
                    rst.AddHp = hpAdd;
                    break;
                case "内伤":
                    int mpDesc = (int)((150-Owner.Attributes["dingli"])/4 * (this.Level) * ( 1 + Tools.GetRandom(0,0.5)));
                    if(Owner.Attributes["mp"] - mpDesc<0) mpDesc = Owner.Attributes["mp"];
                    Owner.Attributes["mp"] -= mpDesc;
                    rst.AddMp = -mpDesc;
                    break;
                case "集气":
                    if (Tools.ProbabilityTest(0.15 + 0.2 * this.Level))
                    {
                        Owner.Balls++;
                        rst.AddBall = 1;
                    }
                    break;
                default:
                    break;
            }
            return rst;
        }
    }

    public class RoundBuffResult
    {
        public int AddHp = 0;
        public int AddMp = 0;
        public int AddBall = 0;

        public BuffInstance buff = null;
    }
}
