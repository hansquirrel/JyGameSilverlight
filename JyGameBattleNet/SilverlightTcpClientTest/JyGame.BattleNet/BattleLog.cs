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

namespace JyGame.BattleNet
{
    /// <summary>
    /// 战斗记录
    /// </summary>
    public class BattleLog
    {
        public DateTime StartTime;
        public DateTime EndTime;

        /// <summary>
        /// 角色，得分
        /// </summary>
        public Dictionary<BattleNetUser, int> Players;
    }
}
