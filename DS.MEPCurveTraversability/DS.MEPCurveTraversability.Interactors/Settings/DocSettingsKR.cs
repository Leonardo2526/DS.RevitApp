using Autodesk.Revit.DB;
using MoreLinq;
using OLMP.RevitAPI.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <inheritdoc/>
    public class DocSettingsKR : DocSettingsBase
    {
        private static readonly Lazy<DocSettingsKR> _instance = 
            new(() =>new DocSettingsKR(_activeDoc, _allDocLinks));

        private static Document _activeDoc;
        private static IEnumerable<RevitLinkInstance> _allDocLinks;

        private DocSettingsKR(Document activeDoc, IEnumerable<RevitLinkInstance> allDocLinks)
        {
            Docs = FilterByLastFolderName(
                    activeDoc,
                    allDocLinks,
                    AutoDocsDetectionFields).ToList();
        }

        public static DocSettingsKR GetInstance(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> allDocLinks)
        {
            _activeDoc = activeDoc;
            _allDocLinks = allDocLinks;
            return _instance.Value;
        }

        #region Properties

        public IWallIntersectionSettings WallIntersectionSettings { get; } = 
            new WallIntersectionSettings();

        /// <inheritdoc/>
        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "КР", "KR" };

        #endregion
    }
}
