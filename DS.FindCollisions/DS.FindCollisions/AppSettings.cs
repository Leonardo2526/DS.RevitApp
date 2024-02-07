using DS.ClassLib.VarUtils;
using OLMP.RevitAPI.Tools.Elements.MEPElements;
using OLMP.RevitAPI.UI;
using Serilog;
using System;
using SimpleInjector;
using System.Collections.Generic;
using Rhino;
using Autodesk.Revit.ApplicationServices;

namespace DS.FindCollisions
{
    internal static class AppSettings
    {
        private static readonly double _mmToFeet =
         RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        public static ILogger Logger { get; } = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

        public static IWindowMessenger Messenger { get; } = new TaskDialogMessenger();

        /// <summary>
        /// Application container.
        /// </summary>
        public static Container AppContainer { get; } = GetAppContainer();

        private static Container GetAppContainer()
        {
            var container = new Container();

            return container;
        }
    }
}
