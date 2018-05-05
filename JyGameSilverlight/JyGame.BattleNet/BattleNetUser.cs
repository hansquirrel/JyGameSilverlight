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

namespace JyGame.BattleNet
{
    public class BattleNetUser
    {
        /// <summary>
        /// 角色账号
        /// unique
        /// </summary>
        public string Name;

        /// <summary>
        /// 积分
        /// </summary>
        public int Score;

        /// <summary>
        /// 密聊频道
        /// </summary>
        public string Channel
        {
            get
            {
                return "PRIVATE_" + Name;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Name, Score);
        }

        static public BattleNetUser Parse(string content)
        {
            try
            {
                BattleNetUser rst = new BattleNetUser();
                rst.Name = content.Split(new char[] { ',' })[0];
                rst.Score = int.Parse(content.Split(new char[] { ',' })[1]);
                return rst;
            }
            catch
            {
                return null;
            }
            
        }
    }
}
