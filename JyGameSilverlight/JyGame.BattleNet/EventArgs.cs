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
    /// 聊天通知
    /// </summary>
    public class ChatEventArgs : EventArgs
    {
        /// <summary>
        /// 用户
        /// </summary>
        public BattleNetUser User;
        
        /// <summary>
        /// 来自频道
        /// </summary>
        public string Channel;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message;
    }

    public class GameArgs : EventArgs
    {
        public int Id;
        public GameType Type;
        public BattleNetUser User1;
        public BattleNetUser User2;
    }

    public class GameMessageArgs : EventArgs
    {
        public int GameId;
        public BattleNetUser User;
        public string Message;
    }
}
