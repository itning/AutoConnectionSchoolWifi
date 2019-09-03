using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AutoConnectionSchoolWifi.src
{
    class DefaultAutoConnection : AbstractAutoConnection
    {
        private ConnectionStateCallBack connectionStateCallBack;

        public DefaultAutoConnection(ConnectionStateCallBack connectionStateCallBack) : base(connectionStateCallBack)
        {
            this.connectionStateCallBack = connectionStateCallBack;
        }

        public override bool CheckWifi()
        {
            connectionStateCallBack.StateChange("检查WIFI是否连接...");
            foreach (var ssid in NativeWifi.EnumerateConnectedNetworkSsids())
            {
                if (ssid.ToString().Equals("HXGNET"))
                {
                    connectionStateCallBack.StateChange("已连接HXGNET...");
                    return true;
                }
            }
            connectionStateCallBack.StateChange("未连接HXGNET...");
            return false;
        }

        public override void ConnectTrueWifi()
        {
            connectionStateCallBack.StateChange("连接正确WIFI...");
            // 打开WLAN
            NativeWifi.TurnOnInterfaceRadio(NativeWifi.EnumerateInterfaces().FirstOrDefault().Id);
            while (true)
            {
                // 找到 WIFI
                var a = NativeWifi.EnumerateAvailableNetworks()
                .Where(x => !string.IsNullOrWhiteSpace(x.ProfileName))
                .OrderByDescending(x => x.SignalQuality)
                .Where(x => x.Ssid.ToString().Equals("HXGNET"))
                .FirstOrDefault();
                if (a == null)
                {
                    connectionStateCallBack.StateChange("等待WIFI...");
                    continue;
                }
                // 连接
                NativeWifi.ConnectNetwork(a.Interface.Id, a.ProfileName, a.BssType);
                connectionStateCallBack.StateChange("已连接WIFI...");
                break;
            }
        }

        public override void RedirectLogin()
        {
            while (!CheckWifi())
            {
                connectionStateCallBack.StateChange("等待WIFI...");
                Thread.Sleep(500);
            }
            connectionStateCallBack.StateChange("自动登录准备...");
            HttpClient httpClient = new HttpClient();
            Task<HttpResponseMessage> httpResponseMessage = httpClient.GetAsync("http://www.baidu.com");
            httpResponseMessage.Wait();
            HttpResponseMessage h = httpResponseMessage.Result;
            var b = h.Content.ReadAsStringAsync();
            b.Wait();
            String s = b.Result;
            var loginUrl = "";
            try
            {
                loginUrl = s.Substring(s.IndexOf("='") + 2, s.LastIndexOf("'<") - (s.IndexOf("='") + 2));
            }
            catch (Exception)
            {
                connectionStateCallBack.StateChange("已连接互联网...");
                return;
            }

            if (!loginUrl.StartsWith("http"))
            {
                connectionStateCallBack.StateChange("已连接互联网...");
                return;
            }
            connectionStateCallBack.StateChange("获取到的登录地址..." + loginUrl);
            string username = "";
            string password = "";
            try
            {
                string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    path = Directory.GetParent(path).ToString();
                }
                connectionStateCallBack.StateChange("用户目录： "+path);
                var user = File.ReadAllText(path + "\\userinfo").Split(new Char[] { ',' });
                if (user.Length != 2)
                {
                    connectionStateCallBack.StateChange("用户信息文件错误");
                    return;
                }
                connectionStateCallBack.StateChange("用户名："+ user[0]);
                connectionStateCallBack.StateChange("密码："+ user[1]);
                username = user[0];
                password = user[1];
            }
            catch (Exception e)
            {
                connectionStateCallBack.StateChange(e.Message);
                return;
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                 new KeyValuePair<string, string>("userId", username),
                 new KeyValuePair<string, string>("passwd", password),
                 new KeyValuePair<string, string>("templatetype", "3"),
                 new KeyValuePair<string, string>("pageid", "21")
            });

            var cc = httpClient.PostAsync(loginUrl, formContent);
            cc.Wait();
            var dd = cc.Result;
            connectionStateCallBack.StateChange("登录结果..." + dd.StatusCode);
        }
    }
}
