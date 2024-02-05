using Rhino;
using OLMP.RevitAPI.Tools.Extensions;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using DS.ClassLib.VarUtils;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <inheritdoc/>
    public class RoomTraversionSettings : IRoomTraversionSettings
    {
        /// <summary>
        /// Default fields to exclude from rooms names.
        /// </summary>
        public static IEnumerable<string> DefaultExcludeFields = new List<string>
        {
            "электр"
        };

        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        public RoomTraversionSettings()
        {
            
        }

        /// <inheritdoc/>
        public bool CheckEndPoints { get; set; } = true;

        /// <inheritdoc/>
        public bool CheckSolid { get; set; } = true;

        /// <inheritdoc/>
        public double MinResidualVolume { get; set; } = 3.CubicCMToFeet();

        /// <inheritdoc/>
        public IEnumerable<string> ExcludeFields { get; set; } = DefaultExcludeFields;

        /// <inheritdoc/>
        public bool StrictFieldCompliance { get; set; } = true;

        /// <inheritdoc/>
        public bool CheckNames { get; set; } = true;

        public void SetDefault()
        {
            ExcludeFields = DefaultExcludeFields;
        }
    }
}