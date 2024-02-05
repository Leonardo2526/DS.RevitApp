using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Tools.Elements.MEPElements;
using OLMP.RevitAPI.UI;
using Serilog;
using System;
using SimpleInjector;
using System.Collections.Generic;
using DS.MEPCurveTraversability.Interactors;
using Rhino;

namespace DS.MEPCurveTraversability
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

            container.RegisterInstance(new DocSettingsAR()
            { 
                RoomTraversionSettings = new RoomTraversionSettings(),
                WallIntersectionSettings = new WallIntersectionSettings
                {
                    WallOffset = 200 * _mmToFeet,
                    InsertsOffset = 200 * _mmToFeet
                },

                AutoDocsDetectionFields = new List<string>() { "АР", "AR", "Тест" }
            });

            container.RegisterInstance(new DocSettingsKR()
            {
                WallIntersectionSettings = new WallIntersectionSettings()
            });



            container.Verify();

            return container;
        }
    }
}
