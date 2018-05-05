using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Text;

namespace SilverlightTcpClientTest
{
    public partial class MainPage : UserControl
    {
        Socket socket = null;
        SocketAsyncEventArgs connectArgs = null;
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

        public MainPage()
        {
            InitializeComponent();
            Connect();
        }

        private void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectArgs = new SocketAsyncEventArgs();
            connectArgs.RemoteEndPoint = new DnsEndPoint("127.0.0.1", 4502);
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketArgs_Completed);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(sendArgs_Completed);
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(recvArgs_Completed);
            socket.ConnectAsync(connectArgs);
        }

        void recvArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                string RecevieStr = Encoding.UTF8.GetString(e.Buffer, 0, e.Buffer.Length).Replace("\0", "");
                Console.WriteLine("收到数据:" + RecevieStr);
                this.Dispatcher.BeginInvoke(() =>
                {
                    recvListBox.Items.Add(new TextBlock() { Text = RecevieStr });
                });
                recvArgs.SetBuffer(new byte[RECV_BUFFER_SIZE], 0, RECV_BUFFER_SIZE);
                socket.ReceiveAsync(recvArgs);
            }
        }

        void sendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    Console.WriteLine("发送成功");
                });
            }
        }

        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect && socket.Connected)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    Console.WriteLine("连接成功");
                });
                recvArgs.SetBuffer(new byte[RECV_BUFFER_SIZE], 0, RECV_BUFFER_SIZE);
                socket.ReceiveAsync(recvArgs);
            }
        }
        const int RECV_BUFFER_SIZE = 1024 * 100;
        
        private void sendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.sendText.Text == string.Empty) return;
            byte[] userbytes = Encoding.UTF8.GetBytes(sendText.Text);
            sendArgs.SetBuffer(userbytes, 0, userbytes.Length);
            socket.SendAsync(sendArgs);
            
        }
    }
}
