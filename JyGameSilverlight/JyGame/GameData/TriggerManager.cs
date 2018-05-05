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
using System.Globalization;

namespace JyGame.GameData
{
    public class TriggerManager
    {
        public static bool judge(EventCondition condition)
        {
            //XXX必须在队中的判定，按照roleName判断，非roleKey
            if (condition.type == "in_team")
            {
                if (!RuntimeData.Instance.NameInTeam(condition.value))
                {
                    return false;
                }
            }

            //XXX必须不在队中的判定，按照roleName判断，非roleKey
            if (condition.type == "not_in_team")
            {
                if (RuntimeData.Instance.NameInTeam(condition.value))
                {
                    return false;
                }
            }

            //XXX必须在队中的判定，按照roleKey
            if (condition.type == "key_in_team")
            {
                if (!RuntimeData.Instance.InTeam(condition.value))
                {
                    return false;
                }
            }

            //XXX必须不在队中的判定，按照roleKey
            if (condition.type == "key_not_in_team")
            {
                if (RuntimeData.Instance.InTeam(condition.value))
                {
                    return false;
                }
            }

            //XXX剧情必须已经完成的判定，有key
            if (condition.type == "should_finish" )
            {
                if (!RuntimeData.Instance.KeyValues.ContainsKey(condition.value))
                {
                    return false;
                }
            }

            //必须没有完成，没key
            if (condition.type == "should_not_finish" )
            {
                if (RuntimeData.Instance.KeyValues.ContainsKey(condition.value))
                {
                    return false;
                }
            }

            if (condition.type == "has_time_key")
            {

                if (!RuntimeData.Instance.KeyValues.ContainsKey(RuntimeData.TIMEKEY_PREF + condition.value))
                {
                    return false;
                }
            }

            if (condition.type == "not_has_time_key")
            {
                if (RuntimeData.Instance.KeyValues.ContainsKey(RuntimeData.TIMEKEY_PREF + condition.value))
                {
                    return false;
                }
            }

            if (condition.type == "not_in_time")
            {
                string[] times = condition.value.Split(new char[] { '#' });
                foreach (var t in times)
                {
                    if( CommonSettings.IsChineseTime(RuntimeData.Instance.Date,t[0]))
                        return false;
                }
            }

            if (condition.type == "in_time")
            {
                string[] times = condition.value.Split(new char[] { '#' });
                foreach (var t in times)
                {
                    if (CommonSettings.IsChineseTime(RuntimeData.Instance.Date, t[0]))
                        return true;
                }
                return false;
            }

            //必须有XXX银子
            if (condition.type == "have_money")
            {
                if (RuntimeData.Instance.Money < int.Parse(condition.value) )
                {
                    return false;
                }
            }

            //必须有XXX物品,XXX个（默认1个。）
            if (condition.type == "have_item")
            {
                int number = 0;
                foreach (var s in RuntimeData.Instance.Items)
                {
                    if (s.Name == condition.value)
                        number++;
                }

                if (number < condition.number||number == 0)
                {
                    return false;
                }
            }

            //游戏模式
            if (condition.type == "game_mode")
            {
                if (RuntimeData.Instance.GameMode != condition.value)
                    return false;
            }

            //游戏已经进行了XXX天以上
            if (condition.type == "exceed_day")
            {
                if ((RuntimeData.Instance.Date - DateTime.ParseExact("0001-01-01 00:00:00","yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)).Days <= int.Parse(condition.value))
                    return false;
            }

            //游戏还没进行XXX天
            if(condition.type == "not_exceed_day")
            {
                if ((RuntimeData.Instance.Date - DateTime.ParseExact("0001-01-01 00:00:00","yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)).Days > int.Parse(condition.value))
                    return false;
            }

            //属于/不属于某个门派
            if (condition.type == "in_menpai")
            {
                if (RuntimeData.Instance.Menpai != condition.value)
                    return false;
            }
            if (condition.type == "not_in_menpai")
            {
                if (RuntimeData.Instance.Menpai == condition.value)
                    return false;
            }
            if (condition.type == "has_menpai")
            {
                return RuntimeData.Instance.Menpai != "";
            }

            //是不是在某个周目内
            if (condition.type == "in_round")
            {
                if (RuntimeData.Instance.Round != int.Parse(condition.value) )
                    return false;
            }
            if (condition.type == "not_in_round")
            {
                if (RuntimeData.Instance.Round == int.Parse(condition.value) )
                    return false;
            }

            //一定概率触发
            if (condition.type == "probability")
            {
                if (! Tools.ProbabilityTest((double)int.Parse(condition.value) / 100.0f))
                    return false;
            }

            //道德
            if (condition.type == "daode_more_than")
            {
                return (RuntimeData.Instance.Daode >= int.Parse(condition.value));
            }
            if (condition.type == "daode_less_than")
            {
                return (RuntimeData.Instance.Daode < int.Parse(condition.value));
            }

            //好感
            if (condition.type == "haogan_more_than")
            {
                return (RuntimeData.Instance.Haogan >= int.Parse(condition.value));
            }
            if (condition.type == "haogan_less_than")
            {
                return (RuntimeData.Instance.Haogan < int.Parse(condition.value));
            }

            //XX武功在多少级（含）以上
            if (condition.type == "skill_more_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                string skillName = paras[1];
                int level = int.Parse(paras[2]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey)
                    {
                        foreach (var s in r.Skills)
                        {
                            if (s.Skill.Name == skillName && s.Level >= level)
                            {
                                return true;
                            }
                        }
                        foreach (var s in r.InternalSkills)
                        {
                            if (s.Skill.Name == skillName && s.Level >= level)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            //XX武功在多少级以下
            if (condition.type == "skill_less_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                string skillName = paras[1];
                int level = int.Parse(paras[2]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey)
                    {
                        bool find = false;
                        foreach (var s in r.Skills)
                        {
                            if (s.Skill.Name == skillName) find = true;
                            if (s.Skill.Name == skillName && s.Level < level)
                            {
                                return true;
                            }
                        }
                        foreach (var s in r.InternalSkills)
                        {
                            if (s.Skill.Name == skillName) find = true;
                            if (s.Skill.Name == skillName && s.Level < level)
                            {
                                return true;
                            }
                        }
                        if (!find)
                            return true;
                    }
                }
                return false;
            }

            //XX在多少级（含）以上
            if (condition.type == "level_greater_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Level >= level)
                        return true;
                }
                return false;
            }

            //XX定力在XX以上
            if (condition.type == "dingli_greater_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Attributes["dingli"] >= level)
                        return true;
                }
                return false;
            }

            //XX悟性在XX以上
            if (condition.type == "wuxing_greater_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Attributes["wuxing"] >= level)
                        return true;
                }
                return false;
            }

            //XX定力在XX以下
            if (condition.type == "dingli_less_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Attributes["dingli"] < level)
                        return true;
                }
                return false;
            }

            //XX悟性在XX以上
            if (condition.type == "wuxing_less_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Attributes["wuxing"] < level)
                        return true;
                }
                return false;
            }

            //XX身法在XX以上
            if (condition.type == "shenfa_greater_than")
            {
                string[] paras = condition.value.Split(new char[] { '#' });
                string roleKey = paras[0];
                int level = int.Parse(paras[1]);
                foreach (var r in RuntimeData.Instance.Team)
                {
                    if (r.Key == roleKey && r.Attributes["shenfa"] >= level)
                        return true;
                }
                return false;
            }

            //队伍中队友达到了X人
            if (condition.type == "friendCount")
            {
                if (RuntimeData.Instance.Team.Count >= int.Parse(condition.value))
                    return true;
                else
                    return false;
            }

            //江湖排名（武道大会）
            if(condition.type == "rank")
            {
                if(RuntimeData.Instance.Rank == -1)
                {
                    return false;
                }
                else
                {
                    return RuntimeData.Instance.Rank <= int.Parse(condition.value);
                }
            }
            return true;
        }
    }
}
