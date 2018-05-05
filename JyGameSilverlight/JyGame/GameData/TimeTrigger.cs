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
using System.Globalization;

namespace JyGame.GameData
{
    public class TimeTrigger
    {
        public int time;
        public string story;
        public List<EventCondition> conditions = new List<EventCondition>();
    }
    public class TimeTriggerManager
    {
        //static public Dictionary<int, string> triggers = new Dictionary<int, string>();
        static private List<TimeTrigger> triggerList = new List<TimeTrigger>();

        static public TimeTrigger GetCurrentTrigger()
        {
            DateTime currentDate = RuntimeData.Instance.Date;
            TimeSpan span = currentDate - DateTime.ParseExact("0001-01-01 10:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            int days = span.Days;
            foreach (var t in triggerList)
            {
                if (t.time <= days && (!RuntimeData.Instance.KeyValues.ContainsKey(t.story)))
                {
                    bool judge = true;
                    foreach (var c in t.conditions)
                    {
                        if (!TriggerManager.judge(c))
                        {
                            judge = false;
                            break;
                        }
                    }
                    if (judge)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
        static public void Init()
        {
            triggerList.Clear();
            foreach (string triggerFile in GameProject.GetFiles("timetrigger"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + triggerFile);
                foreach (XElement times in xmlRoot.Elements("time"))
                {
                    int day = Tools.GetXmlAttributeInt(times, "day");
                    string storyDialog = Tools.GetXmlAttribute(times, "story");
                    TimeTrigger tt = new TimeTrigger();
                    tt.time = day;
                    tt.story = storyDialog;

                    if (times.Element("condition") != null)
                    {
                        foreach(var c in times.Elements("condition"))
                        {
                            EventCondition cd = EventCondition.Parse(c);
                            tt.conditions.Add(cd);
                        }
                    }
                    triggerList.Add(tt);
                }
            }
        }
    }
}
