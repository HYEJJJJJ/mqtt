using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt_Client_Main
{
    public enum EnConnectionState : byte
    {
        Connected = 1,
        Connecting,
        Disconnected
    }
}
