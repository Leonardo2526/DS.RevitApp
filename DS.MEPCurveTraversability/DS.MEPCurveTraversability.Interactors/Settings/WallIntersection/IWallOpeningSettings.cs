using DS.MEPCurveTraversability.Interactors.Settings;

namespace DS.MEPCurveTraversability.Interactors
{
    /// <summary>
    /// Settings to check wall's opening that built from intersection object.
    /// </summary>
    public interface IWallOpeningSettings
    {
        /// <summary>
        /// Specifies if intersection opening settings are enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Offset from wall's opening
        /// </summary>
        double OpeningOffset { get; set; }

        /// <summary>
        /// Offset to check wall's inserts (other openings) zones.
        /// </summary>
        double InsertsOffset { get; set; }

        /// <summary>
        /// Offset to check wall's joints zones.
        /// </summary>
        double JointsOffset { get; set; }

        /// <summary>
        /// Inner offset from wall's out edges to check zones.
        /// </summary>
        double WallOffset { get; set; }
    }
}