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

namespace JyGame.GameData
{
    /// <summary>
    /// 游戏联机战斗时的战斗交互数据
    /// </summary>
    public class OLBattleData
    {
        #region 人物行动前数据

        public Dictionary<int, Role> preRoles = new Dictionary<int,Role>();
        public Dictionary<int, SpiritInfo> preSpiritInfos = new Dictionary<int,SpiritInfo>();
        public Dictionary<int, string> preRoleWords = new Dictionary<int,string>();
        public Dictionary<int, List<BattleWord>> preRoleAttackInfo = new Dictionary<int,List<BattleWord>>();

        public void displayPreBattleEffect(UIHost uihost)
        {
            BattleField field = uihost.battleFieldContainer.field;

            //文字效果
            foreach (Spirit sp in field.Spirits)
            {
                if (preRoleWords.ContainsKey(sp.battleID))
                {
                    field.ShowSmallDialogBox(sp, preRoleWords[sp.battleID], 1.0);
                }

                if (preRoleAttackInfo.ContainsKey(sp.battleID))
                {
                    foreach (BattleWord battleWord in preRoleAttackInfo[sp.battleID])
                    {
                        sp.AddAttackInfo(battleWord.word, battleWord.color);
                    }
                }
            }

            //人物属性
            foreach (Spirit sp in field.Spirits)
            {
                if (preRoles.ContainsKey(sp.battleID))
                {
                    sp.Role = preRoles[sp.battleID];
                }

                if (preSpiritInfos.ContainsKey(sp.battleID))
                {
                    SpiritInfo info = preSpiritInfos[sp.battleID];
                    sp.X = info.X;
                    sp.Y = info.Y;
                    sp.FaceRight = info.faceright==0?false:true;
                    sp.ItemCd = info.itemCD;
                }

                sp.Refresh();
            }

        }
        
        #endregion

        #region 人物行动数据

        public int x = 0, y = 0;
        public bool faceright = true;
        public SkillInfo currentSkill = new SkillInfo();

        public void displayMove(UIHost uihost)
        {
            //uihost.battleFieldContainer.field.BlockUnselective();
            //uihost.battleFieldContainer.field.Status = BattleStatus.Moving;
            uihost.battleFieldContainer.field.RoleMoveTo(x, y, true);

            //Spirit currentSpirit = uihost.battleFieldContainer.field.currentSpirit;
            //currentSpirit.X = x;
            //currentSpirit.Y = y;
            //currentSpirit.FaceRight = faceright;
        }

        public void displaySkill(UIHost uihost, CommonSettings.VoidCallBack callback)
        {
            if (OLBattleGlobalSetting.Instance.battleData.currentSkill.actionType == "SKILL")
                uihost.battleFieldContainer.field.ShowOLSkillAnimation(callback);
            else
                callback();
        }

        #endregion

        #region 战斗对所有对象造成的影响

        public Dictionary<int, Role> roles = new Dictionary<int,Role>();
        public Dictionary<int, SpiritInfo> spiritInfos = new Dictionary<int,SpiritInfo>();
        public Dictionary<int, string> roleWords = new Dictionary<int,string>();
        public Dictionary<int, List<BattleWord>> roleAttackInfo = new Dictionary<int,List<BattleWord>>();
        public List<int> die = new List<int>();

        public void displayBattleEffect(UIHost uihost)
        {
            BattleField field = uihost.battleFieldContainer.field;
            
            //文字效果
            foreach (Spirit sp in field.Spirits)
            {
                if (roleWords.ContainsKey(sp.battleID))
                {
                    field.ShowSmallDialogBox(sp, roleWords[sp.battleID], 1.0);
                }

                if (roleAttackInfo.ContainsKey(sp.battleID))
                {
                    foreach(BattleWord battleWord in roleAttackInfo[sp.battleID])
                    {
                        sp.AddAttackInfo(battleWord.word, battleWord.color); 
                    }
                }
            }

            //人物属性
            foreach (Spirit sp in field.Spirits)
            {
                if (roles.ContainsKey(sp.battleID))
                {
                    sp.Role = roles[sp.battleID];
                }

                if (spiritInfos.ContainsKey(sp.battleID))
                {
                    SpiritInfo info = spiritInfos[sp.battleID];
                    sp.X = info.X;
                    sp.Y = info.Y;
                    sp.FaceRight = info.faceright==0?false:true;
                    sp.ItemCd = info.itemCD;
                }

                sp.Refresh();
            }

            //人物死亡情况
            for (int i = 0; i < die.Count; i++)
            {
                Spirit sp = null;
                foreach (Spirit spirit in uihost.battleFieldContainer.field.Spirits)
                {
                    if (spirit.battleID == die[i])
                    {
                        sp = spirit;
                        break;
                    }
                }

                if (sp != null)
                {
                    sp.Hp = 0;
                    sp.Remove();
                    uihost.battleFieldContainer.field.Spirits.Remove(sp);
                }
            }
        }
        #endregion


        #region 数据传输

        public XElement toXMLData()
        {
            XElement root = new XElement("root");

            //战斗前数据
            XElement preRolesXML = new XElement("preRoles");
            foreach (var roleID in preRoles.Keys)
            {
                XElement preRoleXML = new XElement("preRole");
                preRoleXML.SetAttributeValue("ID", roleID);
                preRoleXML.Add(preRoles[roleID].GenerateRoleXml());
                preRolesXML.Add(preRoleXML);
            }
            XElement preSpiritInfosXML = new XElement("preSpiritInfos");
            foreach (var ID in preSpiritInfos.Keys)
            {
                XElement preSpiritInfoXML = new XElement("preSpiritInfo");
                preSpiritInfoXML.SetAttributeValue("ID", ID);
                preSpiritInfoXML.Add(preSpiritInfos[ID].toXMLData());
                preSpiritInfosXML.Add(preSpiritInfoXML);
            }
            XElement preRoleWordsXML = new XElement("preRoleWords");
            foreach (var ID in preRoleWords.Keys)
            {
                XElement preRoleWordXML = new XElement("preRoleWord");
                preRoleWordXML.SetAttributeValue("ID", ID);
                preRoleWordXML.SetAttributeValue("word", preRoleWords[ID]);
                preRoleWordsXML.Add(preRoleWordXML);
            }
            XElement preRoleAttackInfosXML = new XElement("preRoleAttackInfos");
            foreach (var ID in preRoleAttackInfo.Keys)
            {
                XElement preRoleAttackInfoXML = new XElement("preRoleAttackInfo");
                preRoleAttackInfoXML.SetAttributeValue("ID", ID);
                XElement battleWordListXML = new XElement("battleWords");
                foreach (BattleWord battleWord in preRoleAttackInfo[ID])
                {
                    battleWordListXML.Add(battleWord.toXMLData());
                }
                preRoleAttackInfoXML.Add(battleWordListXML);
                preRoleAttackInfoXML.Add(preRoleAttackInfoXML);
                preRoleAttackInfosXML.Add(preRoleAttackInfoXML);
            }
            root.Add(preRolesXML);
            root.Add(preSpiritInfosXML);
            root.Add(preRoleWordsXML);
            root.Add(preRoleAttackInfosXML);

            //战斗时数据
            XElement moveInfo = new XElement("moveInfos");
            moveInfo.SetAttributeValue("x", x);
            moveInfo.SetAttributeValue("y", y);
            moveInfo.SetAttributeValue("faceright", (faceright?1:0));
            root.Add(moveInfo);
            root.Add(currentSkill.toXMLData());

            //战斗结果数据
            XElement rolesXML = new XElement("roles");
            foreach (var roleID in roles.Keys)
            {
                XElement roleXML = new XElement("role");
                roleXML.SetAttributeValue("ID", roleID);
                roleXML.Add(roles[roleID].GenerateRoleXml());
                rolesXML.Add(roleXML);
            }
            XElement spiritInfosXML = new XElement("spiritInfos");
            foreach (var ID in spiritInfos.Keys)
            {
                XElement spiritInfoXML = new XElement("spiritInfo");
                spiritInfoXML.SetAttributeValue("ID", ID);
                spiritInfoXML.Add(spiritInfos[ID].toXMLData());
                spiritInfosXML.Add(spiritInfoXML);
            }
            XElement roleWordsXML = new XElement("roleWords");
            foreach (var ID in roleWords.Keys)
            {
                XElement roleWordXML = new XElement("roleWord");
                roleWordXML.SetAttributeValue("ID", ID);
                roleWordXML.SetAttributeValue("word", roleWords[ID]);
                roleWordsXML.Add(roleWordXML);
            }
            XElement roleAttackInfosXML = new XElement("roleAttackInfos");
            foreach (var ID in roleAttackInfo.Keys)
            {
                XElement roleAttackInfoXML = new XElement("roleAttackInfo");
                roleAttackInfoXML.SetAttributeValue("ID", ID);
                XElement battleWordListXML = new XElement("battleWords");
                foreach (BattleWord battleWord in roleAttackInfo[ID])
                {
                    battleWordListXML.Add(battleWord.toXMLData());
                }
                roleAttackInfoXML.Add(battleWordListXML);
                roleAttackInfosXML.Add(roleAttackInfoXML);
            }
            XElement diesXML = new XElement("dies");
            foreach (var ID in die)
            {
                XElement dieXML = new XElement("die");
                dieXML.SetAttributeValue("ID", ID);
                diesXML.Add(dieXML);
            }
            root.Add(rolesXML);
            root.Add(spiritInfosXML);
            root.Add(roleWordsXML);
            root.Add(roleAttackInfosXML);
            root.Add(diesXML);

            return root;
        }

        public void parse(string rootString)
        {
            XElement root = XElement.Parse(rootString);

            //战斗前数据
            preRoles.Clear();
            XElement preRolesXML = Tools.GetXmlElement(root, "preRoles");
            foreach (XElement preRoleXML in Tools.GetXmlElements(preRolesXML, "preRole"))
            {
                int ID = Tools.GetXmlAttributeInt(preRoleXML, "ID");
                Role role = Role.Parse(Tools.GetXmlElement(preRoleXML, "role"));
                preRoles.Add(ID, role);
            }
            preSpiritInfos.Clear();
            XElement preSpiritInfosXML = Tools.GetXmlElement(root, "preSpiritInfos");
            foreach (XElement preSpiritInfoXML in Tools.GetXmlElements(preSpiritInfosXML, "preSpiritInfo"))
            {
                int ID = Tools.GetXmlAttributeInt(preSpiritInfoXML, "ID");
                SpiritInfo spInfo = SpiritInfo.parse(preSpiritInfoXML);
                preSpiritInfos.Add(ID, spInfo);
            }
            preRoleWords.Clear();
            XElement preRoleWordsXML = Tools.GetXmlElement(root, "preRoleWords");
            foreach (XElement preRoleWordXML in Tools.GetXmlElements(preRoleWordsXML, "preRoleWord"))
            {
                int ID = Tools.GetXmlAttributeInt(preRoleWordXML, "ID");
                string word = Tools.GetXmlAttribute(preRoleWordXML, "word");
                preRoleWords.Add(ID, word);
            }
            preRoleAttackInfo.Clear();
            XElement preRoleAttackInfosXML = Tools.GetXmlElement(root, "preRoleAttackInfos");
            foreach (XElement preRoleAttackInfoXML in Tools.GetXmlElements(preRoleAttackInfosXML, "preRoleAttackInfo"))
            {
                int ID = Tools.GetXmlAttributeInt(preRoleAttackInfoXML, "ID");
                List<BattleWord> blist = BattleWord.parse(Tools.GetXmlElement(preRoleAttackInfoXML,"battleWords"));
                preRoleAttackInfo.Add(ID, blist);
            }

            //战斗时数据
            XElement moveInfo = Tools.GetXmlElement(root, "moveInfos");
            x = Tools.GetXmlAttributeInt(moveInfo, "x");
            y = Tools.GetXmlAttributeInt(moveInfo, "y");
            faceright = (Tools.GetXmlAttributeInt(moveInfo, "faceright")==0?false:true);
            currentSkill = SkillInfo.parse(root);

            //战斗结果数据
            roles.Clear();
            XElement rolesXML = Tools.GetXmlElement(root, "roles");
            foreach (XElement roleXML in Tools.GetXmlElements(rolesXML, "role"))
            {
                int ID = Tools.GetXmlAttributeInt(roleXML, "ID");
                Role role = Role.Parse(Tools.GetXmlElement(roleXML, "role"));
                roles.Add(ID, role);
            }
            spiritInfos.Clear();
            XElement spiritInfosXML = Tools.GetXmlElement(root, "spiritInfos");
            foreach (XElement spiritInfoXML in Tools.GetXmlElements(spiritInfosXML, "spiritInfo"))
            {
                int ID = Tools.GetXmlAttributeInt(spiritInfoXML, "ID");
                SpiritInfo spInfo = SpiritInfo.parse(spiritInfoXML);
                spiritInfos.Add(ID, spInfo);
            }
            roleWords.Clear();
            XElement roleWordsXML = Tools.GetXmlElement(root, "roleWords");
            foreach (XElement roleWordXML in Tools.GetXmlElements(roleWordsXML, "roleWord"))
            {
                int ID = Tools.GetXmlAttributeInt(roleWordXML, "ID");
                string word = Tools.GetXmlAttribute(roleWordXML, "word");
                roleWords.Add(ID, word);
            }
            roleAttackInfo.Clear();
            XElement roleAttackInfosXML = Tools.GetXmlElement(root, "roleAttackInfos");
            foreach (XElement roleAttackInfoXML in Tools.GetXmlElements(roleAttackInfosXML, "roleAttackInfo"))
            {
                int ID = Tools.GetXmlAttributeInt(roleAttackInfoXML, "ID");
                List<BattleWord> blist = BattleWord.parse(Tools.GetXmlElement(roleAttackInfoXML, "battleWords"));
                roleAttackInfo.Add(ID, blist);
            }
            die.Clear();
            XElement diesXML = Tools.GetXmlElement(root, "dies");
            foreach (XElement dieXML in Tools.GetXmlElements(diesXML, "die"))
            {
                int ID = Tools.GetXmlAttributeInt(dieXML, "ID");
                die.Add(ID);
            }
        }

        public void clear()
        {
            preRoles.Clear();
            preSpiritInfos.Clear();
            preRoleWords.Clear();
            preRoleAttackInfo.Clear();

            currentSkill.Clear();

            roles.Clear();
            spiritInfos.Clear();
            roleWords.Clear();
            roleAttackInfo.Clear();
            die.Clear();
        }

        #endregion
    }

    public class BattleWord
    {
        public string word = "";
        public Color color = Colors.Orange;

        public XElement toXMLData()
        {
            XElement battleWord = new XElement("BattleWord");
            battleWord.SetAttributeValue("word", word);
            battleWord.SetAttributeValue("colorA", color.A);
            battleWord.SetAttributeValue("colorB", color.B);
            battleWord.SetAttributeValue("colorG", color.G);
            battleWord.SetAttributeValue("colorR", color.R);
            return battleWord;
        }

        public static List<BattleWord> parse(XElement node)
        {
            List<BattleWord> blist = new List<BattleWord>();
            foreach (XElement subnode in Tools.GetXmlElements(node, "BattleWord"))
            {
                BattleWord battleWord = new BattleWord();
                battleWord.word = Tools.GetXmlAttribute(subnode, "word");
                battleWord.color.A = (byte)Tools.GetXmlAttributeInt(subnode, "colorA");
                battleWord.color.B = (byte)Tools.GetXmlAttributeInt(subnode, "colorB");
                battleWord.color.G = (byte)Tools.GetXmlAttributeInt(subnode, "colorG");
                battleWord.color.R = (byte)Tools.GetXmlAttributeInt(subnode, "colorR");
                blist.Add(battleWord);
            }
            return blist;
        }
    };

    public class SpiritInfo
    {
        public int itemCD = 0;
        public int X = 0;
        public int Y = 0;
        public int faceright = 0;

        public XElement toXMLData()
        {
            XElement spritInfo = new XElement("SpiritInfo");
            spritInfo.SetAttributeValue("itemCD", itemCD);
            spritInfo.SetAttributeValue("X", X);
            spritInfo.SetAttributeValue("Y", Y);
            spritInfo.SetAttributeValue("faceright", faceright);
            return spritInfo;
        }

        public static SpiritInfo parse(XElement node)
        {
            XElement subnode = Tools.GetXmlElement(node, "SpiritInfo");
            SpiritInfo info = new SpiritInfo();
            info.X = Tools.GetXmlAttributeInt(subnode, "X");
            info.Y = Tools.GetXmlAttributeInt(subnode, "Y");
            info.itemCD = Tools.GetXmlAttributeInt(subnode, "itemCD");
            info.faceright = Tools.GetXmlAttributeInt(subnode, "faceright");
            return info;
        }
    }

    public class SkillInfo
    {
        public string name = "";
        public int fullScreenAnimation = 0;
        public string fullScreenAnimationName = "";

        public int targetx = 0, targety = 0;

        public string actionType = "REST";
        public string skillAnimationTemplate = "";
        public int skillCoverType = 0;
        public int skillCastSize = 1;
        public string audio = "";
        public Color color = Colors.White;

        public int isAoyi = 0;
        public string aoyiName = "";

        public void Clear()
        {
            name = "";
            fullScreenAnimation = 0;
            fullScreenAnimationName = "";

            actionType = "REST";
            skillAnimationTemplate = "";
            skillCoverType = 0;
            skillCastSize = 1;
            audio = "";
            color = Colors.White;

            isAoyi = 0;
            aoyiName = "";
        }

        public XElement toXMLData()
        {
            XElement skillInfo = new XElement("SkillInfo");
            skillInfo.SetAttributeValue("actionType", actionType);
            skillInfo.SetAttributeValue("name", name);
            skillInfo.SetAttributeValue("fullScreenAnimation", fullScreenAnimation);
            skillInfo.SetAttributeValue("fullScreenAnimationName", fullScreenAnimationName);
            skillInfo.SetAttributeValue("targetx", targetx);
            skillInfo.SetAttributeValue("targety", targety);
            skillInfo.SetAttributeValue("skillAnimationTemplate", skillAnimationTemplate);
            skillInfo.SetAttributeValue("skillCoverType", skillCoverType);
            skillInfo.SetAttributeValue("skillCastSize", skillCastSize);
            skillInfo.SetAttributeValue("audio", audio);
            skillInfo.SetAttributeValue("colorR", color.R);
            skillInfo.SetAttributeValue("colorG", color.G);
            skillInfo.SetAttributeValue("colorB", color.B);
            skillInfo.SetAttributeValue("colorA", color.A);

            skillInfo.SetAttributeValue("isAoyi", isAoyi);
            skillInfo.SetAttributeValue("aoyiName", aoyiName);

            return skillInfo;
        }

        public static SkillInfo parse(XElement node)
        {
            XElement subnode = Tools.GetXmlElement(node, "SkillInfo");
            SkillInfo info = new SkillInfo();
            info.actionType = Tools.GetXmlAttribute(subnode, "actionType");
            info.name = Tools.GetXmlAttribute(subnode, "name");
            info.fullScreenAnimation = Tools.GetXmlAttributeInt(subnode, "fullScreenAnimation");
            info.fullScreenAnimationName = Tools.GetXmlAttribute(subnode, "fullScreenAnimationName");
            info.targetx = Tools.GetXmlAttributeInt(subnode, "targetx");
            info.targety = Tools.GetXmlAttributeInt(subnode, "targety");
            info.skillAnimationTemplate = Tools.GetXmlAttribute(subnode, "skillAnimationTemplate");
            info.skillCoverType = Tools.GetXmlAttributeInt(subnode, "skillCoverType");
            info.skillCastSize = Tools.GetXmlAttributeInt(subnode, "skillCastSize");
            info.audio = Tools.GetXmlAttribute(subnode, "audio");
            info.color.R = (byte)Tools.GetXmlAttributeInt(subnode, "colorR");
            info.color.G = (byte)Tools.GetXmlAttributeInt(subnode, "colorG");
            info.color.B = (byte)Tools.GetXmlAttributeInt(subnode, "colorB");
            info.color.A = (byte)Tools.GetXmlAttributeInt(subnode, "colorA");

            info.isAoyi = Tools.GetXmlAttributeInt(subnode, "isAoyi");
            info.aoyiName = Tools.GetXmlAttribute(subnode, "aoyiName");

            return info;
        }
    }

    public class OLBattleGlobalSetting
    {
        public bool OLGame = false;
        public string channel = "";
        public int myTeamIndex = 1;
        public int winCount = 0;
        public int loseCount = 0;

        public List<Role> battleFriends = new List<Role>();
        public List<Role> battleEnemies = new List<Role>();
        public Battle battle = null;
        public bool enemyOK = false;
        public bool enemyLoadFinish = false;
        public OLBattleData battleData = new OLBattleData();

        #region singleton 
        static public OLBattleGlobalSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OLBattleGlobalSetting();
                return _instance;
            }
        }
        static private OLBattleGlobalSetting _instance = null;
        #endregion

        public void init()
        {
            OLGame = true;
            battleFriends.Clear();
            battleEnemies.Clear();
            myTeamIndex = 1;
            channel = "";
            battle = null;
            enemyOK = false;
            enemyLoadFinish = false;

            battleData = new OLBattleData();
        }

        public void init(string channel)
        {
            OLGame = true;
            battleFriends.Clear();
            battleEnemies.Clear();
            myTeamIndex = 1;
            this.channel = channel;
            battle = null;
            enemyOK = false;
            enemyLoadFinish = false;

            battleData = new OLBattleData();
        }

        public void clear()
        {
            battleFriends.Clear();
            battleEnemies.Clear();
            battle = null;
            enemyOK = false;
            enemyLoadFinish = false;
            battleData = new OLBattleData();
        }

        public void close()
        {
            OLGame = false;
        }
    }

}
