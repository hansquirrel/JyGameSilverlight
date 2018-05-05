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
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace JyGame.BattleNet
{
    //回调函数
    public delegate void LoginCallback(bool isLogin, bool isTimeout = false);
    public delegate void OnlineUsersCallback(List<BattleNetUser> users, bool isTimeout = false);
    public delegate void GetUserCallback(BattleNetUser user, bool isTimeout = false);
    public delegate void GetBattleLogCallback(List<string> logs, bool isTimeout = false);
    public delegate void JoinChannelCallback(bool isSuccess, bool isTimeout = false);
    public delegate void GetSavesCallback(List<string> saves, bool isTimeout = false);
    public delegate void SaveCallback(bool isSuccess, bool isTimeout = false);
    public delegate void ConfirmCallback(bool isSuccess, bool isTimeout = false);
    public delegate void PlayWithCallback(bool isConfirm, GameArgs args, bool isTimeout= false);
    public delegate void MessageCallback(string message);
    public class BattleNetManager
    {
        #region singleton with init
        static public BattleNetManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BattleNetManager();
                return _instance;
            }
        }
        static private BattleNetManager _instance = null;
        #endregion

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SetConnectArgs(string ip, int port, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _ip = ip;
            _port = port;
        }

        delegate void OnConnectedCallback(bool isConnected);
        OnConnectedCallback onConnectCallback;
        private void Connect(OnConnectedCallback callback)
        {
            onConnectCallback = callback;
            if (socket != null && socket.Connected)
            {
                return;
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveBufferSize = SOCKET_BUFFER_SIZE;
            socket.SendBufferSize = SOCKET_BUFFER_SIZE;
            connectArgs = new SocketAsyncEventArgs();
            recvArgs = new SocketAsyncEventArgs();
            sendArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = new DnsEndPoint(_ip, _port);
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketArgs_Completed);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(sendArgs_Completed);
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(recvArgs_Completed);
            socket.ConnectAsync(connectArgs);
        }

        const int SOCKET_BUFFER_SIZE = 1024 * 1024 * 2;
        const string MESSAGE_SPLIT_TAG = "#TAG#";
        const string MESSAGE_END_TAG = "#ENDTAG#";
        private string _ip;
        private int _port;

        private string _user;
        private string _password;
        private Dispatcher _dispatcher;
        Socket socket = null;
        SocketAsyncEventArgs connectArgs = null;
        SocketAsyncEventArgs recvArgs = null;
        SocketAsyncEventArgs sendArgs = null;

        void recvArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                string RecevieStr = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length).Replace("\0", "");
                //处理接收的数据
                this.AddToRecvBuffer(RecevieStr);

                if (socket != null && socket.Connected)
                {
                    recvArgs.SetBuffer(new byte[SOCKET_BUFFER_SIZE], 0, SOCKET_BUFFER_SIZE);
                    socket.ReceiveAsync(recvArgs);
                }
            }
            if (e.SocketError != SocketError.Success)
            {
                _dispatcher.BeginInvoke(() =>
                {
                    this.OnDroppedNotify(this, e);
                });
            }
        }

        void sendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
            }
        }

        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect && socket.Connected)
            {
                recvArgs.SetBuffer(new byte[SOCKET_BUFFER_SIZE], 0, SOCKET_BUFFER_SIZE);
                socket.ReceiveAsync(recvArgs);
                if (onConnectCallback != null)
                {
                    onConnectCallback(socket.Connected);
                }
            }
            if (e.SocketError != SocketError.Success)
            {
                onConnectCallback(false);
            }
        }

        private void Send(string msg)
        {
            if (socket != null)
            {
                byte[] userbytes = Encoding.UTF8.GetBytes(msg);
                sendArgs.SetBuffer(userbytes, 0, userbytes.Length);
                socket.SendAsync(sendArgs);
            }
        }

        private void SendMessage(string MessageHead, params object[] args)
        {
            string msg = string.Format("{0}", MessageHead);
            foreach (var o in args)
            {
                msg += string.Format("{0}{1}", MESSAGE_SPLIT_TAG, o.ToString().Replace(MESSAGE_SPLIT_TAG,""));
            }
            msg += MESSAGE_END_TAG;
            this.Send(msg);
        }

        LoginCallback loginCallback;
        OnlineUsersCallback onlineUsersCallback;
        GetUserCallback getUserCallback;
        GetBattleLogCallback getBattleLogCallback;

        void AddToRecvBuffer(string data)
        {
            recvBuffer += data;
            string[] tmp = recvBuffer.Split(new string[] { MESSAGE_END_TAG }, StringSplitOptions.None);
            if (tmp.Length > 1)
            {
                for (int i = 0; i < tmp.Length - 1; ++i)
                {
                    ProcessMessage(tmp[i]);
                }
                recvBuffer = tmp[tmp.Length - 1];
            }
        }

        string recvBuffer = "";

        void ProcessMessage(string msg)
        {
            string[] paras = msg.Split(new string[] { MESSAGE_SPLIT_TAG }, StringSplitOptions.None);
            if (paras.Length == 0)
            {
                return;
            }
            string MessageHead = paras[0];
            switch (MessageHead)
            {
                case "LOGIN":
                    if (loginCallback != null)
                    {
                        _dispatcher.BeginInvoke(() =>
                        {
                            loginCallback(bool.Parse(paras[1]));
                        });
                    }
                    break;
                case "GET_ONLINE_USERS":
                    if (onlineUsersCallback != null)
                    {
                        List<BattleNetUser> rst = new List<BattleNetUser>();
                        for (int i = 1; i < paras.Length; ++i)
                        {
                            BattleNetUser user = BattleNetUser.Parse(paras[i]);
                            if(user != null)
                                rst.Add(user);
                        }
                        _dispatcher.BeginInvoke(() =>
                        {
                            onlineUsersCallback(rst);
                        });
                    }
                    break;
                case "GET_USER":
                    if (getUserCallback != null)
                    {
                        BattleNetUser u = BattleNetUser.Parse(paras[1]);
                        _dispatcher.BeginInvoke(() =>
                        {
                            getUserCallback(u);
                        });
                    }
                    break;
                case "GET_BATTLE_LOG":
                    break;
                case "JOIN_CHANNEL":
                    if (joinChannelCallback != null)
                    {
                        _dispatcher.BeginInvoke(() =>
                        {
                            joinChannelCallback(bool.Parse(paras[1]));
                        });
                    }
                    break;
                case "CHAT":
                    {
                        ChatEventArgs ev = new ChatEventArgs();
                        ev.User = BattleNetUser.Parse(paras[1]);
                        ev.Channel = paras[2];
                        ev.Message = paras[3];
                        _dispatcher.BeginInvoke(() =>
                            {
                                this.OnNewChatNotify(this, ev);
                            });
                        break;
                    }
                case "GET_SAVES":
                    {
                        List<string> rst = new List<string>();
                        for (int i = 1; i < paras.Length; ++i)
                        {
                            rst.Add(paras[i]);
                        }
                        _dispatcher.BeginInvoke(() =>
                        {
                            this.getSavesCallback(rst);
                        });
                        break;
                    }
                case "SAVE":
                    {
                        _dispatcher.BeginInvoke(() =>
                        {
                            saveCallback(bool.Parse(paras[1]));
                        });
                        break;
                    }
                case "BATTLE_RESULT":
                    {
                        _dispatcher.BeginInvoke(() =>
                        {
                            commitBattleResultCallback(bool.Parse(paras[1]));
                        });
                        break;
                    }
                default:
                    break;
            }
        }

        #region 大厅接口

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public void Login(string user, string password, LoginCallback callback)
        {
            _user = user;
            _password = password;
            loginCallback = callback;
            Connect((isConnected) => 
            {
                if (isConnected)
                    SendMessage("LOGIN", user, password);
                else
                {
                    _dispatcher.BeginInvoke(() =>
                    {
                        callback(false, true);
                    });
                }
            });
        }

        public void Logout()
        {
            if (socket!=null && socket.Connected)
            {
                socket.Close();
                socket = null;
            }
        }

        /// <summary>
        /// 获取在线用户
        /// </summary>
        /// <returns></returns>
        public void GetOnlineUsers(OnlineUsersCallback callback)
        {
            onlineUsersCallback = callback;
            SendMessage("GET_ONLINE_USERS");
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void GetUser(string name,GetUserCallback callback)
        {
            getUserCallback = callback;
            SendMessage("GET_USER", name);
        }

        /// <summary>
        /// 获取战斗日志
        /// </summary>
        /// <param name="user"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public void GetBattleLog(string user, DateTime startTime, DateTime endTime, GetBattleLogCallback callback)
        {
            getBattleLogCallback = callback;
            SendMessage("GET_BATTLE_LOG", user, startTime, endTime);
        }

        public void Chat(string channel, string info)
        {
            SendMessage("CHAT", channel, info);
        }

        JoinChannelCallback joinChannelCallback;
        /// <summary>
        /// 加入频道
        /// 
        /// 不在参数中的频道将退出
        /// </summary>
        /// <param name="channels">频道列表(全量)，用#分隔</param>
        /// <returns></returns>
        public void JoinChannel(string[] channels, JoinChannelCallback callback)
        {
            joinChannelCallback = callback;
            string tmp = "";
            foreach (var c in channels)
            {
                tmp += "#" + c;
            }
            tmp = tmp.Trim(new char[]{'#'});
            SendMessage("JOIN_CHANNEL", tmp);
        }

        ConfirmCallback commitBattleResultCallback;
        public void CommitBattleResult(string uuid, string win, string lose, string info, ConfirmCallback callback)
        {
            commitBattleResultCallback = callback;
            SendMessage("BATTLE_RESULT", uuid, win ,lose,info );
        }

        /// <summary>
        /// 聊天通知
        /// </summary>
        public event EventHandler<ChatEventArgs> OnNewChatNotify;

        /// <summary>
        /// 自己掉线了/被服务器踢了/断开连接
        /// </summary>
        public event EventHandler<EventArgs> OnDroppedNotify;

        #endregion

        #region 存档管理
        GetSavesCallback getSavesCallback;
        public void GetSaves(GetSavesCallback callback)
        {
            getSavesCallback = callback;
            SendMessage("GET_SAVES");
        }

        SaveCallback saveCallback;
        public void Save(int index, string content, SaveCallback callback)
        {
            saveCallback = callback;
            SendMessage("SAVE", index, content);
        }

        #endregion

    }

}
