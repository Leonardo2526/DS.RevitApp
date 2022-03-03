using Autodesk.Revit.DB;
using DS.Revit.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    static class ConnectedElement
    {
        public static List<Element> GetConnectedWithExclusions(List<Element> excludedElements, Element sourceElement)
        {
            List<Element> famInstToMove = ConnectorUtils.GetConnectedFamilyInstances(sourceElement);

            var NoIntersections = new List<Element>();

            foreach (var one in famInstToMove)
            {
                if (!excludedElements.Any(two => two.Id == one.Id))
                {
                    NoIntersections.Add(one);
                }
            }

            return NoIntersections;
        }
    }
}
