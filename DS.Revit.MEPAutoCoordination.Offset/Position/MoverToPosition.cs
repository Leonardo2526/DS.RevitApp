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
        private static bool movingAvailable;

        public static bool TryToMoveElements(ObstacleChecker obstacleChecker, XYZ _moveVector)
        {
            movingAvailable = true;

            MoveFamInst(obstacleChecker);
            MoveElem1(_moveVector);

            return movingAvailable;
        }

        private static void MoveFamInst(ObstacleChecker obstacleChecker)
        {
            if (obstacleChecker.FamInstToMoveDic != null && obstacleChecker.FamInstToMoveDic.Count > 0)
            {
                for (int i = obstacleChecker.FamInstToMoveDic.Count; i-- > 0;)
                {
                    if (!ElementMover.Move(obstacleChecker.FamInstToMoveDic.ElementAt(i).Key.Id, 
                        Data.Doc.Application, 
                        obstacleChecker.FamInstToMoveDic.ElementAt(i).Value))
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
