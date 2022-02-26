using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class PositionRefact
    {
        public bool IsAvailableExist { get; set; }
        private bool PositionAvailable;
        private bool StopProcess;
        private XYZ VectorForFamInst;

        private readonly Application App = Data.Doc.Application;
        private XYZ MoveVector;
        private readonly MovableElement _movableElement;
        private readonly Dictionary<Element, XYZ> _staticCenterPoints;

        public PositionRefact(XYZ moveVector, MovableElement movableElement, Dictionary<Element, XYZ> staticCenterPoints)
        {
            MoveVector = moveVector;
            _movableElement = movableElement;
            _staticCenterPoints = staticCenterPoints;
        }


        /// <summary>
        /// Iterate through available positions of current direction. 
        /// Return true if position without collisions found and move Elem1 to it.
        /// </summary>
        public void Get()
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                if (StopProcess)
                    break;

                double moveElem1ToZ = Data.Elem1StartCenterLine.Origin.Z + MoveVector.Z;
                if (MoveVector.Z != 0)
                {
                    if (moveElem1ToZ < Data.MinZCoordinate || moveElem1ToZ > Data.MaxZCoordinate)
                        break;
                }

                Data.MoveVector = MoveVector;

                CheckMovement();

                if (VectorForFamInst != null && PositionAvailable && !StopProcess)
                {
                    TryToMoveElements(VectorForFamInst);
                }
            }
        }

        private void CheckMovement()
        {
            if (PositionAvailable && !StopProcess)
            {
                CheckElem1();
            }

            if (PositionAvailable && !StopProcess)
            {
                CheckObstacles();
            }

            if (PositionAvailable && !StopProcess)
            {
                CheckMovable();
            }
        }


        private void CheckElem1()
        {
            if (!Elem1CollisionChecker.CheckCollisions(MoveVector))
            {
                UpdateMoveVector();
                PositionAvailable = false;
            }
        }

        private void CheckObstacles()
        {
            if (!_movableElement.IsElementsObstacle(_movableElement.PotentialObstacledElements, MoveVector, out VectorForFamInst))
            {
                StopProcess = true;
            }
        }


        private void CheckMovable()
        {
            if (!_movableElement.CheckCurrentCollisions(_movableElement, MoveVector,
              CollisionResolver.StartColllisionsCount, _staticCenterPoints))
            {
                UpdateMoveVector();
                PositionAvailable = false;
            }            
        }

        private void UpdateMoveVector()
        {
            PointUtils pointUtils = new PointUtils();
            MoveVector += pointUtils.GetOffsetByMoveVector(Data.MoveVector, 100);
        }

        private void TryToMoveElements(XYZ VectorForFamInst)
        {
            if (!ElementMover.Move(_movableElement.FamInstToMove.Id, Data.Doc.Application, VectorForFamInst))
            {
                StopProcess = true;
            }
            else
            {
                if (!ElementMover.Move(Data.Elem1Curve.Id, App, MoveVector))
                {
                    StopProcess = true;
                }
            }
        }

    }
}
