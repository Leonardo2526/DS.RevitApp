using Autodesk.Revit.DB;
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

        private void GetElementToMove(MEPCurve mEPCurve)
        {


        }

        public Dictionary<Element, XYZ> GetAllElementsToMove(List<MEPCurve> ObstacledMEPCurves)
        {
            foreach (var mEPCurve in ObstacledMEPCurves)
            {
                GetElementToMove(mEPCurve);
            }

            return FamInstToMove;
        }
    }
}
