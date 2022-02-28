using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class Position
    {
        public bool PositonFound { get; private set; }

        private bool StopProcess;
        private bool PositionAvailable;
        private XYZ VectorForFamInst;

        private readonly Application App = Data.Doc.Application;

        private XYZ _moveVector;
        private readonly MovableElement _movableElement;
        private readonly Dictionary<Element, XYZ> _staticCenterPoints;

        public Position(XYZ moveVector, MovableElement movableElement, Dictionary<Element, XYZ> staticCenterPoints)
        {
            _moveVector = moveVector;
            _movableElement = movableElement;
            _staticCenterPoints = staticCenterPoints;
        }


        /// <summary>
        /// Find through available positions of current direction.
        /// </summary>
        public void Find()
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                PositionAvailable = true;

                Data.MoveVector = _moveVector;

                CheckMovement();

                if (StopProcess)
                    break;

                if (PositionAvailable)
                {
                    TryToMoveElements();
                    break;
                }

            }
        }

        private void CheckMovement()
        {
            if (PositionAvailable)
            {
                CheckZ();
            }

            if (PositionAvailable)
            {
                CheckObstacles();
            }

            if (!StopProcess && PositionAvailable)
            {
                CheckElem1();
            }

            if (!StopProcess && PositionAvailable)
            {
                CheckMovable();
            }
        }

        private void CheckZ()
        {
            double moveElem1ToZ = Data.Elem1StartCenterLine.Origin.Z + _moveVector.Z;
            if (_moveVector.Z != 0)
            {
                if (moveElem1ToZ < Data.MinZCoordinate || moveElem1ToZ > Data.MaxZCoordinate)
                    PositionAvailable = false;
            }
        }

        private void CheckObstacles()
        {
            if (!_movableElement.IsElementsObstacle(_movableElement.PotentialObstacledElements, _moveVector, out VectorForFamInst))
            {
                StopProcess = true;
            }
        }

        private void CheckElem1()
        {
            if (!Elem1CollisionChecker.CheckCollisions(_moveVector))
            {
                UpdateMoveVector();
                PositionAvailable = false;
            }
        }     


        private void CheckMovable()
        {
            if (!_movableElement.CheckCurrentCollisions(_movableElement, _moveVector,
              CollisionResolver.StartColllisionsCount, _staticCenterPoints))
            {
                UpdateMoveVector();
                PositionAvailable = false;
            }
        }

        private void UpdateMoveVector()
        {
            PointUtils pointUtils = new PointUtils();
            _moveVector += pointUtils.GetOffsetByMoveVector(Data.MoveVector, 100);
        }

        private void TryToMoveElements()
        {
            bool elem1MovingAvailable = true;

            if (VectorForFamInst != null)
            {
                if (!ElementMover.Move(ObstacleElement.ElementToMove.Id, App, VectorForFamInst))
                {
                    elem1MovingAvailable = false;
                }
            }

            if (elem1MovingAvailable)
            {
                if (ElementMover.Move(Data.Elem1Curve.Id, App, _moveVector))
                {
                    PositonFound = true;
                }
            }
        }

    }
}
