using Rhino;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using DS.ClassLib.VarUtils;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    public class RoomTraversionSettings : IRoomTraversionSettings
    {
        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);



        public static IEnumerable<string> DefaultExcludeFields = new List<string>
        {
            "электр"
        };

        public RoomTraversionSettings()
        {
            //var excludeFields = new List<string>();
            //excludeFields.AddRange(DefaultExcludeFields);
            //ExcludeFields = excludeFields;
        }


        public bool CheckEndPoints { get; set; } = true;

        public bool CheckSolid { get; set; } = true;

        public double MinResidualVolume { get; set; } = 3.CubicCMToFeet();

        /// <inheritdoc/>
        public IEnumerable<string> ExcludeFields { get; set; } = DefaultExcludeFields.DeepCopy();

        public bool StrictFieldCompliance { get; set; } = true;
        public bool CheckNames { get; set; } = true;
    }
}