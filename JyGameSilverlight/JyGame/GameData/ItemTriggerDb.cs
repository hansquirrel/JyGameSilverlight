using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace JyGame.GameData
{
    public class ITTriggerDb
    {
        public static ITTriggerDb Parse(XElement xml)
        {
            ITTriggerDb rst = new ITTriggerDb();
            if (xml.Attribute("name") != null)
            {
                rst.Name = Tools.GetXmlAttribute(xml, "name");
            }
            if (xml.Attribute("minlevel") != null)
            {
                rst.MinLevel = Tools.GetXmlAttributeInt(xml, "minlevel");
            }
            if (xml.Attribute("maxlevel") != null)
            {
                rst.MaxLevel = Tools.GetXmlAttributeInt(xml, "maxlevel");
            }
            if (xml.Elements("trigger") != null)
            {
                foreach(var node in xml.Elements("trigger"))
                {
                    rst.Triggers.Add(ITTrigger.Parse(node));
                }
            }

            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("item_trigger");
            rst.SetAttributeValue("minlevel", MinLevel);
            rst.SetAttributeValue("maxlevel", MaxLevel);
            foreach(var t in Triggers)
            {
                rst.Add(t.GenerateXml());
            }
            return rst;
        }

        public int MinLevel = -1;
        public int MaxLevel = -1;
        public string Name = "";
        public List<ITTrigger> Triggers = new List<ITTrigger>();
    }

    public class ITTrigger
    {
        public string Name = "";
        public int Weight = 100;
        public List<ITParam> Params = new List<ITParam>();

        public bool HasPool
        {
            get
            {
                foreach(var p in Params)
                {
                    if (p.Pool != string.Empty) return true;
                }
                return false;
            }
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("trigger");
            rst.SetAttributeValue("name", Name);
            rst.SetAttributeValue("w", Weight);
            foreach (var p in Params)
            {
                rst.Add(p.GenerateXml());
            }
            return rst;
        }

        public static ITTrigger Parse(XElement xml)
        {
            ITTrigger rst = new ITTrigger();
            rst.Name = Tools.GetXmlAttribute(xml, "name");
            if (xml.Elements("param") != null)
            {
                foreach (var node in xml.Elements("param"))
                {
                    rst.Params.Add(ITParam.Parse(node));
                }
            }
            if (xml.Attribute("w") != null)
            {
                rst.Weight = Tools.GetXmlAttributeInt(xml, "w");
            }
            return rst;
        }

        public ItemTrigger GenerateItemTrigger()
        {
            ItemTrigger rst = new ItemTrigger();
            rst.Name = this.Name;
            for (int i = 0; i < this.Params.Count; ++i) //赋值
            {
                ITParam param = this.Params[i];
                if (param.Min != -1)
                {
                    rst.Argvs.Add((Tools.GetRandomInt(param.Min, param.Max)).ToString());
                }
                if (param.Pool != string.Empty) //多选一
                {
                    string poolSelect = param.PoolList[Tools.GetRandomInt(0, param.PoolList.Length - 1)];
                    rst.Argvs.Add(poolSelect);
                }
            }
            return rst;
        }
    }

    public class ITParam
    {
        public int Min = -1;
        public int Max = -1;
        public string Pool = "";
        public string[] PoolList
        {
            get
            {
                return Pool.Split(new char[] { ',' });
            }
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("param");
            rst.SetAttributeValue("pool", Pool);
            rst.SetAttributeValue("min", Min);
            rst.SetAttributeValue("max", Max);
            return rst;
        }

        public static ITParam Parse(XElement xml)
        {
            ITParam rst = new ITParam();
            if(xml.Attribute("min") != null)
            {
                rst.Min = Tools.GetXmlAttributeInt(xml, "min");
            }
            if (xml.Attribute("max") != null)
            {
                rst.Max = Tools.GetXmlAttributeInt(xml, "max");
            }
            if (xml.Attribute("pool") != null)
            {
                rst.Pool = Tools.GetXmlAttribute(xml, "pool");
            }
            return rst;
        }
    }
    
}
