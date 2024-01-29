using Autodesk.Revit.DB;
using MoreLinq;
using OLMP.RevitAPI.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace DS.MEPCurveTraversability.Interactors.Settings
{
    public class DocSettingsKR : DocSettingsBase
    {
        private static readonly Lazy<DocSettingsKR> _instance = 
            new(() =>new DocSettingsKR());


        private DocSettingsKR()
        {}

        public static DocSettingsKR GetInstance()
        {
            return _instance.Value;
        }

        #region Properties

        public WallIntersectionSettings WallIntersectionSettings { get; } = new();
        public override IEnumerable<string> AutoDocsDetectionFields { get; set; } =
            new List<string>() { "КР", "KR" };

        #endregion
    }
}
