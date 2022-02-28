using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{
    static class ReducibleCurve
    {
        /// <summary>
        /// Get collisions count of MEPCurves which increase it's length due to moving elem1
        /// </summary>
        /// <param name="staticCenterPoints"></param>
        /// <param name="movableElement"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public static int GetCollisions(Dictionary<MEPCurve, XYZ> staticCenterPoints, MovableElement movableElement, XYZ moveVector)
        {
            int totalCount = 0;

            LineCollision lineCollision = new LineCollision();

            LinesUtils linesUtils = new LinesUtils(moveVector);


            foreach (KeyValuePair<MEPCurve, XYZ> keyValue in staticCenterPoints)
            {
                int count = 0;

                List<Line> reducibleElementLines = linesUtils.CreateAllReducibleLines(keyValue.Key, keyValue.Value, moveVector, false);

                List<Element> excludedElements = new List<Element>();
                excludedElements.AddRange(movableElement.MovableElements);

                if (ObstacleElement.ElementsToMove != null)
                {
                    excludedElements.AddRange(ObstacleElement.ElementsToMove);
                }

                lineCollision.SetModelSolids(reducibleElementLines, excludedElements);

                foreach (Line gLine in reducibleElementLines)
                {
                    IList<Element> CheckCollisions = lineCollision.GetAllLinesCollisions(gLine);
                    if (count < CheckCollisions.Count)
                        count = CheckCollisions.Count;
                }

                totalCount += count;
            }



            return totalCount;
        }
    }
}
