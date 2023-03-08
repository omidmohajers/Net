using PA.Net.Clients;
using System;

namespace PA.Net.Core
{
    public delegate void DataReceiveHandler(object sender,INetClient client, byte[] data);
    public delegate void ExceptionReportHandler(object sender, Exception ex);
    public class ClientConnectionEventArgs : EventArgs
    {
        public ClientConnectionEventArgs(INetClient clinet)
        {
            Clinet = clinet;
        }
        public INetClient Clinet { get; private set; }
    }
}
