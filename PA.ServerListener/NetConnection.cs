using PA.Net.Clients;
using PA.Net.Core;
using System;
using System.Net;
using System.Threading;

namespace PA.Server.Cores
{
    public class NetConnection
    {
        private Thread listener;
        public event EventHandler<PackageEventArgs> PackageReceived = null;
        public event EventHandler<ClientConnectionEventArgs> ClientConnected = null;
        public int ServerPort { get; set; }
        public IPAddress ServerIP { get; set; }
        public bool Cancel { get; private set; }

        public void RaisePackageReceived(Package package)
        {
            if (PackageReceived != null)
                PackageReceived(this, new PackageEventArgs(package));
        }

        public void RaiseClientConnected(INetClient client)
        {
            if (ClientConnected != null)
                ClientConnected(this, new ClientConnectionEventArgs(client));
        }

        public virtual void Initialize()
        {
        }

        public void Start()
        {
            listener = new Thread(new ThreadStart(DoWork));
            Cancel = false;
            listener.Start();
           // listener.Join();
        }

        public void Stop()
        {
            Cancel = true;
        }

        private void DoWork()
        {
            Listen();
        }

        public virtual void Listen()
        {

        }

        public virtual void SendExistanceCommand(IPAddress senderIP, bool isExixsts)
        {

        }
    }
}
