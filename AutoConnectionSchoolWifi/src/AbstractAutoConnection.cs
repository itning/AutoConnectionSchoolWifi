using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoConnectionSchoolWifi.src
{
    abstract class AbstractAutoConnection
    {
        private ConnectionStateCallBack connectionStateCallBack;

        public AbstractAutoConnection(ConnectionStateCallBack connectionStateCallBack)
        {
            this.connectionStateCallBack = connectionStateCallBack;
        }

        public void Connection()
        {
            connectionStateCallBack.StateChange("正在准备...");
            if (CheckWifi())
            {
                if (Ping())
                {
                    connectionStateCallBack.StateChange("已连接互联网...");
                }
                else
                {
                    RedirectLogin();
                }
            }
            else
            {
                ConnectTrueWifi();
                if (Ping())
                {
                    connectionStateCallBack.StateChange("已连接互联网...");
                }
                else
                {
                    RedirectLogin();
                }
            }
        }

        public abstract bool CheckWifi();

        public abstract void ConnectTrueWifi();

        public bool Ping()
        {
            PingOptions pingOption = new PingOptions
            {
                DontFragment = true
            };

            string data = "sendData:baidu";
            int timeout = 1000;
            try
            {
                PingReply reply = new Ping().Send("www.baidu.com", timeout);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (PingException e)
            {
                connectionStateCallBack.StateChange(e.Message);
                return false;
            }
        }

        public abstract void RedirectLogin();
    }
}
