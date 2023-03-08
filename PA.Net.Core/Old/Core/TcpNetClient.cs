using System;
using System.Net.Sockets;
using System.Threading;

namespace PA.Net.Core
{
    public class TcpNetClient : NetClient
    {
        protected TcpClient client;
        protected Semaphore semaphor = new Semaphore(1, 1);
        public bool Connected
        {
            get
            {
                if (client != null)
                    return client.Connected;
                else
                    return false;
            }
        }
        public TcpClient Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
            }
        }

        public TcpNetClient():base()
        {
            client = new TcpClient(); 
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void SendCommand(Package pak)
        {
            if (client != null && client.Connected)
            {
                sender = new Thread(SendPackage);
                sender.Start(pak);
            }
            else
                RaiseSendingFailed();
        }
        protected override bool SendPackageToNet(Package pak)
        {
            try
            {
                semaphor.WaitOne();
                NetworkStream networkStream = client.GetStream();
                byte[] buffer = Package.ToByteArray(pak);

                byte[] sizeBuff = new byte[4];
                sizeBuff = BitConverter.GetBytes(buffer.Length);
                networkStream.Write(sizeBuff, 0, 4);
                networkStream.Flush();

                networkStream.Write(buffer, 0, buffer.Length);
                networkStream.Flush();
                semaphor.Release();
                return true;
            }
            catch
            {
                semaphor.Release();
                return false;
            }
        }

        protected override void StartReceive()
        {
            while (client.Connected)
            {
                byte[] data = null;
                try
                {
                    NetworkStream stream = client.GetStream();
                    //if (!stream.DataAvailable)
                    //{
                    //    // Give up the remaining time slice.
                    //    Thread.Sleep(1);
                    //}
                    int i = 0;

                    byte[] bytes = new byte[4];
                    if (!client.Connected)
                        break;
                    if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        //if (BitConverter.IsLittleEndian)
                        //    Array.Reverse(bytes);
                        if (data == null)
                        {
                            data = new byte[bytes.Length];
                            Array.Copy(bytes, data, bytes.Length);
                        }

                    }
                    else
                        continue;

                    bytes = new byte[BitConverter.ToInt32(data, 0)];
                    data = null;
                    if (!client.Connected)
                        break;
                    if ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        //if (BitConverter.IsLittleEndian)
                        //    Array.Reverse(bytes);
                        if (data == null)
                        {
                            data = new byte[bytes.Length];
                            Array.Copy(bytes, data, bytes.Length);
                        }
                        else
                        {
                            byte[] tmp = new byte[data.Length + bytes.Length];
                            Array.Copy(data, tmp, data.Length);
                            Array.Copy(bytes, 0, tmp, data.Length, bytes.Length);
                            data = tmp;
                        }
                    }

                    Package package = Package.FromByteArray(data);
                    RaisePackageReceived(package);
                }
                catch (Exception ex)
                {
                    Log.Add(ReportType.Error, ClientName, string.Format("خطا در زمان دریافت اطلاعات:{0}", ex.Message), ex, DateTime.Now);
                    RaiseReceivingFailed();
                }
            }
            RaiseDisconnectSuceeded();
        }

        public override bool Disconnect()
        {
            return base.Disconnect();
        }
    }
}
