using System.Collections.Generic;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <summary>
    /// Settings to check traversabililty through rooms.
    /// </summary>
    public interface IRoomTraversionSettings
    {
        /// <summary>
        /// Specifies whether to check points containment in rooms or not.
        /// </summary>
        bool CheckEndPoints { get; set; }

        /// <summary>
        /// Specifies whether to check solids containment in rooms or not.
        /// </summary>
        bool CheckSolid { get; set; }

        /// <summary>
        /// Minimumal residual volume to check solids containment. 
        /// </summary>
        double MinResidualVolume { get; set; }

        /// <summary>
        /// Specifies whether to check rooms names or not.
        /// </summary>
        bool CheckNames { get; set; }

        /// <summary>
        /// Fields to exclude from rooms names.
        /// </summary>
        IEnumerable<string> ExcludeFields { get; set; }

        /// <summary>
        /// Specifies if room names should conatain content fields fully or not.
        /// </summary>
        bool StrictFieldCompliance { get; set; }

        /// <summary>
        /// Set settings to default state.
        /// </summary>
        void SetDefault();
    }
}