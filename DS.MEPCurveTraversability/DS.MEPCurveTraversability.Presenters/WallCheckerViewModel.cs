using DS.MEPCurveTraversability.Interactors;
using Rhino;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability.Presenters
{
    public class WallCheckerViewModel(IWallIntersectionSettings wallIntersectionSettings) :
        IWallIntersectionSettings
    {
        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        private static readonly double _feetToMM =
          RhinoMath.UnitScale(UnitSystem.Feet, UnitSystem.Millimeters);

        private readonly IWallIntersectionSettings _settings = wallIntersectionSettings;

        public string Title { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get => _settings.IsEnabled;
            set => _settings.IsEnabled = value;
        }

        /// <inheritdoc/>
        public double InsertsOffset
        {
            get => _settings.InsertsOffset * _feetToMM;
            set => _settings.InsertsOffset = value * _mmToFeet;
        }

        /// <inheritdoc/>
        public double JointsOffset
        {
            get => _settings.JointsOffset * _feetToMM;
            set => _settings.JointsOffset = value * _mmToFeet;
        }

        /// <inheritdoc/>
        public double NormalAngleLimit
        {
            get => RhinoMath.ToDegrees(_settings.NormalAngleLimit);
            set => _settings.NormalAngleLimit = RhinoMath.ToRadians(value);
        }

        /// <inheritdoc/>
        public double OpeningOffset
        {
            get => _settings.OpeningOffset * _feetToMM;
            set => _settings.OpeningOffset = value * _mmToFeet;
        }

        /// <inheritdoc/>
        public double WallOffset
        {
            get => _settings.WallOffset * _feetToMM;
            set => _settings.WallOffset = value * _mmToFeet;
        }
    }
}
