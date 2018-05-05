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
    public class Manager
    {
        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Manager(string ip, int port)
        {

        }

        #region 大厅接口

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string user, string password)
        {
            return true;
        }

        /// <summary>
        /// 获取在线用户
        /// </summary>
        /// <returns></returns>
        public List<BattleNetUser> GetOnlineUsers()
        {
            return null;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BattleNetUser GetUser(string name)
        {
            return null;
        }

        /// <summary>
        /// 获取战斗日志
        /// </summary>
        /// <param name="user"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<BattleLog> GetBattleLog(string user, DateTime startTime, DateTime endTime)
        {
            return null;
        }

        /// <summary>
        /// 提交聊天信息
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="info">聊天内容</param>
        /// <returns></returns>
        public bool Chat(string channel, string info)
        {
            return true;
        }

        /// <summary>
        /// 加入频道
        /// 
        /// 不在参数中的频道将退出
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public bool JoinChannel(List<string> channels)
        {
            return true;
        }

        /// <summary>
        /// 开始游戏
        /// 
        /// 说明：调用了此函数后，服务器将判断能否开始游戏，会向两个匹配的玩家发送OnNewGameReady的消息
        /// </summary>
        /// <param name="type">游戏类型</param>
        /// <param name="name">对手名（如果是天梯，则不需要填名）</param>
        public void CreateGame(GameType type, string name="")
        {
            return;
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="id">游戏id</param>
        public void LaunchGame(int id)
        {

        }

        /// <summary>
        /// 聊天通知
        /// </summary>
        public event EventHandler<ChatEventArgs> OnNewChatNotify;

        /// <summary>
        /// 游戏准备开始通知
        /// </summary>
        public event EventHandler<GameArgs> OnGameReadyNotify;

        /// <summary>
        /// 游戏开始通知
        /// </summary>
        public event EventHandler<GameArgs> OnGameStartNotify;

        /// <summary>
        /// 自己掉线了/被服务器踢了/断开连接
        /// </summary>
        public event EventHandler<UserDroppedArgs> OnDroppedNotify;

        #endregion

        #region 存档管理
        
        public List<string> GetSaves()
        {
            return null;
        }

        public bool Save(int index, string content)
        {
            return true;
        }

        #endregion

        #region 游戏内容
        //1、编排阵容

        /// <summary>
        /// 开始新局
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="round"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public bool StartRound(int gameId, int round, string roles)
        {
            return true;
        }

        /// <summary>
        /// 新局开始通知
        /// </summary>
        public EventHandler<GameRoundArgs> OnNewRoundNotify;

        //2、战斗LOOP

        /// <summary>
        /// 提交action
        /// </summary>
        /// <param name="round"></param>
        /// <param name="turn"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ComitAction(int round, int turn, string action)
        {
            return true;
        }

        /// <summary>
        /// 新的ACTION到达通知
        /// </summary>
        public EventHandler<GameActionArgs> OnNewActionNotify;

        //3、提交结果

        /// <summary>
        /// 提交战斗结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ComitBattleResult(BattleLog result)
        {
            return true;
        }

        //网络处理

        /// <summary>
        /// 对手掉线了
        /// </summary>
        public EventHandler<UserDroppedArgs> OnOpponentDroppedNotify;
        #endregion

    }
}
