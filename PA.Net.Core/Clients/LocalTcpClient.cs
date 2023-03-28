using PA.Crypto;
using PA.Net.Core;
using PA.Net.Core.Clients;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace PA.Net.Clients
{
    public delegate void OnUserExistingMessageReceive(object sender, INetClient client, bool exists);
    public delegate void OnStateReceive(object sender, INetClient clinet, ContactStatus status);
    public delegate void OnUserStateReceive(object sender, long userID,IPAddress ip, ContactStatus status);
    public delegate void OnChackExists(object sender, INetClient client);
    public delegate void OnMessageReceive(object sender, INetClient client, Package pak);
    public delegate void OnStartBoardcastVideo(object sender, INetClient client, Package pak);
    public delegate void OnRequestClientList(object sender, INetClient client);
    public delegate void OnClientListReceived(object sender, string[] clinets);
    public delegate void OnSayHello(object sender, INetClient client);
    public class LocalTcpClient : TcpNetClient
    {
        public event OnUserExistingMessageReceive ExistingMessageReceived = null;
        public event OnMessageReceive TextMessageReceived = null;
        public event OnUserStateReceive UserStateReceived = null;
        public event OnClientListReceived ClientListReceived = null;
        public event OnStartBoardcastVideo VideoBroadcastingMessageReceived = null;
        public virtual IPAddress ServerIP
        {
            get
            {
                if (Connected)
                    return ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                else
                    return IPAddress.None;
            }
        }
        public virtual int ServerPort
        {
            get
            {
                if (Connected)
                    return ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                else
                    return -1;
            }
        }

        public LocalTcpClient() : base()
        {
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }
        public LocalTcpClient(long userID) : base(0,userID)
        {
            UserID = userID;
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            
        }

        public virtual void SayHello()
        {
            //Inform to all clients that this client is now online.
            Package pak = new Package(CommandType.Hello, IPAddress.Broadcast, IP, IP.ToString(), null);
            pak.UserID = UserID;
            pak.SenderIP = IP;
            Send(Package.ToByteArray(pak));
        }
        public virtual void TryLogin(string username, string password) 
        {
            Package pak = new Package(CommandType.TryLogin, ServerIP, IP, String.Format("{0}|{1}", username, password), null);
            pak.UserID = UserID;
            pak.SenderIP = IP;
            Send(Package.ToByteArray(pak));
        }
        public virtual void RequestServerTime(long marketID)
        {
            Package pak = new Package(CommandType.SeverTimeRequest, ServerIP, IP, null, null);
            pak.Data= marketID;
            pak.UserID = UserID;
            pak.SenderIP = IP;
            Send(Package.ToByteArray(pak));
        }
        public virtual void RequestSymbols(long marketID)
        {
            Package pak = new Package(CommandType.SymbolListRequest, ServerIP, IP, null, null)
            {
                Data = marketID,
                UserID = UserID,
                SenderIP = IP
            };
            Send(Package.ToByteArray(pak));
        }

        public virtual void SayGoodbye()
        {
            Package pak = new Package(CommandType.Goodbye, IPAddress.Broadcast, IP, "0:offline", null);
            pak.UserID = UserID;
            pak.SenderIP = IP;
            Send(Package.ToByteArray(pak));
        }

        public virtual void SendMessage(string msg, long room)
        {
            Package pak = new Package(CommandType.Message, IPAddress.Broadcast, IP, msg, room);
            pak.UserID = UserID;
            Send(Package.ToByteArray(pak));
        }
        public override bool Connect(IPAddress serverIP, int port, CryptoTypes channelType)
        {
            State = Core.Clients.ClientState.Connecting;
            try
            {
                client = new System.Net.Sockets.TcpClient();
                client.Connect(serverIP, port);
                CreateChannel(channelType);
                ClientName = string.Format("{0} ({1} : {2})", UserID, IP, Port);
                RaiseConnectionSuceeded();
                SayHello();
                return true;
            }
            catch (Exception ex)
            {
                Log.Add(ReportType.Error, ClientName, string.Format("خطا در زمان اتصال به سرور:{0}", ex.Message), ex, DateTime.Now);
                RaiseConnectionFailed();
                return false;
            }
        }

        public override void CreateChannel(CryptoTypes channelType)
        {
            ChannelType = channelType;
            switch (channelType)
            {
                case CryptoTypes.Open:
                    channel = new OpenChannel(client.GetStream(), this, true);
                    break;
                case CryptoTypes.AES:
                    channel = new AESChannel(client.GetStream(), this, true);
                    break;
                case CryptoTypes.RSA:
                    channel = new RSAChannel(client.GetStream(), this, true);
                    break;
            }
            channel.DataReceived += Channel_DataReceived;
            channel.ReadFinished += Channel_ReadFinished;
            channel.ReadStarted += Channel_ReadStarted;
            channel.ReceivingFailed += Channel_ReceivingFailed;
            channel.SendingFailed += Channel_SendingFailed;
            channel.SendingSuceeded += Channel_SendingSuceeded;
            channel.WriteFinished += Channel_WriteFinished;
            channel.WriteStarted += Channel_WriteStarted;

            channel.Start();
        }

        public override bool Disconnect()
        {
            if (client != null && client.Connected)
            {
                State = ClientState.Disconnecting; 
                try
                {
                    SayGoodbye();
                    client.Client.Disconnect(true);
                    if (client.Connected)
                    {
                        client.GetStream().Close();
                        client.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Add(ReportType.Error, ClientName, string.Format("خطا در زمان قطع اتصال از سرور:{0}", ex.Message), ex, DateTime.Now);
                    RaiseDisconnectFaild();
                    return false;
                }

            }
            else
                return true;
        }

        #region Channel Events
        //--------------------------------------------------------------------------------------
        private void Channel_WriteStarted(object sender, EventArgs e)
        {
            Log.Add(ReportType.Event, ClientName, "Write Started.", null, DateTime.Now);
        }

        private void Channel_WriteFinished(object sender, EventArgs e)
        {
            Log.Add(ReportType.Event, ClientName, "Write Finished.", null, DateTime.Now);
        }

        private void Channel_SendingSuceeded(object sender, EventArgs e)
        {
            Log.Add(ReportType.Event, ClientName, "Data Sent.", null, DateTime.Now);
        }

        private void Channel_SendingFailed(object sender, Exception ex)
        {
            Log.Add(ReportType.Error, ClientName, "Send Faild.", ex, DateTime.Now);
        }

        private void Channel_ReceivingFailed(object sender, Exception ex)
        {
            Log.Add(ReportType.Error, ClientName, "Receive Faild.", ex, DateTime.Now);
        }

        private void Channel_ReadStarted(object sender, EventArgs e)
        {
            Log.Add(ReportType.Event, ClientName, "Read Started.", null, DateTime.Now);
        }

        private void Channel_ReadFinished(object sender, EventArgs e)
        {
            Log.Add(ReportType.Event, ClientName, "Read Finished.", null, DateTime.Now);
        }

        private void Channel_DataReceived(object sender, INetClient client, byte[] data)
        {
            State = ClientState.DataReceive;
            Package pak = Package.FromByteArray(data);
            switch (pak.CommandType)
            {
                case CommandType.IsNameExists:
                    CompileUserExisting(pak);
                    break;
                case (CommandType.Message):
                    if (TextMessageReceived != null)
                        TextMessageReceived(this, this, pak);
                    break;

                case (CommandType.BroadcastState):
                    SendStatus(pak);
                    break;
                case (CommandType.SendClientList):
                    GetClientList(pak);
                    break;
                case (CommandType.Goodbye):
                    ContactStatus cs = ContactStatus.FromBytes((byte[])pak.Data);
                    if (UserStateReceived != null)
                        UserStateReceived(this, pak.UserID, pak.SenderIP, cs);
                    break;
                //case CommandType.FileControl:
                //    FileManager.Upload(pak);
                //    break;
                //case CommandType.FileOrder:
                //    SendMessageToRoom(pak);
                //    break;
                case CommandType.StartBroadcastingVideo:
                    if (VideoBroadcastingMessageReceived != null)
                        VideoBroadcastingMessageReceived(this, this, pak);
                    break;
                
            }
            State = ClientState.Idle;
        }

        private void GetClientList(Package pak)
        {
            string val = Encoding.UTF8.GetString((byte[])pak.Data);
            StringReader reader = new StringReader(val);
            string line = reader.ReadLine();
            List<String> result = new List<string>();
            while (!string.IsNullOrWhiteSpace(line))
            {
                result.Add(line);
                line = reader.ReadLine();
            }
            if (ClientListReceived != null)
                ClientListReceived(this, result.ToArray());
        }

        private void SendStatus(Package pak)
        {
            if (UserStateReceived != null)
                UserStateReceived(this, pak.UserID, pak.SenderIP, ContactStatus.FromBytes((byte[])pak.Data));
        }

        private void CompileUserExisting(Package pak)
        {
            string s = System.Text.Encoding.UTF8.GetString((byte[])pak.Data);
            bool exists = bool.Parse(s);
            if (ExistingMessageReceived != null)
                ExistingMessageReceived(this, this, exists);
            if (exists)
            {
                Disconnect();
            }
        }

        public void SetState(ClientState state)
        {
            State = state;
        }

        public void RequestMarketList()
        {
            Package pak = new Package(CommandType.MarketListRequest, ServerIP, IP, null, null);
            pak.UserID = UserID;
            pak.SenderIP = IP;
            Send(Package.ToByteArray(pak));
        }
        //--------------------------------------------------------------------------------------
        #endregion Channel Events
    }
}


