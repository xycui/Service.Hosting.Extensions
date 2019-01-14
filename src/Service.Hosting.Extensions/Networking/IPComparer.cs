namespace Service.Hosting.Extensions.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    // ReSharper disable once InconsistentNaming
    public class IPComparer : IEqualityComparer<IPAddress>
    {
        private static readonly Lazy<IEqualityComparer<IPAddress>> LazyInstance = new Lazy<IEqualityComparer<IPAddress>>(() => new IPComparer());
        public static IEqualityComparer<IPAddress> Instance => LazyInstance.Value;

        public bool Equals(IPAddress x, IPAddress y)
        {
            return x?.ToString() == y?.ToString();
        }

        public int GetHashCode(IPAddress obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
