using Autodesk.Revit.DB;
using DS.Revit.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class Obstacle
    {
        public static XYZ VectorForFamInst { get; set; }
        public static List<Element> ElementsToMove { get; set; }

        /// <summary>
        /// Get MEPCurves obstructive for current move vector
        /// </summary>
        /// <param name="potentialObstructiveMEPCurves"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public List<MEPCurve> GetObstructiveMEPCurves(List<MEPCurve> potentialObstructiveMEPCurves, XYZ moveVector)
        {
            List<MEPCurve> obstactedMEPCurves = new List<MEPCurve>();

            foreach (var mepCurve in potentialObstructiveMEPCurves)
            {
                MovableElementChecker movableElementChecker = new MovableElementChecker(moveVector, mepCurve);
                movableElementChecker.GetData();

                if (!movableElementChecker.CheckPosition())
                {
                    if (!movableElementChecker.CheckLength(movableElementChecker.AngleRad, out XYZ moveVectorForFamInst))
                    {
                        obstactedMEPCurves.Add(mepCurve);
                    }
                }

            }

            return obstactedMEPCurves;
        }

      

        public static List<Element> GetElementToMove(List<Element> movableElements, Element reducibleElement)
        {
            ElementsToMove = new List<Element>();
            List<Element> famInstToMove = ConnectorUtils.GetConnectedFamilyInstances(reducibleElement);

            var NoIntersections = new List<Element>();

            foreach (var one in famInstToMove)
            {
                if (!movableElements.Any(two => two.Id == one.Id))
                {
                    ElementsToMove.Add(one);
                }
            }

            return ElementsToMove;
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
