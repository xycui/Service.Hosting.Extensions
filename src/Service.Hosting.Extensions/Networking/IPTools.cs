namespace Service.Hosting.Extensions.Networking
{
    using LumiSoft.Net.STUN.Client;
    using Newtonsoft.Json;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    // ReSharper disable once InconsistentNaming
    public static class IPTools
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // https://gist.github.com/yetithefoot/7592580
        // https://gist.github.com/mondain/b0ec1cf5f60ae726202e
        private static readonly IReadOnlyList<string> StunServerList = new List<string>
        {
            "stun.voipstunt.com",
            "stun.sipgate.net",
            "stun.zoiper.com",
            "stun.ekiga.net",
            "stun.pjsip.org"
            //"stun.voipgate.com",
            //"stun.iptel.org",
            //"stunserver.org",
        };

        private const int StunTimeOutInMs = 3500;
        private const string IpApiUri = "http://api.ipify.org";
        private const string IpInfoApiUri = "http://ip-api.com/json/";
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        public static IPAddress GetLocalIpv4Address()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipProp = ni.GetIPProperties();
                var addr = ipProp.GatewayAddresses.FirstOrDefault();
                if (ni.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (addr == null || addr.Address.ToString().Equals("0.0.0.0"))
                {
                    continue;
                }

                if (ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                {
                    continue;
                }

                foreach (var ip in ipProp.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address;
                    }
                }
            }


            var hostName = Dns.GetHostName();
            return
                Dns.GetHostAddresses(hostName).First(item => item.AddressFamily == AddressFamily.InterNetwork && Dns.GetHostEntry(item).HostName.StartsWith(hostName));
        }

        public static IPAddress GetPublicIpv4Address(string customStunAddress = null, int stunPort = 3478)
        {
            var localIp = GetLocalIpv4Address();
            var taskList = StunServerList.Select(x => CreateGetPublicIpTask(x, 3478, localIp)).ToList();

            if (!string.IsNullOrEmpty(customStunAddress))
            {
                taskList.Add(CreateGetPublicIpTask(customStunAddress, stunPort, localIp));
            }


            var res = Task.WhenAll(taskList).Result;
            var results = Task.WhenAll(taskList).Result.Where(x => x != null).Aggregate(new Dictionary<IPAddress, int>(IPComparer.Instance),
                (ints, address) =>
                {
                    ints[address] = ints.TryGetValue(address, out var count) ? ++count : 1;
                    return ints;
                });

            return results.OrderByDescending(pair => pair.Value).FirstOrDefault().Key;
        }

        public static async Task<IPAddress> GetPublicIpv4AddressAsync(string customStunAddress = null, int stunPort = 3478)
        {
            var localIp = GetLocalIpv4Address();
            var taskList = StunServerList.Select(x => CreateGetPublicIpTask(x, 3478, localIp)).ToList();

            if (!string.IsNullOrEmpty(customStunAddress))
            {
                taskList.Add(CreateGetPublicIpTask(customStunAddress, stunPort, localIp));
            }

            var results = (await Task.WhenAll(taskList)).Where(x => x != null).Aggregate(new Dictionary<IPAddress, int>(IPComparer.Instance),
                (ints, address) =>
                {
                    ints[address] = ints.TryGetValue(address, out var count) ? ++count : 1;
                    return ints;
                });

            return results.OrderByDescending(pair => pair.Value).FirstOrDefault().Key;
        }

        public static async Task<IPAddress> GetPublicIpAsync()
        {
            try
            {
                var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, IpApiUri));
                return !response.IsSuccessStatusCode ? null : IPAddress.Parse(await response.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }
        }

        public static async Task<IpInfo> GetIpInfoAsync(IPAddress ip)
        {
            try
            {
                var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, IpInfoApiUri));
                return !response.IsSuccessStatusCode ? null : JsonConvert.DeserializeObject<IpInfo>(await response.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }
        }

        private static Task<IPAddress> CreateGetPublicIpTask(string stunAddress, int port, IPAddress localIPaddress)
        {
            return Task.WhenAny(Task.Run(() =>
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    var ip = STUN_Client.GetPublicIP(stunAddress, port, localIPaddress);
                    Logger.Debug($"Get public ip from stun server [{stunAddress}] with time: [{stopWatch.ElapsedMilliseconds}]");
                    return ip;
                }
                catch
                {
                    return null;
                }
            }), Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(StunTimeOutInMs));
                return (IPAddress)null;
            })).Unwrap();
        }
    }
}
