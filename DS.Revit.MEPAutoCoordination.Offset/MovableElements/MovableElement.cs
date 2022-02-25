using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DS.Revit.Utils.MEP;

namespace DS.Revit.MEPAutoCoordination.Offset
{

    class MovableElement
    {
        readonly XYZ MoveVector;

        public MovableElement(XYZ moveVector)
        {
            MoveVector = moveVector;
        }

        ConnectorUtils connectorUtils = new ConnectorUtils();
        ElementUtils elementUtils = new ElementUtils();

        public List<Element> MovableElements = new List<Element>();
        public List<Element> PotentialObstacledElements = new List<Element>();

        public bool IsMovableElementsCountValid { get; set; } = true;

        public Element FamInstToMove { get; set; }

        /// <summary>
        /// Check if current move vector is available for correct system connection.
        /// </summary>
        public bool GetMovableElements(XYZ moveVector)
        {
            List<Element> elements = new List<Element>();
            elements.Add(Data.Elem1Curve);
            List<Element> preElements = new List<Element>();

            SearchConnected(elements, preElements);


            return true;
        }

        /// <summary>
        /// Get elements obstructed for moving.
        /// </summary>
        void SearchConnected(List<Element> elements, List<Element> preElements)
        {

            List<Element> connectedToCurrent = new List<Element>();
            List<Element> elementsForSearch = new List<Element>();
            IEnumerable<ElementId> elementsIds = elements.Select(el => el.Id);
            IEnumerable<ElementId> preElementsIds = preElements.Select(el => el.Id);

            foreach (Element element in elements)
            {
                IEnumerable<ElementId> connectedElemIdsEnum = ConnectorUtils.GetConnectedElements(element).Select(el => el.Id);
                foreach (ElementId elId in connectedElemIdsEnum)
                {
                    if (!preElementsIds.Contains(elId))
                        connectedToCurrent.Add(Data.Doc.GetElement(elId));
                }
            }

            elementsForSearch = GetElementsForSearch(connectedToCurrent);

            if (elementsForSearch.Count > 0)
                SearchConnected(elementsForSearch, elements);
        }

        /// <summary>
        /// Get elements for next search step. Add to output if it are families or MEP elements with axis athwart to move vector. 
        /// If obstacles found, return null.
        /// </summary>
        List<Element> GetElementsForSearch(List<Element> elements)
        {
            List<Element> elementsForNewSearch = new List<Element>();

            foreach (Element element in elements)
            {
                MovableElementChecker movableElementChecker = new MovableElementChecker(MoveVector, element);

                Type type = element.GetType();
                if (type.ToString().Contains("System") | type.ToString().Contains("Insulation"))
                    continue;

                MovableElements.Add(element);

                if (movableElementChecker.GetTypeName() == "FamilyInstance")
                    elementsForNewSearch.Add(element);
                else
                {
                    movableElementChecker.GetData();
                    if (!movableElementChecker.CheckAngle())
                        PotentialObstacledElements.Add(element);
                    else if (movableElementChecker.CheckAngle())
                        elementsForNewSearch.Add(element);
                }
            }

            return elementsForNewSearch;
        }

       

        public bool IsElementsObstacle(List<Element> elements, XYZ moveVector, out XYZ VectorForFamInst)
        {
            VectorForFamInst = null;
            foreach (Element element in elements)
            {
                MovableElementChecker movableElementChecker = new MovableElementChecker(moveVector, element);
                movableElementChecker.GetData();

                if (!movableElementChecker.CheckPosition())
                {
                    if (!movableElementChecker.CheckLength(movableElementChecker.AngleRad, out XYZ moveVectorForFamInst))
                    {
                        //Check if family instance is available to move
                        VectorForFamInst = moveVectorForFamInst;
                        List<Element> famInstToMove = ConnectorUtils.GetConnectedFamilyInstances(element);

                        for (int i = 0; i < famInstToMove.Count; i++)
                        {
                            for (int j = 0; j < MovableElements.Count; j++)
                            {
                                if (famInstToMove[i].Id == MovableElements[j].Id)
                                    continue;
                                else
                                {
                                    FamInstToMove = famInstToMove[i];
                                    break;
                                }

                            }
                        }
                    }
                }
                
            }

            return true;
        }

        public List<int> GetCollisions()
        {
            GetMovableElements(MoveVector);

            if (MovableElements.Count == 0)
                return new List<int>();


            if (MovableElements.Count > 150)
            {
                IsMovableElementsCountValid = false;
                return new List<int>();
            }

            ElementUtils elementUtils = new ElementUtils();
            List<Solid> movableElementsSolids = elementUtils.GetSolidsOfElements(MovableElements);

            IBoundingBoxFilter elementsBoundingBoxFilter = new SolidsBoundingBox(movableElementsSolids);

            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();
            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(elementsBoundingBoxFilter);

            IMovableElemCollision movableElemCollision =
                   new MovableElementCollision(MovableElements, boundingBoxIntersectsFilter, movableElementsSolids, FamInstToMove);

            List<int> collisions = movableElemCollision.GetCollisions();

            if (!movableElemCollision.IsModelsElementsCountValid)
            {
                IsMovableElementsCountValid = false;
                return new List<int>();
            }

            return collisions;
        }

        public List<int> GetCollisionsByTransform(List<Element> movableElements, XYZ moveVector)
        {
            ElementUtils elementUtils = new ElementUtils();
            List<Solid> movableElementsSolids = elementUtils.GetTransformSolidsOfElements(movableElements, moveVector);

            IBoundingBoxFilter elementsBoundingBoxFilter = new SolidsBoundingBox(movableElementsSolids);

            BoundingBoxFilter boundingBoxFilter = new BoundingBoxFilter();
            BoundingBoxIntersectsFilter boundingBoxIntersectsFilter =
                boundingBoxFilter.GetBoundingBoxFilter(elementsBoundingBoxFilter);

            IMovableElemCollision movableElemCollision =
                   new MovableElementCollision(MovableElements, boundingBoxIntersectsFilter, movableElementsSolids, FamInstToMove);

            return movableElemCollision.GetCollisions();
        }

        public Dictionary<Element, XYZ> GetStaticCenterPoints()
        {
            Dictionary<Element, XYZ> staticCenterPoints = new Dictionary<Element, XYZ>();

            elementUtils.GetPoints(Data.Elem1Curve, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            foreach (Element element in PotentialObstacledElements)
            {
                elementUtils.GetPoints(element, out XYZ p1, out XYZ p2, out XYZ cp);
                if (p1.DistanceTo(centerPoint) < p2.DistanceTo(centerPoint))
                    staticCenterPoints.Add(element, p2);
                else
                    staticCenterPoints.Add(element, p1);

            }

            return staticCenterPoints;
        }

        /// <summary>
        /// Get collisions of MEPCurves which increase it's length due to moving elem1
        /// </summary>
        /// <param name="staticCenterPoints"></param>
        /// <param name="movableElement"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        int GetCollisionsByObstacled(Dictionary<Element, XYZ> staticCenterPoints, MovableElement movableElement, XYZ moveVector)
        {
            int totalCount = 0;

            LineCollision lineCollision = new LineCollision(Data.Doc);

            LinesUtils linesUtils = new LinesUtils(moveVector);


            foreach (KeyValuePair<Element, XYZ> keyValue in staticCenterPoints)
            {
                int count = 0;

                List<Line> obstacledElementLines = linesUtils.CreateAllObstacledElementLines(keyValue.Key, keyValue.Value, moveVector, false);

                lineCollision.GetAllModelSolidsForObstacled(obstacledElementLines, movableElement);

                foreach (Line gLine in obstacledElementLines)
                {
                    IList<Element> CheckCollisions = lineCollision.GetAllLinesCollisions(gLine);
                    if (count < CheckCollisions.Count)
                        count = CheckCollisions.Count;
                }

                totalCount = totalCount + count;
            }



            return totalCount;
        }

        public bool CheckCurrentCollisions(
           MovableElement movableElement, XYZ moveVector, int startColllisionsCount, Dictionary<Element, XYZ> staticCenterPoints)
        {
            if (staticCenterPoints.Count == 0)
                return true;

            List<Element> checkedElements = new List<Element>();
            List<ElementId> potentialObstacledElementsIds =
                movableElement.PotentialObstacledElements.Select(el => el.Id).ToList();

            foreach (Element element in movableElement.MovableElements)
            {
                if (!potentialObstacledElementsIds.Contains(element.Id))
                    checkedElements.Add(element);
            }

            List<int> currentMovableElementCollisions =
                        movableElement.GetCollisionsByTransform(checkedElements, moveVector);

            int currentCollisionsCount = 0;
            foreach (int c in currentMovableElementCollisions)
                currentCollisionsCount += c;

            if (staticCenterPoints.Count > 0)
            {
                int obstacledElementsCollision = GetCollisionsByObstacled(staticCenterPoints, movableElement, moveVector);
                currentCollisionsCount = currentCollisionsCount + obstacledElementsCollision;
            }


            if (currentCollisionsCount > startColllisionsCount)
                return false;

            return true;
        }
    }


}
