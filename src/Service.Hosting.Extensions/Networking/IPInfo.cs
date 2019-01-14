﻿namespace Service.Hosting.Extensions.Networking
{
    using System.Net;

    public class IpInfo
    {
        public IPAddress Ip => IPAddress.Parse(Query);
        public string As { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Isp { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string Org { get; set; }
        public string Query { get; set; }
        public string Region { get; set; }
        public string RegionName { get; set; }
        public string Status { get; set; }
        public string Timezone { get; set; }
        public string Zip { get; set; }
    }
}
