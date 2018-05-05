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
    public class Location
    {
        public string name{get;set;}
        public string description{get;set;}
        public int x;
        public int y;

        public List<Event> events = new List<Event>();
        public XElement GenerateXml()
        {
            XElement rst = new XElement("location");
            rst.SetAttributeValue("name", name);
            rst.SetAttributeValue("x", x);
            rst.SetAttributeValue("y", y);
            rst.SetAttributeValue("description", description);
            if(events.Count>0)
            {
                foreach(var evt in events)
                {
                    rst.Add(evt.GenerateXml());
                }
            }
            return rst;
        }


    }

    public class MapRole
    {
        public string roleKey { get; set; }
        public string pic { get; set; }
        public string description { get; set; }
        public bool hide = true;

        public List<Event> events = new List<Event>();

        public XElement GenerateXml()
        {
            XElement rst = new XElement("maprole");
            rst.SetAttributeValue("roleKey", roleKey);
            rst.SetAttributeValue("pic", pic);
            if (!string.IsNullOrEmpty(description))
            {
                rst.SetAttributeValue("description", description);
            }
            if(!hide)
            {
                rst.SetAttributeValue("hide", "0");
            }
            if(events.Count> 0)
            {
                foreach (var evt in events)
                {
                    rst.Add(evt.GenerateXml());
                }
            }
            return rst;
        }
    }
    public enum EventRepeatType
    {
        Once,
        Unlimited,
    }
    public class Event
    {
        public string image { get; set; }
        public int lv = 0;
        public string description { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public EventRepeatType RepeatType { get; set; }
        public int probability = 100;
        public List<EventCondition> conditions = new List<EventCondition>();

        public XElement GenerateXml()
        {
            XElement rst = new XElement("event");
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("value", Value);
            
            if (!string.IsNullOrEmpty(image))
            {
                rst.SetAttributeValue("image", image);
            }
            if(!string.IsNullOrEmpty(description))
            {
                rst.SetAttributeValue("description", description);
            }
            if (probability != 100)
            {
                rst.SetAttributeValue("probability", probability);
            }
            if (lv != 0)
            {
                rst.SetAttributeValue("lv", lv);
            }
            if(RepeatType == EventRepeatType.Once)
            {
                rst.SetAttributeValue("repeat", "once");
            }
            if (conditions.Count > 0)
            {
                foreach (var c in conditions)
                {
                    rst.Add(c.GenerateXml());
                }
            }
            return rst;
        }

        static public Event Parse(XElement node)
        {
            Event aEvent = new Event();
            //推荐进入等级
            if (node.Attribute("lv") != null)
            {
                aEvent.lv = Tools.GetXmlAttributeInt(node, "lv");
            }
            if (node.Attribute("image") != null)
            {
                aEvent.image = Tools.GetXmlAttribute(node, "image");
            }
            else
            {
                aEvent.image = "";
            }
            if (node.Attribute("description") != null)
            {
                aEvent.description = Tools.GetXmlAttribute(node, "description");
            }
            
            aEvent.Value = Tools.GetXmlAttribute(node, "value");
            aEvent.Type = Tools.GetXmlAttribute(node, "type");
            if (node.Attribute("probability") == null)
            {
                aEvent.probability = 100;
            }
            else
            {
                aEvent.probability = Tools.GetXmlAttributeInt(node, "probability");
            }

            if (node.Attribute("repeat") != null && node.Attribute("repeat").Value == "once")
            {
                aEvent.RepeatType = EventRepeatType.Once;
            }
            else
            {
                aEvent.RepeatType = EventRepeatType.Unlimited;
            }
            aEvent.conditions = new List<EventCondition>();
            foreach (XElement eventCondition in node.Elements("condition"))
            {
                EventCondition eventCont = new EventCondition();
                eventCont.type = Tools.GetXmlAttribute(eventCondition, "type");
                eventCont.value = Tools.GetXmlAttribute(eventCondition, "value");
                if (eventCondition.Attribute("number") != null)
                    eventCont.number = Tools.GetXmlAttributeInt(eventCondition, "number");
                aEvent.conditions.Add(eventCont);
            }
            return aEvent;
        }
    };

    public class BigMap
    {
        public string Name;
        public string desc = "";
        public string Pic = "";
        public ImageSource Background
        {
            get
            {
                return Tools.GetImage(ResourceManager.Get(Pic));
            }
        }
        public string BackgroundUrl
        {
            get
            {
                return ResourceManager.Get(Pic);
            }
        }
        public List<Location> locationList = new List<Location>();
        public List<MapRole> roleList = new List<MapRole>();
        //public Dictionary<string, List<Event>> eventsList = new Dictionary<string, List<Event>>();
        public List<string> Musics = new List<string>();
        public XElement xmlNode = null;

        public List<Event> getEvents(string locationName)
        {
            foreach(var l in locationList)
            {
                if(l.name == locationName)
                {
                    return l.events;
                }
            }
            foreach(var r in roleList)
            {
                if(r.roleKey == locationName)
                {
                    return r.events;
                }
            }
            MessageBox.Show("错误，调用events错误,location=" + locationName);
            return null;
        }

        public List<Location> getLocations()
        {
            return locationList;
        }

        /// <summary>
        /// 有地点（是大地图）
        /// </summary>
        public bool HasLocation
        {
            get
            {
                return locationList.Count > 0;
            }
        }

        public List<MapRole> getMapRoles()
        {
            return roleList;
        }

        public string GetRandomMusic()
        {
            int k = (int)Tools.GetRandom(0, Musics.Count);
            if (k >= Musics.Count)
                k = 0;
            return Musics[k];
        }

        public XElement GenerateXml()
        {
            XElement mapNode = new XElement("map");

            mapNode.SetAttributeValue("name", this.Name);
            mapNode.SetAttributeValue("pic", this.Pic);
            if(Musics.Count>0)
            {
                XElement musicsNode = new XElement("musics");
                mapNode.Add(musicsNode);
                foreach(var m in Musics)
                {
                    XElement musicNode = new XElement("music");
                    musicNode.SetAttributeValue("name", m);
                    musicsNode.Add(musicNode);
                }
            }
            if (locationList.Count > 0)
            {
                foreach (var l in locationList)
                {
                    XElement locationNode = l.GenerateXml();
                    mapNode.Add(locationNode);
                }
            }
            if(roleList.Count > 0)
            {
                foreach (var r in roleList)
                {
                    mapNode.Add(r.GenerateXml());
                }
            }

            return mapNode;
        }
    }

    public class EventCondition
    {
        public string type  { get; set; }
        public string value { get; set; }
        public int number = -1;

        public override string ToString()
        {
            string rst = string.Format("{0}/{1}", type, value);
            if (number != -1) { rst += ",数量:" + number.ToString(); }
            return rst;
        }

        public static EventCondition Parse(XElement node)
        {
            EventCondition rst = new EventCondition();
            rst.type = Tools.GetXmlAttribute(node, "type");
            rst.value = Tools.GetXmlAttribute(node, "value");
            if (node.Attribute("number") != null)
            {
                rst.number = Tools.GetXmlAttributeInt(node, "number");
            }
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("condition");
            rst.SetAttributeValue("type", type);
            rst.SetAttributeValue("value", value);
            if(number != -1)
            {
                rst.SetAttributeValue("number", number);
            }
            return rst;
        }
    };

    public class MapEventsManager
    {
        static public List<BigMap> bigMaps = new List<BigMap>();

        static public void Export(string dir)
        {
            string fileFullName = dir + "/maps.xml";
            XElement root = new XElement("root");
            XElement mapsNode = new XElement("maps");
            root.Add(mapsNode);
            foreach (var map in bigMaps)
            {
                mapsNode.Add(map.GenerateXml());
            }
            using (StreamWriter sw = new StreamWriter(fileFullName))
            {
                sw.Write(root.ToString());
            }
        }

        static public void Init()
        {
            bigMaps.Clear();
            foreach (var eventXmlFile in GameProject.GetFiles("map"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + eventXmlFile);
                XElement mapsRoot = Tools.GetXmlElement(xmlRoot, "maps");
                if (mapsRoot == null || mapsRoot.Element("map") == null) continue;
                foreach (var map in Tools.GetXmlElements(mapsRoot, "map"))
                {
                    BigMap bigMap = new BigMap();
                    
                    bigMap.Name = Tools.GetXmlAttribute(map, "name");
                    //只抓取大地图的位置信息，其他的抓取人物信息
                    if (bigMap.Name != "大地图")
                        continue;
                    bigMap.xmlNode = map;
                    bigMap.Pic = Tools.GetXmlAttribute(map, "pic");
                    foreach (var music in map.Element("musics").Elements("music"))
                    {
                        bigMap.Musics.Add(Tools.GetXmlAttribute(music, "name"));
                    }
                    foreach (XElement t in map.Elements("location"))
                    {
                        string name = Tools.GetXmlAttribute(t, "name");
                        Location location = new Location();
                        location.name = name;
                        location.description = Tools.GetXmlAttribute(t, "description");
                        location.x = Tools.GetXmlAttributeInt(t, "x");
                        location.y = Tools.GetXmlAttributeInt(t, "y");
                        bigMap.locationList.Add(location);

                        foreach (XElement eventX in t.Elements("event"))
                        {
                            location.events.Add(Event.Parse(eventX));
                        }
                    }
                    bigMaps.Add(bigMap);
                }

                foreach (var map in Tools.GetXmlElements(mapsRoot, "map"))
                {
                    BigMap bigMap = new BigMap();
                    bigMap.xmlNode = map;
                    bigMap.Name = Tools.GetXmlAttribute(map, "name");
                    if (map.Attribute("desc") != null)
                        bigMap.desc = Tools.GetXmlAttribute(map, "desc");

                    //抓取人物信息
                    if (bigMap.Name == "大地图")
                        continue;
                    bigMap.Pic = Tools.GetXmlAttribute(map, "pic");
                    if (map.Element("musics") != null)
                    {
                        foreach (var music in map.Element("musics").Elements("music"))
                        {
                            bigMap.Musics.Add(Tools.GetXmlAttribute(music, "name"));
                        }
                    }
                    foreach (XElement t in map.Elements("maprole"))
                    {
                        string roleKey = Tools.GetXmlAttribute(t, "roleKey");
                        MapRole mapRole = new MapRole();
                        mapRole.roleKey = roleKey;
                        if (t.Attribute("pic") != null)
                        {
                            mapRole.pic = Tools.GetXmlAttribute(t, "pic");
                        }
                        if (t.Attribute("description") != null)
                        {
                            mapRole.description = Tools.GetXmlAttribute(t, "description");
                        }
                        if (t.Attribute("hide") != null)
                        {
                            if (Tools.GetXmlAttributeInt(t, "hide") == 0)
                            {
                                mapRole.hide = false;
                            }
                        }
                        bigMap.roleList.Add(mapRole);
                        foreach (XElement eventX in t.Elements("event"))
                        {
                            mapRole.events.Add(Event.Parse(eventX));
                        }
                    }
                    bigMaps.Add(bigMap);
                }
            }

            if(Configer.Instance.Debug)
            {
                App.DebugPanel.InitMaps(bigMaps);
            }
        }

        static public BigMap GetBigMap(string name)
        {
            foreach (var t in bigMaps)
            {
                if (t.Name.Equals(name)) return t;
            }
            MessageBox.Show("错误，调用了未定义的地图:" + name);
            return null;
        }
    }
}
