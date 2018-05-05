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

namespace JyGame.GameData
{
    //所有的天赋效果种类
    public enum TalentEffectEnum
    {
        attackEffect,
        defenceEffect,
        other
    }

    //一个天赋
    public class RoleTalent
    {
        public string key;
        public List<Talent> talents = new List<Talent>();
    }

    //一个子天赋的触发条件和效果
    public class Talent
    {
        static public int GetTalentCost(string t)
        {
            int cost = int.Parse(ResourceManager.Get("天赋." + t).Split(new char[] { '#' })[0]);
            return cost;
        }

        /// <summary>
        /// 获取天赋说明
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public string GetTalentInfo(string name, bool displayCost = true)
        {
            string talentInfo = ResourceManager.Get("天赋." + name).Split(new char[] { '#' })[1];

            if (displayCost) 
            {
                return string.Format("【{0}】\n (消耗武学常识:{1})\n {2}",
                    name,
                    GetTalentCost(name),
                    Tools.StringToMultiLine(talentInfo,50));
            }
            else
            {
                return string.Format("【{0}】\n {1}",
                    name,
                    Tools.StringToMultiLine(talentInfo,50));
            }
        }


        //以下代码暂时作废

        public List<TalentEffect> effects = new List<TalentEffect>();
        public List<TalentCondition> conditions = new List<TalentCondition>();
        public List<TalentEffectEnum> effectEnum = new List<TalentEffectEnum>();
        public Showword showword = new Showword();

        //判断是否需要Judge
        public bool shouldJudge(TalentEffectEnum effect2Judge)
        {
            if(effectEnum.Contains(effect2Judge))
                return true;

            return false;
        }

        //判断天赋是否能发动
        public bool JudgeConditions(Role source, Role target, UIHost uihost)
        {
            bool triggered = true;
            foreach (TalentCondition condition in conditions)
            {
                if (!condition.JudgeCondition(source, target, uihost))
                {
                    triggered = false;
                    break;
                }
            }
            return triggered;
        }
    };

    public class Showword
    {
        public float probability = 0.5f;
        public List<string> word = new List<string>();
    }

    //天赋的一个细分触发条件
    public class TalentCondition
    {
        public string type;
        public string value;

        public bool JudgeCondition(Role source, Role target, UIHost uihost)
        {
            if (type == "攻击方生命少于" && (source.Attributes["hp"] / (double)source.Attributes["maxhp"] > double.Parse(value)))
            {
                return false;
            }

            if (type == "防御方生命少于" && (target.Attributes["hp"] / (double)target.Attributes["maxhp"] > double.Parse(value)))
            {
                return false;
            }

            return true;
        }
    };

    //天赋的一个子效果
    public class TalentEffect
    {
        public string type;
        public string value;
        public TalentEffectEnum effectEnum;

        //攻击加成效果
        public AttackEffect attackEffect(Role source, Role target, AttackEffect attackEffect)
        {
            AttackEffect ad = new AttackEffect(attackEffect);

            if (effectEnum == TalentEffectEnum.attackEffect)
            {
                ad.attackHigh *= double.Parse(value);
                ad.attackLow *= double.Parse(value);
                ad.criticalHit *= double.Parse(value);
            }

            return ad;
        }

        //防御加成效果
        public double defenceEffect(Role source, Role target, double defence)
        {
            if (effectEnum == TalentEffectEnum.defenceEffect)
                return defence * double.Parse(value);
            return defence;
        }
    };

    public class TalentManager
    {
        public static UIHost uihost = null;
        public static Dictionary<string, RoleTalent> talentDic = new Dictionary<string, RoleTalent>();

        #region 初始化
        public static void init()
        {
            talentDic.Clear();
            foreach (string talentXmlFile in GameProject.GetFiles("talent"))
            {
                XElement xmlRoot = Tools.LoadXml("Scripts/" + talentXmlFile);
                foreach (var roleTalentNode in xmlRoot.Elements("roleTalent"))
                {
                    RoleTalent roleTalent = new RoleTalent();
                    roleTalent.key = Tools.GetXmlAttribute(roleTalentNode, "key");
                    roleTalent.talents = new List<Talent>();
                    foreach (var talentNode in roleTalentNode.Elements("talent"))
                    {
                        Talent talent = new Talent();
                        talent.conditions = new List<TalentCondition>();
                        talent.effects = new List<TalentEffect>();
                        foreach (var conditionNode in talentNode.Elements("condition"))
                        {
                            TalentCondition talentCondition = new TalentCondition();
                            talentCondition.type = Tools.GetXmlAttribute(conditionNode, "type");
                            talentCondition.value = Tools.GetXmlAttribute(conditionNode, "value");
                            talent.conditions.Add(talentCondition);
                        }
                        foreach (var effectNode in talentNode.Elements("effect"))
                        {
                            TalentEffect talentEffect = new TalentEffect();
                            talentEffect.type = Tools.GetXmlAttribute(effectNode, "type");
                            talentEffect.value = Tools.GetXmlAttribute(effectNode, "value");

                            //判断类别
                            switch (talentEffect.type)
                            {
                                case "攻击加成":
                                    talentEffect.effectEnum = TalentEffectEnum.attackEffect;
                                    break;
                                case "防御加成":
                                    talentEffect.effectEnum = TalentEffectEnum.defenceEffect;
                                    break;
                                default:
                                    talentEffect.effectEnum = TalentEffectEnum.other;
                                    break;
                            }

                            talent.effects.Add(talentEffect);
                            if (!talent.effectEnum.Contains(talentEffect.effectEnum))
                                talent.effectEnum.Add(talentEffect.effectEnum);
                        }

                        if (talentNode.Element("showword") != null)
                        {
                            talent.showword = new Showword();
                            XElement showwordNode = talentNode.Element("showword");
                            if (showwordNode.Attribute("probability") != null)
                            {
                                talent.showword.probability = Tools.GetXmlAttributeFloat(showwordNode, "probability");
                            }
                            foreach (var wordNode in showwordNode.Elements("word"))
                            {
                                talent.showword.word.Add(Tools.GetXmlAttribute(wordNode, "value"));
                            }
                        }

                        roleTalent.talents.Add(talent);
                    }
                    talentDic.Add(roleTalent.key, roleTalent);
                }
            }
        }
        #endregion

        #region 攻防加成
        //攻击全面加成计算
        public static AttackEffect attackEffect(Role source, Role target, AttackEffect attackEffect, AttackResult result)
        {
            AttackEffect ad = new AttackEffect(attackEffect);
            
            //攻击方天赋
            foreach (string talentKey in source.Talents)
            {
                if (!talentDic.ContainsKey(talentKey))
                    continue;
                foreach (Talent talent in talentDic[talentKey].talents)
                {
                    if (talent.shouldJudge(TalentEffectEnum.attackEffect) && talent.JudgeConditions(source, target, uihost))
                    {
                        if(talent.showword.word.Count != 0)
                            CommonSettings.SetSourceCastInfo(talent.showword.word.ToArray(), result, talent.showword.probability);
                        foreach (TalentEffect effect in talent.effects)
                        {
                            ad = effect.attackEffect(source, target, ad);
                        }
                    }
                }
            }

            return ad;
        }
        //防御加成计算
        public static double defenceEffect(Role source, Role target, double defence, AttackResult result)
        {
            double def = defence;

            //防守方天赋
            foreach (string talentKey in target.Talents)
            {
                if (!talentDic.ContainsKey(talentKey))
                    continue;
                foreach (Talent talent in talentDic[talentKey].talents)
                {
                    if (talent.shouldJudge(TalentEffectEnum.defenceEffect) && talent.JudgeConditions(source, target, uihost))
                    {
                        if (talent.showword.word.Count != 0)
                            CommonSettings.SetTargetCastInfo(talent.showword.word.ToArray(), result, talent.showword.probability);
                        foreach (TalentEffect effect in talent.effects)
                        {
                            def = effect.defenceEffect(source, target, defence);
                        }
                    }
                }
            }

            return def;
        }
        #endregion
    }

    public class AttackEffect
    {
        public double attackHigh = 0.0;
        public double attackLow = 0.0;
        public double criticalHit = 0.0;

        public AttackEffect(AttackEffect attackEffect)
        {
            this.attackHigh = attackEffect.attackHigh;
            this.attackLow = attackEffect.attackLow;
            this.criticalHit = attackEffect.criticalHit;
        }

        public AttackEffect(double attackHigh, double attackLow, double criticalHit)
        {
            this.attackHigh = attackHigh;
            this.attackLow = attackLow;
            this.criticalHit = criticalHit;
        }
    }
}
