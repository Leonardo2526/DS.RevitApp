using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.CollisionsElliminator
{
    class CollisionCorrector
    {
        readonly Document Doc;
        readonly Data data;

        public CollisionCorrector(Document doc, Data dat)
        {
            Doc = doc;
            data = dat;
        }

        List<int> startMovableElementCollisions = new List<int>();
        public static int StartColllisionsCount;

        /// <summary>
        /// Intiate searching for available Elem1 position through set of directions
        /// </summary>
        public bool IsCorrected()
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
                moveVector = data.GetNormOffset(Data.ElementClearence, dxy[i], dz[i]);

                MovableElement movableElement = new MovableElement(Doc, data, moveVector);
                startMovableElementCollisions = movableElement.GetCollisions();

                if (!movableElement.IsMovableElementsCountValid)
                    continue;

                if (StartColllisionsCount == 0)
                {
                    foreach (int c in startMovableElementCollisions)
                        StartColllisionsCount += c;
                }
              

                Dictionary<Element, XYZ> staticCenterPoints = movableElement.GetStaticCenterPoints();

                if (Position.IfAvailableExist(moveVector, movableElement, staticCenterPoints))
                    return true;

            }

            return false;
        }

       
    }

}
