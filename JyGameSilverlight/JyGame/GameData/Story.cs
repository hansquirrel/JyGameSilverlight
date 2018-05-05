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
using System.IO;

namespace JyGame.GameData
{
    public class Action
    {
        public static List<string> TypeList
        {
            get { return _typeList; }
            private set { _typeList = value; }
        }
        private static List<string> _typeList = new List<string>();

        public string Type;
        public string Value;

        public string[] Params
        {
            get
            {
                return Value.Split(new char[] { '#' });
            }
        }

        public static Action Parse(XElement node)
        {
            Action rst = new Action();
            rst.Type = Tools.GetXmlAttribute(node, "type");
            if (!TypeList.Contains(rst.Type))
            {
                TypeList.Add(rst.Type);
            }
            rst.Value = Tools.GetXmlAttribute(node, "value");
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("action");
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("value", Value);
            return rst;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Type, Value);
        }
    }

    public class ScriptControl
    {
        public string Type;
        public string Value;
        public List<EventCondition> Conditions = new List<EventCondition>();

        public override string ToString()
        {
            string cstr = "";
            foreach(var c in Conditions)
            {
                cstr += c.ToString() + ",";
            }

            return string.Format("{0}=>{1}/{2}", cstr.TrimEnd(new char[]{','}), Type, Value);
        }

        public static ScriptControl Parse(XElement node)
        {
            ScriptControl rst = new ScriptControl();
            rst.Type = Tools.GetXmlAttribute(node, "type");
            rst.Value = Tools.GetXmlAttribute(node, "value");
            if (node.Elements("condition") != null)
            {
                foreach (var conditionNode in node.Elements("condition"))
                {
                    rst.Conditions.Add(EventCondition.Parse(conditionNode));
                }
            }
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("control");
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("value", Value);
            foreach(var c in Conditions)
            {
                rst.Add(c.GenerateXml());
            }
            return rst;
        }
    }

    public class Result
    {
        public string Ret = "";
        public string Type = "";
        public string Value = "";

        public List<ScriptControl> Controls = new List<ScriptControl>();

        public override string ToString()
        {
            if(Controls.Count>0)
            {
                string controlStr= "";
                foreach(var c in Controls)
                {
                    controlStr += "\n\t" + c.ToString();
                }
                return string.Format("{0}=>条件分枝 {1}", Ret, controlStr);
            }
            return string.Format("{0}=>{1}/{2}", Ret, Type, Value);
        }

        public static Result Parse(XElement node)
        {
            Result rst = new Result();
            rst.Ret = Tools.GetXmlAttribute(node, "ret");
            if (node.Attribute("type") != null)
            {
                rst.Type = Tools.GetXmlAttribute(node, "type");
            }
            if (node.Attribute("value") != null)
            {
                rst.Value = Tools.GetXmlAttribute(node, "value");
            }
            if (node.Elements("control") != null)
            {
                foreach (var ctrl in node.Elements("control"))
                {
                    rst.Controls.Add(ScriptControl.Parse(ctrl));
                }
            }
            return rst;
        }

        public XElement GenerateXml()
        {
            XElement rst = new XElement("result");
            rst.SetAttributeValue("ret", Ret);
            rst.SetAttributeValue("type", Type);
            rst.SetAttributeValue("value", Value);
            foreach(var ctrl in Controls)
            {
                rst.Add(ctrl.GenerateXml());
            }
            return rst;
        }
    }

    public class Story
    {
        public string Name;
        public List<Action> Actions = new List<Action>();
        public List<Result> Results = new List<Result>();
        public XElement XmlNode = null;

        public XElement GenerateXml()
        {
            XElement rst = new XElement("story");
            rst.SetAttributeValue("name", Name);
            foreach(var a in Actions)
            {
                rst.Add(a.GenerateXml());
            }
            foreach (var r in Results)
            {
                rst.Add(r.GenerateXml());
            }

            return rst;
        }

        public XElement GenerateResultsXml()
        {
            XElement rst = new XElement("results");
            foreach (var r in Results)
            {
                rst.Add(r.GenerateXml());
            }
            return rst;
        }
    }

    public class StoryManager
    {
        static public List<Story> storys = null;
        static private UIHost uiHost { get { return RuntimeData.Instance.gameEngine.uihost; } }
        public static void Init()
        {
            storys = new List<Story>();
            storys.Clear();
            foreach (string dialogXmlFile in GameProject.GetFiles("story"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + dialogXmlFile);
                foreach(var storyNode in xmlRoot.Elements("story"))
                {
                    Story story = new Story();
                    story.XmlNode = storyNode;
                    story.Name = Tools.GetXmlAttribute(storyNode, "name");
                    //actions
                    foreach (var actionNode in storyNode.Elements("action"))
                    {
                        story.Actions.Add(Action.Parse(actionNode));
                    }
                    //results
                    foreach(var resultNode in storyNode.Elements("result"))
                    {
                        story.Results.Add(Result.Parse(resultNode));
                    }
                    storys.Add(story);
                }
            }

            if(Configer.Instance.Debug)
            {
                App.DebugPanel.InitStorys(storys);
            }
        }

        public static void Export(string dir)
        {
            string file = dir + "/storys.xml";

            XElement rootNode = new XElement("root");
            foreach(var story in storys)
            {
                rootNode.Add(story.GenerateXml());
            }
            using(StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(rootNode.ToString());
            }
        }

        public static Story GetStory(string name)
        {
            foreach(var story in storys)
            {
                if (story.Name == name)
                    return story;
            }
            if (Configer.Instance.Debug)
            {
                MessageBox.Show("错误，调用了未定义的story:" + name);
            }
            return null;
        }

        private static List<string> GetRoles(Story story)
        {
            List<string> rst = new List<string>();
            foreach (var action in story.Actions)
            {
                if (action.Type == "DIALOG")
                {
                    string roleName = action.Value.Split(new char[] { '#' })[0];
                    if (!rst.Contains(roleName))
                        rst.Add(roleName);
                }
            }
            return rst;
        }

        private static int currentActionIndex = -1;
        private static Story currentStory = null;
        private static string storyResult = "0";
        public static void PlayStory(string name, bool lastScenceIsMap)
        {
            Story story = GetStory(name);
            if (story == null)
            {
                MessageBox.Show("错误，执行了未定义的story:" + name);
                return;
            }
            if(Configer.Instance.Debug)
            {
                App.DebugPanel.PlayStory(story);
            }
            PlayStory(story, lastScenceIsMap);
        }

        public static void PlayStory(Story story, bool lastScenceIsMap)
        {
            uiHost.mapUI.resetHead();
            storyResult = "0";
            currentStory = story;
            if (story == null)
            {
                MessageBox.Show("错误，执行了空story");
                return;
            }
            //如果上一个scene是map，则默认设置成map的背景
            if (lastScenceIsMap)
            {
                uiHost.scence.SetBackground(uiHost.mapUI.currentMap.Background);
            }
            else
            {
                uiHost.scence.Visibility = Visibility.Visible;
            }
            //设置对话角色
            List<string> roles = GetRoles(story);
            uiHost.scence.SetRoles(roles);
            currentActionIndex = -1;
            uiHost.mapUI.IsEnabled = false;
            NextAction();
        }

        public static void NextAction()
        {
            currentActionIndex++;
            Story story = currentStory;
            if (currentActionIndex >= currentStory.Actions.Count)
            {
                StoryResult();
            }
            else
            {
                ExecuteAction(story.Actions[currentActionIndex]);
            }
        }

        public static void ExecuteAction(Action action, bool isSingleAction = false)
        {
            string[] paras = SpliteValue(action.Value);
            switch (action.Type)
            {
                case "BACKGROUND":
                    {
                        uiHost.scence.SetBackground(action.Value);
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "MUSIC":
                    {
                        AudioManager.PlayMusic(ResourceManager.Get(paras[0]));
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "DIALOG":
                    {
                        string roleName = paras[0];
                        string info = paras[1];
                        info = info.Replace("$FEMALE$", RuntimeData.Instance.femaleName);
                        info = info.Replace("$MALE$", RuntimeData.Instance.maleName);
                        ShowDialog(roleName, info, isSingleAction);
                        break;
                    }
                case "JOIN":
                    {
                        string role = action.Value;

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        RuntimeData.Instance.addTeamMember(role, name);
                        string date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";

                        RuntimeData.Instance.Log += date + "，" + "【" + name + "】加入队伍。" + "\r\n";
                        ShowDialog(role, "【" + name + "】加入队伍。", isSingleAction);
                        break;
                    }
                case "LEAVE":
                    {
                        string role = action.Value;

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        RuntimeData.Instance.removeTeamMember(role);
                        string date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";

                        RuntimeData.Instance.Log += date + "，" + "【" + name + "】离开。" + "\r\n";
                        ShowDialog(role, "【" + name + "】离开队伍。", isSingleAction);
                        break;
                    }
                case "HAOGAN":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Haogan = RuntimeData.Instance.Haogan + number;
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "DAODE":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Daode = RuntimeData.Instance.Daode + number;
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "ITEM":
                    {
                        string itemName = paras[0];
                        int itemNumber = 1;
                        if(paras.Length>1)
                            itemNumber = int.Parse(paras[1]);

                        if (itemNumber > 0) //获取物品
                        {
                            for (int i = 0; i < itemNumber; i++)
                            {
                                RuntimeData.Instance.Items.Add(ItemManager.GetItem(itemName).Clone());
                            }
                            ShowDialog("主角", "获得 " + itemName + " x " + itemNumber, isSingleAction);
                        }
                        else //失去物品
                        {
                            for (int i = 0; i < Math.Abs(itemNumber); ++i)
                            {
                                Item tobeDel = null;
                                foreach (var item in RuntimeData.Instance.Items)
                                {
                                    if (item.Name == itemName)
                                    {
                                        tobeDel = item;
                                        break;
                                    }
                                }
                                if (tobeDel == null)
                                {
                                    MessageBox.Show("error! 失去了一个没有的物品");
                                }
                                else
                                {
                                    RuntimeData.Instance.Items.Remove(tobeDel);
                                }
                            }
                            ShowDialog("主角", "失去 " + itemName + " x " + Math.Abs(itemNumber), isSingleAction);
                        }
                        break;
                    }
                case "LOG":
                    {
                        string date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";
                        RuntimeData.Instance.Log += date + "，" + action.Value + "\r\n";
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "MENPAI":
                    {
                        String date = "江湖" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Year] + "年" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Month] + "月" + CommonSettings.chineseNumber[RuntimeData.Instance.Date.Day] + "日";
                        RuntimeData.Instance.Log += date + "，加入" + action.Value + "。\r\n";
                        RuntimeData.Instance.Menpai = action.Value;
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "NICK":
                    {
                        RuntimeData.Instance.addNick(action.Value);
                        RuntimeData.Instance.CurrentNick = action.Value;
                        uiHost.mapUI.nickLabel.Text = action.Value;
                        ShowDialog("主角", "获得称号：【" + action.Value + "】！", isSingleAction);
                        break;
                    }
                case "COST_MONEY":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Money = RuntimeData.Instance.Money - number;
                        ShowDialog("主角", "失去 " + number.ToString() + " 两银子。", isSingleAction);
                        break;
                    }
                case "GET_MONEY":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Money = RuntimeData.Instance.Money + number;
                        ShowDialog("主角", "得到 " + number.ToString() + " 两银子。", isSingleAction);
                        break;
                    }
                case "COST_ITEM":
                    {
                        string itemName = paras[0];
                        int number = int.Parse(paras[1]);
                        for (int i = 0; i < number; i++)
                        {
                            Item item = null;
                            foreach (var s in RuntimeData.Instance.Items)
                            {
                                if (s.Name == itemName)
                                {
                                    item = s;
                                    break;
                                }
                            }
                            if (item != null)
                            {
                                RuntimeData.Instance.Items.Remove(item);
                            }
                        }
                        ShowDialog("主角", "失去 " + itemName + "*" + number, isSingleAction);
                        break;
                    }
                case "COST_DAY":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddDays(number);
                        //uiHost.mapUI.resetMap();
                        ShowDialog("主角", "一共用了" + number + "天...", isSingleAction);
                        //if (!isSingleAction) NextAction();
                        break;
                    }
                case "COST_HOUR":
                    {
                        int number = int.Parse(action.Value);
                        RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(number);
                        //uiHost.mapUI.resetMap();
                        ShowDialog("主角", "过了" + number + "个时辰...", isSingleAction);
                        //if (!isSingleAction) NextAction();
                        break;
                    }
                case "GET_POINT":
                    {
                        //获取分配点数
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.LeftPoint += value;
                                if (r.LeftPoint < 0) r.LeftPoint = 0;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "属性分配点增加【" + value.ToString() + "】！", isSingleAction);
                        else
                            ShowDialog(role, name + "属性分配点减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.MAXHP":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["maxhp"] = r.Attributes["maxhp"] + value;
                                r.Attributes["hp"] = r.Attributes["maxhp"];
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value >0)
                            ShowDialog(role, name + "气血上限增加【" + value.ToString() + "】！", isSingleAction);
                        else
                            ShowDialog(role, name + "气血上限减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.MAXMP":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["maxmp"] = r.Attributes["maxmp"] + value;
                                r.Attributes["mp"] = r.Attributes["maxmp"];
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "内力上限增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "内力上限减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.根骨":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["gengu"] = r.Attributes["gengu"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "根骨增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "根骨减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.身法":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["shenfa"] = r.Attributes["shenfa"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "身法增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "身法减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.悟性":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["wuxing"] = r.Attributes["wuxing"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "悟性增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "悟性减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.臂力":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["bili"] = r.Attributes["bili"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "臂力增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "臂力减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.福缘":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["fuyuan"] = r.Attributes["fuyuan"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "福缘增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "福缘减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.定力":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["dingli"] = r.Attributes["dingli"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "定力增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "定力减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.拳掌":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["quanzhang"] = r.Attributes["quanzhang"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "拳掌增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "拳掌减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.剑法":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["jianfa"] = r.Attributes["jianfa"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "剑法增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "剑法减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.刀法":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["daofa"] = r.Attributes["daofa"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "刀法增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "刀法减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.奇门":
                    {
                        string role = paras[0];
                        int value = int.Parse(paras[1]);
                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                r.Attributes["qimen"] = r.Attributes["qimen"] + value;
                            }
                        }

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        if(value>0)
                            ShowDialog(role, name + "奇门增加【" + value.ToString() + "】！",isSingleAction);
                        else
                            ShowDialog(role, name + "奇门减少【" + (-value).ToString() + "】！", isSingleAction);
                        break;
                    }
                case "UPGRADE.SKILL":
                    {
                        string role = paras[0];
                        string skillName = paras[1];
                        int number = int.Parse(paras[2]);

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key != role)
                                continue;

                            //是否已经学会了某个技能
                            SkillInstance s = null;
                            foreach (var skill in r.Skills)
                            {
                                if (skill.Skill.Name == skillName)
                                {
                                    s = skill;
                                    break;
                                }
                            }

                            //没学会
                            if (s == null)
                            {
                                SkillInstance sintance = new SkillInstance()
                                {
                                    Skill = SkillManager.GetSkill(skillName),
                                    Level = number,
                                    MaxLevel = 10,
                                    Owner = r,
                                    uihost = uiHost
                                };
                                sintance.Exp = 0;
                                r.Skills.Add(sintance);

                                ShowDialog(role, name + "掌握了武功【" + skillName + "】(" + number + "级)！", isSingleAction);
                                break;
                            }
                            //已经学会了该武功
                            else
                            {
                                //无法再提升该武学了
                                if (s.Level >= s.MaxLevel)
                                {
                                    ShowDialog(role, name + "的武功【" + skillName + "】已经到达等级上限（" + s.MaxLevel + "级)，无法再提升了！", isSingleAction);
                                    break;
                                }
                                else
                                {
                                    s.Level += number;
                                    if (s.Level > s.MaxLevel)
                                        s.Level = s.MaxLevel;
                                    ShowDialog(role, name + "的武功【" + skillName + "】提升了" + number + "级！", isSingleAction);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case "LEARN.SKILL":
                    {
                        string role = paras[0];
                        string skillName = paras[1];
                        int number = int.Parse(paras[2]);

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                SkillInstance s = null;
                                foreach (var skill in r.Skills)
                                {
                                    if (skill.Skill.Name == skillName)
                                    {
                                        s = skill;
                                        break;
                                    }
                                }
                                if (s == null)
                                {
                                    SkillInstance sintance = new SkillInstance()
                                    {
                                        Skill = SkillManager.GetSkill(skillName),
                                        Level = number,
                                        MaxLevel = Math.Max(number, 10),
                                        Owner = r,
                                        uihost = uiHost
                                    };
                                    sintance.Exp = 0;
                                    r.Skills.Add(sintance);
                                }
                                else
                                {
                                    s.Level = Math.Max(s.Level, number);
                                    s.MaxLevel = Math.Max(s.Level, s.MaxLevel);
                                }
                            }
                        }
                        ShowDialog(role, name + "掌握了武功【" + skillName + "】" + number + "级！",isSingleAction);
                        break;
                    }
                case "UPGRADE.INTERNALSKILL":
                    {
                        string role = paras[0];
                        string skillName = paras[1];
                        int number = int.Parse(paras[2]);

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key != role)
                                continue;

                            //是否已经学会了某个技能
                            InternalSkillInstance s = null;
                            foreach (var skill in r.InternalSkills)
                            {
                                if (skill.Skill.Name == skillName)
                                {
                                    s = skill;
                                    break;
                                }
                            }

                            //没学会
                            if (s == null)
                            {
                                InternalSkillInstance sintance = new InternalSkillInstance()
                                {
                                    Skill = SkillManager.GetInternalSkill(skillName),
                                    Level = number,
                                    MaxLevel = Math.Max(number,10),
                                    Owner = r
                                };
                                sintance.Exp = 0;
                                r.InternalSkills.Add(sintance);

                                ShowDialog(role, name + "掌握了内功【" + skillName + "】(" + number + "级)！", isSingleAction);
                                break;
                            }
                            //已经学会了该内功
                            else
                            {
                                //无法再提升该内功了
                                if (s.Level >= s.MaxLevel)
                                {
                                    ShowDialog(role, name + "的内功【" + skillName + "】已经到达等级上限(" + s.MaxLevel + "级)！无法再提升了！", isSingleAction);
                                    break;
                                }
                                else
                                {
                                    s.Level += number;
                                    if (s.Level > s.MaxLevel)
                                        s.Level = s.MaxLevel;
                                    ShowDialog(role, name + "的内功【" + skillName + "】提升了" + number + "级！",isSingleAction);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case "LEARN.INTERNALSKILL":
                    {
                        string role = paras[0];
                        string skillName = paras[1];
                        int number = int.Parse(paras[2]);

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                InternalSkillInstance s = null;
                                foreach (var skill in r.InternalSkills)
                                {
                                    if (skill.Skill.Name == skillName)
                                    {
                                        s = skill;
                                        break;
                                    }
                                }
                                if (s == null)
                                {
                                    InternalSkillInstance sintance = new InternalSkillInstance()
                                    {
                                        Skill = SkillManager.GetInternalSkill(skillName),
                                        Level = number,
                                        MaxLevel = Math.Max(number,10),
                                        Owner = r
                                    };
                                    sintance.Exp = 0;
                                    r.InternalSkills.Add(sintance);
                                }
                                else
                                {
                                    s.Level = Math.Max(s.Level, number);
                                    s.MaxLevel = Math.Max(s.Level, s.MaxLevel);
                                }
                            }
                        }
                        ShowDialog(role, name + "掌握了内功【" + skillName + "】" + number + "级！", isSingleAction);
                        break;
                    }
                case "LEARN.SPECIALSKILL":
                    {
                        string role = paras[0];
                        string skillName = paras[1];

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                SpecialSkillInstance s = null;
                                foreach (var skill in r.SpecialSkills)
                                {
                                    if (skill.Skill.Name == skillName)
                                    {
                                        s = skill;
                                        break;
                                    }
                                }
                                if (s == null)
                                {
                                    SpecialSkillInstance sintance = new SpecialSkillInstance()
                                    {
                                        Skill = SkillManager.GetSpecialSkill(skillName),
                                        Owner = r
                                    };
                                    r.SpecialSkills.Add(sintance);
                                }
                                else
                                {
                                }
                            }
                        }
                        ShowDialog(role, name + "掌握了特殊攻击【" + skillName + "】", isSingleAction);
                        break;
                    }
                case "LEARN.TALENT":
                    {
                        string role = paras[0];
                        string talentName = paras[1];

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                string s = null;
                                foreach (var talent in r.Talents)
                                {
                                    if (talent == talentName)
                                    {
                                        s = talent;
                                        break;
                                    }
                                }
                                if (s == null)
                                {
                                    if (r.Talent == null || r.Talent == "")
                                        r.Talent = talentName;
                                    else
                                        r.Talent += "#" + talentName;
                                }
                            }
                        }
                        ShowDialog(role, name + "领悟了天赋【" + talentName + "】", isSingleAction);
                        break;
                    }
                case "REMOVE.TALENT":
                    {
                        string role = paras[0];
                        string talentName = paras[1];

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;

                        foreach (var r in RuntimeData.Instance.Team)
                        {
                            if (r.Key == role)
                            {
                                string s = null;
                                foreach (var talent in r.Talents)
                                {
                                    if (talent == talentName)
                                    {
                                        s = talent;
                                        break;
                                    }
                                }
                                if (s != null)
                                {
                                    r.RemoveTalent(s);
                                }
                            }
                        }
                        ShowDialog(role, name + "移除了天赋【" + talentName + "】", isSingleAction);
                        break;
                    }
                case "SELECT":
                    {
                        UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
                        string content = action.Value;
                        string[] tmp = content.Split(new char[] { '#' });
                        //0位目前没用(存的角色名)
                        //1位是标题
                        //后续是选择项
                        string title = tmp[1];
                        List<string> opts = new List<string>();
                        for (int i = 2; i < tmp.Length; ++i)
                        {
                            opts.Add(tmp[i]);
                        }
                        uiHost.multiSelectBox.Show(title, opts, (selected) =>
                        {
                            storyResult = selected.ToString();
                            if (!isSingleAction) NextAction();
                        });
                        break;
                    }
                case "BATTLE":
                    {
                        string battleKey = action.Value;
                        //uihost.battleFieldContainer.Load();
                        uiHost.battleFieldContainer.Load(
                            battleKey,
                            (battleRst) =>
                            {
                                if (battleRst == 1)
                                {
                                    storyResult = "win";
                                    if (!isSingleAction) NextAction();
                                }
                                else
                                {
                                    storyResult = "lose";
                                    if (!isSingleAction) NextAction();
                                }
                            }
                        );
                        break;
                    }
                //给女主角更名
                case "CHANGE_FEMALE_NAME":
                    {
                        uiHost.textBox.Show("更名", NameTextBoxType.NameBox, action.Value, () =>
                            {
                                RuntimeData.Instance.femaleName = uiHost.textBox.text.Text;
                                uiHost.textBox.Visibility = Visibility.Collapsed;
                                NextAction();
                            });
                        uiHost.textBox.Visibility = Visibility.Visible;
                        break;
                    }
                case "EFFECT":
                    {
                        string effectKey = action.Value;
                        AudioManager.PlayEffect(ResourceManager.Get(effectKey));
                        if (!isSingleAction) NextAction();
                        break;
                    }
                case "SET_TIME_KEY": //设置时间开关
                    {
                        string key = paras[0];
                        int days = int.Parse(paras[1]); //过期时间
                        RuntimeData.Instance.AddTimeKey(key, days);
                        NextAction();
                        break;
                    }
                case "CLEAR_TIME_KEY": //清除时间开关
                    {
                        string key = paras[0];
                        RuntimeData.Instance.RemoveTimeKey(key);
                        NextAction();
                        break;
                    }
                case "ANIMATION": //改变人物动画
                    {
                        string role = paras[0];
                        string animationName = paras[1];

                        string name = "";
                        if (role == "女主")
                            name = RuntimeData.Instance.femaleName;
                        else if (role == "主角")
                            name = RuntimeData.Instance.maleName;
                        else
                            name = RoleManager.GetRole(role).Name;
                        foreach(var r in RuntimeData.Instance.Team)
                        {
                            if(r.Name == name)
                            {
                                r.Animation = animationName;
                            }
                        }
                        NextAction();
                        break;
                    }
                case "HEAD": //更换主角头像
                    {
                        RuntimeData.Instance.Team[0].HeadPicPath = paras[0];
                        NextAction();
                        break;
                    }
                default:
                    MessageBox.Show("错误，调用了未定义的action:" + action.Type);
                    break;
            }//switch
        }

        private static void ShowDialog(string role, string info, bool isSingleAction)
        {
            uiHost.dialogPanel.ShowDialog(
                role, 
                info, 
                (ret) => {
                    uiHost.dialogPanel.Hide();
                    if (!isSingleAction) NextAction();
                });
        }

        private static string[] SpliteValue(string value)
        {
            return value.Split(new char[] { '#' });
        }

        private static void StoryResult()
        {
            uiHost.mapUI.IsEnabled = true;
            uiHost.scence.Hide();
            RuntimeData.Instance.KeyValues[currentStory.Name] = storyResult;
            foreach (var r in currentStory.Results)
            {
                if (r.Ret.Equals(storyResult))
                {
                    if (r.Controls.Count == 0)
                    {
                        RuntimeData.Instance.gameEngine.CallScence(null, new NextGameState() { Type = r.Type, Value = r.Value });
                        return;
                    }
                    else //control
                    {
                        foreach (var c in r.Controls)
                        {
                            bool conditionOk = true;
                            foreach (var condition in c.Conditions)
                            {
                                if (!TriggerManager.judge(condition))
                                {
                                    conditionOk = false;
                                    break;
                                }
                            }
                            if (conditionOk)
                            {
                                RuntimeData.Instance.gameEngine.CallScence(null, new NextGameState() { Type = c.Type, Value = c.Value });
                                return;
                            }
                        }
                    }
                }
            }
            if (storyResult == "lose") //战斗结束
            {
                RuntimeData.Instance.gameEngine.CallScence(null, new NextGameState() { Type = "gameOver", Value = "gameOver" });
                return;
            }

            //返回当前地图
            RuntimeData.Instance.gameEngine.CallScence(null, new NextGameState() { Type = "map", Value = RuntimeData.Instance.CurrentBigMap });
            return;
        }
    }
}
