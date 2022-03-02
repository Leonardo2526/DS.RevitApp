using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class Position
    {
        public bool PositonFound { get; private set; }

        private XYZ _moveVector;
        private readonly MovableElement _movableElement;
        private readonly Dictionary<MEPCurve, XYZ> _staticCenterPoints;

        public Position(XYZ moveVector, MovableElement movableElement, Dictionary<MEPCurve, XYZ> staticCenterPoints)
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
                Data.MoveVector = _moveVector;

                ObstacleChecker obstacleChecker = new ObstacleChecker(_movableElement.PotentialReducibleElements, _moveVector);
                PositionChecker positionChecker = new PositionChecker(_moveVector, _movableElement, _staticCenterPoints, obstacleChecker);

                positionChecker.CheckMovement();

                if (positionChecker.Stop)
                    break;

                if (positionChecker.PositionAvailable)
                {
                    if (MoverToPosition.TryToMoveElements(obstacleChecker, _moveVector))
                    {
                        PositonFound = true;
                    }
                    break;
                }

            }
        }



    }
}
