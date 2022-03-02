using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    static class MoverToPosition
    {
        private static bool movingAvailable = true;

        public static bool TryToMoveElements(ObstacleChecker obstacleChecker, XYZ _moveVector)
        {
            MoveFamInst(obstacleChecker);
            MoveElem1(_moveVector);

            return movingAvailable;
        }

        private static void MoveFamInst(ObstacleChecker obstacleChecker)
        {
            if (obstacleChecker.FamInstToMove != null && obstacleChecker.FamInstToMove.Count > 0)
            {
                foreach (var item in obstacleChecker.FamInstToMove)
                {
                    if (!ElementMover.Move(item.Key.Id, Data.Doc.Application, item.Value))
                    {
                        movingAvailable = false;
                    }
                }
            }
        }

        private static void MoveElem1(XYZ _moveVector)
        {
            if (movingAvailable)
            {
                if (!ElementMover.Move(Data.Elem1Curve.Id, Data.Doc.Application, _moveVector))
                {
                    movingAvailable = false;
                }
            }
           
        }
    }
}
