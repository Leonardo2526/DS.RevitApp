using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    class PositionChecker
    {

        private XYZ _moveVector;
        private MovableElement _movableElement;
        private readonly Dictionary<MEPCurve, XYZ> _staticCenterPoints;
        ObstacleChecker _obstacleChecker;

        public PositionChecker(XYZ moveVector, MovableElement movableElement, 
            Dictionary<MEPCurve, XYZ> staticCenterPoints, ObstacleChecker obstChecker)
        {
            _moveVector = moveVector;
            _movableElement = movableElement;
            _staticCenterPoints = staticCenterPoints;
            _obstacleChecker = obstChecker;
        }

        public bool PositionAvailable { get; private set; } = true;
        public bool Stop { get; private set; }

        public void CheckMovement()
        {
            if (PositionAvailable)
            {
                CheckZ();
            }

            if (PositionAvailable)
            {
                CheckObstacles();
            }

            if (!Stop && PositionAvailable)
            {
                CheckElem1();
            }

            if (!Stop && PositionAvailable)
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
            _obstacleChecker.Check();

            if (_obstacleChecker.UnableToMove)
            {
                Stop = true;
            }


        }

        private void CheckElem1()
        {
            if (!Elem1CollisionChecker.CheckCollisions(_moveVector, _obstacleChecker.FamInstToMove))
            {
                UpdateMoveVector();
                PositionAvailable = false;
            }
        }


        private void CheckMovable()
        {
            if (!_movableElement.CheckCurrentCollisions(_movableElement, _moveVector,
              CollisionResolver.StartCollisionsCount, _staticCenterPoints, _obstacleChecker.FamInstToMove))
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

    }
}
