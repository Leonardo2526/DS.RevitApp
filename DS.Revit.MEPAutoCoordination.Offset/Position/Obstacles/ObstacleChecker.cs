using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class ObstacleChecker
    {
        readonly List<MEPCurve> _mEPCurves;
        readonly XYZ _moveVector;
        readonly MovableElement _movableElement;

        public ObstacleChecker(List<MEPCurve> mEPCurves, XYZ moveVector, MovableElement movableElement)
        {
            _mEPCurves = mEPCurves;
            _moveVector = moveVector;
            _movableElement = movableElement;
        }

        public Dictionary<Element, XYZ> FamInstToMoveDic { get; private set; } = new Dictionary<Element, XYZ>();
        public List<Element> FamInstToMove
        {
            get
            {
                List<Element> famInstToMove = new List<Element>();
                foreach (var item in FamInstToMoveDic)
                {
                    famInstToMove.Add(item.Key);
                }

                return famInstToMove;
            }
        }

        public bool UnableToMove { get; set; }

        /// <summary>
        /// Check if obstacle curves exists in mepCurves. 
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
            MovedFamInstDicCreator movedFamInstDicCreator = new MovedFamInstDicCreator(_moveVector, _movableElement);
            FamInstToMoveDic = movedFamInstDicCreator.GetAllFamInstToMove(obstacteMEPCurves);
            return FamInstToMoveDic;
        }
    }
}
