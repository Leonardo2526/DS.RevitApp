using Autodesk.Revit.DB;
using DS.Revit.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class ObstacleElement
    {
        public static XYZ VectorForFamInst { get; set; }
        public static Element ElementToMove { get; set; }


        public static Element GetElementToMove(List<Element> movableElements, Element ruducibleElement)
        {
            List<Element> famInstToMove = ConnectorUtils.GetConnectedFamilyInstances(ruducibleElement);

            var NoIntersections = new List<Element>();

            foreach (var one in famInstToMove)
            {
                if (!movableElements.Any(two => two.Id == one.Id))
                {
                    ElementToMove = one;
                    return one;
                }
            }

            return null;
        }

        public static XYZ GetMoveVector(double curvelength, double MinCurveLength)
        {
            double deltaF = curvelength - MinCurveLength;
            double delta = UnitUtils.Convert(deltaF,
                                       DisplayUnitType.DUT_DECIMAL_FEET,
                                       DisplayUnitType.DUT_MILLIMETERS);

            PointUtils pointUtils = new PointUtils();
            XYZ newoffset = pointUtils.GetOffsetByMoveVector(Data.MoveVector, delta);

            VectorForFamInst = new XYZ(Data.MoveVector.X - newoffset.X, Data.MoveVector.Y - newoffset.Y, Data.MoveVector.Z - newoffset.Z);
            return VectorForFamInst;
        }
    }
}
