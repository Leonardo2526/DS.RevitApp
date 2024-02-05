using Rhino;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallIntersectionSettings : IWallIntersectionSettings
    {
        private static readonly double _mmToFeet =
            RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        /// <inheritdoc/>
        public double NormalAngleLimit { get; set; } = RhinoMath.DefaultAngleTolerance;

        /// <inheritdoc/>
        public double OpeningOffset { get; set; } = 100 * _mmToFeet;

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public double WallOffset { get; set; } = 1000 * _mmToFeet;

        /// <inheritdoc/>
        public double InsertsOffset { get; set; } = 500 * _mmToFeet;

        /// <inheritdoc/>
        public double JointsOffset { get; set; }
    }
}
