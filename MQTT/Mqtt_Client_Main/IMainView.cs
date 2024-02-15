using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt_Client_Main
{
    public interface IMainView
    {
        void MessageReceived(string topic, string data);

        void ClientConnectionChanged();
    }
}