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
using JyGame.Logic;
using System.IO;

namespace JyGame.GameData
{
    public class MapTemplate
    {
        public string Key;
        public bool[,] mapCoverLayer = new bool[CommonSettings.MAXWIDTH, CommonSettings.MAXHEIGHT];
        public ImageSource Background
        {
            get { return ResourceManager.GetImage(BackgroundStr); }
        }
        public string BackgroundUrl
        {
            get { return ResourceManager.Get(BackgroundStr); }
        }
        public string BackgroundStr;
        public string Music { get { return ResourceManager.Get(_music); } }
        public string _music;
        public int backgroundWidth;
        public int backgroundHeight;
        public int actualXBlockNo;
        public int actualYBlockNo;
        public string Cover;
        public XElement xmlNode;

        public MapTemplate()
        {
            for (int i = 0; i < actualXBlockNo; ++i)
                for (int j = 0; j < actualYBlockNo; ++j)
                    mapCoverLayer[i, j] = false;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("maptemplate");
            rst.SetAttributeValue("key", Key);
            rst.SetAttributeValue("background", BackgroundStr);
            rst.SetAttributeValue("music", _music);
            rst.SetAttributeValue("width", backgroundWidth);
            rst.SetAttributeValue("height", backgroundHeight);
            rst.SetAttributeValue("xblocks", actualXBlockNo);
            rst.SetAttributeValue("yblocks", actualYBlockNo);
            rst.SetAttributeValue("cover", Cover);
            return rst;
        }

        public static MapTemplate Parse(XElement node)
        {
            MapTemplate template = new MapTemplate();
            template.xmlNode = node;
            template.Key = Tools.GetXmlAttribute(node, "key");
            template.BackgroundStr = Tools.GetXmlAttribute(node, "background");
            if (node.Attribute("music") != null && node.Attribute("music").Value != "")
                template._music = Tools.GetXmlAttribute(node, "music");
            if(node.Attribute("height") != null)
                template.backgroundHeight = Tools.GetXmlAttributeInt(node, "height");
            if(node.Attribute("width") != null)
                template.backgroundWidth = Tools.GetXmlAttributeInt(node, "width");
            if(node.Attribute("xblocks") != null)
                template.actualXBlockNo = Tools.GetXmlAttributeInt(node, "xblocks");
            if(node.Attribute("yblocks") != null)
                template.actualYBlockNo = Tools.GetXmlAttributeInt(node, "yblocks");
            if (node.Attribute("cover") != null)
            {
                template.Cover = Tools.GetXmlAttribute(node, "cover");
                bool isx = true;
                int x = 0;
                int y = 0;
                foreach (var p in template.Cover.Split(new char[] { ',' }))
                {
                    int pos = int.Parse(p);
                    if (isx)
                    {
                        x = pos;
                        isx = false;
                    }
                    else
                    {
                        y = pos;
                        isx = true;
                        template.mapCoverLayer[x, y] = true;
                    }
                }//foreach of coverlayer
            }
            return template;
        }
    }

    public class BattleRole
    {
        public string roleKey = null;
        public int index = -1;
        public int team;

        public int x;
        public int y;

        public bool faceRight;

        public XElement GenerateXml()
        {
            XElement rst = new XElement("role");
            if(index != - 1)
            {
                rst.SetAttributeValue("index", index);
            }
            else if(!string.IsNullOrEmpty(roleKey))
            {
                rst.SetAttributeValue("key", roleKey);
            }
            rst.SetAttributeValue("team", team);
            rst.SetAttributeValue("x", x);
            rst.SetAttributeValue("y", y);
            rst.SetAttributeValue("face", faceRight ? 1 : 0);
            return rst;
        }

        public static BattleRole Parse(XElement node)
        {
            BattleRole br = new BattleRole();
            if (node.Attribute("index") != null)
            {
                br.index = Tools.GetXmlAttributeInt(node, "index");
            }
            if(node.Attribute("key")!=null)
            {
                br.roleKey = Tools.GetXmlAttribute(node, "key");
            }
            if(node.Attribute("team")!=null)
                br.team = Tools.GetXmlAttributeInt(node, "team");
            if(node.Attribute("x") != null)
                br.x = Tools.GetXmlAttributeInt(node, "x");
            if(node.Attribute("y") != null)
                br.y = Tools.GetXmlAttributeInt(node, "y");
            if(node.Attribute("face") != null)
                br.faceRight = Tools.GetXmlAttributeInt(node, "face") == 0;

            return br;
        }
    }

    public class RandomRole
    {
        public int x;
        public int y;

        public bool faceRight;

        public static RandomRole Parse(XElement node)
        {
            RandomRole br = new RandomRole();
            br.x = Tools.GetXmlAttributeInt(node, "x");
            br.y = Tools.GetXmlAttributeInt(node, "y");
            if (node.Attribute("face") != null)
                br.faceRight = Tools.GetXmlAttributeInt(node, "face") == 0;

            return br;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("role");
            rst.SetAttributeValue("x", x);
            rst.SetAttributeValue("y", y);
            rst.SetAttributeValue("face", faceRight ? 1 : 0);
            return rst;
        }
    }


    public class Battle
    {
        public string Key;
        public MapTemplate Template
        {
            get
            {
                return BattleManager.GetMapTemplate(templateKey);
            }
        }
        public string templateKey;
        public int xoffset=0;
        public int yoffset=0;
        public List<BattleRole> battleRoles = new List<BattleRole>();
        public int randomRoleLevel = 0;
        public string randomRoleName = "杂鱼";
        public string randomRoleAnimation = "";
        public bool isRandomBoss = false;
        public List<RandomRole> randomRoles = new List<RandomRole>();
        public string arena = "no";
        public string must = "";
        public bool bonus = true;
        public List<Action> Actions = new List<Action>();
        public int maxRound = CommonSettings.DEFAULT_MAX_GAME_TURN;

        private string _music = null;
        public string Music
        {
            get
            {
                if (_music != null) return ResourceManager.Get(_music);
                else
                {
                    return BattleManager.GetMapTemplate(this.templateKey).Music;
                }
            }
        }

        public string[] GetBattleMusts()
        {
            if (must == null || must == string.Empty)
                return null;
            else
            {
                return must.Split(new char[] { '#' });
            }
        }

        public Battle Clone()
        {
            return Battle.Parse(this.xmlNode);
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("battle");
            rst.SetAttributeValue("key", Key);
            rst.SetAttributeValue("template", Template.Key);
            if (!string.IsNullOrEmpty(must))
                rst.SetAttributeValue("must", must);
            if(maxRound != CommonSettings.DEFAULT_MAX_GAME_TURN)
                rst.SetAttributeValue("maxround", maxRound);
            if (!string.IsNullOrEmpty(_music))
                rst.SetAttributeValue("music", _music);

            XElement battleRolesRootNode = new XElement("roles");
            rst.Add(battleRolesRootNode);
            foreach(var br in battleRoles)
            {
                battleRolesRootNode.Add(br.GenerateXml());
            }

            if(randomRoles.Count>0)
            {
                XElement randomRolesRootNode = new XElement("random");
                rst.Add(randomRolesRootNode);
                randomRolesRootNode.SetAttributeValue("level", randomRoleLevel);
                randomRolesRootNode.SetAttributeValue("name", randomRoleName);
                foreach(var r in randomRoles)
                {
                    randomRolesRootNode.Add(r.GenerateXml());
                }
            }

            if(Actions.Count>0)
            {
                XElement storyNode = new XElement("story");
                rst.Add(storyNode);
                foreach(var a in Actions)
                {
                    storyNode.Add(a.GenerateXml());
                }
            }
            return rst;
        }

        public XElement xmlNode = null;
        public static Battle Parse(XElement node)
        {
            Battle battle = new Battle();
            battle.xmlNode = node;
            battle.Key = Tools.GetXmlAttribute(node, "key");

            if (battle.Key.Contains(" "))
            {
                MessageBox.Show("错误，BATTLE KEY不能包含空格," + battle.Key);
            }
            battle.templateKey = Tools.GetXmlAttribute(node, "template");

            if (node.Attribute("xoffset") != null)
            {
                battle.xoffset = Tools.GetXmlAttributeInt(node, "xoffset");
            }
            if (node.Attribute("yoffset") != null)
            {
                battle.yoffset = Tools.GetXmlAttributeInt(node, "yoffset");
            }
            if (node.Attribute("must") != null)
            {
                battle.must = Tools.GetXmlAttribute(node, "must");
            }
            if (node.Attribute("bonus") != null)
            {
                battle.bonus = bool.Parse(Tools.GetXmlAttribute(node, "bonus"));
            }
            if (node.Attribute("music") != null)
            {
                battle._music = Tools.GetXmlAttribute(node, "music");
            }
            if (node.Element("roles") != null)
            {
                foreach (XElement role in node.Element("roles").Elements("role"))
                {
                    battle.battleRoles.Add(BattleRole.Parse(role));
                }
            }

            if (node.Element("random") != null)
            {
                battle.randomRoleLevel = Tools.GetXmlAttributeInt(node.Element("random"), "level");
                if (node.Element("random").Attribute("name") != null)
                    battle.randomRoleName = Tools.GetXmlAttribute(node.Element("random"), "name");

                if (node.Element("random").Attribute("animation") != null)
                {
                    battle.randomRoleAnimation = Tools.GetXmlAttribute(node.Element("random"), "animation");
                }

                if (node.Element("random").Attribute("boss") != null)
                {
                    battle.isRandomBoss = bool.Parse(Tools.GetXmlAttribute(node.Element("random"), "boss"));
                }
                
                foreach (XElement role in node.Element("random").Elements("role"))
                {
                    battle.randomRoles.Add(RandomRole.Parse(role));
                }
            }

            

            if (node.Attribute("arena") != null)
            {
                battle.arena = Tools.GetXmlAttribute(node, "arena");
            }

            if (node.Element("story") != null)
            {
                foreach (var an in node.Element("story").Elements("action"))
                {
                    battle.Actions.Add(Action.Parse(an));
                }
            }

            if (node.Element("maxround") != null)
            {
                battle.maxRound = Tools.GetXmlAttributeInt(node, "maxround");
            }

            return battle;
        }
    }

    public class BattleManager
    {
        static public List<MapTemplate> MapTemplates = new List<MapTemplate>();
        static public List<Battle> Battles = new List<Battle>();
        static public List<Battle> Arenas = new List<Battle>();
        static public List<Battle> Towers = new List<Battle>();

        static public void Init()
        {
            Battles.Clear();
            MapTemplates.Clear();
            Arenas.Clear();
            Towers.Clear();
            foreach (var mapXmlFile in GameProject.GetFiles("battle"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + mapXmlFile);
                foreach (XElement node in xmlRoot.Element("maptemplates").Elements("maptemplate"))
                {
                    MapTemplate template = MapTemplate.Parse(node);
                    MapTemplates.Add(template);
                }

                foreach (XElement node in xmlRoot.Element("battles").Elements("battle"))
                {
                    Battle battle = Battle.Parse(node);
                    Battles.Add(battle);

                    //如果是arena地图，则加入arena中
                    if (battle.arena == "yes")
                    {
                        Arenas.Add(battle);
                    }
                }
            }
        }

        static public void Export(string dir)
        {
            XElement rootNode = new XElement("root");
            XElement templateRootNode = new XElement("maptemplates");
            rootNode.Add(templateRootNode);

            foreach (var t in MapTemplates)
            {
                XElement node = t.GenerateXml();
                templateRootNode.Add(node);
            }

            XElement battleRootNode = new XElement("battles");
            rootNode.Add(battleRootNode);
            foreach(var b in Battles)
            {
                battleRootNode.Add(b.GenerateXml());
            }

            string file = dir + "/battles.xml";
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(rootNode.ToString());
            }
        }

        static public MapTemplate GetMapTemplate(string key)
        {
            foreach(var mt in MapTemplates)
            {
                if (mt.Key == key)
                    return mt;
            }
            return null;
        }

        static public Battle GetBattle(string key)
        {
            foreach (var bt in Battles)
            {
                if (bt.Key == key)
                    return bt;
            }
            return null;
        }

        static public List<Battle> getArenas()
        {
            return Arenas;
        }
    }

    public class TowerManager
    {
        static private Dictionary<string, List<Battle>> Towers = new Dictionary<string, List<Battle> >();
        static private Dictionary<string, List<EventCondition>> Conditions = new Dictionary<string, List<EventCondition>>();
        static private Dictionary<string, List<BonusItem>> BonusItems = new Dictionary<string, List<BonusItem>>();
        static private Dictionary<string, string> TowerDesc = new Dictionary<string, string>();
        static public void Init()
        {
            Towers.Clear(); 
            Conditions.Clear();
            BonusItems.Clear();
            TowerDesc.Clear();
            foreach (var mapXmlFile in GameProject.GetFiles("tower"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + mapXmlFile);
                foreach (XElement node in xmlRoot.Elements("tower"))
                {
                    string key = Tools.GetXmlAttribute(node, "key");
                    int number = Tools.GetXmlAttributeInt(node, "number");
                    if (node.Attribute("desc") != null)
                    {
                        TowerDesc[key] = Tools.GetXmlAttribute(node, "desc");
                    }
                    List<Battle> battles = new List<Battle>();
                    battles.Clear();
                    battles.Capacity = number;
                    foreach (XElement map in node.Element("maps").Elements("map"))
                    {
                        String battleKey = Tools.GetXmlAttribute(map, "key");
                        int index = Tools.GetXmlAttributeInt(map, "index");
                        battles.Add(BattleManager.GetBattle(battleKey));
                        
                        //处理奖励物品
                        List<BonusItem> items = new List<BonusItem>();
                        items.Clear();
                        foreach (XElement item in map.Elements("item"))
                        {
                            string itemKey = Tools.GetXmlAttribute(item, "key");
                            int itemNumber = 0;
                            double itemProb = 1.0;
                            if (item.Attribute("number") != null)
                                itemNumber = Tools.GetXmlAttributeInt(item, "number");
                            if (item.Attribute("probability") != null)
                                itemProb = (double)Tools.GetXmlAttributeFloat(item, "probability");

                            items.Add(new BonusItem(itemKey, itemNumber, itemProb));
                        }
                        BonusItems.Add(battleKey, items);
                    }
                    Towers.Add(key, battles);

                    //开启天关的条件
                    List<EventCondition> conditions = new List<EventCondition>();
                    conditions.Clear();
                    foreach (XElement condition in node.Element("conditions").Elements("condition"))
                    {
                        EventCondition cond = new EventCondition();
                        cond.type = Tools.GetXmlAttribute(condition, "type");
                        cond.value = Tools.GetXmlAttribute(condition, "value");
                        if (condition.Attribute("number") != null)
                            cond.number = Tools.GetXmlAttributeInt(condition, "number");
                        conditions.Add(cond);
                    }
                    Conditions.Add(key, conditions);
                }
            }
        }
        static public string getTowerDesc(string towerName)
        {
            if (TowerDesc.ContainsKey(towerName))
                return TowerDesc[towerName];
            else
                return "";
        }

        static public List<String> getTowers()
        {
            List<String> towers = new List<String>();
            towers.Clear();
            foreach (string key in Towers.Keys)
            {
                towers.Add(key);
            }
            return towers;
        }

        static public List<Battle> getTower(string towerKey)
        {
            if (Towers.ContainsKey(towerKey))
                return Towers[towerKey];
            else
            {
                MessageBox.Show("错误，调用了未定义的tower:" + towerKey);
                return null;
            }
        }

        static public List<EventCondition> getCondition(string towerKey)
        {
            if (Conditions.ContainsKey(towerKey))
                return Conditions[towerKey];
            else
                return null;
        }

        static public string GetRandomBonus(string mapKey)
        {
            if (BonusItems.ContainsKey(mapKey))
                return BonusItem.GetRandomBonus(BonusItems[mapKey]);
            else
                return "鸡腿";
        }
    }
}
