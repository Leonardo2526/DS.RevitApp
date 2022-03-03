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
                    if (!movableElementChecker.CheckLength(movableElementChecker.AngleRad))
                    {
                        obstactedMEPCurves.Add(mepCurve);
                    }
                }

            }

            return obstactedMEPCurves;
        }      
    }
}
