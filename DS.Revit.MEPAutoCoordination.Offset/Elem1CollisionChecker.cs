using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.CollisionsElliminator
{
    class Elem1CollisionChecker
    {
        /// <summary>
        /// Check collisions of all element lines in current position. Return false if collisions exist.
        /// </summary>
        public static bool CheckCollisions(XYZ moveVector)
        {
            LineCollision lineCollision = new LineCollision(Data.Elem1.Document);

            List<Line> lines = CreateLinesByVector(moveVector);

            lineCollision.GetAllModelSolids(lines);

            foreach (Line gLine in lines)
            {
                IList<Element> CheckCollisions = lineCollision.GetAllLinesCollisions(gLine);

                if (CheckCollisions.Count != 0)
                    return false;
            }

            return true;
        }

        public static List<Line> CreateLinesByVector(XYZ moveVector)
        {
            LinesUtils linesUtils = new LinesUtils(moveVector);
            return linesUtils.CreateAllElementLines(Data.Elem1, moveVector, false);
        }
    }
}
