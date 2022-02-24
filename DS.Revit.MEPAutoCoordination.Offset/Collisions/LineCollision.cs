using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class LineCollision
    {
        readonly Document Doc;

        public LineCollision(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        Dictionary<Element, List<Solid>> modelSolids = new Dictionary<Element, List<Solid>>();
        Dictionary<Element, List<Solid>> linksSolids = new Dictionary<Element, List<Solid>>();

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

        /// <summary>
        /// Get solids of elements by boundig box from current model.
        /// </summary>   
        public Dictionary<Element, List<Solid>> GetModelSolids(FilteredElementCollector collector, BoundingBoxIntersectsFilter boundingBoxFilter)
        {
            //Get the elements witch intersect bounding box
            IList<Element> intersectedElementsBox = collector.WherePasses(boundingBoxFilter).ToElements();

            List<Solid> solids = new List<Solid>();
            Dictionary<Element, List<Solid>> collectorSolids = new Dictionary<Element, List<Solid>>();

            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElemUtils.GetSolids(elem);
                collectorSolids.Add(elem, solids);
            }

            return collectorSolids;
        }

        /// <summary>
        /// Get solids of elements by boundig box from all linked models.
        /// </summary>   
        public Dictionary<Element, List<Solid>> GetLinkSolids(BoundingBoxIntersectsFilter boundingBoxFilter)
        {
            foreach (RevitLinkInstance m_currentInstance in Data.AllLinks)
            {
                if (m_currentInstance != null)
                {
                    // Get the handle to the element in the link
                    Document linkedDoc = m_currentInstance.GetLinkDocument();
                    FilteredElementCollector collector = new FilteredElementCollector(linkedDoc, Data.AllLinkedElementsIds);
                    Dictionary<Element, List<Solid>> currentLinkSolids = GetModelSolids(collector, boundingBoxFilter);

                    foreach (KeyValuePair<Element, List<Solid>> keyValue in currentLinkSolids)
                        linksSolids.Add(keyValue.Key, keyValue.Value);

                }
            }

            return linksSolids;

        }
     

        /// <summary>
        /// Get solids of elements by boundig box from current model and all linked models.
        /// </summary>    
        public void GetAllModelSolids(List<Line> allCurrentPositionLines)
        {
            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();

            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = 
                boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(allCurrentPositionLines));

            FilteredElementCollector collector = null;
            try
            {
                collector = new FilteredElementCollector(Doc, Data.NotConnectedToElem1ElementIds);
                modelSolids = GetModelSolids(collector, boundingBoxIntersectsFilter);
            }
            catch
            { }

            linksSolids = GetLinkSolids(boundingBoxIntersectsFilter);
        }

        public void GetAllModelSolidsForObstacled(List<Line> allCurrentPositionLines, MovableElement movableElement)
        {
            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();

            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(new LinesBoundingBox(allCurrentPositionLines));

            //Get excluded elements
            ICollection<ElementId> elementIds = new List<ElementId>();
            List<Element> elements = new List<Element>();

            foreach (Element el in movableElement.MovableElements)
                elementIds.Add(el.Id);

            if (movableElement.FamInstToMove !=null)
            elementIds.Add(movableElement.FamInstToMove.Id);

            ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);

            //Get collector
            FilteredElementCollector collector = null;
            try
            {
                collector = new FilteredElementCollector(Doc, Data.NotConnectedToElem1ElementIds);
                collector.WherePasses(exclusionFilter);
                modelSolids = GetModelSolids(collector, boundingBoxIntersectsFilter);
            }
            catch
            { }

            linksSolids = GetLinkSolids(boundingBoxIntersectsFilter);
        }

        /// <summary>
        /// Get collisions of line with current model and all linked models elements. 
        /// </summary>  
        public IList<Element> GetAllLinesCollisions(Curve curve)
        {
            IList<Element> CollisionsInModel = GetElementsCurveCollisions(curve, modelSolids);
            IList<Element> CollisionsInLink = GetElementsCurveCollisions(curve, linksSolids);


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
