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
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.IO;



namespace JyGame.GameData
{
    public class Dialog
    {
        public string type { get; set; }

        public string role{get;set;}

        public string info;

        public int number=1;

        public ImageSource img;
    };

    /// <summary>
    /// 场景对话管理器
    /// </summary>
    public class DialogManager
    {
        static private Dictionary<string, List<Dialog>> storyDialogs = new Dictionary<string, List<Dialog>>();
        static private Dictionary<string, string> storyDialogMap = new Dictionary<string, string>();
        static private Dictionary<string, string> storyDialogType = new Dictionary<string, string>();
        static private Dictionary<string, Collection<String>> battleMusts = new Dictionary<string, Collection<string>>();

        public static string scenarioName { get; set; }

        public static List<Dialog> GetDialogList(string story_dialogs)
        {
            return storyDialogs[story_dialogs];
        }

        public static string GetDialogsMapKey(string story_dialogs)
        {
            return storyDialogMap[story_dialogs];
        }

        public static string GetDialogsType(string story_dialogs)
        {
            return storyDialogType[story_dialogs];
        }

        public static Collection<String> GetBattleMusts(string story_dialogs)
        {
            if(battleMusts.ContainsKey(story_dialogs))
            {
                return battleMusts[story_dialogs];
            }
            else
            {
                return null;
            }
        }
    }

}
