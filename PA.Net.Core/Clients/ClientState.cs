using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.Net.Core.Clients
{
    public enum ClientState
    {
        Connected,
        Disconnected,
        Connecting,
        Disconnecting,
        Error,
        Retry,
        Sending,
        Idle,
        DataReceive,
    }
}
