namespace Service.Hosting.Extensions.Tests
{
    using Networking;
    using NLog;
    using NLog.Config;
    using System.Diagnostics;
    using Xunit;
    using Xunit.Abstractions;

    // ReSharper disable once InconsistentNaming
    public class IPToolTests
    {
        private readonly ITestOutputHelper _output;

        public IPToolTests(ITestOutputHelper output)
        {
            _output = output;
            var config = LogManager.Configuration;
            var nlogTarget = new NLogTarget(_output);
            config.AddTarget("xUnit", nlogTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, nlogTarget));
            LogManager.Configuration = config;
        }

        [Fact]
        public void TestGetPublicIp()
        {
            var stopWatch = Stopwatch.StartNew();
            var ip = IPTools.GetPublicIpv4Address();
            _output.WriteLine(stopWatch.ElapsedMilliseconds.ToString());
            Assert.NotNull(ip);
        }
    }
}
