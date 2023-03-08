using PA.Net.Collections;
using System;
using System.Threading;

namespace PA.Net.Core
{
    public class NetClient
    {
        protected Thread receiver = null;
        protected Thread sender = null;
        protected ReportCollection Log = new ReportCollection();
        public event EventHandler<PackageEventArgs> PackageReceived = null;
        public event EventHandler DisconnectSuceeded = null;
        public event EventHandler DisconnectFaild = null;
        public event EventHandler ConnectionSuceeded = null;
        public event EventHandler ConnectionFailed = null;
        public event EventHandler SendingSuceeded = null;
        public event EventHandler SendingFailed = null;
        public event EventHandler ReceivingFailed = null;

        public string ClientName
        {
            get;
            set;
        }

        public NetClient()
        {
            ClientName = "";
        }

        public virtual void Start()
        {
            Log.Add(ReportType.Event, ClientName, "Starting...");
            receiver = new Thread(new ThreadStart(StartReceive));
            receiver.Start();
            Log.Add(ReportType.Event, ClientName, "Started.");
        }

        public virtual void Stop()
        {
            Log.Add(ReportType.Event, ClientName, "Stopping...");
            receiver.Abort();
            Log.Add(ReportType.Event, ClientName, "Stoped.");
        }

        public virtual void SendCommand(Package pak)
        {
            
        }

        protected void SendPackage(object data)
        {
            Package pak = data as Package;
            bool result = SendPackageToNet(pak);
            if (result)
            {
                RaiseSendingSuceeded();
            }
            else
            {
                RaiseSendingFailed();
            }
        }
        protected virtual bool SendPackageToNet(Package pak)
        {
            return true;
        }
        protected virtual void StartReceive()
        {

        }

        public virtual bool Disconnect()
        {
            return true;
        }

        public void RaisePackageReceived(Package pak)
        {
                if (PackageReceived != null)
                    PackageReceived(this, new PackageEventArgs(pak));
            Log.Add(ReportType.Event, ClientName, "Package Received.");
        }

        public void RaiseDisconnectSuceeded()
        {
            if (DisconnectSuceeded != null)
                DisconnectSuceeded(this, EventArgs.Empty);
            Log.Add(ReportType.Event, ClientName, "Disconnected.");
        }
        public void RaiseDisconnectFaild()
        {
            if (DisconnectFaild != null)
                DisconnectFaild(this, EventArgs.Empty);
            Log.Add(ReportType.Error, ClientName, "Disconnect Faild.");
        }
        public void RaiseConnectionSuceeded()
        {
            if (ConnectionSuceeded != null)
                ConnectionSuceeded(this, EventArgs.Empty);
            Log.Add(ReportType.Event, ClientName, "Connected.");
        }
        public void RaiseConnectionFailed()
        {
            if (ConnectionFailed != null)
                ConnectionFailed(this, EventArgs.Empty);
            Log.Add(ReportType.Error, ClientName, "Connect Faild.");
        }
        public void RaiseSendingSuceeded()
        {
            if (SendingSuceeded != null)
                SendingSuceeded(this, EventArgs.Empty);
            Log.Add(ReportType.Event, ClientName, "Data Sent.");
        }
        public void RaiseSendingFailed()
        {
            if (SendingFailed != null)
                SendingFailed(this, EventArgs.Empty);
            Log.Add(ReportType.Error, ClientName, "Send Faild.");
        }
        public void RaiseReceivingFailed()
        {
            if (ReceivingFailed != null)
                ReceivingFailed(this, EventArgs.Empty);
            Log.Add(ReportType.Error, ClientName, "Receive Faild.");
        }
    }
}
