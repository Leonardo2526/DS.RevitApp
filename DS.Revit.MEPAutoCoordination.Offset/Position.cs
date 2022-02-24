using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class Position
    {
        /// <summary>
        /// Iterate through available positions of current direction. 
        /// Return true if position without collisions found and move Elem1 to it.
        /// </summary>
        public static bool IfAvailableExist(XYZ moveVector, MovableElement movableElement, Dictionary<Element, XYZ> staticCenterPoints)
        {
            Application App = Data.Doc.Application;
            PointUtils pointUtils = new PointUtils();

            int i;
            for (i = 0; i < 20; i++)
            {

                double moveElem1ToZ = Data.Elem1StartCenterLine.Origin.Z + moveVector.Z;
                if (moveVector.Z != 0)
                {
                    if (moveElem1ToZ < Data.MinZCoordinate || moveElem1ToZ > Data.MaxZCoordinate)
                        break;
                }

                Data.MoveVector = moveVector;

                if (!movableElement.IsElementsObstacle(movableElement.PotentialObstacledElements, moveVector, out XYZ VectorForFamInst))
                    break;


                if (Elem1CollisionChecker.CheckCollisions(moveVector))
                {
                    if (movableElement.CheckCurrentCollisions(movableElement, moveVector,
                    CollisionCorrector.StartColllisionsCount, staticCenterPoints))
                    {
                        if (VectorForFamInst != null && !ElementMover.Move(movableElement.FamInstToMove.Id, Data.Doc.Application, VectorForFamInst))
                            break;

                        if (!ElementMover.Move(Data.Elem1Curve.Id, App, moveVector))
                            break;
                        else
                            return true;
                    }
                    else
                    {

                        moveVector = moveVector + pointUtils.GetOffsetByMoveVector(Data.MoveVector, 100);
                        continue;
                    }
                }
                else
                {

                    moveVector = moveVector + pointUtils.GetOffsetByMoveVector(Data.MoveVector, 100);
                    continue;
                }


            }
            return false;
        }

    }
}
