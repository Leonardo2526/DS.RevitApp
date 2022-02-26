using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class CollisionResolver
    {       
        List<int> startMovableElementCollisions = new List<int>();
        public static int StartColllisionsCount;
        public bool IsResolved;

        /// <summary>
        /// Resolve collision by intiating search for available Elem1 position through set of directions
        /// </summary>
        public void Resolve()
        {
            List<int> dxy = new List<int>
            {
                1,
                0,
                -1,
                0
            };
            List<int> dz = new List<int>
            {
                0,
                -1,
                0,
                1
            };
            XYZ moveVector = new XYZ();

            int i;
            for (i = 0; i < dxy.Count; i++)
            {
                moveVector = Data.GetNormOffset(Data.ElementClearence, dxy[i], dz[i]);

                MovableElement movableElement = new MovableElement(moveVector);
                startMovableElementCollisions = movableElement.GetCollisions();

                if (!movableElement.IsMovableElementsCountValid)
                    continue;

                if (StartColllisionsCount == 0)
                {
                    foreach (int c in startMovableElementCollisions)
                        StartColllisionsCount += c;
                }
              

                Dictionary<Element, XYZ> staticCenterPoints = movableElement.GetStaticCenterPoints();

                var positionRefact = new Position(moveVector, movableElement, staticCenterPoints);
                positionRefact.Find();
                if (positionRefact.PositonFound)
                {
                    IsResolved = true;
                    break;
                }

            }
        }

       
    }

}
