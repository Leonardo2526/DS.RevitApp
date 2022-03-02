using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class ObstacleChecker
    {
        readonly List<MEPCurve> _mEPCurves;
        readonly XYZ _moveVector;

        public ObstacleChecker(List<MEPCurve> mEPCurves, XYZ moveVector)
        {
            _mEPCurves = mEPCurves;
            _moveVector = moveVector;
        }

        public Dictionary<Element, XYZ> FamInstToMove { get; private set; } = new Dictionary<Element, XYZ>();
        public bool UnableToMove { get; set; }

        /// <summary>
        /// Check if obstacle elements exists in mepCurves. 
        /// Return true if no obstales curves exist for moving and false if it exist.
        /// In the last case dictionary with families instances for move will be filled. 
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            Obstacle obstacle = new Obstacle();
            List<MEPCurve> obstacteMEPCurves = obstacle.GetObstructiveMEPCurves(_mEPCurves, _moveVector);

            if (obstacteMEPCurves.Count > 0)
            {
                return false;
            }
            else
            {
                FillFamInstToMove(obstacteMEPCurves);
            }

            return true;
        }

        private Dictionary<Element, XYZ> FillFamInstToMove(List<MEPCurve> obstacteMEPCurves)
        {
            MovedFamInstDicCreator movedFamInstDicCreator = new MovedFamInstDicCreator(_moveVector);
            FamInstToMove = movedFamInstDicCreator.GetAllFamInstToMove(obstacteMEPCurves);
            return FamInstToMove;
        }
    }
}
