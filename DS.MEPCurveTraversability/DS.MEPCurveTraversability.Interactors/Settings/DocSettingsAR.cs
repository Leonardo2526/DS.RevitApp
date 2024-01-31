using Autodesk.Revit.DB;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <inheritdoc/>
    public class DocSettingsAR : DocSettingsBase
    {
        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        private static readonly Lazy<DocSettingsAR> _instance =
            new(() => new DocSettingsAR(_activeDoc, _allDocLinks));

        private static Document _activeDoc;
        private static IEnumerable<RevitLinkInstance> _allDocLinks;

        private DocSettingsAR(Document activeDoc, IEnumerable<RevitLinkInstance> allDocLinks)
        {
            Docs = FilterByLastFolderName(
                    activeDoc,
                    allDocLinks,
                    AutoDocsDetectionFields).ToList();
        }

        public static DocSettingsAR GetInstance(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> allDocLinks)
        {
            _activeDoc = activeDoc;
            _allDocLinks = allDocLinks;
            return _instance.Value;
        }

        #region Properties

        public IWallIntersectionSettings WallIntersectionSettings { get; } = new WallIntersectionSettings()
        {
            WallOffset = 200 * _mmToFeet,
            InsertsOffset = 200 * _mmToFeet,
        };

        public IRoomTraversionSettings RoomTraversionSettings { get; } = new RoomTraversionSettings();

        /// <inheritdoc/>
        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "АР", "AR", "Тест" };

        #endregion
    }
}
