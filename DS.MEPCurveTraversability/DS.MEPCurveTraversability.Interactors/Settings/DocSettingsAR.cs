using Autodesk.Revit.DB;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    public class DocSettingsAR : DocSettingsBase
    {
        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        private static readonly Lazy<DocSettingsAR> _instance =
            new(() => new DocSettingsAR());

        private DocSettingsAR()
        { }

        public static DocSettingsAR GetInstance()
        {
            return _instance.Value;
        }

        #region Properties

        public WallIntersectionSettings WallIntersectionSettings { get; } = new()
        {
            WallOffset = 200 * _mmToFeet,
            InsertsOffset = 200 * _mmToFeet,
        };

        public RoomTraversionSettings RoomTraversionSettings { get; } = new();

        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "АР", "AR", "Тест" };

        #endregion
    }
}
