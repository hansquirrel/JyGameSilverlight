using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using JyGame.GameData;

namespace JyGame
{
	public partial class RollRole : UserControl
	{
        public UIHost uiHost;
		public RollRole()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            AudioManager.PlayMusic(ResourceManager.Get("音乐.室内_世俗"));

            this.rolePanel.HideUI();
            this.rolePanel.Visibility = System.Windows.Visibility.Collapsed;

            this.resetButton.Visibility = System.Windows.Visibility.Collapsed;
            this.okButton.Visibility = System.Windows.Visibility.Collapsed;
            this.returnButton.Visibility = System.Windows.Visibility.Collapsed;
            this.info.Visibility = System.Windows.Visibility.Collapsed;

            this.itemPanel.HideUI();
            this.itemPanel.ArrangeButton.Visibility = System.Windows.Visibility.Collapsed;
            this.itemPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.itemPanel.Callback = ( item ) => { };

            this.headSelectPanel.Visibility = System.Windows.Visibility.Collapsed;
            this.headSelectPanel.Heads = new List<string>()
            {
                "头像.主角","头像.刀客","头像.杨逍","头像.欧阳克","头像.全冠清","头像.李白",
                "头像.归辛树","头像.狄云","头像.独孤求败","头像.张翠山","头像.殷梨亭","头像.莫声谷","头像.陈近南","头像.石中玉",
                "头像.商宝震","头像.尹志平","头像.流浪汉","头像.梁发", "头像.卓一航", "头像.黑煞", "头像.白煞", "头像.烟霞神龙",
                "头像.双手开碑", "头像.流星赶月", "头像.盖七省", "头像.公子1", "头像.老色鬼", "头像.上官云", "头像.石清", "头像.年轻谢逊",
                "头像.侠客2",
            };
            
            List<string> opts = new List<string>();
            opts.Add("继续..");
            uiHost.multiSelectBox.Show("在来到这个世界之前，请允许询问您几个问题", opts, SelectCallback1);
            results.Clear();
        }

        private void SelectCallback1(int rst)
        {
            List<string> opts = new List<string>();
            opts.Add("商人的儿子");
            opts.Add("大草原上长大的孩子");
            opts.Add("名门世家");
            opts.Add("市井流浪的汉子");
            opts.Add("疯子");
            opts.Add("书香门第");

            uiHost.multiSelectBox.Show("你希望你在武侠小说中的出身是", opts, SelectCallback2);
        }

        private void SelectCallback2(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("无尽的财宝");
            opts.Add("永恒的爱情");
            opts.Add("坚强的意志");
            opts.Add("自由");
            opts.Add("至高无上的权力");
            uiHost.multiSelectBox.Show("以下你觉得最宝贵的是", opts, SelectCallback3);
        }

        private void SelectCallback3(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("调戏妇女");
            opts.Add("欺软怕硬");
            opts.Add("偷奸耍滑");
            opts.Add("切JJ练神功");
            opts.Add("有美女不泡");
            uiHost.multiSelectBox.Show("以下你觉得最可恶的行为是", opts, SelectCallback31);
        }

        private void SelectCallback31(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("拳");
            opts.Add("剑");
            opts.Add("刀");
            opts.Add("暗器");
            uiHost.multiSelectBox.Show("你最喜欢的兵刃是", opts, SelectCallback32);
        }

        private void SelectCallback32(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("黄蓉");
            opts.Add("小龙女");
            opts.Add("郭襄");
            opts.Add("梅超风");
            opts.Add("周芷若");
            uiHost.multiSelectBox.Show("以下女性角色你最喜欢的是", opts, SelectCallback33);
        }

        private void SelectCallback33(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("张无忌");
            opts.Add("郭靖");
            opts.Add("杨过");
            opts.Add("令狐冲");
            opts.Add("林平之");
            uiHost.multiSelectBox.Show("以下男性角色你最喜欢的是", opts, SelectCallback4);
        }

        private void SelectCallback4(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("乔峰");
            opts.Add("韦小宝");
            opts.Add("金庸先生");
            opts.Add("东方不败");
            opts.Add("汉家松鼠");
            opts.Add("半瓶神仙醋");
            uiHost.multiSelectBox.Show("以下你觉得最牛逼的人是", opts, SelectCallback5);
        }

        private void SelectCallback5(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            if(RuntimeData.Instance.Round == 1)
                opts.Add("简单（新手推荐）");

            opts.Add("进阶（难度较高）");
            if(RuntimeData.Instance.Round > 1)
                opts.Add("炼狱（极度变态狂选这个..请慎重)");
            uiHost.multiSelectBox.Show("选择你的游戏难度", opts, SelectCallback6);
        }

        private void SelectCallback6(int rst)
        {
            results.Add(rst);
            List<string> opts = new List<string>();
            opts.Add("继续..");
            uiHost.multiSelectBox.Show("请输入你的名字", opts, SelectCallback7);
        }

        private void SelectCallback7(int rst)
        {
            uiHost.textBox.Show("更名", NameTextBoxType.NameBox, "小虾米", () =>
            {
                RuntimeData.Instance.maleName = uiHost.textBox.text.Text;
                RuntimeData.Instance.Team[0].Name = RuntimeData.Instance.maleName;
                uiHost.textBox.Visibility = Visibility.Collapsed;

                SelectCallback8();
            });
            uiHost.textBox.Visibility = Visibility.Visible;
        }

        private void SelectCallback8()
        {
            this.headSelectPanel.Callback = (selectKey) =>
            {
                selectHeadKey = selectKey;

                List<string> opts = new List<string>();
                opts.Add("继续..");
                uiHost.multiSelectBox.Show("欢迎来到金庸群侠传的世界", opts, (rr) =>
                {
                    AudioManager.PlayEffect(ResourceManager.Get("音效.恢复3"));
                    this.rolePanel.Visibility = System.Windows.Visibility.Visible;
                    this.resetButton.Visibility = System.Windows.Visibility.Visible;
                    this.okButton.Visibility = System.Windows.Visibility.Visible;
                    this.returnButton.Visibility = System.Windows.Visibility.Visible;
                    this.info.Visibility = System.Windows.Visibility.Visible;
                    this.itemPanel.Visibility = System.Windows.Visibility.Visible;

                    Reset();
                });
            };
            this.headSelectPanel.Show();
        }

        private void Reset()
        {
            //根据答案生成初始角色和物品
            MakeBeginningCondition();
            //随机调整
            MakeRandomCondition();
            //显示
            ShowBeginningCondition();
        }

        private void ShowBeginningCondition()
        {
            this.rolePanel.Show(makeRole);
            this.itemPanel.Show(makeItems, false, makeMoney);
        }

        List<int> results = new List<int>();

        private Role makeRole;
        private int makeMoney;
        private List<Item> makeItems;
        private string selectHeadKey;

        private void MakeRandomCondition()
        {
            string[] randomAttr = new string[]{
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

            for (int i = 0; i < 3; ++i)
            {
                int rnd = Tools.GetRandomInt(0, randomAttr.Length - 1);
                string attr = randomAttr[rnd];
                makeRole.Attributes[attr]+=10;
            }
            for (int i = 0; i < 10; ++i)
            {
                int rnd = Tools.GetRandomInt(0, randomAttr.Length - 1);
                string attr = randomAttr[rnd];
                makeRole.Attributes[attr]++;
            }
        }
        private void MakeBeginningCondition()
        {
            makeRole = RuntimeData.Instance.Team[0].Clone();
            makeRole.HeadPicPath = selectHeadKey;
            makeMoney = 100;
            makeItems = new List<Item>();
            //makeItems.Add(ItemManager.GetItem("松果"));
            makeItems.Add(ItemManager.GetItem("小还丹"));
            makeItems.Add(ItemManager.GetItem("小还丹"));
            makeItems.Add(ItemManager.GetItem("小还丹"));

            /*
             * opts.Add("商人的儿子");
            opts.Add("大草原上长大的孩子");
            opts.Add("名门世家");
            opts.Add("市井流浪的汉子");
            opts.Add("疯子");
             */

            switch (results[0])
            {
                case 0: //商人的儿子
                    makeMoney += 5000;
                    makeRole.Attributes["bili"] -= 1;
                    makeItems.Add(ItemManager.GetItem("黑玉断续膏"));
                    makeItems.Add(ItemManager.GetItem("九转熊蛇丸"));
                    makeItems.Add(ItemManager.GetItem("金丝道袍"));
                    makeItems.Add(ItemManager.GetItem("金头箍"));
                    makeRole.Animation = "gongzi_jian";
                    break;
                case 1: //大草原上长大的孩子
                    makeRole.Attributes["shenfa"] += 15;
                    makeRole.Attributes["dingli"] -= 2;
                    makeRole.Attributes["quanzhang"] += 15;
                    makeRole.Talent += "#猎人";
                    makeRole.Animation = "shaonian_quan";
                    break;
                case 2: //名门世家
                    makeRole.Attributes["fuyuan"] += 3;
                    makeRole.Attributes["bili"] -= 3;
                    makeRole.Attributes["dingli"] += 2;
                    makeRole.Attributes["wuxing"] += 20;
                    makeRole.Attributes["jianfa"] += 2;
                    makeRole.Attributes["gengu"] += 2;
                    makeItems.Add(ItemManager.GetItem("银手镯"));
                    makeMoney += 100;
                    makeRole.Animation = "jianke_cool2";
                    break;
                case 3: //市井流浪的汉子
                    makeRole.Attributes["fuyuan"] -= 5;
                    makeRole.Attributes["bili"] += 12;
                    makeRole.Attributes["daofa"] += 15;
                    makeRole.Attributes["qimen"] += 12;
                    makeItems.Add(ItemManager.GetItem("草帽"));
                    makeRole.Animation = "daoke_shaonian";
                    makeMoney = 0;
                    break;
                case 4: //疯子
                    makeRole.Attributes["wuxing"] += 35;
                    makeRole.Attributes["dingli"] -= 10;
                    makeRole.Attributes["gengu"] -= 10;
                    makeRole.Animation = "fengzi";
                    makeRole.Talent += "#神经病";
                    break;
                case 5://书香门第
                    makeRole.Attributes["wuxing"] += 20;
                    makeRole.Attributes["bili"] += 1;
                    makeRole.Attributes["shenfa"] -= 10;
                    makeRole.Attributes["gengu"] -= 5;
                    makeRole.Animation = "duanyu";
                    break;
                default:
                    break;
            }

            /*
             * opts.Add("无尽的财宝");
            opts.Add("永恒的爱情");
            opts.Add("坚强的意志");
            opts.Add("自由");
            opts.Add("至高无上的权力");
             */
            switch (results[1])
            {
                case 0: //无尽的财宝
                    makeMoney += 1000;
                    break;
                case 1: //永恒的爱情
                    makeRole.Attributes["fuyuan"] += 15;
                    break;
                case 2: //坚强的意志
                    makeRole.Attributes["dingli"] += 15;
                    break;
                case 3: //自由
                    makeRole.Attributes["shenfa"] += 15;
                    break;
                case 4: //至高无上的权力
                    makeRole.Talent += "#自我主义";
                    break;
                default:
                    break;
            }

            /*
             * opts.Add("调戏妇女");
            opts.Add("欺软怕硬");
            opts.Add("偷奸耍滑");
            opts.Add("切JJ练神功");
            opts.Add("有美女不泡");
             */
            switch (results[2])
            {
                case 0: //调戏妇女
                    makeRole.Attributes["dingli"] += 9;
                    break;
                case 1: //欺软怕硬
                    makeRole.Attributes["gengu"] += 6;
                    makeRole.Attributes["bili"] += 6;
                    break;
                case 2: //偷奸耍滑
                    makeRole.Attributes["wuxing"] += 10;
                    break;
                case 3: //切JJ练神功
                    makeRole.Attributes["gengu"] += 10;
                    break;
                case 4: //有美女不泡
                    makeRole.Talent += "#好色";
                    break;
                default:
                    break;
            }

            //opts.Add("拳");
            //opts.Add("剑");
            //opts.Add("刀");
            //opts.Add("暗器");
            switch (results[3])
            {
                case 0: //拳
                    makeRole.Attributes["quanzhang"] += 10;
                    break;
                case 1: //剑
                    makeRole.Attributes["jianfa"] += 10;
                    break;
                case 2: //刀
                    makeRole.Attributes["daofa"] += 10;
                    break;
                case 3: //暗器
                    makeRole.Attributes["qimen"] += 10;
                    break;
                default:
                    break;
            }

            //opts.Add("黄蓉");
            //opts.Add("小龙女");
            //opts.Add("郭襄");
            //opts.Add("梅超风");
            //opts.Add("周芷若");
            switch (results[4])
            {
                case 0: //黄蓉
                    makeRole.Attributes["wuxing"] += 5;
                    break;
                case 1: //小龙女
                    makeRole.Attributes["dingli"] += 5;
                    break;
                case 2: //郭襄
                    makeRole.Attributes["fuyuan"] += 5;
                    makeRole.Attributes["gengu"] += 5;
                    break;
                case 3: //梅超风
                    makeRole.Attributes["quanzhang"] += 6;
                    makeRole.Attributes["bili"] += 6;
                    break;
                case 4: //周芷若
                    makeRole.Attributes["dingli"] += 10;
                    break;
                default:
                    break;
            }

            //opts.Add("张无忌");
            //opts.Add("郭靖");
            //opts.Add("杨过");
            //opts.Add("令狐冲");
            //opts.Add("林平之");
            switch (results[5])
            {
                case 0: //张无忌
                    makeRole.Attributes["wuxing"] += 5;
                    makeRole.Attributes["gengu"] += 10;
                    break;
                case 1: //郭靖
                    makeRole.Attributes["wuxing"] -= 10;
                    makeRole.Attributes["fuyuan"] += 15;
                    makeRole.Attributes["bili"] += 5;
                    break;
                case 2: //杨过
                    makeRole.Attributes["wuxing"] += 5;
                    makeRole.Attributes["dingli"] += 5;
                    break;
                case 3: //令狐冲
                    makeRole.Attributes["wuxing"] += 10;
                    break;
                case 4: //林平之
                    makeRole.Attributes["jianfa"] += 10;
                    makeRole.Attributes["dingli"] += 10;
                    break;
                default:
                    break;
            }
            /*
             * opts.Add("乔峰");
            opts.Add("韦小宝");
            opts.Add("金庸先生");
            opts.Add("东方不败");
            opts.Add("本游戏的作者");
            opts.Add("半瓶神仙醋");
             */
            switch (results[6])
            {
                case 0: //乔峰
                    makeRole.Attributes["bili"] += 10;
                    makeRole.Attributes["quanzhang"] += 9;
                    break;
                case 1: //韦小宝
                    makeRole.Attributes["fuyuan"] += 30;
                    break;
                case 2: //金庸先生
                    makeRole.Attributes["wuxing"] += 13;
                    makeRole.Attributes["jianfa"] += 5;
                    makeRole.Attributes["daofa"] += 5;
                    makeRole.Attributes["quanzhang"] += 5;
                    makeRole.Attributes["qimen"] += 5;
                    break;
                case 3: //东方不败
                    InternalSkillInstance sintance = new InternalSkillInstance()
                    {
                        Skill = SkillManager.GetInternalSkill("葵花宝典"),
                        Level = 1,
                        Equipped = false,
                        MaxLevel = 4,
                        Owner = makeRole,
                        Exp = 0,
                    };
                    makeRole.InternalSkills.Add(sintance);
                    makeItems.Add(ItemManager.GetItem("葵花宝典残章"));
                    makeItems.Add(ItemManager.GetItem("葵花宝典残章"));
                    break;
                case 4: //本游戏的作者
                    makeRole.Skills[0].MaxLevel = 10; //解锁野球拳10级
                    makeRole.InternalSkills[0].Level = 10; //初始10级基本内功
                    makeRole.InternalSkills[0].MaxLevel = 20; //20级上限
                    makeRole.Attributes["gengu"] += 10;
                    break;
                case 5: //半瓶神仙醋
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    makeItems.Add(ItemManager.GetItem("天王保命丹"));
                    break;
                default:
                    break;
            }

            if (RuntimeData.Instance.Round == 1)
            {
                switch (results[7])
                {
                    case 0:
                        RuntimeData.Instance.GameMode = "normal";
                        RuntimeData.Instance.FriendlyFire = false;
                        break;
                    case 1:
                        RuntimeData.Instance.GameMode = "hard";
                        RuntimeData.Instance.FriendlyFire = true;
                        break;
                    case 2:
                        RuntimeData.Instance.GameMode = "crazy";
                        RuntimeData.Instance.FriendlyFire = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (results[7])
                {
                    case 0:
                        RuntimeData.Instance.GameMode = "hard";
                        RuntimeData.Instance.FriendlyFire = true;
                        break;
                    case 1:
                        RuntimeData.Instance.GameMode = "crazy";
                        RuntimeData.Instance.FriendlyFire = true;
                        break;
                    default:
                        break;
                }
            }

            //根据周目获得一些初始奖励
            //MessageBox.Show(RuntimeData.Instance.Round.ToString());
            List<string> bonusItems = new List<string>();
            List<string> bonusWeapon = new List<string>();
            bonusItems.Clear();
            bonusWeapon.Clear();
            switch (RuntimeData.Instance.Round)
            {
                case 1:
                    if (RuntimeData.Instance.GameMode == "normal")
                    {
                        makeItems.Add(ItemManager.GetItem("新手礼包-大蟠桃"));
                        makeItems.Add(ItemManager.GetItem("新手礼包-大蟠桃"));
                        makeItems.Add(ItemManager.GetItem("新手礼包-大蟠桃"));
                        makeItems.Add(ItemManager.GetItem("新手礼包-大蟠桃"));
                        makeItems.Add(ItemManager.GetItem("新手礼包-大蟠桃"));
                    }
                    break;
                case 2:
                    bonusItems.Add("佛光普照");
                    bonusItems.Add("百变千幻云雾十三式秘籍");
                    bonusItems.Add("反两仪刀法");
                    bonusItems.Add("伏魔杖法");
                    bonusWeapon.Add("灭仙爪");
                    bonusWeapon.Add("倚天剑");
                    bonusWeapon.Add("屠龙刀");
                    bonusWeapon.Add("打狗棒");
                    makeItems.Add(ItemManager.GetItem(bonusItems[Tools.GetRandomInt(0, bonusItems.Count) % bonusItems.Count]));
                    makeItems.Add(ItemManager.GetItem(bonusWeapon[Tools.GetRandomInt(0, bonusWeapon.Count) % bonusWeapon.Count]));
                    break;
                case 3:
                    bonusItems.Add("隔空取物");
                    bonusItems.Add("妙手仁心");
                    bonusItems.Add("飞向天际");
                    bonusItems.Add("血刀");
                    bonusWeapon.Add("仙丽雅的项链");
                    bonusWeapon.Add("李延宗的项链");
                    bonusWeapon.Add("王语嫣的武学概要");
                    bonusWeapon.Add("神木王鼎");
                    makeItems.Add(ItemManager.GetItem(bonusItems[Tools.GetRandomInt(0, bonusItems.Count) % bonusItems.Count]));
                    makeItems.Add(ItemManager.GetItem(bonusWeapon[Tools.GetRandomInt(0, bonusWeapon.Count) % bonusWeapon.Count]));
                    break;
                default: 
                    bonusItems.Add("碎裂的怒吼");
                    bonusItems.Add("沾衣十八跌");
                    bonusItems.Add("灵心慧质");
                    bonusItems.Add("不老长春功法");
                    bonusWeapon.Add("仙丽雅的项链");
                    bonusWeapon.Add("李延宗的项链");
                    bonusWeapon.Add("王语嫣的武学概要");
                    bonusWeapon.Add("神木王鼎");
                    makeItems.Add(ItemManager.GetItem(bonusItems[Tools.GetRandomInt(0, bonusItems.Count) % bonusItems.Count]));
                    makeItems.Add(ItemManager.GetItem(bonusWeapon[Tools.GetRandomInt(0, bonusWeapon.Count) % bonusWeapon.Count]));

                    break;
            }
            //根据通过试炼角色数量调整奖励
            string[] roles = RuntimeData.Instance.TrialRoles.Split(new char[] { '#' });
            int trailNumber = roles.Length;
            List<string> makeTrailBonusItem = new List<string>();
            if (trailNumber < 3)
            {
                
            }
            else if (trailNumber >= 3 && trailNumber < 6)
            {
                makeItems.Add(ItemManager.GetItem("王母蟠桃").Clone());
                makeItems.Add(ItemManager.GetItem("道家仙丹").Clone());
            }
            else if (trailNumber >= 6 && trailNumber < 9)
            {
                makeItems.Add(ItemManager.GetItem("灵心慧质").Clone());
                makeItems.Add(ItemManager.GetItem("妙手仁心").Clone());
            }
            else if (trailNumber >= 9 && trailNumber < 12)
            {
                makeItems.Add(ItemManager.GetItem("素心神剑心得").Clone());
                makeItems.Add(ItemManager.GetItem("太极心得手抄本").Clone());
                makeItems.Add(ItemManager.GetItem("乾坤大挪移心法").Clone());
            }
            else if (trailNumber >= 12 && trailNumber < 15)
            {
                makeItems.Add(ItemManager.GetItem("沾衣十八跌").Clone());
                makeItems.Add(ItemManager.GetItem("易筋经").Clone());
                makeItems.Add(ItemManager.GetItem("厚黑学").Clone());
            }
            else if (trailNumber >= 15 && trailNumber < 20)
            {
                makeItems.Add(ItemManager.GetItem("武穆遗书").Clone());
                makeItems.Add(ItemManager.GetItem("笑傲江湖曲").Clone());
            }
            else if (trailNumber >= 20)
            {
                makeItems.Add(ItemManager.GetItem("真葵花宝典").Clone());
                
            }
          
        }

        private void JumpSelectCallback(int rst)
        {
            RuntimeData.Instance.Money = makeMoney;
            RuntimeData.Instance.Team[0] = makeRole;
            RuntimeData.Instance.Items = makeItems;

            List<string> bonusRole = new List<string>();
            bonusRole.Clear();
            switch(RuntimeData.Instance.Round)
            {
                case 1:
                    break;
                case 2:
                    bonusRole.Add("鲁连荣");
                    bonusRole.Add("冲虚道长");
                    bonusRole.Add("方证大师");
                    bonusRole.Add("灭绝师太");
                    bonusRole.Add("张翠山");
                    bonusRole.Add("宋远桥");
                    bonusRole.Add("韦一笑");
                    bonusRole.Add("仪清");
                    bonusRole.Add("何太冲");
                    bonusRole.Add("哑仆");
                    bonusRole.Add("温方达");
                    bonusRole.Add("温方义");
                    bonusRole.Add("温方山");
                    bonusRole.Add("温方施");
                    bonusRole.Add("温方悟");
                    bonusRole.Add("安小慧");
                    bonusRole.Add("阿九");
                    break;
                case 3:
                    bonusRole.Add("紫衫龙王");
                    bonusRole.Add("白眉鹰王");
                    bonusRole.Add("商剑鸣");
                    bonusRole.Add("杨逍");
                    bonusRole.Add("范遥");
                    bonusRole.Add("霍都");
                    bonusRole.Add("孙不二");
                    bonusRole.Add("龙岛主");
                    bonusRole.Add("木岛主");
                    bonusRole.Add("善勇");
                    break;
                case 4:
                    bonusRole.Add("白自在");
                    bonusRole.Add("向问天");
                    bonusRole.Add("丁春秋");
                    bonusRole.Add("成昆");
                    bonusRole.Add("段延庆");
                    bonusRole.Add("丘处机");                    
                    bonusRole.Add("欧阳锋");
                    break;
                default:
                    bonusRole.Add("任我行");
                    bonusRole.Add("王重阳");
                    bonusRole.Add("林朝英");
                    bonusRole.Add("归辛树");
                    bonusRole.Add("玉真子");
                    bonusRole.Add("慕容博");
                    bonusRole.Add("卓一航");
                    bonusRole.Add("谢逊");
                    break;
            }
            if (bonusRole.Count > 0)
            {
                RuntimeData.Instance.Team.Add(RoleManager.GetRole(bonusRole[Tools.GetRandomInt(0, bonusRole.Count) % bonusRole.Count]).Clone());
            }

            if (rst == 0)
            {
                RuntimeData.Instance.gameEngine.NewGame();
            }
            else
            {
                RuntimeData.Instance.gameEngine.NewGameJump();
            }
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            List<string> opts = new List<string>();
            opts.Add("是");
            opts.Add("否！让我直接开始冒险吧。");

            uiHost.multiSelectBox.Show("观看序章吗？", opts, JumpSelectCallback);
            //JumpSelectCallback(0);

        }

        private void resetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
            Reset();
        }

        private void returnButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            uiHost.mainMenu.Load();

            this.Visibility = System.Windows.Visibility.Collapsed;
        }
	}
}