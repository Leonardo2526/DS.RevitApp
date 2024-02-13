using System.Collections.Generic;


namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <inheritdoc/>
    public class DocSettingsKR : DocSettingsBase
    {

        #region Properties

        /// <inheritdoc/>
        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "КР", "KR" };

        #endregion
    }
}
