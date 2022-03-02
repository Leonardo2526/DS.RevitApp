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

        XYZ _moveVector;

        public MovedFamInstDicCreator(XYZ moveVector)
        {
            _moveVector = moveVector;
        }

        public Dictionary<Element, XYZ> FamInstToMove { get; set; } = new Dictionary<Element, XYZ>();


        private Element GetFamInstToMove(List<Element> passedElements, MEPCurve obstacleMEPCurve)
        {
            List<Element> famInstToMove = ConnectorUtils.GetConnectedFamilyInstances(obstacleMEPCurve);

            var NoIntersections = new List<Element>();

            foreach (var one in famInstToMove)
            {
                if (!passedElements.Any(two => two.Id == one.Id))
                {
                    return one;
                }
            }

            return null;
        }

        /// <summary>
        /// Get family instances to move for current obstacleMEPCurve and all next
        /// </summary>
        /// <param name="obstacleMEPCurves"></param>
        /// <returns></returns>
        private void GetFamInstToMoveNext(MEPCurve obstacleMEPCurve)
        {
            Element famInstToMove = GetFamInstToMove(passedElements, obstacleMEPCurve);


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
    }
}
