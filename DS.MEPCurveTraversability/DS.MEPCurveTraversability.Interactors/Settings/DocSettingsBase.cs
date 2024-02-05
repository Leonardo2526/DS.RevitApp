using Autodesk.Revit.DB;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using Rhino.Geometry;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DS.MEPCurveTraversability.Interactors.Settings
{
    /// <summary>
    /// The object that represent <see cref="Document"/>s settings.
    /// </summary>
    public abstract class DocSettingsBase
    {

        public DocSettingsBase()
        {
            
        }

        /// <summary>
        /// Fields to get <see cref="Document"/>s automatically.
        /// </summary>
        public abstract IEnumerable<string> AutoDocsDetectionFields { get; set; }

        /// <summary>
        /// <see cref="Document"/>s to apply settings.
        /// </summary>
        public List<Document> Docs { get; protected set; }
        
        /// <summary>
        /// Settings for check walls intersections.
        /// </summary>
        public IWallIntersectionSettings WallIntersectionSettings { get; set; } =
            new WallIntersectionSettings();

        /// <summary>
        /// Get <see cref="Document"/>s by <paramref name="detectionFields"/>.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="allDocLinks"></param>
        /// <param name="detectionFields"></param>
        /// <returns></returns>
        protected IEnumerable<Document> FilterByLastFolderName(
          Document activeDoc, IEnumerable<RevitLinkInstance> allDocLinks,
          IEnumerable<string> detectionFields)
        {
            var allDocs = activeDoc.GetDocuments(allDocLinks);

            if (detectionFields == null || detectionFields.Count() == 0) { return allDocs; }

            var lowerFields = detectionFields.Select(s => s.ToLower());

            return allDocs.Where(d => nameContainsField(d, lowerFields));

            bool nameContainsField(Document doc, IEnumerable<string> fields)
            {
                var path = doc.GetPathName();
                var lastFolderName = Path.GetFileName(Path.GetDirectoryName(path));
                return fields.Any(f => lastFolderName.ToLower().Contains(f));
            }
        }

        /// <summary>
        /// Get filter for <see cref="Docs"/>.
        /// </summary>
        /// <param name="globalFilter"></param>
        /// <returns></returns>
        public IDocumentFilter GetLocalFilter(IDocumentFilter globalFilter)
        {
            var localFilter=  globalFilter.Clone();
            localFilter.Docs = Docs;
            return localFilter;
        }
      
        /// <summary>
        /// Try to set <see cref="Docs"/> automatically.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="allDocLinks"></param>
        /// <returns></returns>
        public DocSettingsBase TrySetFilteredAutoDocs(
            Document activeDoc, 
            IEnumerable<RevitLinkInstance> allDocLinks)
        {
            var allDocs = activeDoc.GetDocuments(allDocLinks);
            var isValid = Docs is not null && 
                Docs.TrueForAll(d => d.IsValidObject && allDocs.Any(ad => ad.Title == d.Title));

            Docs = !isValid ? FilterByLastFolderName(
                    activeDoc,
                    allDocLinks,
                    AutoDocsDetectionFields).ToList() : Docs;
            return this;
        }
    }
}