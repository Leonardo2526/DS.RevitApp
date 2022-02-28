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
        public static Dictionary<Element, List<Solid>> GetSolidsForMovable(List<Line> allCurrentPositionLines, List<Element> excludedElements)
        {
            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();
            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(allCurrentPositionLines));

            ExclusionFilter exclusionFilter = DS.Revit.Utils.ElementFilterUtils.GetExclustionFilter(excludedElements);

            FilteredElementCollector collector = new FilteredElementCollector(Data.Doc, Data.NotConnectedToElem1ElementIds);

            SolidByCollector solidByCollector = new SolidByCollector(collector, boundingBoxIntersectsFilter, exclusionFilter);
            return solidByCollector.GetModelSolids();
        }

    }
}
