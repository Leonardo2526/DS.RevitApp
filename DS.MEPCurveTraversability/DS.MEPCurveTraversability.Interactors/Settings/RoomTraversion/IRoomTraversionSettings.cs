using System.Collections.Generic;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    public interface IRoomTraversionSettings
    {
        bool CheckEndPoints { get; set; }
        bool CheckSolid { get; set; }

        bool CheckNames { get; set; }

        /// <summary>
        /// Fields to exclude.
        /// </summary>
        IEnumerable<string> ExcludeFields { get; set; }


        double MinResidualVolume { get; set; }


        /// <summary>
        /// Specifies if room names should conatain content fields fully or not.
        /// </summary>
        bool StrictFieldCompliance { get; set; }
    }
}