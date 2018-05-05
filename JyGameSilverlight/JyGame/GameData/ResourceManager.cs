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
using System.Xml.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;

namespace JyGame.GameData
{
    public class ResourceManager
    {
        public static Dictionary<string, string> ResourceMap = new Dictionary<string, string>();
        static public void Init()
        {
            ResourceMap.Clear();
            foreach (var resourceXmlFile in GameProject.GetFiles("resources"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + resourceXmlFile);
                foreach (XElement t in xmlRoot.Elements())
                {
                    ResourceMap.Add(Tools.GetXmlAttribute(t, "key"), Tools.GetXmlAttribute(t, "value"));
                }
            }
        }

        static public string Get(string key)
        {
            if (ResourceMap.ContainsKey(key))
            {
                return ResourceMap[key];
            }
            else
            {
                if (Configer.Instance.Debug)
                {
                    MessageBox.Show("错误，调用了未定义的resource key: " + key);
                }
                return null;
            }
        }

        static public BitmapSource GetImage(string key)
        {
            if (key == null) return null;
            if (ResourceMap.ContainsKey(key))
            {
                return Tools.GetImage(ResourceMap[key]);
            }
            else
            {
                if (Configer.Instance.Debug)
                {
                    MessageBox.Show("错误，调用了未定义的图片资源：" + key);
                }
                return null;
            }
        }

        static public void Export(string dir)
        {
            XElement rootNode = new XElement("resources");

            foreach (var r in ResourceMap)
            {
                XElement node = new XElement("resource");
                node.SetAttributeValue("key", r.Key);
                node.SetAttributeValue("value", r.Value);
                rootNode.Add(node);
            }

            string file = dir + "/resource.xml";
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(rootNode.ToString());
            }
        }

        static public List<string> GetResourceKeyStartsWith(string startsWith)
        {
            List<string> rst = new List<string>();
            foreach(var key in ResourceMap.Keys)
            {
                if (key.StartsWith(startsWith))
                    rst.Add(key);
            }
            return rst;
        }
    }
}
