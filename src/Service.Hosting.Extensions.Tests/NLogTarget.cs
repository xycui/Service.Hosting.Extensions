namespace Service.Hosting.Extensions.Tests
{
    using NLog.Common;
    using NLog.Targets;
    using Xunit.Abstractions;

    public sealed class NLogTarget : TargetWithLayout
    {
        private readonly ITestOutputHelper _helper;

        public NLogTarget(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            _helper.WriteLine(Layout.Render(logEvent.LogEvent));
        }
    }
}
