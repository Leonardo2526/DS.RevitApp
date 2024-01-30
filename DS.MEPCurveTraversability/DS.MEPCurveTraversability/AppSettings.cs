using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.UI;
using Serilog;
using System;

namespace DS.MEPCurveTraversability
{
    internal class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new(() =>
        {
            return new AppSettings();
        });

        public static ILogger Logger { get; } = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

        public static IWindowMessenger Messenger { get; } = new TaskDialogMessenger();

        public static AppSettings GetInstance()
        {
            return _instance.Value;
        }
    }
}
