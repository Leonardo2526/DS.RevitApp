using Autodesk.Revit.DB;
using DS.Revit.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class MovedFamInstDicCreator
    {
        MovableElement _movableElement;
        XYZ _moveVector;

        public MovedFamInstDicCreator(XYZ moveVector, MovableElement movableElement)
        {
            _moveVector = moveVector;
            _movableElement = movableElement;
        }

        public Dictionary<Element, XYZ> FamInstToMove { get; set; } = new Dictionary<Element, XYZ>();

        /// <summary>
        /// Get family instances to move for current obstacleMEPCurve and all next
        /// </summary>
        /// <param name="obstacleMEPCurves"></param>
        /// <returns></returns>
        private void GetFamInstToMoveNext(MEPCurve obstacleMEPCurve)
        {
            List<Element> passedElements = _movableElement.MovableElements;
            Element famInstToMove = ConnectedElement.GetConnectedWithExclusions(passedElements, obstacleMEPCurve).FirstOrDefault();
            passedElements.Add(famInstToMove);

            double curvelength = MEPCurveUtils.GetLength(obstacleMEPCurve);
            XYZ moveVector = GetMoveVector(curvelength, Data.MinCurveLength);

            FamInstToMove.Add(famInstToMove, moveVector);

            var nextMEPCurves = GetNextMEPCurves(passedElements, famInstToMove);

            Obstacle obstacle = new Obstacle();
            List<MEPCurve> obstacteMEPCurves = obstacle.GetObstructiveMEPCurves(nextMEPCurves, _moveVector);

            if (obstacteMEPCurves.Count > 0)
            {
                GetFamInstToMoveNext(obstacteMEPCurves.FirstOrDefault());
            }
        }

        /// <summary>
        /// Get family instances to move for all obstacleMEPCurves
        /// </summary>
        /// <param name="obstacleMEPCurves"></param>
        /// <returns></returns>
        public Dictionary<Element, XYZ> GetAllFamInstToMove(List<MEPCurve> obstacleMEPCurves)
        {
            foreach (var mEPCurve in obstacleMEPCurves)
            {
                GetFamInstToMoveNext(mEPCurve);
            }

            return FamInstToMove;
        }

        public XYZ GetMoveVector(double curvelength, double MinCurveLength)
        {
            double deltaF = curvelength - MinCurveLength;
            double delta = UnitUtils.Convert(deltaF,
                                       DisplayUnitType.DUT_DECIMAL_FEET,
                                       DisplayUnitType.DUT_MILLIMETERS);

            PointUtils pointUtils = new PointUtils();
            XYZ newoffset = pointUtils.GetOffsetByMoveVector(Data.MoveVector, delta);

            
            return new XYZ(Data.MoveVector.X - newoffset.X, Data.MoveVector.Y - newoffset.Y, Data.MoveVector.Z - newoffset.Z);
        }

        private List<MEPCurve> GetNextMEPCurves(List<Element> passedElements, Element famInstToMove)
        {
            var nextMEPCurves = new List<MEPCurve>();

            List<Element> nextElements = ConnectedElement.GetConnectedWithExclusions(passedElements, famInstToMove);
            foreach (var item in nextMEPCurves)
            {
                Type type = item.GetType();

                if (type.Name != "FamilyInstance")
                {
                    nextMEPCurves.Add(item);
                }
            }

            return nextMEPCurves;
        }
    }
}
