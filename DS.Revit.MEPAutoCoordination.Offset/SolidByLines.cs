using Autodesk.Revit.DB;
using DS.Revit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    static class SolidByLines
    {
        /// <summary>
        /// Get solids of elements by lines and boundig box from model.
        /// </summary>   
        public static Dictionary<Element, List<Solid>> GetSolidsByExclusions(List<Line> allCurrentPositionLines, List<Element> excludedElements)
        {
            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();
            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(allCurrentPositionLines));

            ExclusionFilter exclusionFilter = null;
            if (excludedElements.Count > 0)
            {
                exclusionFilter = DS.Revit.Utils.ElementFilterUtils.GetExclustionFilter(excludedElements);
            }

            FilteredElementCollector collector = new FilteredElementCollector(Data.Doc, Data.NotConnectedToElem1ElementIds);

            SolidByCollector solidByCollector = new SolidByCollector(collector, boundingBoxIntersectsFilter, exclusionFilter);
            return solidByCollector.GetModelSolids();
        }

        /// <summary>
        /// Get solids of elements by lines and boundig box from all linked models.
        /// </summary>   
        public static Dictionary<Element, List<Solid>> GetSolidsByExclusionsInLinks(List<Line> allCurrentPositionLines, List<Element> excludedElements)
        {
            var linksSolids = new Dictionary<Element, List<Solid>>();

            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();
            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(allCurrentPositionLines));

            foreach (RevitLinkInstance m_currentInstance in Data.AllLinks)
            {
                if (m_currentInstance != null)
                {
                    // Get the handle to the element in the link
                    Document linkedDoc = m_currentInstance.GetLinkDocument();
                    FilteredElementCollector collector = new FilteredElementCollector(linkedDoc, Data.AllLinkedElementsIds);

                    ExclusionFilter exclusionFilter = DS.Revit.Utils.ElementFilterUtils.GetExclustionFilter(excludedElements);

                    SolidByCollector solidByCollector = new SolidByCollector(collector, boundingBoxIntersectsFilter, exclusionFilter);
                    Dictionary<Element, List<Solid>> currentLinkSolids = solidByCollector.GetModelSolids();

                    foreach (KeyValuePair<Element, List<Solid>> keyValue in currentLinkSolids)
                        linksSolids.Add(keyValue.Key, keyValue.Value);
                }
            }

            return linksSolids;

        }

    }
}
