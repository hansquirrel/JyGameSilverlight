using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace JyGame.GameData
{

    class Danmu
    {
        public int Id = -1;
        public string Info = "";
    }

    public class WudaoOpponent
    {
        public List<Role> Team = new List<Role>();
        public DateTime GameTime = DateTime.MinValue;
        public string Menpai = "";
        public int Rank = 0;
        public string UUID = "";
        public int Round = 0;
        public string Say = "";

        public bool IsCheat
        {
            get
            {
                if (this.Power > 5500) return true;

                foreach(var r in Team)
                {
                    foreach(var item in r.Equipment)
                    {
                        if (item != null && item.IsCheat)
                            return true;
                    }
                }

                return false;
            }
        }

        //战斗力评估
        public int Power
        {
            get
            {
                int rst = 0;
                foreach(var r in Team)
                {
                    rst += (int)(r.Attack * 10);
                    rst += (int)(r.Defence * 5);
                }
                return rst;
            }
        }

        public static WudaoOpponent Parse(XElement node)
        {
            try
            {
                WudaoOpponent rst = new WudaoOpponent();
                rst.Rank = Tools.GetXmlAttributeInt(node, "rank");

                XElement content = node.Element("root");
                rst.GameTime = Tools.GetXmlAttributeDate(content, "time");
                foreach (var rnode in content.Elements("role"))
                {
                    rst.Team.Add(Role.Parse(rnode));
                }

                if (content.Attribute("menpai") != null)
                    rst.Menpai = content.Attribute("menpai").Value;
                if (content.Attribute("round") != null)
                    rst.Round = Tools.GetXmlAttributeInt(content, "round");
                if (content.Attribute("say") != null)
                    rst.Say = Tools.GetXmlAttribute(content, "say");
                rst.UUID = content.Attribute("uuid").Value;
                return rst;
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }
    }

    /// <summary>
    /// 弹幕管理器
    /// </summary>
    public class GameServerManager
    {
        private const string DANMU_URL = "http://www.jy-x.com/0.6/getdanmu.php";
        private const string DANMU_ADD_URL = "http://www.jy-x.com/0.6/adddanmu.php?danmu=";
        private const string TEAM_COMMIT_URL = "http://www.jy-x.com/0.6/addteam_new.php?uuid=";
        private const string TEAM_GET_URL = "http://www.jy-x.com/0.6/getteam_new.php?uuid=";
        private const string BEAT_URL = "http://www.jy-x.com/0.6/beat.php?uuid=";


        static public GameServerManager Instance
        {
            get 
            {
                if (_instance == null)
                    _instance = new GameServerManager();
                return _instance;
            }
        }
        static private GameServerManager _instance = null;
        private GameServerManager() { }

        public void Init()
        {
            this.DanmuInit();
            this.WuDaoDahuiInit();
        }

        #region 弹幕
        private void DanmuInit()
        {
            visitedId = new List<int>();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();

            webClient = new WebClient();
            webClient.OpenReadCompleted += (s, e) =>
            {
                busy = false;
                if (e.Error == null && e.Result != null)
                {
                    try
                    {
                        TextReader pReader = new StreamReader(e.Result);

                        string content = pReader.ReadToEnd();
                        List<Danmu> danmus = ParseDanmu(content);
                        if (danmus == null || danmus.Count == 0)
                            return;
                        foreach (var danmu in danmus)
                        {
                            if (!visitedId.Contains(danmu.Id))
                            {
                                visitedId.Add(danmu.Id);
                                RuntimeData.Instance.gameEngine.uihost.AddDanMu(danmu.Info);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            };
        }

        List<Danmu> ParseDanmu(string content)
        {
            try
            {
                List<Danmu> rst = new List<Danmu>();
                if (string.IsNullOrEmpty(content))
                    return null;
                string[] danmuList = content.Split(new string[] { "#DANMU#" }, StringSplitOptions.None);
                
                foreach (var danmu in danmuList)
                {
                    if (danmu == "") continue;
                    int id = int.Parse(danmu.Split(new string[] { "#SPLIT#" }, StringSplitOptions.None)[0]);
                    string info = danmu.Split(new string[] { "#SPLIT#" }, StringSplitOptions.None)[1];
                    Danmu d = new Danmu() { Id = id, Info = info };
                    rst.Add(d);
                }
                return rst;
            }
            catch {
                return null;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (Configer.Instance.Danmu && !busy)
            {
                busy = true;
                webClient.OpenReadAsync(new Uri(DANMU_URL + "?token=" + Tools.GetRandomInt(0,100000),UriKind.Absolute));
            }
        }

        public void SendDanmu(string info)
        {
            WebClient web = new WebClient();
            web.OpenReadAsync(new Uri(DANMU_ADD_URL + info));
        }

        WebClient webClient = new WebClient();
        bool busy = false;
        DispatcherTimer timer = null;
        private List<int> visitedId = new List<int>();
        #endregion

        #region 武道大会

        private void WuDaoDahuiInit()
        {
            wudaodahuiClient = new WebClient();
            wudaodahuiClient.OpenReadCompleted += (s, e) =>
            {
                wudaoBusy = false;
                if (e.Error == null && e.Result != null)
                {
                    try
                    {
                        TextReader pReader = new StreamReader(e.Result);
                        string content = pReader.ReadToEnd();
                        wudaoCallback(ParseWudaoResult(content));
                    }
                    catch (Exception ex)
                    {
                        wudaoCallback(null);
                    }
                }
            };
        }

        private List<WudaoOpponent> ParseWudaoResult(string content)
        {
            List<WudaoOpponent> rst = new List<WudaoOpponent>();
            XElement rootNode = XElement.Parse(content);
            int rank = Tools.GetXmlAttributeInt(rootNode, "rank");
            RuntimeData.Instance.Rank = rank;

            if (rootNode.Element("save") == null)
            {
                return rst;
            }
            foreach(var node in rootNode.Elements("save"))
            {
                rst.Add(WudaoOpponent.Parse(node));
            }
            return rst;
        }

        public delegate void WudaoDahuiCallback(List<WudaoOpponent> opponents);

        private DateTime LastSendWudaoTime = DateTime.MinValue;
        public void SendWudaodahuiTeam(string uuid, List<Role> roles)
        {
            //每10分钟只发一次
            //if ((DateTime.Now - LastSendWudaoTime).TotalMinutes < 10)
            //    return;
            string gameTime = RuntimeData.Instance.Date.ToString("yyyy-MM-dd HH:mm:ss");
            XElement root = new XElement("root");
            root.SetAttributeValue("uuid", uuid);
            root.SetAttributeValue("time", gameTime);
            root.SetAttributeValue("menpai", RuntimeData.Instance.Menpai);
            root.SetAttributeValue("round", RuntimeData.Instance.Round);
            root.SetAttributeValue("say", RuntimeData.Instance.WudaoSay);
            foreach(var r in roles)
            {
                root.Add(r.GenerateRoleXml());
            }
            WebClient webClient = new WebClient();
            webClient.UploadStringAsync(new Uri(TEAM_COMMIT_URL + uuid + "&time=" + gameTime), root.ToString());
            LastSendWudaoTime = DateTime.Now;
        }

        WebClient wudaodahuiClient = null;
        bool wudaoBusy = false;
        WudaoDahuiCallback wudaoCallback = null;
        public void GetWudaodahuiOpponents(WudaoDahuiCallback callback)
        {
            if (!wudaoBusy)
            {
                string gameTime = RuntimeData.Instance.Date.ToString("yyyy-MM-dd HH:mm:ss");
                wudaoBusy = true;
                wudaoCallback = callback;
                wudaodahuiClient.OpenReadAsync(new Uri(TEAM_GET_URL + RuntimeData.Instance.UUID + "&time=" + gameTime));
            }

            //for test
            //List<WudaoOpponent> rst = new List<WudaoOpponent>();
            //WudaoOpponent opp = new WudaoOpponent() { GameTime = DateTime.MinValue };
            //opp.Team.Add(RoleManager.GetRole("成昆").Clone());
            //rst.Add(opp);

            //opp = new WudaoOpponent() { GameTime = DateTime.MinValue };
            //opp.Team.Add(RoleManager.GetRole("张无忌").Clone());
            //opp.Team.Add(RoleManager.GetRole("成昆").Clone());
            //opp.Team.Add(RoleManager.GetRole("韦一笑").Clone());
            //rst.Add(opp);

            //opp = new WudaoOpponent() { GameTime = DateTime.MinValue };
            //opp.Team.Add(RoleManager.GetRole("谢逊").Clone());
            //opp.Team.Add(RoleManager.GetRole("左冷禅").Clone());
            //rst.Add(opp);

            //DispatcherTimer t = new DispatcherTimer();
            //t.Interval = TimeSpan.FromSeconds(0.5);
            //t.Tick += (s, e) =>
            //{
            //    t.Stop();
            //    t = null;
            //    callback(rst);
            //};
            //t.Start();
        }
        //WebClient wudaoWebClient = new WebClient();

        public void BeatWudaoOpponent(WudaoOpponent opp)
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadAsync(new Uri(BEAT_URL + RuntimeData.Instance.UUID + "&opp=" + opp.UUID));
        }

        #endregion
    }
}
