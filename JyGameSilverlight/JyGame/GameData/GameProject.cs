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

namespace JyGame.GameData
{
    public class GameProject
    {

        static public bool IsLoaded = false;
        static public void LoadGameProject(string projectFile = "")
        {
            if (projectFile != "")
            {
                Configer.Instance.ScriptRootMenu = projectFile.Replace("Scripts\\project.xml", "");
            }
            ResourceManager.Init();
            AnimationManager.Init();
            SkillManager.Init();
            TalentManager.init();
            ItemManager.Init();
            RoleManager.Init();
            ShopManager.Init();
            SellShopManager.Init();
            MapEventsManager.Init();
            BattleManager.Init();
            TowerManager.Init();
            StoryManager.Init();
            TimeTriggerManager.Init();
            AudioManager.Init();
            IsLoaded = true;
        }

        static public List<string> GetFiles(string type)
        {
            List<string> results = new List<string>();
            XElement xmlRoot = Tools.LoadXml("Scripts/project.xml");
            foreach (var node in xmlRoot.Element("files").Elements("file"))
            {
                string t = Tools.GetXmlAttribute(node, "type");
                if (t.Equals(type))
                {
                    results.Add(Tools.GetXmlAttribute(node, "path"));
                }
            }
            return results;
        }
    }
}
