using Autodesk.Revit.DB;
using System.Collections.Generic;
using DS.Revit.Utils;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class LineCollision
    {
        public Dictionary<Element, List<Solid>> ModelSolidsForCollisionCheck { get; set; } = new Dictionary<Element, List<Solid>>();
        public Dictionary<Element, List<Solid>> LinksSolidsForCollisionCheck { get; set; } = new Dictionary<Element, List<Solid>>();

        public IList<Element> GetElementsCurveCollisions(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            IList<Element> intersectedElements = new List<Element>();

            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {
                    SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();

                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);

                    if (intersection.SegmentCount != 0)
                        intersectedElements.Add(keyValue.Key);
                }
            }

            return intersectedElements;
        }

        public void SetModelSolids(List<Line> allCurrentPositionLines, List<Element> excludedElements)
        {
            ModelSolidsForCollisionCheck = SolidByLines.GetSolidsByExclusions(allCurrentPositionLines, excludedElements);
            LinksSolidsForCollisionCheck = SolidByLines.GetSolidsByExclusionsInLinks(allCurrentPositionLines, excludedElements);
        }

        /// <summary>
        /// Get collisions of line with current model and all linked models elements. 
        /// </summary>  
        public IList<Element> GetAllLinesCollisions(Curve curve)
        {
            IList<Element> CollisionsInModel = GetElementsCurveCollisions(curve, ModelSolidsForCollisionCheck);
            IList<Element> CollisionsInLink = GetElementsCurveCollisions(curve, LinksSolidsForCollisionCheck);


            List<Element> allCollisions = new List<Element>();
            allCollisions.AddRange(CollisionsInModel);
            allCollisions.AddRange(CollisionsInLink);

            return allCollisions;
        }

        /// <summary>
        /// Get bounding box by list of lines
        /// </summary>
        public BoundingBoxIntersectsFilter GetBoundingBoxFilter(List<Line> allCurrentPositionLines)
        {
            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByLines(allCurrentPositionLines, out XYZ minPoint, out XYZ maxPoint);

            XYZ minRefPoint = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
            XYZ maxRefPoint = new XYZ(maxPoint.X, maxPoint.Y, maxPoint.Z);

            Outline myOutLn = new Outline(minRefPoint, maxRefPoint);

            //TransactionUtils transactionUtils = new TransactionUtils();
            //transactionUtils.CreateModelCurve(new CreateModelCurveTransaction(Doc, minRefPoint, maxRefPoint));

            return new BoundingBoxIntersectsFilter(myOutLn);
        }

    }
}
