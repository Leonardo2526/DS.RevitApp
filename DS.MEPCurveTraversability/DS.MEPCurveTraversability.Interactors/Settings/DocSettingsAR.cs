using System.Collections.Generic;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <inheritdoc/>
    public class DocSettingsAR : DocSettingsBase
    {
        /// <summary>
        /// Settings for check rooms traversions.
        /// </summary>
        public IRoomTraversionSettings RoomTraversionSettings { get; set; } =
            new RoomTraversionSettings();

        /// <inheritdoc/>
        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "АР", "AR" };
    }
}
