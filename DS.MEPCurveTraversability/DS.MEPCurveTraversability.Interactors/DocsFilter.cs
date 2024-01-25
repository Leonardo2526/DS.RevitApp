using Autodesk.Revit.DB;
using OLMP.RevitAPI.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPCurveTraversability.Interactors
{
    public static class DocsFilter
    {
        public static IEnumerable<Document> FilterByLastFolderName(
           (Document, IEnumerable<RevitLinkInstance>) allDocLinks,
           IEnumerable<string> detectionFields)
        {
            var allLinks = allDocLinks.Item2;
            var allDocs = allDocLinks.Item1.GetDocuments(allLinks);

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
    }
}
