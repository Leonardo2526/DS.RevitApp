using Autodesk.Revit.UI;
using DS.MEPCurveTraversability.Interactors;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.UI;
using Rhino;
using Rhino.UI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.ClassLib.VarUtils;
using Autodesk.Revit.DB;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability
{
    internal class AppSettings
    {
        private static Lazy<AppSettings> _instance = new Lazy<AppSettings>(() =>
        {
            return new AppSettings();
        });

        private static readonly double _mmToFeet =
           RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);


        public static ILogger Logger { get; } = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

        public static IWindowMessenger Messenger { get; } = new TaskDialogMessenger();


        public static (Document, IEnumerable<RevitLinkInstance>) DocLinks { get; set; }  
        

        public static WallIntersectionSettings WallIntersectionSettingsAR { get; } = new()
        {
            WallOffset = 200 * _mmToFeet,
            InsertsOffset = 200 * _mmToFeet,
        };

        public static (Document, IEnumerable<RevitLinkInstance>) DocLinksAR { get; set; }

        public static WallIntersectionSettings WallIntersectionSettingsKR { get; } = new();

        public static (Document, IEnumerable<RevitLinkInstance>) DocLinksKR { get; set; }

        public static AppSettings GetInstance()
        {
            return _instance.Value;
        }
    }
}
