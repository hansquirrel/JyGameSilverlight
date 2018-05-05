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
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using JyGame.UserControls;
using System.Globalization;

namespace JyGame.GameData
{
    /// <summary>
    /// 游戏运行时数据
    /// </summary>
    public class RuntimeData
    {

        #region runtime data

        #region 团队
        /// <summary>
        /// 团队
        /// </summary>
        public List<Role> Team = new List<Role>();

        /// <summary>
        /// 角色是否在团队
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool InTeam(string roleKey)
        {
            foreach(var r in Team)
            {
                if (r.Key.Equals(roleKey)) return true;
            }
            return false;
        }

        public bool NameInTeam(string roleName)
        {
            foreach (var r in Team)
            {
                if (r.Name.Equals(roleName)) return true;
            }
            return false;
        }

        public Role GetTeamRole(string roleKey)
        {
            foreach(Role r in Team)
            {
                if (r.Key.Equals(roleKey)) return r;
            }
            return null;
        }
        #endregion

        #region 物品
        /// <summary>
        /// 物品
        /// </summary>
        public List<Item> Items = new List<Item>();

        public void RemoveItem(string name, int count)
        {
            List<Item> toremoveItems = new List<Item>();
            foreach (var item in Items)
            {
                if(item.Name.Equals(name) && toremoveItems.Count < count)
                {
                    toremoveItems.Add(item);
                }
                if (toremoveItems.Count >= count) break;
            }
            foreach (var item in toremoveItems)
            {
                Items.Remove(item);
            }
        }

        public Item GetItemByName(string name)
        {
            foreach (var item in Items)
            {
                if (item.Name.Equals(name))
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 整理物品
        /// </summary>
        public void ArrangeItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                for (int j = 0; j < Items.Count - 1; j++)
                {
                    if (ItemGreater(Items[j], Items[j + 1]))
                    {
                        Item tmp = null;
                        tmp = Items[j];
                        Items[j] = Items[j + 1];
                        Items[j + 1] = tmp;
                    }
                }
            }
        }

        private bool ItemGreater(Item item1, Item item2)
        {
            if (item1.Type > item2.Type)
                return true;
            else if (item1.Type == item2.Type && item1.Type != 4 && item1.price > item2.price)
                return true;
            else if (item1.Type == 4 && item2.Type == 4)
            {
                int type1 = 0, type2 = 0;
                if (item1.PicInfo == "物品.拳谱")
                    type1 = 0;
                if (item1.PicInfo == "物品.剑谱")
                    type1 = 1;
                if (item1.PicInfo == "物品.刀谱")
                    type1 = 2;
                if (item1.PicInfo == "物品.奇门谱")
                    type1 = 3;
                if (item2.PicInfo == "物品.拳谱")
                    type2 = 0;
                if (item2.PicInfo == "物品.剑谱")
                    type2 = 1;
                if (item2.PicInfo == "物品.刀谱")
                    type2 = 2;
                if (item2.PicInfo == "物品.奇门谱")
                    type2 = 3;

                if (type1 > type2)
                    return true;
                else if (type1 == type2 && item1.level > item2.level)
                    return true;
                else
                    return false;
            }
            else if (item1.Type == (int)ItemType.Canzhang && item2.Type == (int)ItemType.Canzhang &&
                Tools.StringHashtoInt(item1.Name) > Tools.StringHashtoInt(item2.Name))
            {
                return true;
            }
            else
                return false;
        }
        #endregion 

        /// <summary>
        /// 称号
        /// </summary>
        public List<string> Nicks
        {
            get
            {
                List<string> rst = new List<string>();
                if(IsolatedStorageSettings.ApplicationSettings.Contains("nick"))
                {
                    string nick = IsolatedStorageSettings.ApplicationSettings["nick"] as string;

                    foreach (var n in nick.Split(new char[] { '#' }))
                        rst.Add(n);
                }
                return rst;
            }
            set
            {
                string rst = "";
                foreach(var nick in value)
                {
                    rst += "#" + nick;
                }
                IsolatedStorageSettings.ApplicationSettings["nick"] = rst.TrimStart(new char[] { '#' });
            }
        }
        public string CurrentNick
        {
            get 
            {
                if (this.IsCheated) return "作弊佬";
                return _currentNick; 
            }
            set { _currentNick = value; }
        }

        private string _currentNick = "初出茅庐";

        #region 键值对存储器
        /// <summary>
        /// 键值对存储器
        /// </summary>
        public Dictionary<string, string> KeyValues = new Dictionary<string, string>();

        public string WudaoSay
        {
            get
            {
                if (!KeyValues.ContainsKey("wudaosay"))
                    KeyValues.Add("wudaosay", "这家伙很懒，什么都没说。");
                return KeyValues["wudaosay"];
            }
            set
            {
                if (!KeyValues.ContainsKey("wudaosay"))
                    KeyValues.Add("wudaosay", "这家伙很懒，什么都没说。");
                if (CommonSettings.IsBanWord(value))
                {
                    MessageBox.Show("你输入的宣言中含有非法字符");
                    KeyValues["wudaosay"] = "这家伙很懒，什么都没说。";
                }
                else
                {
                    KeyValues["wudaosay"] = value.ToString();
                }
            }
        }
        
        public int Round
        {
            get
            {
                if (!KeyValues.ContainsKey("round"))
                    KeyValues.Add("round", "1");
                return int.Parse(KeyValues["round"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("round"))
                    KeyValues.Add("round", "1");
                KeyValues["round"] = value.ToString();
            }
        }

        public List<Item> Xiangzi
        {
            get
            {
                List<Item> rst = new List<Item>();
                if (!KeyValues.ContainsKey("xiangzi"))
                    return rst;
                string content = KeyValues["xiangzi"];
                XElement xml = XElement.Parse(content);
                foreach (var item in xml.Elements("item"))
                {
                    string itemName = Tools.GetXmlAttribute(item, "name");

                    Item itemNew = ItemManager.GetItem(itemName);
                    if (item.Element("addition_triggers") != null)
                        itemNew.SetAdditionTriggers(item.Element("addition_triggers"));
                    rst.Add(itemNew);
                }
                return rst;
            }
            set
            {
                if (value == null) return;
                XElement root = new XElement("xiangzi");
                foreach (var item in value)
                {
                    root.Add(item.ToXml());
                }
                KeyValues["xiangzi"] = root.ToString();
            }
        }

        public int Haogan
        {
            get
            {
                if (!KeyValues.ContainsKey("haogan"))
                    KeyValues.Add("haogan", "50");
                return int.Parse(KeyValues["haogan"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("haogan"))
                    KeyValues.Add("haogan", "70");
                KeyValues["haogan"] = value.ToString();
            }
        }

        public string UUID
        {
            get
            {
                if (!KeyValues.ContainsKey("UUID"))
                    KeyValues.Add("UUID", System.Guid.NewGuid().ToString());
                return KeyValues["UUID"];
            }
            set
            {
                KeyValues["UUID"] = value;
            }
        }

        public int Rank
        {
            get
            {
                return _rank;
            }
            set
            {
                _rank = value;
                if (value == -1)
                {
                    gameEngine.uihost.mapUI.rankText.Text = string.Format("江湖排名: 未上榜", _rank);
                }
                else
                {
                    gameEngine.uihost.mapUI.rankText.Text = string.Format("江湖排名: {0}", _rank);
                    gameEngine.uihost.mapUI.rankText.Visibility = Visibility.Visible;
                }   
            }
        }
        private int _rank = -1;

        public int Daode
        {
            get
            {
                if (!KeyValues.ContainsKey("daode"))
                    KeyValues.Add("daode", "50");
                return int.Parse(KeyValues["daode"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("daode"))
                    KeyValues.Add("daode", "50");
                KeyValues["daode"] = value.ToString();
            }
        }

        public string femaleName
        {
            get
            {
                if (!KeyValues.ContainsKey("femaleName"))
                    KeyValues.Add("femaleName", "铃兰");
                return KeyValues["femaleName"];
            }
            set
            {
                if (!KeyValues.ContainsKey("femaleName"))
                    KeyValues.Add("femaleName", "铃兰");
                KeyValues["femaleName"] = value.ToString();
            }
        }

        public string maleName
        {
            get
            {
                if (!KeyValues.ContainsKey("maleName"))
                    KeyValues.Add("maleName", "小虾米");
                return KeyValues["maleName"];
            }
            set
            {
                if (!KeyValues.ContainsKey("maleName"))
                    KeyValues.Add("maleName", "小虾米");
                KeyValues["maleName"] = value.ToString();
            }
        }

        public int Money
        {
            get
            {
                if (!KeyValues.ContainsKey("money"))
                    KeyValues.Add("money", "0");
                return int.Parse(KeyValues["money"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("money"))
                    KeyValues.Add("money", "0");
                KeyValues["money"] = value.ToString();
            }
        }

        public int YuanBao
        {
            get
            {
                int count = 0;
                foreach(var item in Items)
                {
                    if (item.Name.Equals("元宝")) count++;
                }
                return count;
            }
            set
            {
                int currentYuanbao = this.YuanBao;
                if(value > currentYuanbao)
                {
                    for(int i=0;i<value - currentYuanbao;++i)
                    {
                        Items.Add(ItemManager.GetItem("元宝").Clone());
                    }
                }else if(value < currentYuanbao)
                {
                    this.RemoveItem("元宝", currentYuanbao - value);
                }
            }
        }

        public DateTime Date
        {
            get
            {
                if (!KeyValues.ContainsKey("date"))
                    KeyValues.Add("date", DateTime.ParseExact("0001-01-01 14:00:00","yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss"));
                try
                {
                    return DateTime.Parse(KeyValues["date"]);
                }
                catch
                {
                    return DateTime.ParseExact(KeyValues["date"], "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                }
            }
            set
            {
                if (!KeyValues.ContainsKey("date"))
                    KeyValues.Add("date", DateTime.ParseExact("0001-01-01 14:00:00","yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss"));
                KeyValues["date"] = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public string GameMode
        {
            get
            {
                if (!KeyValues.ContainsKey("mode"))
                    KeyValues.Add("mode", "normal");
                return KeyValues["mode"];
            }
            set
            {
                if (!KeyValues.ContainsKey("mode"))
                    KeyValues.Add("mode", "normal");
                KeyValues["mode"] = value;
            }
        }

        public string GameModeChinese
        {
            get
            {
                switch(GameMode)
                {
                    case "normal": return "简单";
                    case "hard": return "进阶";
                    case "crazy": return "炼狱";
                    default: return "未定义";
                }
            }
        }

        /// <summary>
        /// 友军伤害
        /// </summary>
        public bool FriendlyFire
        {
            get
            {
                if (!KeyValues.ContainsKey("friendlyfire"))
                    KeyValues.Add("friendlyfire", "false");
                return bool.Parse(KeyValues["friendlyfire"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("friendlyfire"))
                    KeyValues.Add("friendlyfire", "false");
                KeyValues["friendlyfire"] = value.ToString();
            }
        }

        public string Menpai
        {
            get
            {
                if (!KeyValues.ContainsKey("menpai"))
                    KeyValues.Add("menpai", "");
                return KeyValues["menpai"];
            }
            set
            {
                if (!KeyValues.ContainsKey("menpai"))
                    KeyValues.Add("menpai", "");
                KeyValues["menpai"] = value;
            }
        }

        public void AddLog(string info)
        {
            string date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";
            RuntimeData.Instance.Log += date + "，" + info + "\r\n";
        }
        public String Log
        {
            get
            {
                if (!KeyValues.ContainsKey("log"))
                    KeyValues.Add("log", "");
                return KeyValues["log"];
            }
            set
            {
                if (!KeyValues.ContainsKey("log"))
                    KeyValues.Add("log", "");
                KeyValues["log"] = value;
            }
        }

        public int DodgePoint
        {
            get
            {
                if (!KeyValues.ContainsKey("dodgePoint"))
                    KeyValues.Add("dodgePoint", "0");
                return int.Parse(KeyValues["dodgePoint"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("dodgePoint"))
                    KeyValues.Add("dodgePoint", "0");
                KeyValues["dodgePoint"] = value.ToString();
            }
        }

        public int biliPoint
        {
            get
            {
                if (!KeyValues.ContainsKey("biliPoint"))
                    KeyValues.Add("biliPoint", "0");
                return int.Parse(KeyValues["biliPoint"]);
            }
            set
            {
                if (!KeyValues.ContainsKey("biliPoint"))
                    KeyValues.Add("biliPoint", "0");
                KeyValues["biliPoint"] = value.ToString();
            }
        }

        public Boolean IsCheated
        {
            get;
            set;
        }

        public void SetLocation(string mapKey, string location)
        {
            string key = "location." + mapKey;
            if (!KeyValues.ContainsKey(key))
                KeyValues.Add(key, "");
            KeyValues[key] = location;
        }

        public string GetLocation(string mapKey)
        {
            string key = "location." + mapKey;
            if (!KeyValues.ContainsKey(key))
                KeyValues.Add(key, "");
            return KeyValues[key];
        }

        public string CurrentBigMap
        {
            get
            {
                if (!KeyValues.ContainsKey("currentBigMap"))
                    KeyValues.Add("currentBigMap", "");
                return KeyValues["currentBigMap"];
            }
            set
            {
                if (!KeyValues.ContainsKey("currentBigMap"))
                    KeyValues.Add("currentBigMap", "");
                KeyValues["currentBigMap"] = value;
            }
        }

        /// <summary>
        /// 通过试炼之地的队友名单，用#分割
        /// </summary>
        public string TrialRoles
        {
            get
            {
                if (!KeyValues.ContainsKey("trailRoles"))
                    KeyValues.Add("trailRoles", "");
                return KeyValues["trailRoles"];
            }
            set
            {
                if (!KeyValues.ContainsKey("trailRoles"))
                    KeyValues.Add("trailRoles", "");
                KeyValues["trailRoles"] = value;
            }
        }

        #endregion

        public GameEngine gameEngine = null;
        public BattleField battleField
        {
            get
            {
                if (gameEngine == null) return null;
                return gameEngine.uihost.battleFieldContainer.field;
            }
        }

        #endregion

        #region methods
        public void Init()
        {
            this.Clear();

            this.UUID = System.Guid.NewGuid().ToString();
            //用于测试的默认团队
            addTeamMember("主角");
            //for(int i=0;i<50;++i) addTeamMember("田伯光");
            //addTeamMember("段正淳");
            //addTeamMember("杨过");
            //addTeamMember("小龙女");
            //addTeamMember("乔峰");
            //addTeamMember("慕容复");
            //addTeamMember("慕容博");
            //addTeamMember("无崖子");
            //addTeamMember("逍遥子");
            
            Money = 100;
        }

        public void Clear()
        {
            this.Team.Clear();
            this.Items.Clear();
            this.KeyValues.Clear();
        }

        public void NextZhoumuClear()
        {
            int zm = this.Round;
            List<Item> xiangzi = this.Xiangzi;
            string trailRoles = this.TrialRoles;
            Clear();

            this.Round = zm;
            this.TrialRoles = trailRoles;
            this.Xiangzi = xiangzi;
            addTeamMember("主角");
            Money = 100;
        }

        public void addTeamMember(string roleKey)
        {
            Team.Add(RoleManager.GetRole(roleKey).Clone());
        }

        public void addTeamMember(string roleName, string changeName)
        {
            Role role = RoleManager.GetRole(roleName).Clone();
            role.Name = changeName;
            Team.Add(role);
        }

        public void removeTeamMember(string roleKey)
        {
            Role r = null;
            foreach (var role in Team)
            {
                if (role.Key.Equals(roleKey))
                {
                    r = role;
                    break;
                }
            }
            if (r != null)
            {
                Team.Remove(r);
            }
        }

        public void addNick(string nick)
        {
            List<string> nicks = this.Nicks;
            if (nicks.Contains(nick))
                return;
            nicks.Add(nick);
            this.Nicks = nicks;
        }

        public bool Save(string key) 
        {
            //if (SaveLoadManager.Instance.SaveCount >= CommonSettings.MAX_SAVE_COUNT && key != "自动存档")
            //{
            //    MessageBox.Show("存储错误，最多存储" + CommonSettings.MAX_SAVE_COUNT+ "个存档，请清除一些存档再试！");
            //    return false;
            //}
            //string fileKey = DEncryptHelper.Encrypt(key);
            XElement rootNode = new XElement("root");

            //团队
            XElement rolesNode = new XElement("roles");
            rootNode.Add(rolesNode);
            foreach (var role in this.Team)
            {
                rolesNode.Add(role.GenerateRoleXml());
            }

            //物品
            XElement itemsNode = new XElement("items");
            rootNode.Add(itemsNode);
            foreach (var item in this.Items)
            {
                XElement itemNode = new XElement("item");
                itemNode.SetAttributeValue("name", item.Name);
                if(item.AdditionTriggers != null && item.AdditionTriggers.Count>0)
                {
                    itemNode.Add(item.GenerateAdditionTriggersXml());
                }
                itemsNode.Add(itemNode);
            }

            //称号
            XElement nicksNode = new XElement("nicks");
            rootNode.Add(nicksNode);
            nicksNode.SetAttributeValue("current", CurrentNick);
            //foreach (var nick in this.Nicks)
            //{
            //    XElement nickNode = new XElement("nick");
            //    nickNode.SetAttributeValue("name", nick);
            //    nicksNode.Add(nickNode);
            //}

            //事件
            XElement eventsMarkNode = new XElement("keyvalues");
            rootNode.Add(eventsMarkNode);
            foreach (var ev in this.KeyValues)
            {
                eventsMarkNode.SetAttributeValue(ev.Key, ev.Value);
            }

            SaveLoadManager.Instance.Save(key, rootNode);

            //using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    using (StreamWriter sw = new StreamWriter(store.OpenFile(fileKey, FileMode.Create, FileAccess.Write)))
            //    {
            //        string enc = DEncryptHelper.Encrypt(rootNode.ToString());//加密
            //        sw.Write(enc);
            //    }
            //}

            //FileEncryption.EncryptFile(rootNode.ToString(), key, "qJzGEh6hESZDVJeCnFPGuxzaiB7NLQM3");
            return true;
        }
        public void Load(string key) 
        {
            //string content = null;
            //string fileKey = DEncryptHelper.Encrypt(key);
            //using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    using (StreamReader sr = new StreamReader(store.OpenFile(fileKey, FileMode.Open, FileAccess.Read)))
            //    {
            //        string tmp = sr.ReadToEnd();
            //        content = DEncryptHelper.Decrypt(tmp); //解密
            //    }
            //}
            ////content = FileEncryption.DecryptFile(key, "qJzGEh6hESZDVJeCnFPGuxzaiB7NLQM3");
            //XElement root = XElement.Parse(content);
            XElement root = SaveLoadManager.Instance.Load(key);

            List<Item> loadItems = new List<Item>();
            List<String> loadNicks = new List<String>();loadNicks.Clear();
            List<Role> loadTeam = new List<Role>();
            Dictionary<string, string> loadEventsMark = new Dictionary<string, string>();

            //item
            XElement itemsNode = Tools.GetXmlElement(root, "items");
            foreach(var itemNode in Tools.GetXmlElements(itemsNode,"item"))
            {
                Item item = ItemManager.GetItem(Tools.GetXmlAttribute(itemNode, "name"));
                if (itemNode.Element("addition_triggers")!=null)
                {
                    item.SetAdditionTriggers(itemNode.Element("addition_triggers"));
                }
                loadItems.Add(item);
            }
            //称号
            if(root.Element("nicks") != null)
            {
                XElement nicksNode = Tools.GetXmlElement(root, "nicks");
                if (nicksNode.Attribute("current") != null)
                    CurrentNick = Tools.GetXmlAttribute(nicksNode, "current");
                foreach (var nickNode in Tools.GetXmlElements(nicksNode, "nick"))
                {
                    String nick = Tools.GetXmlAttribute(nickNode, "name");
                    loadNicks.Add(nick);
                }
            }

            //eventmask
            XElement eventMaskNode = Tools.GetXmlElement(root, "keyvalues");
            foreach (var attr in eventMaskNode.Attributes())
            {
                loadEventsMark[attr.Name.ToString()] = attr.Value;
            }
            //roles
            XElement rolesNode = Tools.GetXmlElement(root, "roles");
            foreach(var roleNode in Tools.GetXmlElements( rolesNode,"role"))
            {
                loadTeam.Add(Role.Parse(roleNode));
            }

            this.Clear();
            this.Items = loadItems;
            //this.Nicks = loadNicks;
            this.Team = loadTeam;
            this.KeyValues = loadEventsMark;
            //MessageBox.Show("载入成功!");
            if (KeyValues.ContainsKey("UUID"))
                this.UUID = KeyValues["UUID"];
            else
                this.UUID = System.Guid.NewGuid().ToString();
            this.gameEngine.LoadGame();
        }

        public List<SaveInfo> GetSaveList()
        {
            //using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    string[] fileNames = store.GetFileNames();
            //    List<string> rst = new List<string>();
            //    foreach (var f in fileNames)
            //    {
            //        try
            //        {
            //            rst.Add(DEncryptHelper.Decrypt(f));
            //        }
            //        catch
            //        {
            //        }
            //    }

            //    return rst.ToArray();
            //}
            return SaveLoadManager.Instance.GetList();
        }

        public void DeleteSave(string key)
        {
            //string fileKey = DEncryptHelper.Encrypt(key);
            //using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    store.DeleteFile(fileKey);
            //}
            SaveLoadManager.Instance.DeleteSave(key);
        }
        #endregion

        #region singleton
        static public RuntimeData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RuntimeData();
                return _instance;
            }
        }

        static private RuntimeData _instance = null;
        #endregion

        #region 作弊检测
        public void CheckCheat()
        {
            if (this.IsCheated) 
                return;

            int cheatAttr = 500;
            string[] checkAttrlist = new string[]{
                "gengu",
                "bili",
                "fuyuan",
                "shenfa",
                "dingli",
                "wuxing",
                "quanzhang",
                "jianfa",
                "daofa",
                "qimen",
            };

            //目前是很简单的检测属性和武功等级
            foreach (var r in this.Team)
            {
                if(r.GetTotalWuxueCost() > r.Attributes["wuxue"] + 400)
                {
                    this.IsCheated = true;
                    return;
                }
                if(r.Skills.Count > 20)
                {
                    this.IsCheated = true;
                    return;
                }
                if(r.Attributes["maxhp"] > 35000 || r.Attributes["maxmp"] > 35000)
                {
                    this.IsCheated = true;
                    return;
                }

                if(r.InternalSkills.Count > 20)
                {
                    this.IsCheated = true;
                    return;
                }

                foreach(var c in checkAttrlist)
                {
                    if (r.Attributes[c] > cheatAttr)
                    {
                        this.IsCheated = true;
                        return;
                    }
                }

                foreach(var s in r.Skills)
                {
                    if(s.Level > CommonSettings.MAX_SKILL_LEVEL)
                    {
                        this.IsCheated = true;
                        return;
                    }
                }

                foreach (var s in r.InternalSkills)
                {
                    if(s.Level > CommonSettings.MAX_INTERNALSKILL_LEVEL)
                    {
                        this.IsCheated = true;
                        return;
                    }
                }
            }
        }
        #endregion 
        #region 时间标志检测

        public const string TIMEKEY_PREF = "TIMEKEY_";
        public void AddTimeKey(string key, int days)
        {
            string timeKey = TIMEKEY_PREF + key;
            RuntimeData.Instance.KeyValues[timeKey] = string.Format("{0}#{1}", RuntimeData.Instance.Date, days);
        }

        public void RemoveTimeKey(string key)
        {
            string timeKey = TIMEKEY_PREF + key;
            if (RuntimeData.Instance.KeyValues.ContainsKey(timeKey))
            {
                RuntimeData.Instance.KeyValues.Remove(timeKey);
            }
        }

        /// <summary>
        /// 检测重置的时间标志
        /// </summary>
        public void CheckTimeFlags()
        {
            List<string> tobeRemoved = new List<string>();
            foreach (var key in KeyValues.Keys)
            {
                if (key.StartsWith(TIMEKEY_PREF))
                {
                    string value = KeyValues[key];
                    DateTime t = DateTime.MinValue;
                    try
                    {
                        t = DateTime.Parse(value.Split(new char[] { '#' })[0]);
                    }
                    catch
                    {
                        t = DateTime.ParseExact(value.Split(new char[] { '#' })[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                    }
                    int days = int.Parse(value.Split(new char[] { '#' })[1]);
                    if ((this.Date - t).TotalDays > days)
                    {
                        tobeRemoved.Add(key);
                    }
                }
            }

            foreach (var key in tobeRemoved)
            {
                KeyValues.Remove(key);
            }
        }
        #endregion
    }


}
