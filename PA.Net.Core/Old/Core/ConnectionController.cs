using System;

namespace PA.Net.Core
{
    public class ConnectionController
    {
        public NetClient Client { get; }

        public bool KeepAlive { get; set; }

        public ConnectionController(NetClient client)
        {
            Client = client;
            if (Client == null)
                return;
            Client.ConnectionSuceeded += Client_ConnectionSuceeded;
            Client.ConnectionFailed += Client_ConnectionFailed;
            Client.DisconnectFaild += Client_DisconnectFaild;
            Client.DisconnectSuceeded += Client_DisconnectSuceeded;
        }

        private void Client_DisconnectSuceeded(object sender, EventArgs e)
        {
        }

        private void Client_DisconnectFaild(object sender, EventArgs e)
        {
        }

        private void Client_ConnectionFailed(object sender, EventArgs e)
        {
        }

        private void Client_ConnectionSuceeded(object sender, EventArgs e)
        {
        }
    }
}
