using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallIntersectionSettings : IWallIntersectionSettings
    {
        private static readonly double _mmToFeet =
            RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        public double NormalAngleLimit { get; set; } = RhinoMath.DefaultAngleTolerance;

        public double OpeningOffset { get; set; } = 100 * _mmToFeet;

        public bool CheckOpenings { get; set; } = true;

        /// <summary>
        /// Wall clerance.
        /// </summary>
        public double WallOffset { get; set; } = 1000 * _mmToFeet;

        /// <summary>
        /// Clerance for walls inserts.
        /// </summary>
        public double InsertsOffset { get; set; } = 500 * _mmToFeet;

        /// <summary>
        /// Clerance for walls joints.
        /// </summary>
        public double JointsOffset { get; set; }
    }
}
