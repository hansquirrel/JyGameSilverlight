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
using JyGame.UserControls;

namespace JyGame.GameData
{
    public class CommonSettings
    {

        #region MAP
        public const int MAXWIDTH = 50;//14;
        public const int MAXHEIGHT = 50;//10;

        public const int SPIRIT_X_MARGIN = 15;
        public const int SPIRIT_Y_MARGIN = 40;
        public const int SPIRIT_BLOCK_SIZE = 55;

        public const int SCENARIO_MAP_CHANGE_TICK = 500;
        #endregion

        #region ZINDEX
        //BattleField上的层叠关系
        public const int Z_EFFECTAOYI = 13000;
        public const int Z_EFFECTAOYIBG = 12999;
        public const int Z_EFFECTROLE = 12000;
        public const int Z_EFFECT = 11000;
        public const int Z_EFFECTCOVER = 10000;

        public const int Z_SPIRIT = 900;
        public const int Z_SKILL = 1000;
        public const int Z_ATTACKINFO = 1001;
        public const int Z_SELECTMENU = 1002;
        public const int Z_DIALOGMASK = 1003;
        public const int Z_SPIRIT_SMALLDIALOG = 1005;
        public const int Z_DIALOG = 1006;

        //MainUI上的层叠关系
        public const int Z_MAINUI_BATTLEFIELD = 1000;
        public const int Z_MAINUI_ROLEDETAILPANEL = 1210;
        public const int Z_MAINUI_ITEMSELECTPANEL = 1220;
        public const int Z_MAINUI_ROLEACTIONPANEL = 1230;
        public const int Z_MAINUI_ROLEPANEL = 1240;

        public const int Z_MAINUI_UP = 1050;
        public const int Z_MAINUI_DOWN = 1051;
        public const int Z_MAINUI_LEFT = 1052;
        public const int Z_MAINUI_RIGHT = 1053;
        public const int Z_MAINUI_DIALOG_COVER = 1299;
        public const int Z_MAINUI_DIALOG = 1300;
       

        //大地图上的层叠关系
        public const int Z_MAPUI_MAP = 1000;
        public const int Z_MAPUI_LOCATION = 1100;
        public const int Z_MAPUI_ROLEHEAD = 100;
        public const int Z_MAPUI_MAPPOINTER = 1101; //地图上的主角头像标引
        public const int Z_MAPUI_INFO = 1200;
        public const int Z_MAPUI_DIALOG_COVER = 1299;
        public const int Z_MAPUI_DIALOG = 1300;
        public const int Z_MAPUI_DIALOG_INDICATOR = 1400;

        public const int MAPUI_ROLEHEAD_X = 20;
        public const int MAPUI_ROLEHEAD_Y = 100;
        public const int MAPUI_ROLEHEAD_WIDTH = 60;
        public const int MAPUI_ROLEHEAD_HEIGHT = 60;
        public const int MAPUI_ROLEHEAD_GAP = 20;
        #endregion

        #region DELEGATE
        public delegate void VoidCallBack();
        public delegate void IntCallBack(int rst);
        public delegate void ItemCallBack(Dictionary<string,int> items, int point);
        public delegate void StringCallBack(string rst);
        public delegate void ObjectCallBack(object obj);
        #endregion

        #region 数字转换
        public static String[] chineseNumber = new String[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十", "二十一", "二十二", "二十三", "二十四", "二十五", "二十六", "二十七", "二十八", "二十九", "三十", "三十一" };

        public static char[] chineseTime = new char[] { '子', '丑', '寅', '卯', '辰', '巳', '午', '未', '申', '酉', '戌', '亥' };
        public static double[] timeOpacity = new double[] { 0.4, 0.4,  0.5, 0.5,  0.6, 0.7,  1, 1 , 1   , 0.8,  0.6 ,  0.4};

        public static bool IsChineseTime(System.DateTime t, char time)
        {
            return chineseTime[(int)(t.Hour/2)] == time;
        }
        #endregion

        #region BAN WORDS
        static private string[] BanWords = new string[] { "`", "'", "\"", "#", "阴道", "强奸", "鸡巴", "妓女", "一夜情", "SM", "裸", "广告", "操", "做爱", "政府", "共产党", "湿润", "小穴", "乳", "强暴", "性", "xing", "zuo", "外挂", "作弊", "修改", "激情", "交友", "cao", "打炮", "干炮", "淫", "口交", "361982762", "JB", "我草", "个逼", "草你", "狗b", "狗B", "日b", "日B", "法轮", "sb", "SB", "血逼", "血B", "约炮", "吗的", "吗B", "妈B", "婊", "法轮" };

        static public bool IsBanWord(string content)
        {
            foreach(var ban in BanWords)
            {
                if (content.Contains(ban))
                    return true;
            }
            return false;
        }
        #endregion

        public const string Key = "zbl06"; //加密密钥的另一部分
        public static int AI_WAITTIME //AI控制的NPC移动后等待的时间
        {
            get
            {
                switch(Configer.Instance.AnimationSpeed)
                {
                    case SkillAnimationSpeed.FAST: return 80;
                    case SkillAnimationSpeed.NORMAL: return 200;
                    case SkillAnimationSpeed.SLOW: return 300;
                    default: return 300;
                }
            }
        }
        public static int MOVE_WAITTIME //战斗移动等待时间
        {
            get
            {
                switch (Configer.Instance.AnimationSpeed)
                {
                    case SkillAnimationSpeed.FAST: return 50;
                    case SkillAnimationSpeed.NORMAL: return 120;
                    case SkillAnimationSpeed.SLOW: return 120;
                    default: return 120;
                }
            }
        }
        public static int SPRITE_SWITCH_WAITTIME //角色动画切换等待时间
        {
            get
            {
                switch (Configer.Instance.AnimationSpeed)
                {
                    case SkillAnimationSpeed.FAST: return 50;
                    case SkillAnimationSpeed.NORMAL: return 100;
                    case SkillAnimationSpeed.SLOW: return 120;
                    default: return 100;
                }
            }
        }
        public const int SP_COMPUTE_DURATION = 10; //sp集气的计算间隔
        public const int BUFF_RUN_CYCLE = 50;
        public const int SCENARIO_FINISHCHECKTIME = 500; //当前场景/关卡是否完成的检查间隔时间
        public const int FIELD_LOADTIME = 10; //场景载入载出每帧时间
        public const int OL_LOAD_SHOWWORD_TIME = 500;//载入联机战斗时文字刷新时间
        public const int DEFAULT_MAX_GAME_TURN = 30; //默认的战斗最大回合数限制
        public const int DEFAULT_MAX_GAME_SPTIME = 3000;
        public const int MAX_SAVE_COUNT = 99999; //最多存档个数

        public const double CrazyModeEnemyHpMpAdd = 1.1; //炼狱难度NPC加成
        public const double HardModeEnemyHpMpAdd = 1.0; //进阶难度NPC加成

        public const double ZHOUMU_ATTACK_ADD = 0.10;
        public const double ZHOUMU_DEFENCE_ADD = 0.08;

        #region 画面滚屏相关
        public const int SCREEN_HEIGHT = 600;
        public const int SCREEN_WIDTH = 800;
        public const int SCROLL_SPEED = 10;
        public const int SCROLL_MOUSE_TICK = 10;
        public const int CAMERA_MOVE_CHECKER = 200;
        public const int SCROLL_MARGIN_X = 100;
        public const int SCROLL_MARGIN_Y = 100;
        #endregion

        #region 角色属性

        public const int MAX_ATTRIBUTE = 150; //最大属性
        public const int SMALLGAME_MAX_ATTRIBUTE = 70; //小游戏能够学到的最大属性
        public const int MAX_INTERNALSKILL_COUNT = 5; //最大内功数量
        public const int MAX_SKILL_COUNT = 10; //最大外功数量

        /// <summary>
        /// 属性列表
        /// </summary>
        static public string[] RoleAttributeList = new string[] {
            "hp",
            "maxhp",
            "mp",
            "maxmp",
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
            "female",
        };

        static public string[] RoleAttributeChineseList = new string[] {
            "生命",
            "生命上限",
            "内力",
            "内力上限",
            "根骨",
            "臂力",
            "福缘",
            "身法",
            "定力",
            "悟性",
            "搏击格斗",
            "使剑技巧",
            "耍刀技巧",
            "奇门兵器",
            "是否女性"
        };

        static public string ChineseToAttribute(string chineseAttribute)
        {
            for (int i = 0; i < RoleAttributeChineseList.Length; ++i)
            {
                if (RoleAttributeChineseList[i] == chineseAttribute)
                {
                    return RoleAttributeList[i];
                }
            }
            return null;
        }

        static public string AttributeToChinese(string attr)
        {
            for (int i = 0; i < RoleAttributeList.Length; ++i)
            {
                if (RoleAttributeList[i].Equals(attr))
                    return RoleAttributeChineseList[i];
            }
            throw new Exception("invalid attribute " + attr);
            return null;
        }

        static public string AttributeDesc(string attr)
        {
            for (int i = 0; i < RoleAttributeList.Length; ++i)
            {
                if (RoleAttributeList[i].Equals(attr))
                    return RoleAttributeDesc[i];
            }
            for (int i = 0; i < RoleAttributeChineseList.Length; ++i)
            {
                if (RoleAttributeChineseList[i].Equals(attr))
                    return RoleAttributeDesc[i];
            }
            throw new Exception("invalid attribute " + attr);
            return null;
        }

        static public string[] RoleAttributeDesc = new string[] {
            "",
            "",
            "",
            "",
            "根骨关系到角色的内功掌握程度，以及内功绝技的施展能力。并且一定程度影响角色防御力。",
            "臂力直接影响角色的攻击力",
            "福缘影响角色的暴击概率，集气概率，获取BUFF的能力",
            "身法影响角色的行动力，防御力",
            "定力影响角色的防御力，集气概率，抗不良状态能力",
            "悟性影响角色学习武学的速度",
            "搏击格斗是角色的拳、掌、指、爪类攻击的能力",
            "使剑技巧是角色使剑的能力，影响剑类武学的攻击判定",
            "耍刀技巧是角色耍刀的能力，影响刀类武学的攻击判定",
            "奇门兵器是角色各种奇门武器的掌控的能力，如棍、棒、鞭、枪、钩等",
        };

        static public string[] EnemyRandomTalentsList = new string[]{
            "飘然",
            "斗魂",
            "哀歌",
            "奋战",
            "百足之虫",
            "真气护体",
            "暴躁",
            "金钟罩",
            "诸般封印",
            "刀封印",
            "剑封印",
            "奇门封印",
            "拳掌封印",
            "自我主义",
            "大小姐",
            "破甲",
            "好色",
            "瘸子",
            "白内障",
            "左手剑",
            "右臂有伤",
            "拳掌增益",
            "剑法增益",
            "刀法增益",
            "奇门增益",
            "锐眼"
        };

        static public string[] EnemyRandomTalentListCrazyDefence = new string[]{
            "百足之虫",
            "真气护体",
            "金钟罩",
            "苦命儿",
            "老江湖",
            "暴躁",
            "灵心慧质",
            "精打细算",
            "白内障",
            "右臂有伤",
            "神经病",
            "鲁莽",
        };

        static public string[] EnemyRandomTalentListCrazyAttack = new string[]{
            "斗魂",
            "奋战",
            "暴躁",
            "自我主义",
            "破甲",
            "铁拳无双",
            "素心神剑",
            "左右互搏",
            "博览群书",
            "阴谋家",
            "琴胆剑心",
            "追魂",
            "铁口直断",
            "左手剑",
            "拳掌增益",
            "剑法增益",
            "刀法增益",
            "奇门增益",
            "锐眼"
        };

        static public string[] EnemyRandomTalentListCrazyOther = new string[]{
            "刀封印",
            "剑封印",
            "奇门封印",
            "拳掌封印",
            "清心",
            "哀歌",
            "幽居",
            "金刚",
            "嗜血狂魔",
            "清风",
            "御风",
            "轻功高超",
            "瘸子",
        };

        static public string GetEnemyRandomTalent(bool female)
        {
            string rst = "";
            string[] list;
            list = EnemyRandomTalentsList;
            while (true)
            {
                int index = Tools.GetRandomInt(0, list.Length);
                rst = list[index % list.Length];
                if (female && rst == "好色")
                {
                    continue;
                }
                if (!female && rst == "大小姐")
                {
                    continue;
                }
                break;
            }
            return rst;
        }

        static public string GetEnemyRandomTalentListCrazyDefence()
        {
            int index = Tools.GetRandomInt(0, EnemyRandomTalentListCrazyDefence.Length);
            return EnemyRandomTalentListCrazyDefence[index % EnemyRandomTalentListCrazyDefence.Length];
        }

        static public string GetEnemyRandomTalentListCrazyAttack()
        {
            int index = Tools.GetRandomInt(0, EnemyRandomTalentListCrazyAttack.Length);
            return EnemyRandomTalentListCrazyAttack[index % EnemyRandomTalentListCrazyAttack.Length];
        }

        static public string GetEnemyRandomTalentListCrazyOther()
        {
            int index = Tools.GetRandomInt(0, EnemyRandomTalentListCrazyOther.Length);
            return EnemyRandomTalentListCrazyOther[index % EnemyRandomTalentListCrazyOther.Length];
        }

        static public List<string> EnemyRandomWeaponsItem = new List<string>();

        static public List<string> EnemyRandomDefenceItem = new List<string>();

        static public List<string> EnemyRandomSpecialItem = new List<string>();

        static private bool _initedEnemyRandomItems = false; 
        static private void InitEnemyRandomItems()
        {
            foreach(var item in ItemManager.Items)
            {
                if (!item.IsDrop) continue;
                if(item.Type == (int)ItemType.Weapon)
                {
                    EnemyRandomWeaponsItem.Add(item.Name);
                }else if(item.Type == (int)ItemType.Armor)
                {
                    EnemyRandomDefenceItem.Add(item.Name);
                }else if(item.Type == (int)ItemType.Accessories)
                {
                    EnemyRandomSpecialItem.Add(item.Name);
                }
            }
        }

        static public string GetEnemyRandomWeaponItem()
        {
            if (!_initedEnemyRandomItems)
                InitEnemyRandomItems();
            int index = Tools.GetRandomInt(0, EnemyRandomWeaponsItem.Count);
            return EnemyRandomWeaponsItem[index % EnemyRandomWeaponsItem.Count];
        }

        static public string GetEnemyRandomDefenceItem()
        {
            if (!_initedEnemyRandomItems)
                InitEnemyRandomItems();
            int index = Tools.GetRandomInt(0, EnemyRandomDefenceItem.Count);
            return EnemyRandomDefenceItem[index % EnemyRandomDefenceItem.Count];
        }

        static public string GetEnemyRandomSpecialItem()
        {
            if (!_initedEnemyRandomItems)
                InitEnemyRandomItems();
            int index = Tools.GetRandomInt(0, EnemyRandomSpecialItem.Count);
            return EnemyRandomSpecialItem[index % EnemyRandomSpecialItem.Count];
        }

        #endregion

        #region 升级经验

        public const int MAX_LEVEL = 30;

        public static int LevelupExp(int level)
        {
            if (level <= 0) return 0;
            return (int)(level * 20 + 1.1 * LevelupExp(level - 1)); //递归计算升级经验
        }
        #endregion

        #region 技能相关

        //<!-- type 0拳掌 1剑法 2刀法 3奇门 4内功 -->
        public const int MAX_SKILL_LEVEL = 20;
        public const int MAX_INTERNALSKILL_LEVEL = 20;

        public const int SKILLTYPE_QUAN = 0;
        public const int SKILLTYPE_JIAN = 1;
        public const int SKILLTYPE_DAO = 2;
        public const int SKILLTYPE_QIMEN = 3;
        public const int SKILLTYPE_NEIGONG = 4;
        private static string[] SkillAttributeMap = new string[] { "quanzhang", "jianfa", "daofa", "qimen" };
        public static string SkillTypeToString(int type)
        {
            return SkillAttributeMap[type];
        }

        public static string GetCoverTypeInfo(SkillCoverType type)
        {
            return new SkillCoverTypeHelper(type).CoverTypeInfo;
        }

        public static void SetSourceCastInfo(string[] infos, AttackResult result, double property = 1)
        {
            result.sourceCastInfo = infos[Tools.GetRandomInt(0, infos.Length) % infos.Length];
            result.sourceCastProperty = property;
        }
        public static void SetTargetCastInfo(string[] infos, AttackResult result, double property = 1)
        {
            result.targetCastInfo = infos[Tools.GetRandomInt(0, infos.Length) % infos.Length];
            result.targetCastProperty = property;
        }

        public static AttackResult GetAttackResult(
            Spirit sourceSpirit, Spirit targetSpirit, SkillBox skill)
        {
            Role source = sourceSpirit.Role;
            Role target = targetSpirit.Role;
            //source.RefreshItemTriggerCache();
            //target.RefreshItemTriggerCache();
            AttackResult result = new AttackResult();

            //先处理各种特殊攻击
            if (skill.IsSpecial)
            {
                result.Critical = true;
                foreach (var b in skill.Buffs)
                {
                    if (b.IsDebuff)
                    {
                        double property = 0;
                        if (b.Property == -1)
                        {
                            property = 2 - ((double)target.AttributesFinal["dingli"] / 100f) * 0.5;
                        }
                        else
                        {
                            property = b.Property / 100;
                        }

                        if (Tools.ProbabilityTest(property))
                        {
                            result.Debuff.Add(b);
                        }
                    }
                    else
                    {
                        double property = 0;
                        if (b.Property == -1)
                        {
                            property = 2 - 0.2 + ((double)target.AttributesFinal["fuyuan"] / 100f) * 0.5;
                        }
                        else
                        {
                            property = b.Property / 100;
                        }

                        if (Tools.ProbabilityTest(property))
                        {
                            result.Buff.Add(b);
                        }
                    }
                }

                if (skill.Name == "华佗再世")
                {
                    SetSourceCastInfo(new string[]{"救死扶伤，医者本分也。"}, result);
                    result.Hp = - (targetSpirit.Role.Attributes["maxhp"] - targetSpirit.Role.Attributes["hp"]);
                    return result;
                }
                if (skill.Name == "解毒")
                {
                    SetSourceCastInfo(new string[] { "百毒不侵！", "这都是小case" }, result);
                    result.Hp = 0;
                    return result;
                }
                if (skill.Name == "闪电貂")
                {
                    SetSourceCastInfo(new string[] { "貂儿，上！", "大坏人呀！" }, result);
                    result.Hp = 0;
                    return result;
                }
                if (skill.Name == "一刀两断")
                {
                    SetSourceCastInfo(new string[] { "啊！！！！斩！" }, result);
                    if (Tools.ProbabilityTest(0.5))
                    {
                        result.Hp = (int)((float)targetSpirit.Role.Attributes["hp"] / 2.0f);
                        return result;
                    }
                    else
                    {
                        result.Hp = 0;
                        return result;
                    }
                }
                if (skill.Name == "沉鱼落雁")
                {
                    SetSourceCastInfo(new string[] { "我...美么？" }, result);
                    result.Hp = 0; result.costBall = targetSpirit.Role.Balls;
                    return result;
                }
                if (skill.Name == "溜须拍马")
                {
                    SetSourceCastInfo(new string[] { "各位好汉英明神武，鸟生鱼汤~" }, result);
                    result.Hp = 0;
                    return result;
                }
                if (skill.Name == "Power Up!")
                {
                    SetSourceCastInfo(new string[] { 
                        "啊~~~我的左手正在熊熊燃烧！", 
                        "爆发吧，小宇宙!" 
                    }, result);
                    result.Hp = 0; result.costBall = -2;
                    return result;
                }
                if (skill.Name == "诗酒飘零")
                {
                    SetSourceCastInfo(new string[] { "美酒过后诗百篇，醉卧长安梦不觉" }, result);
                    result.Hp = 0; result.costBall = 0;
                    return result;
                }
                if (skill.Name == "凌波微步")
                {
                    SetSourceCastInfo(new string[] { "凌波微步，罗袜生尘..." }, result);
                    result.Hp = 0;
                    return result;
                }
                if (skill.Name == "襄儿的心愿")
                {
                    SetSourceCastInfo(new string[] { "神雕大侠！襄儿在呼唤你！" }, result);
                    result.Hp = Tools.GetRandomInt(1000, 2000 + 100 * source.Level);
                    return result;
                }
                if (skill.Name == "火枪")
                {
                    SetSourceCastInfo(new string[] { "BIU BIU BIU!", "让你瞧瞧红毛鬼子的火器!" }, result);
                    result.Hp = Tools.GetRandomInt(200 + 20 * source.Level, 200 + 40 * source.Level);
                    return result;
                }
                if (skill.Name == "撒石灰")
                {
                    SetSourceCastInfo(new string[] { "看我的石灰粉！", "让你瞧瞧红毛鬼子的火器!" }, result);
                    return result;
                }
                if (skill.Name == "雪遁步行")
                {
                    SetSourceCastInfo(new string[] { "看我雪遁步行！", "血刀门也有轻功，不知道吧？哈哈！" }, result);
                    return result;
                }
                if (skill.Name == "武穆兵法")
                {
                    SetSourceCastInfo(new string[] { "将在谋，不在勇，吾万人敌" }, result);
                }
                return result;
            }

            int skillTypeValue = 0;
            switch (skill.Type)
            {
                case CommonSettings.SKILLTYPE_QUAN:
                    skillTypeValue = source.AttributesFinal["quanzhang"];
                    break;
                case CommonSettings.SKILLTYPE_JIAN:
                    skillTypeValue = source.AttributesFinal["jianfa"];
                    break;
                case CommonSettings.SKILLTYPE_DAO:
                    skillTypeValue = source.AttributesFinal["daofa"];
                    break;
                case CommonSettings.SKILLTYPE_QIMEN:
                    skillTypeValue = source.AttributesFinal["qimen"];
                    break;
                case CommonSettings.SKILLTYPE_NEIGONG:
                    skillTypeValue = source.AttributesFinal["gengu"];
                    break;
                default:
                    MessageBox.Show("error, skillType = " + skill.Type);
                    return null;
            }
            //本系武学加成
            skillTypeValue += source.GetSkillAddition(skill.Type);

            if (source.HasTalent("浪子剑客") && skill.Type == CommonSettings.SKILLTYPE_JIAN)
            {
                skillTypeValue = (int)(skillTypeValue * 1.2);
                SetSourceCastInfo(new string[] { 
                       "无招胜有招!",
                       "剑随心动",
                    }, result, 0.2);
            }
            if (source.HasTalent("拳掌增益") && skill.Type == CommonSettings.SKILLTYPE_QUAN)
            {
                skillTypeValue = (int)(skillTypeValue * 1.05);
            }
            if (source.HasTalent("剑法增益") && skill.Type == CommonSettings.SKILLTYPE_JIAN)
            {
                skillTypeValue = (int)(skillTypeValue * 1.05);
            }
            if (source.HasTalent("刀法增益") && skill.Type == CommonSettings.SKILLTYPE_DAO)
            {
                skillTypeValue = (int)(skillTypeValue * 1.05);
            }
            if (source.HasTalent("奇门增益") && skill.Type == CommonSettings.SKILLTYPE_QIMEN)
            {
                skillTypeValue = (int)(skillTypeValue * 1.05);
            }
            if (source.HasTalent("神拳无敌") && skill.Type == CommonSettings.SKILLTYPE_QUAN)
            {
                skillTypeValue = (int)(skillTypeValue * 1.2);
                SetSourceCastInfo(new string[] { 
                       "一双铁拳打天下!",
                       "看谁的拳头更硬！",
                    }, result, 0.2);
            }

            InternalSkillInstance sourceInternal = source.GetEquippedInternalSkill();
            InternalSkillInstance targetInternal = target.GetEquippedInternalSkill();

            //适性因子，调和武功均为高级武功，取内力加成的上限
            double suitFactor = skill.Tiaohe ?
                (Math.Max(sourceInternal.Yin, sourceInternal.Yang) / 100f)
                :
                skill.Suit > 0 ? skill.Suit * sourceInternal.Yang / 100f : 0 +
                skill.Suit < 0 ? -skill.Suit * sourceInternal.Yin / 100f : 0;

            //技能等级修正因子
            //double skillHardFactor = Math.Pow(1.05, (double)skill.HardLevel)/1.5;
            //double skillHardFactor = 0.6;

            //周目修正因子
            double enemyZMAttackFactor = 1.0 + CommonSettings.ZHOUMU_ATTACK_ADD * (RuntimeData.Instance.Round - 1);
            double enemyZMDefenceFactor = 1.0 + CommonSettings.ZHOUMU_DEFENCE_ADD * (RuntimeData.Instance.Round - 1);
            //double friendZMAttackFactor = 1.0 - 0.15 * (RuntimeData.Instance.Round - 1);
            
            //result.Hp = (int)((float)(Power * 15) * (float)(1f + (float)skillTypeValue / 100f));
            //攻击评估下限
            double attackLow = (skill.Power) * (2.0 + skillTypeValue / 120.0)* 2.5 *
                (4.0 + source.AttributesFinal["bili"] / 120.0) * (1 + suitFactor) ;
                

            //攻击评估上限
            double attackUp = (skill.Power) * (2.0 + skillTypeValue / 120.0) * 2.5 *
                (4.0 + source.AttributesFinal["bili"] / 120.0) * (1 + suitFactor) * (1 + sourceInternal.Attack) ;
                

            //暴击概率
            double criticalHit = (source.AttributesFinal["fuyuan"] / 50.0) 
                / 20.0 * (1 + sourceInternal.Critical) * (1 + suitFactor);

            //防御评估
            double defence = 150 + (10 + target.AttributesFinal["dingli"] / 40.0 + target.AttributesFinal["gengu"] / 70.0) *
                8.0 * (1 + targetInternal.Defence);

            //if (sourceSpirit.Team == 1)
            //{
            //    attackLow = attackLow * friendZMAttackFactor;
            //    attackUp = attackUp * friendZMAttackFactor;
            //}
            if (sourceSpirit.Team == 2)
            {
                attackLow = attackLow * enemyZMAttackFactor;
                attackUp = attackUp * enemyZMAttackFactor;
            }
            if (targetSpirit.Team == 2)
            {
                defence = defence * enemyZMDefenceFactor;
            }

            #region 各种天赋的加成

            //可配置的天赋攻防加成 by 子尹
            AttackEffect attackEffect = new AttackEffect(attackUp, attackLow, criticalHit);
            attackEffect = TalentManager.attackEffect(source, target, attackEffect, result);
            attackUp = attackEffect.attackHigh;
            attackLow = attackEffect.attackLow;
            criticalHit = attackEffect.criticalHit;

            defence = TalentManager.defenceEffect(source, target, defence, result);

            //原有天赋书写方案，任务量有点大，懒得改了。by 子尹
            if (source.HasTalent("异世人"))
            {
                double yishiRenRate = 0.3;
                if (source.HasTalent("草头百姓"))
                    yishiRenRate = 0.4;
                if ((float)source.Attributes["hp"] / (float)source.Attributes["maxhp"] <= yishiRenRate)
                {
                    SetSourceCastInfo(new string[] { 
                        "来自异世的威力！",
                        "天外飞仙！",
                    }, result, 0.8);
                    attackLow *= 2.0;
                    attackUp *= 2.0;
                    criticalHit *= 2.0;
                }
            }
            if (target.HasTalent("异世人"))
            {
                double yishiRenRate = 0.3;
                if (source.HasTalent("草头百姓"))
                    yishiRenRate = 0.4;
                if ((float)target.Attributes["hp"] / (float)target.Attributes["maxhp"] <= yishiRenRate)
                {
                    SetTargetCastInfo(new string[] { 
                        "绝不会倒下！ ",
                        "固若金汤！",
                    }, result, 0.8);
                    defence *= 1.5;
                }
            }

            if (target.HasTalent("金刚"))
            {
                SetTargetCastInfo(new string[] { 
                       "金刚不坏很耐打！",
                        "壮哉我！抗住啊！",
                    }, result, 0.4);
                defence *= 1.2;
                defence += 10 * target.Level;
            }
            if (source.HasTalent("混元一气") && skill.Name.Contains("混元掌"))
            {
                criticalHit += 0.25;
                SetSourceCastInfo(new string[] { 
                        "混元一气！",
                        "引气归田",
                        "抱元归一"
                    }, result, 0.8);
            }
            if (target.HasTalent("混元一气") && target.GetEquippedInternalSkill().Skill.Name.Equals("混元功"))
            {
                defence *= 1.5;
                SetTargetCastInfo(new string[] { 
                        "混元一气！",
                        "引气归田",
                        "抱元归一"
                    }, result, 0.8);
            }
            if (source.HasTalent("奋战") && !source.HasTalent("异世人"))
            {
                if ((float)source.Attributes["hp"] / (float)source.Attributes["maxhp"] <= 0.3)
                {
                    SetSourceCastInfo(new string[] { 
                        "杀杀杀！",
                        "跟我来！",
                    }, result, 0.5);
                    attackLow *= 1.5;
                    attackUp *= 1.5;
                    criticalHit *= 1.5;
                }
            }
            if (source.HasTalent("不稳定的六脉神剑") && skill.Name.Contains("六脉神剑"))
            {
                SetSourceCastInfo(new string[] { 
                        "还是不能随心所欲施展…… ",
                        "六脉神剑，给我挣点气呀",
                        "啊呀，对不起！"
                    }, result, 0.2);
                attackLow *= 0.5;
                if (attackLow < 0)
                    attackLow = 0;
                attackUp *= 1.5;
            }
            if (source.HasTalent("好色") && target.Female)
            {
                SetSourceCastInfo(new string[] { 
                        "花姑娘，大大的！",
                        "哟西，花姑娘 ",
                        "美女，我所欲也"
                    }, result, 0.3);
                attackLow *= 1.2;
                attackUp *= 1.2;
            }
            if (source.Female && target.HasTalent("好色"))
            {
                SetSourceCastInfo(new string[] { 
                        "色狼，受死吧！",
                        "讨厌！",
                    }, result, 0.3);
                defence *= 0.8;
            }

            if (source.HasTalent("神雕大侠") && (skill.Name == "玄铁剑法" || skill.Name == "黯然销魂掌"))
            {
                if (skill.Name == "黯然销魂掌")
                {
                    SetSourceCastInfo(new string[] { 
                        "黯然销魂，唯别而已。",
                    }, result);
                }
                if (skill.Name == "玄铁剑法")
                {
                    SetSourceCastInfo(new string[] { 
                        "重剑无锋，大巧不工。",
                    }, result);
                }
                criticalHit += 0.25;
            }
            if (source.HasTalent("雪山飞狐") && skill.Name.Contains("胡家刀法"))
            {
                SetSourceCastInfo(new string[] { 
                        "雪山飞狐！",
                        "飞天狐狸！"
                    }, result);
                criticalHit += 0.5;
            }
            if (source.HasTalent("阴谋家"))
            {
                double rate = (double)target.Attributes["hp"] / (double)target.Attributes["maxhp"];
                attackLow *= (1 + 0.5 * (1 - rate));
                attackUp *= (1 + 0.5 * (1 - rate));
            }
            if (source.HasTalent("孤独求败"))
            {
                if (Tools.ProbabilityTest(0.3))
                {
                    SetSourceCastInfo(new string[] { 
                       "洞悉一切弱点",
                        "你不是我的对手",
                        "我，站在天下武学之巅",
                    }, result, 0.8);
                    defence *= 0.3;
                    criticalHit += 0.25;
                }
            }
            if (source.HasTalent("太极高手") && skill.Name.Contains("太极"))
            {
                SetSourceCastInfo(new string[] { 
                       "以柔克刚！",
                       "左右野马分鬃",
                       "白鹤晾翅",
                       "左揽雀尾",
                    }, result, 0.4);
                criticalHit += 0.25;
            }
            if (source.HasTalent("太极宗师") && skill.Name.Contains("太极"))
            {
                SetSourceCastInfo(new string[] { 
                       "意体相随！",
                        "四两拨千斤！",
                        "以柔克刚！",
                       "左右野马分鬃",
                       "白鹤晾翅",
                       "左揽雀尾",
                    }, result, 0.5);
                attackLow *= 1.20;
                attackUp *= 1.20;
                criticalHit += 0.15;
            }
            if (target.HasTalent("太极宗师") && target.GetEquippedInternalSkill().Skill.Name.Contains("太极"))
            {
                SetTargetCastInfo(new string[] { 
                       "意体相随！",
                        "四两拨千斤！",
                    }, result, 0.4);
                defence *= 1.2;
            }
            if (target.HasTalent("太极宗师") && target.GetEquippedInternalSkill().Skill.Name.Contains("纯阳无极功"))
            {
                SetTargetCastInfo(new string[] { 
                       "我几十年的童子身不是白守的！",
                        "纯阳无极功",
                    }, result, 0.4);
                defence *= 1.2;
            }
            if(target.HasTalent("臭蛤蟆") &&  target.GetEquippedInternalSkill().Skill.Name.Contains("蛤蟆功"))
            {
                SetTargetCastInfo(new string[] { 
                       "呱！！！尝尝我蛤蟆功的厉害。",
                    }, result, 0.3);
                defence *= 1.3;
            }
            if (source.HasTalent("臭蛤蟆"))  
            {
                if (skill.Name.Contains("蛤蟆功"))
                {
                    SetSourceCastInfo(new string[] { 
                       "让你们见识见识蛤蟆功的威力！",
                        "呱！！！尝尝我蛤蟆功的厉害。",
                    }, result, 0.3);
                    attackLow += 400;
                    attackUp += 400;
                }
                else if (source.GetEquippedInternalSkill().Skill.Name.Contains("蛤蟆功"))
                {
                    SetSourceCastInfo(new string[] { 
                       "呱！！",
                    }, result);
                    attackLow += 250;
                    attackUp += 250;
                }
            }
            if (source.HasTalent("猎人") && target.Animal)
            {
                SetSourceCastInfo(new string[] { 
                       "颤抖吧，猎物们！",
                       "我，是打猎的能手！",
                    }, result, 0.8);
                attackLow *= 1.5;
                attackUp *= 1.5;
            }
            if (target.HasTalent("金钟罩"))
            {
                if (Tools.ProbabilityTest(0.25))
                {
                    SetTargetCastInfo(new string[] { 
                       "我扛！",
                        "切换防御姿态！",
                    }, result, 0.4);
                    defence *= 2;
                }
            }
            if (source.HasTalent("阉人") && (skill.Name.Contains("葵花宝典") || skill.Name.Contains("辟邪剑法")) )
            {
                SetSourceCastInfo(new string[] { 
                       "你以为我JJ是白切的？",
                       "嘿嘿嘿嘿……",
                    }, result, 0.6);
                criticalHit = 1;
            }
            if (source.HasTalent("怒不可遏") )
            {
                SetSourceCastInfo(new string[] { 
                       "老子要发飙啦！",
                       "怒火，将会焚烧一切！",
                    }, result, 0.8);

                int ballup = sourceSpirit.Role.Balls;
                attackLow *= (1 + ballup * 0.1);
                attackUp *= (1 + ballup * 0.1);
                 
            }
            if (target.HasTalent("暴躁"))
            {
                criticalHit += 0.1;
            }
            if (source.HasTalent("精打细算"))
            {
                if (Tools.ProbabilityTest(0.25))
                {
                    SetSourceCastInfo(new string[] { 
                       "你漫天要价,我落地还钱",
                       "九出十三归!",
                    }, result, 0.5);
                    criticalHit *= Tools.GetRandom(1, 2.0);
                    attackLow *= Tools.GetRandom(1, 1.5);
                    attackUp *= Tools.GetRandom(1, 1.5);
                }

            }
            if (source.HasTalent("精明"))
            {
                if (Tools.ProbabilityTest(0.25))
                SetSourceCastInfo(new string[] { 
                       "想要骗我不容易",
                       "说好的倍伤呢?",
                    }, result, 0.5);
                criticalHit *= Tools.GetRandom(1, 1.5);
                attackLow *= Tools.GetRandom(1, 1.3);
                attackUp *= Tools.GetRandom(1, 1.3);
            }

            //double debuffProperty = criticalHit * 3 - ((double)target.Attributes["dingli"] / 100f) * 0.5;
            double debuffProperty = (source.AttributesFinal["fuyuan"] / 100) - (target.AttributesFinal["dingli"] / 100);
            if(debuffProperty<0) debuffProperty = 0;

            double tiequanProb = 0.25;
            if (source.HasTalent("神拳无敌"))
                tiequanProb += 0.12;
            if (source.HasTalent("铁拳无双") && Tools.ProbabilityTest(tiequanProb) &&
                skill.Type == CommonSettings.SKILLTYPE_QUAN )
            {
                Buff b = new Buff();
                b.Name = "晕眩";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
                SetSourceCastInfo(new string[] { 
                       "尝尝我的拳头的滋味！",
                        "拳头硬才是硬道理！",
                    }, result, 0.4);
            }
            if (source.HasTalent("追魂") && Tools.ProbabilityTest(debuffProperty))
            {
                SetSourceCastInfo(new string[] { 
                       "夺命追魂！",
                        "把你K到死！",
                    }, result, 0.4);

                BuffInstance buff = null;
                foreach (var s in target.Buffs)
                {
                    if (s.buff.Name == "伤害加深")
                    {
                        buff = s;
                        break;
                    }
                }

                if (buff == null)
                {
                    Buff b = new Buff();
                    b.Name = "伤害加深";
                    b.Level = 1;
                    b.Round = 4;
                    result.Debuff.Add(b);
                }
                else
                {
                    Buff b = new Buff();
                    b.Name = "伤害加深";
                    b.Level = buff.Level + 1 <= 10 ? buff.Level + 1 : 10;
                    b.Round = 4;
                    result.Debuff.Add(b);
                }
            }
            if (source.HasTalent("诸般封印") && Tools.GetRandom(0, 1.0) <= debuffProperty)
            {
                Buff b = new Buff();
                b.Name = "诸般封印";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if (source.HasTalent("剑封印") && Tools.ProbabilityTest(debuffProperty))
            {
                Buff b = new Buff();
                b.Name = "剑封印";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if (source.HasTalent("刀封印") && Tools.ProbabilityTest(debuffProperty))
            {
                Buff b = new Buff();
                b.Name = "刀封印";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if (source.HasTalent("拳掌封印") && Tools.ProbabilityTest(debuffProperty))
            {
                Buff b = new Buff();
                b.Name = "拳掌封印";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if (source.HasTalent("奇门封印") && Tools.ProbabilityTest(debuffProperty))
            {
                Buff b = new Buff();
                b.Name = "奇门封印";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if(source.HasTalent("阴阳") && Tools.ProbabilityTest(source.Level * 0.01))
            {
                Buff b = new Buff();
                b.Name = "麻痹";
                b.Level = 0;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            if(source.HasTalent("寒冰真气") && Tools.ProbabilityTest(source.Level * 0.5))
            {
                Buff b = new Buff();
                b.Name = "麻痹";
                b.Level = 3;
                b.Round = 2;
                result.Debuff.Add(b);
            }
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            if (source.HasTalent("大小姐") || source.HasTalent("自我主义")) //坑队友
            {
                
                float factor = 0.1f;
                if (source.HasTalent("自我主义")) factor = 0.18f;

                int teamateNum = 0;
                foreach (var s in uiHost.battleFieldContainer.field.Spirits)
                {
                    if (s.Team == sourceSpirit.Team)
                    {
                        teamateNum++;
                    }
                }
                if (teamateNum > 10) teamateNum = 10; //设置一个上限，否则太过于变态
                attackLow *= (1 + factor * teamateNum);
                attackUp *= (1 + factor * teamateNum);

                if (source.HasTalent("大小姐"))
                {
                    SetSourceCastInfo(new string[] { 
                       "哼！",
                        "你们，不准欺负我！",
                        "谁让你们欺负我的！",
                    }, result, 0.3);
                }
                else if (source.HasTalent("自我主义"))
                {
                    SetSourceCastInfo(new string[] { 
                       "老子才不管你们的死活！",
                        "哼！唯我独尊。",
                        "我，是世界的主宰！",
                    }, result, 0.3);
                }
            }
            foreach (var s in uiHost.battleFieldContainer.field.Spirits)
            {
                if((s.Role.HasTalent("大小姐")||s.Role.HasTalent("自我主义"))&&
                    (s != sourceSpirit) &&
                    (s.Team == sourceSpirit.Team)) //被队友坑了
                {
                    attackUp *= 0.9;
                    attackLow *= 0.9;
                }
            }
            if (source.HasTalent("破甲")&&Tools.ProbabilityTest(0.3))
            {
                defence -= 30;
                if (defence < 0) defence = 0;
                SetSourceCastInfo(new string[] { 
                       "看我致命一击！（天赋*破甲发动）",
                    }, result, 0.3);
            }
            if (source.HasTalent("芷若的眷念"))
            {
                SetSourceCastInfo(new string[] { 
                       "芷若，看我的！",
                       "芷若，我永不会忘记汉江之遇。",
                    }, result, 0.3);
                attackLow *= 1.1;
                attackUp *= 1.1;
            }
            if (source.HasTalent("臭叫花") && skill.Name.Contains("打狗棒法"))
            {
                attackUp *= 1.2;
                attackLow *= 1.2;
                criticalHit += 0.2;
                SetSourceCastInfo(new string[] { 
                       "叫花子人穷志不穷！",
                       "这年头叫花子也不好当啊。",
                    }, result, 0.3);
            }
            if (source.HasTalent("金蛇郎君") && skill.Name.Contains("金蛇剑法"))
            {
                criticalHit += 0.5;
                SetSourceCastInfo(new string[] { 
                       "金蛇郎君的意志!",
                       "看我的金蛇剑法",
                    }, result, 0.3);
            }
            if (source.HasTalent("金蛇狂舞") && skill.Name.Contains("金蛇剑法"))
            {
                attackUp *= 1.4;
                attackLow *= 1.4;
                SetSourceCastInfo(new string[] { 
                       "金蛇狂舞!",
                    }, result, 0.3);
            }
            if (source.HasTalent("铁骨墨萼") && skill.Name.Contains("连城剑法"))
            {
                attackUp *= 1.4;
                attackLow *= 1.4;
                SetSourceCastInfo(new string[] { 
                       "天花落不尽，处处鸟衔飞","孤鸿海上来，池潢不敢顾","俯听闻惊风，连山若波涛","落日照大旗，马鸣风萧萧"
                    }, result, 0.3);
                criticalHit += 0.2;
            }
            if(target.HasTalent("御风"))
            {
                criticalHit -= 0.3 * (target.Level / 30);
                SetTargetCastInfo(new string[] { 
                       "髣髴兮若轻云之蔽月","飘飖兮若流风之回雪"
                    }, result, 0.3);
            }
            if (source.HasTalent("俗家弟子") && Tools.ProbabilityTest(0.3))
            {
                double rate = 0.8 - (0.005 * source.Level);
                defence *= rate;
                SetSourceCastInfo(new string[] { 
                       "少林美名天下传","内练一口气，外练筋骨皮","看我少林俗家弟子的厉害"
                    }, result, 0.3);
            }
            if (source.HasTalent("北冥真气") && source.GetEquippedInternalSkill().Skill.Name == "北冥神功")
            {
                attackLow *= 1.8;
                attackUp *= 1.8;
            }
            if (target.HasTalent("北冥真气") && target.GetEquippedInternalSkill().Skill.Name == "北冥神功")
            {
                defence *= 2;
            }

            if(source.HasTalent("老江湖"))
            {
                attackUp *= 0.9;
                attackLow *= 1.2;
                if(attackLow > attackUp)
                {
                    double tmp = attackLow;
                    attackLow = attackUp;
                    attackUp = tmp;
                }
            }
            if(source.HasTalent("左手剑"))
            {
                attackUp *= 1.05;
                attackLow *= 1.05;
                criticalHit -= 0.02;
            }
            if(source.HasTalent("右臂有伤"))
            {
                attackUp *= 0.95;
                attackLow *= 0.95;
            }
            if(source.HasTalent("神兵"))
            {
                attackUp *= 1.05;
                attackLow *= 1.05;
            }
            if(source.HasTalent("神经病"))
            {
                attackUp *= 1.1;
            }
            if (source.HasTalent("鲁莽"))
            {
                attackUp *= 1.06;
                attackLow *= 1.06;
            }
            if(target.HasTalent("苦命儿"))
            {
                defence += 30;
            }
            if (source.HasTalent("狗杂种") && Tools.ProbabilityTest(0.3) &&
                (source.GetEquippedInternalSkill().Skill.Name == "太玄神功" || source.GetEquippedInternalSkill().Skill.Name == "罗汉伏魔功"))
            {
                criticalHit += 0.3;
                SetSourceCastInfo(new string[] { 
                       "阿黄叫你回家吃饭啦！",
                       "不许叫我狗杂种！",
                       "狗杂种也能逆袭啊❤"
                    }, result, 1);
            }
            if (target.HasTalent("狗杂种") && (target.GetEquippedInternalSkill().Skill.Name == "太玄神功" || target.GetEquippedInternalSkill().Skill.Name == "罗汉伏魔功"))
            {
                SetTargetCastInfo(new string[] { 
                       "不许叫我狗杂种！",
                       "狗杂种也能逆袭啊❤"
                    }, result, 1);
                defence += 150;
            }
           
            #endregion

            foreach (var b in skill.Buffs)
            {
                if (b.IsDebuff)
                {
                    double property = 0;
                    if (b.Property == -1)
                    {
                        //property = criticalHit * 3 - ((double)target.AttributesFinal["dingli"] / 100f) * 0.5;
                        property = (source.AttributesFinal["fuyuan"] / 100) - (target.AttributesFinal["dingli"] / 100);
                        if (property < 0.1) property = 0.1;
                    }
                    else
                    {
                        property = b.Property / 100;
                    }
                    if (Tools.ProbabilityTest(property))
                    {
                        result.Debuff.Add(b);
                    }
                }
                else
                {

                    double property = 0;
                    if (b.Property == -1)
                    {
                        property = (source.AttributesFinal["fuyuan"] / 300);
                    }
                    else
                    {
                        property = b.Property / 100;
                    }
                    if (Tools.ProbabilityTest(property))
                    {
                        result.Buff.Add(b);
                    }
                }
            }

            double criticalHitFactor = 1.5; //暴击伤害加成

            //装备增益
            foreach (var item in target.Equipment)//防御增益
            {
                if (item != null)
                    item.EquipFilter(false, skill, ref attackLow, ref attackUp, ref criticalHit, ref criticalHitFactor,ref defence);
            }
            foreach (var item in source.Equipment)//攻击增益
            {
                if (item != null)
                    item.EquipFilter(true, skill, ref attackLow, ref attackUp, ref criticalHit, ref criticalHitFactor, ref defence);
            }

            //攻防BUFF & DEBUFF加成
            BuffInstance addAttack = sourceSpirit.Role.GetBuff("攻击强化");
            BuffInstance minusAttack = sourceSpirit.Role.GetBuff("攻击弱化");
            if (addAttack != null)
            {
                attackLow = attackLow * (1 + (double)addAttack.Level / 10.0f);
                attackUp = attackUp * (1 + (double)addAttack.Level / 10.0f);
            }
            if (minusAttack != null)
            {
                attackLow = attackLow * (1 - (double)minusAttack.Level / 10.0f);
                attackUp = attackUp * (1 - (double)minusAttack.Level / 10.0f);
            }

            //伤害加深DEBUFF加成
            BuffInstance bleeding = targetSpirit.Role.GetBuff("伤害加深");
            if (bleeding != null)
            {
                attackLow = attackLow * Math.Pow(1.15, bleeding.Level);
                attackUp = attackUp * Math.Pow(1.15, bleeding.Level);
            }

            //对于普通难度，我方攻击力增加100%
            if (RuntimeData.Instance.GameMode == "normal" && sourceSpirit.Team == 1)
            {
                attackLow = attackLow * 2.0;
                attackUp = attackUp * 2.0;
            }
            if (RuntimeData.Instance.GameMode == "normal" && sourceSpirit.Team != 1) //敌方攻击力减少50%
            {
                attackLow = attackLow * 0.5;
                attackUp = attackUp * 0.5;
            }

            //天赋，防御加强
            BuffInstance defenceBuff = target.GetBuff("防御强化");
            if (defenceBuff != null)
            {
                defence += (defenceBuff.Level + 1) * 20;
            }

            //防御力减伤
            double defenceW = 0.9 - Math.Pow(0.9,(0.02 * (defence + 50)));

            #region 各种攻击类型对攻击的修正
            //刀，增加5%暴击伤害
            if (skill.Type == CommonSettings.SKILLTYPE_DAO)
            {
                criticalHitFactor += 0.1;
            }
            //剑，减少10%暴击伤害
            if (skill.Type == CommonSettings.SKILLTYPE_JIAN)
            {
                criticalHitFactor -= 0.1;
            }
            //拳，增加 5%暴击伤害
            if (skill.Type == CommonSettings.SKILLTYPE_QUAN)
            {
                criticalHitFactor += 0.05;
            }
            //奇门，增加15%暴击概率
            if (skill.Type == CommonSettings.SKILLTYPE_QIMEN)
            {
                criticalHit += 0.15;
            }
            //内功，增加5%暴击伤害
            if (skill.Type == CommonSettings.SKILLTYPE_NEIGONG)
            {
                criticalHitFactor += 0.05;
            }
            #endregion
            bool isCritical = Tools.ProbabilityTest(criticalHit);
            if (source.GetBuff("魔神降临") != null)
            {
                isCritical = true;
                criticalHitFactor += 0.25;
                SetSourceCastInfo(new string[] { 
                       "尝尝魔神的威力！",
                        "嘿嘿嘿嘿嘿！",
                    }, result, 0.5);
            }

            result.Hp = (int)(Tools.GetRandom(attackLow, attackUp) * (isCritical ? criticalHitFactor : 1.0) * (1 - defenceW) );

            
            if (target.HasTalent("乾坤大挪移奥义"))//乾坤大挪移奥义，一定免疫
            {
                result.Hp = (int)(result.Hp * 0.5);
                SetTargetCastInfo(new string[] { 
                       "铜墙铁壁！（乾坤大挪移奥义发动）",
                        "乾坤大挪移奥义式！",
                        "打不疼我（乾坤大挪移奥义发动）",
                    }, result, 0.3);
            }
            else if (target.HasTalent("乾坤大挪移") && Tools.ProbabilityTest(0.5))//乾坤大挪移,50%概率
            {
                result.Hp = (int)(result.Hp * 0.5);
                SetTargetCastInfo(new string[] { 
                       "我挪！",
                        "乾坤大挪移！",
                        "打不疼我",
                    }, result, 0.3);
            }

            //与乾坤同属于减伤害系统的精明和精打细算
            if (target.HasTalent("精打细算") && Tools.ProbabilityTest(0.2))
            {
                if (result.Hp > 200)
                    result.Hp = 200;
                SetTargetCastInfo(new string[]{
                    "嘿嘿，你可伤不到我。"
                }, result, 0.3);
            } 
            else if (target.HasTalent("精明") && Tools.ProbabilityTest(0.25))
            {
                if (result.Hp > 500)
                {
                    result.Mp += (result.Hp - 500);
                    result.Hp = 500;
                }
                SetTargetCastInfo(new string[]{
                    "这一招太狠了，我得躲开一点。"
                }, result, 0.3);
            }
            //溜须拍马与易容
            if (targetSpirit.Team != sourceSpirit.Team && target.GetBuff("溜须拍马") != null)
            {
                result.Hp = 0;
                result.Mp = 0;
                result.Buff.Clear();
                SetTargetCastInfo(new string[] { "好汉饶命啊！ (溜须拍马生效)" }, result);
            }
            if (targetSpirit.Team != sourceSpirit.Team && target.GetBuff("易容") != null)
            {
                result.Hp = 0;
                result.Mp = 0;
                result.Buff.Clear();
                SetTargetCastInfo(new string[] { "改面易容，伺机而动！ (易容生效)" }, result);
            }

            //天赋：黑天死炎
            if (source.HasTalent("黑天死炎") && Tools.ProbabilityTest(0.25))
            {
                //1/4几率转成一刀两断攻击模式，产生一半的一刀两断效果
                result.Hp = Math.Max(result.Hp, (int)((float)targetSpirit.Role.Attributes["hp"] * 0.25f));
                SetSourceCastInfo(new string[] { "黑暗！（天赋*黑天死炎发动）" }, result);
            }

            if (target.HasTalent("宝甲"))
            {
                result.Hp = (int)(result.Hp * 0.95);
            }
            if (target.HasTalent("神经病"))
            {
                result.Hp = (int)(result.Hp * 1.1);
            }
            if (target.HasTalent("鲁莽"))
            {
                result.Hp = (int)(result.Hp * 1.1);
            }
            if (source.HasTalent("攀云乘龙") && Tools.ProbabilityTest(0.5))
            {
                SetSourceCastInfo(new string[] { 
                       "我从月下来，偷走你最爱……",
                       "攀云乘龙，神行百变，千变万劫！",
                    }, result, 0.6);
                int Agile = sourceSpirit.Role.AttributesFinal["shenfa"];
                result.Hp += Tools.GetRandomInt(0, Agile);
            }

            result.Hp = result.Hp <= 0 ? 0 : result.Hp;
            //result.Hp = (int)hp;
            result.Critical = isCritical;

            if (sourceSpirit.Team == targetSpirit.Team) //友军伤害 1/4
            {
                bool noFriendHit = false;
                if (skill.IsAoyi)
                {
                    noFriendHit = true;
                }
                if (sourceSpirit.Role.HasTalent("灵心慧质"))
                {
                    noFriendHit = true;
                }
                if (noFriendHit)
                {
                    result.Hp = 0;
                    result.Debuff.Clear();
                    result.Mp = 0;
                    result.sourceCastInfo = null;
                    result.targetCastInfo = null;
                }
                else
                {
                    if (result.Hp > 0)
                    {
                        result.Hp = (int)(result.Hp / 4);
                    }
                }
            }

            //如果伤害过于判定为致命一击，则有一定比例减成
            if(result.Hp > 0)
            {
                double rate = ((double)result.Hp / (double)target.Attributes["maxhp"]);
                if (rate > 1) rate = 1;
                result.Hp = (int)(result.Hp * (1 - 0.4 * rate));
            }

            return result;
        }

        public static string BuffInfo(List<Buff> buffs)
        {
            string rst = string.Empty;
            foreach (var s in buffs)
            {
                rst += string.Format("\n\t{0}({1}) {2}回合 ", 
                    s.Name, 
                    s.Level, 
                    s.Round
                    );
                if (s.Property == 100) rst += "【必定命中】";
            }
            return rst.TrimEnd();
        }
        /*public static string BuffInfo(List<BuffInstance> buffs)
        {
            string rst = string.Empty;
            foreach (var s in buffs)
            {
                rst += string.Format("{0}:{1} ", s.buff.Name, s.buff.Level);
            }
            return rst.TrimEnd();
        }*/

        #endregion

        #region 难度相关

        #endregion

        #region 团队颜色

        /// <summary>
        /// 团队颜色是战斗中人物名字的颜色
        /// </summary>
        public static Color[] TeamColor = new Color[] { Colors.Cyan, Colors.Cyan, Colors.Magenta, Colors.Yellow };

        #endregion

        #region 精灵绘制
        public const double SpiritScaleRate = 0.75;
        #endregion

        #region URL
        public const string GonglueUrl = "http://www.jy-x.com/?cat=11";
        public const string BBSUrl = "http://tieba.baidu.com/f?kw=%BA%BA%BC%D2%CB%C9%CA%F3&fr=index";

        public const string DonateUrl = "https://me.alipay.com/hanjiasongshu";
        
        #endregion 

        #region AI
        public const int AI_MAX_COMPUTE_SKILL = 4;
        public const int AI_MAX_COMPUTE_MOVERANGE = 6;
        #endregion 

        #region 时间
        static public string DateTimeToGameTime(DateTime t)
        {
            string date = "江湖" + CommonSettings.chineseNumber[t.Year] + "年" + 
                CommonSettings.chineseNumber[t.Month] + "月" + 
                CommonSettings.chineseNumber[t.Day] + "日";
            return date;
        }
        #endregion
    }
}
